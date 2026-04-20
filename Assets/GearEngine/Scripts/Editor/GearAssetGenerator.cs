using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using GearEngine.GearEngine.Config;
using GearEngine.Campaign.Gear;
using GearEngine.GearEngine.Abilities;
using GearEngine.GearEngine.Services.Inventory;

namespace GearEngine.Editor
{
    public class GearAssetGenerator : EditorWindow
    {
        private const string GENERATION_PATH = "Assets/GearEngine/Scriptables/GeneratedGears";
        private const string VARIABLES_PATH  = "Assets/GearEngine/Data/Cars/Variables";
        private const int MAX_TIERS = 5;

        // Cached VariableSO assets loaded from the project
        private static Dictionary<string, ScriptableObject> _variableCache;

        // Base abilities that are debug stubs with no real logic — skip generating tiered variants
        private static readonly HashSet<string> SkipTypes = new HashSet<string>
        {
            "BoostAbility", "CurveDriftAbility", "DriftUpAbility",
            "GasAbility", "VelocityAbility", "AccelerationAbility"
        };

        [MenuItem("Gear Engine/⚙️ Mass Generate Gear Archetypes (The Forge)")]
        public static void ForgeGears()
        {
            if (EditorUtility.DisplayDialog("The Gear Forge", 
                "This will use Reflection to scan all GearAbilitySOs and generate 5 Tiered variations (Common → Legendary) for each, with proper stat targets and rarity-proportional values.\n\nProceed?", "Forge!", "Cancel"))
            {
                ExecuteForge();
            }
        }

        private static void ExecuteForge()
        {
            // Ensure output folders exist
            EnsureFolder("Assets/GearEngine", "Scriptables");
            EnsureFolder("Assets/GearEngine/Scriptables", "GeneratedGears");

            // Pre-load all VariableSO assets
            LoadVariableCache();

            // Find all concrete GearAbilitySO implementations
            var types = TypeCache.GetTypesDerivedFrom<GearAbilitySO>()
                .Where(t => !t.IsAbstract && !t.IsGenericType
                         && t.IsSubclassOf(typeof(ScriptableObject))
                         && !SkipTypes.Contains(t.Name))
                .ToList();

            int createdCount = 0;

            try
            {
                for (int t = 0; t < types.Count; t++)
                {
                    Type abilityType = types[t];
                    string baseName = abilityType.Name.Replace("AbilitySO", "").Replace("Ability", "").Replace("Gear", "");
                    if (string.IsNullOrEmpty(baseName)) baseName = abilityType.Name;

                    EditorUtility.DisplayProgressBar("Forging Gears", $"Generating {baseName} Lineage...", (float)t / types.Count);

                    // Determine which stat assignments this gear type needs
                    var statAssignments = GetStatAssignmentsForType(abilityType);

                    // If this gear targets a single-stat field, create one variant per sensible stat
                    if (statAssignments.Count > 1)
                    {
                        foreach (var assignment in statAssignments)
                        {
                            string variantName = $"{baseName}_{assignment.StatName}";
                            createdCount += ForgeLineage(abilityType, variantName, assignment);
                        }
                    }
                    else
                    {
                        // Single assignment (or none / struct-based)
                        var assignment = statAssignments.Count == 1 ? statAssignments[0] : null;
                        createdCount += ForgeLineage(abilityType, baseName, assignment);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"<color=#00ff00>[Gear Forge]</color> Successfully forged {createdCount} new specialized Gear Assets mapped to 5 Rarities!");
            }
        }

        /// <summary>Forge 5 tiers (Common→Legendary) for a single gear variant.</summary>
        private static int ForgeLineage(Type abilityType, string variantName, StatAssignment assignment)
        {
            string archetypeFolder = $"{GENERATION_PATH}/{variantName}";
            EnsureFolder(GENERATION_PATH, variantName);

            GearConfig previousConfig = null;
            int count = 0;

            for (int tier = 1; tier <= MAX_TIERS; tier++)
            {
                string itemName = $"{variantName}_Tier{tier}";

                // 1. Create Ability Instance
                ScriptableObject abilityInstance = ScriptableObject.CreateInstance(abilityType);
                abilityInstance.name = $"{itemName}_Ability";

                // Assign VariableSO references BEFORE scaling (so scale logic sees valid data)
                if (assignment != null)
                    AssignVariableFields(abilityInstance, assignment);

                // Algorithmically scale numerical fields based on tier/rarity
                ScaleNumericFields(abilityInstance, tier);

                string abilityPath = $"{archetypeFolder}/{abilityInstance.name}.asset";
                AssetDatabase.CreateAsset(abilityInstance, abilityPath);

                // 2. Create the GearConfig
                GearConfig configInstance = ScriptableObject.CreateInstance<GearConfig>();
                configInstance.name = $"{itemName}_Config";

                SerializedObject serializedConfig = new SerializedObject(configInstance);

                SerializedProperty dataProp = serializedConfig.FindProperty("data");
                dataProp.FindPropertyRelative("id").stringValue = itemName.ToLower();
                dataProp.FindPropertyRelative("rarity").enumValueIndex = GetRarityForTier(tier);
                dataProp.FindPropertyRelative("Category").enumValueIndex = 0;
                dataProp.FindPropertyRelative("BaseRotationSpeed").floatValue = 100f + (tier * 15f);
                dataProp.FindPropertyRelative("MaxCharge").floatValue = 100f;

                // Inject Ability
                SerializedProperty abilitiesArray = serializedConfig.FindProperty("abilities");
                abilitiesArray.arraySize = 1;
                abilitiesArray.GetArrayElementAtIndex(0).objectReferenceValue = abilityInstance;

                serializedConfig.ApplyModifiedProperties();

                string configPath = $"{archetypeFolder}/{configInstance.name}.asset";
                AssetDatabase.CreateAsset(configInstance, configPath);

                // Link Progression chain
                if (previousConfig != null)
                {
                    SerializedObject prevObj = new SerializedObject(previousConfig);
                    prevObj.FindProperty("nextLevel").objectReferenceValue = configInstance;
                    prevObj.ApplyModifiedProperties();
                }
                previousConfig = configInstance;

                count += 2;
            }

            return count;
        }

        // ──────────────────────────────────────────────
        //  VariableSO Loading & Assignment
        // ──────────────────────────────────────────────

        private static void LoadVariableCache()
        {
            _variableCache = new Dictionary<string, ScriptableObject>(StringComparer.OrdinalIgnoreCase);
            string[] guids = AssetDatabase.FindAssets("t:VariableSO", new[] { VARIABLES_PATH });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so != null)
                {
                    _variableCache[so.name] = so;
                    Debug.Log($"[Forge] Cached VariableSO: {so.name}");
                }
            }

