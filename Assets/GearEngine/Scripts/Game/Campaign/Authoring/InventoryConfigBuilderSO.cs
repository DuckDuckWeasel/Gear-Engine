using System.Collections.Generic;
using System.Linq;
using GearEngine.GearEngine.Config;
using LiveOps.Modules.DTO.Inventory;
using Scaffold.LiveOps.Authoring;
using UnityEngine;

namespace GearEngine.Campaign.Authoring
{
    [CreateAssetMenu(menuName = "LiveOps/Authoring/Inventory Config Builder", fileName = "InventoryConfigBuilder")]
    public sealed class InventoryConfigBuilderSO : ConfigBuilderSO<InventoryConfig>
    {
        [SerializeField]
        private int baseSlots = 8;

        [SerializeField]
        [Tooltip("Core/motor gear (SO). Its catalog id becomes startingGearIds[0] in Remote Config.")]
        private GearItem motorCogGear;

        [SerializeField]
        [Tooltip("Other starting gears (SO references). Ids are appended after the motor in Remote Config.")]
        private List<GearItem> additionalStartingGears = new List<GearItem>();

        [SerializeField]
        [Tooltip("Resolves Remote Config gear ids back to references when using Apply (pull from cloud).")]
        private GearCatalogSO gearCatalogForApply;

        public override string ConfigKey => nameof(InventoryConfig);

        public override InventoryConfig Build()
        {
            var ids = new List<string>();

            if (motorCogGear != null && !string.IsNullOrEmpty(motorCogGear.Id))
            {
                ids.Add(motorCogGear.Id);
            }

            if (additionalStartingGears != null)
            {
                foreach (GearItem gear in additionalStartingGears)
                {
                    if (gear == null || string.IsNullOrEmpty(gear.Id))
                    {
                        continue;
                    }

                    if (ids.Contains(gear.Id))
                    {
                        continue;
                    }

                    ids.Add(gear.Id);
                }
            }

            return new InventoryConfig
            {
                BaseSlots = baseSlots,
                StartingGearIds = ids,
            };
        }

        public override void Apply(InventoryConfig pulled)
        {
            if (pulled == null)
            {
                return;
            }

            baseSlots = pulled.BaseSlots;
            if (pulled.StartingGearIds == null || pulled.StartingGearIds.Count == 0)
            {
                motorCogGear = null;
                additionalStartingGears = new List<GearItem>();
                return;
            }

            if (gearCatalogForApply == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    "[InventoryConfigBuilder] Apply skipped resolving startingGearIds to GearItem refs — assign gearCatalogForApply on this asset.",
                    this);
#endif
                return;
            }

            string coreId = pulled.GetCoreGearCatalogId();
            motorCogGear = string.IsNullOrEmpty(coreId) ? null : gearCatalogForApply.Get(coreId);

            var rest = new List<GearItem>();
            for (int i = 1; i < pulled.StartingGearIds.Count; i++)
            {
                string id = pulled.StartingGearIds[i];
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                GearItem cfg = gearCatalogForApply.Get(id);
                if (cfg != null)
                {
                    rest.Add(cfg);
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogWarning($"[InventoryConfigBuilder] Apply: no GearItem in catalog for id '{id}'.", this);
                }
#endif
            }

            additionalStartingGears = rest;
        }
    }
}
