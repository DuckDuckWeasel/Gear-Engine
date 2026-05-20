using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiveOps.DTO.GameModule;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.Inventory;
using Scaffold.LiveOps.Authoring;
using UnityEngine;

namespace GearEngine.App.Bootstrap.Offline
{
    /// <summary>
    /// Builds an <see cref="OfflineLiveOpsService"/> from local <see cref="ConfigBuilderSOBase"/> assets.
    /// For each builder, looks up the matching <c>XGameData(XPersistence, XConfig)</c> constructor by
    /// naming convention and instantiates it with a fresh persistence (session-only, not persisted).
    /// </summary>
    public static class OfflineLiveOpsServiceBuilder
    {
        public static OfflineLiveOpsService Build(IReadOnlyList<ConfigBuilderSOBase> builders)
        {
            var modules = new Dictionary<Type, IGameModuleData>();

            if (builders == null || builders.Count == 0)
            {
                Debug.LogWarning("[OfflineLiveOps] No ConfigBuilderSO assets were assigned. GetModuleData<T>() will return null for every module.");
                return new OfflineLiveOpsService(modules);
            }

            foreach (ConfigBuilderSOBase builder in builders)
            {
                if (builder == null)
                {
                    continue;
                }

                try
                {
                    IGameModuleData data = BuildModuleData(builder);
                    if (data != null)
                    {
                        modules[data.GetType()] = data;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[OfflineLiveOps] Failed to build module data from {builder.name} ({builder.ConfigType?.Name}): {ex.Message}\n{ex.StackTrace}");
                }
            }

            Debug.Log($"[OfflineLiveOps] Built {modules.Count} module(s) from {builders.Count} config builder(s).");
            return new OfflineLiveOpsService(modules);
        }

        private static IGameModuleData BuildModuleData(ConfigBuilderSOBase builder)
        {
            Type configType = builder.ConfigType;
            if (configType == null)
            {
                Debug.LogWarning($"[OfflineLiveOps] Builder {builder.name} has no ConfigType.");
                return null;
            }

            object config = builder.BuildBoxed();
            if (config == null)
            {
                Debug.LogWarning($"[OfflineLiveOps] Builder {builder.name} produced a null config.");
                return null;
            }

            Type gameDataType = ResolveGameDataType(configType);
            if (gameDataType == null)
            {
                Debug.LogWarning($"[OfflineLiveOps] Could not find IGameModuleData type matching config {configType.Name}.");
                return null;
            }

            ConstructorInfo ctor = FindPersistenceConfigCtor(gameDataType, configType);
            if (ctor == null)
            {
                Debug.LogWarning($"[OfflineLiveOps] {gameDataType.Name} has no public (persistence, config) constructor.");
                return null;
            }

            Type persistenceType = ctor.GetParameters()[0].ParameterType;
            object persistence = Activator.CreateInstance(persistenceType);
            SeedPersistence(persistence, config);

            return (IGameModuleData)ctor.Invoke(new[] { persistence, config });
        }

        private static Type ResolveGameDataType(Type configType)
        {
            // Convention: FooConfig -> FooGameData in the same assembly. Currency is the exception
            // (CurrencyConfig -> CurrencyGameData) which still matches the rule.
            string configName = configType.Name;
            string gameDataName = configName.EndsWith("Config", StringComparison.Ordinal)
                ? configName.Substring(0, configName.Length - "Config".Length) + "GameData"
                : configName + "GameData";

            Type match = configType.Assembly
                .GetTypes()
                .FirstOrDefault(t =>
                    t.Name == gameDataName
                    && !t.IsAbstract
                    && typeof(IGameModuleData).IsAssignableFrom(t));

            return match;
        }

        private static ConstructorInfo FindPersistenceConfigCtor(Type gameDataType, Type configType)
        {
            foreach (ConstructorInfo ctor in gameDataType.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                ParameterInfo[] ps = ctor.GetParameters();
                if (ps.Length == 2 && ps[1].ParameterType == configType)
                {
                    return ctor;
                }
            }

            return null;
        }

        private static void SeedPersistence(object persistence, object config)
        {
            switch (persistence)
            {
                case CurrencyPersistence cp when config is CurrencyConfig cc:
                    CurrencyPersistenceSeeder.SeedAndClampInPlace(cp, cc);
                    break;
                case InventoryPersistence ip when config is InventoryConfig ic:
                    SeedInventoryStartingGears(ip, ic);
                    break;
            }
        }

        private static void SeedInventoryStartingGears(InventoryPersistence persistence, InventoryConfig config)
        {
            if (persistence.StartingGearsSeeded || config.StartingGearIds == null || config.StartingGearIds.Count == 0)
            {
                return;
            }

            persistence.Gears ??= new List<OwnedGearEntry>();
            for (int i = 0; i < config.StartingGearIds.Count; i++)
            {
                string gearId = config.StartingGearIds[i];
                if (string.IsNullOrEmpty(gearId))
                {
                    continue;
                }

                string instanceId = i == 0 ? "motor" : $"start_{i}";
                persistence.Gears.Add(new OwnedGearEntry { InstanceId = instanceId, GearId = gearId });
            }

            persistence.StartingGearsSeeded = true;
        }
    }
}