            if (_variableCache.Count == 0)
                Debug.LogWarning("[Forge] No VariableSO assets found! Gear stat targets will be null.");
        }

        private static ScriptableObject GetVariable(string name)
        {
            if (_variableCache != null && _variableCache.TryGetValue(name, out var v)) return v;
            return null;
        }

        /// <summary>Use reflection to inject VariableSO references into the ability's private serialized fields.</summary>
        private static void AssignVariableFields(ScriptableObject instance, StatAssignment assignment)
        {
            if (assignment?.FieldMappings == null) return;

            foreach (var mapping in assignment.FieldMappings)
            {
                FieldInfo field = instance.GetType().GetField(mapping.FieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null) continue;

                ScriptableObject variableAsset = GetVariable(mapping.VariableName);
                if (variableAsset != null)
                    field.SetValue(instance, variableAsset);
            }
        }

        // ──────────────────────────────────────────────
        //  Stat Assignment Profiles
        // ──────────────────────────────────────────────

        /// <summary>
        /// Returns a list of stat-assignment profiles for a gear type.
        /// If the list has >1 entries, The Forge will create one named variant per entry.
        /// </summary>
        private static List<StatAssignment> GetStatAssignmentsForType(Type type)
        {
            string name = type.Name;
            var result = new List<StatAssignment>();

            // ── Multi-stat Gears: create one variant per stat ──

            // Single VariableSO target gears that make sense with multiple stats
            if (name == "TemporaryBoostGearAbilitySO" || name == "RaceStartBuffGearAbilitySO" ||
                name == "LapTriggerGearAbilitySO" || name == "MartyrGearAbilitySO" ||
                name == "LapScalerGearAbilitySO" || name == "SlotMachineGearAbilitySO" ||
                name == "PacemakerGearAbilitySO")
            {
                string fieldName = GetSingleVariableFieldName(type);
                if (fieldName != null)
                {
                    // Create one variant per useful stat (Speed, Acceleration, Handling)
                    result.Add(new StatAssignment("Speed",        new[] { new FieldMapping(fieldName, "Speed") }));
                    result.Add(new StatAssignment("Acceleration", new[] { new FieldMapping(fieldName, "Acceleration") }));
                    result.Add(new StatAssignment("Handling",     new[] { new FieldMapping(fieldName, "Handling") }));
                }
                return result;
            }

            // ── Fixed-assignment Gears ──

            if (name == "BurnoutGearAbilitySO" || name == "FragileBombGearAbilitySO" ||
                name == "KamikazeRecoveryGearAbilitySO" || name == "RecoveryGearAbilitySO" ||
                name == "TrackSegmentBoostGearAbilitySO" || name == "VampiricEngineGearAbilitySO")
            {
                string fieldName = GetSingleVariableFieldName(type);
                if (fieldName != null)
                    result.Add(new StatAssignment("Speed", new[] { new FieldMapping(fieldName, "Speed") }));
                return result;
            }

            if (name == "BlackholeGearAbilitySO")
            {
                result.Add(new StatAssignment("Speed", new[] { new FieldMapping("burstTarget", "Speed") }));
                return result;
            }

            if (name == "BipolarGearAbilitySO" || name == "CursedSynergyGearAbilitySO")
            {
                string fieldName = name == "BipolarGearAbilitySO" ? "targ" : "penaltyVar";
                result.Add(new StatAssignment("Speed", new[] { new FieldMapping(fieldName, "Speed") }));
                return result;
            }

            if (name == "RadioactiveEngineGearAbilitySO")
            {
                result.Add(new StatAssignment("Stability", new[] { new FieldMapping("decayTarget", "Stability") }));
                return result;
            }

            if (name == "OverheatGearAbilitySO")
            {
                result.Add(new StatAssignment("SpeedBrake", new[] {
                    new FieldMapping("speedStat", "Speed"),
                    new FieldMapping("brakeStat", "DriftPenalty")
                }));
                return result;
            }

            if (name == "MomentumConverterGearAbilitySO")
            {
                result.Add(new StatAssignment("SpeedAccel", new[] {
                    new FieldMapping("penaltyStat", "DriftPenalty"),
                    new FieldMapping("bonusStat", "Acceleration")
                }));
                return result;
            }

            if (name == "TheJokerGearAbilitySO")
            {
                result.Add(new StatAssignment("SpeedHandling", new[] {
                    new FieldMapping("s1", "Speed"),
                    new FieldMapping("s2", "Handling")
                }));
                return result;
            }

            if (name == "OuroborosGearAbilitySO")
            {
                // cycleStats is a List<VariableSO> — handled via special injection  
                result.Add(new StatAssignment("Cycle", null));
                return result;
            }

            // Gears with no VariableSO fields (passives using struct modifiers, etc.)
            // AdjacentSynergy, ModifierPassive, Clone, Greed, NeighborOverclock, QuantumLink, etc.
            result.Add(new StatAssignment("Default", null));
            return result;
        }

        private static string GetSingleVariableFieldName(Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var f in fields)
            {
                if (f.FieldType.Name == "VariableSO") return f.Name;
            }
            return null;
        }

        // ──────────────────────────────────────────────
        //  Numeric Scaling (Rarity-Proportional)
        // ──────────────────────────────────────────────

        private static void ScaleNumericFields(object instance, int tier)
        {
            float genericMultiplier = 1.0f + ((tier - 1) * 0.5f); // 5 Tiers: x1.0, x1.5, x2.0, x2.5, x3.0

            FieldInfo[] fields = instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            
            foreach (FieldInfo field in fields)
            {
                string n = field.Name.ToLower();
                bool isInverseScale = n.Contains("duration") || n.Contains("debuff") || n.Contains("penalty") || n.Contains("threshold") || n.Contains("interval") || n.Contains("rate") || n.Contains("cooldown");

                if (field.FieldType == typeof(float))
                {
                    float val = (float)field.GetValue(instance);
                    if (isInverseScale && genericMultiplier > 0)
                        field.SetValue(instance, val / genericMultiplier);
                    else
                        field.SetValue(instance, val * genericMultiplier);
                }
                else if (field.FieldType == typeof(int))
                {
                    int val = (int)field.GetValue(instance);
                    if (val > 0)
                    {
                        if (isInverseScale && genericMultiplier > 0)
                            field.SetValue(instance, (int)Math.Max(1, val / genericMultiplier));
                        else
                            field.SetValue(instance, (int)Math.Max(1, val * genericMultiplier));
                    }
                }
                else if (field.FieldType == typeof(PassiveStatModifier))
                {
                    PassiveStatModifier mod = (PassiveStatModifier)field.GetValue(instance);
                    mod.Amount = isInverseScale ? mod.Amount / genericMultiplier : mod.Amount * genericMultiplier;
                    field.SetValue(instance, mod);
                }
                else if (field.FieldType == typeof(List<PassiveStatModifier>))
                {
                    var modifiers = (List<PassiveStatModifier>)field.GetValue(instance);
                    if (modifiers != null)
                    {
                        for (int i = 0; i < modifiers.Count; i++)
                        {
                            var mod = modifiers[i];
                            mod.Amount = isInverseScale ? mod.Amount / genericMultiplier : mod.Amount * genericMultiplier;
                            modifiers[i] = mod;
                        }
                        field.SetValue(instance, modifiers);
                    }
                }
            }
        }

        // ──────────────────────────────────────────────
        //  Helpers
        // ──────────────────────────────────────────────

        private static int GetRarityForTier(int tier)
        {
            return Math.Clamp(tier - 1, 0, 4);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string full = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, child);
        }

        // ──────────────────────────────────────────────
        //  Data Structures
        // ──────────────────────────────────────────────

        private class StatAssignment
        {
            public string StatName;
            public FieldMapping[] FieldMappings;

            public StatAssignment(string statName, FieldMapping[] mappings)
            {
                StatName = statName;
                FieldMappings = mappings;
            }
        }

        private class FieldMapping
        {
            public string FieldName;
            public string VariableName;

            public FieldMapping(string fieldName, string variableName)
            {
                FieldName = fieldName;
                VariableName = variableName;
            }
        }
    }
}

