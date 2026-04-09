using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Game.GearEngine.Presentation;

namespace Game.GearEngine.Editor
{
    public static class SetupBasicConfigsTool
    {
        [MenuItem("GearEngine/Step 1: Generate Basic Setup Configs")]
        public static void GenerateConfigs()
        {
            string folderPath = "Assets/Game/GearEngine/Configs";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            // 1. Create Default Tags
            TagSO gridBoardTag = ScriptableObject.CreateInstance<TagSO>();
            gridBoardTag.Description = "Valid 2D surface area for gears to be dropped onto.";
            AssetDatabase.CreateAsset(gridBoardTag, $"{folderPath}/GridBoard_Tag.asset");

            TagSO inventoryTag = ScriptableObject.CreateInstance<TagSO>();
            inventoryTag.Description = "Marks an item inside the inventory that can be picked up.";
            AssetDatabase.CreateAsset(inventoryTag, $"{folderPath}/Inventory_Tag.asset");

            // 2. Create Core Gear
            GearConfig coreGear = ScriptableObject.CreateInstance<GearConfig>();
            SetPrivateData(coreGear, new GearConfigData
            {
                Id = "core_gear_1",
                BaseRotationSpeed = 30f,
                TriggerPattern = TriggerPattern.FourWay,
                IsInteractable = true,
                MaxCharge = 0f, // Core doesn't hold charge
                ChargeOverTimeAmount = 0f,
                ChargeOnTriggerAmount = 0f
            }, null, null);
            AssetDatabase.CreateAsset(coreGear, $"{folderPath}/CoreGearConfig.asset");

            // 2. Create Base Gear Level 2
            GearConfig baseGearLv2 = ScriptableObject.CreateInstance<GearConfig>();
            SetPrivateData(baseGearLv2, new GearConfigData
            {
                Id = "base_gear_2",
                BaseRotationSpeed = 0f, // Spin depends purely on triggers? Or maybe 0 since it's passive
                IsInteractable = true,
                MaxCharge = 200f,
                ChargeOverTimeAmount = 0f,
                ChargeOnTriggerAmount = 50f
            }, null, null);
            AssetDatabase.CreateAsset(baseGearLv2, $"{folderPath}/BaseGearConfig_Level2.asset");

            // 3. Create Base Gear Level 1 (Links to Level 2)
            GearConfig baseGearLv1 = ScriptableObject.CreateInstance<GearConfig>();
            SetPrivateData(baseGearLv1, new GearConfigData
            {
                Id = "base_gear_1",
                BaseRotationSpeed = 0f,
                IsInteractable = true,
                MaxCharge = 100f,
                ChargeOverTimeAmount = 0f, // Doesn't gain passive time charge
                ChargeOnTriggerAmount = 25f // Takes 4 triggers
            }, baseGearLv2, null);
            AssetDatabase.CreateAsset(baseGearLv1, $"{folderPath}/BaseGearConfig_Level1.asset");

            // 4. Create Abilities
            DestroySelfAbility destroyAbility = ScriptableObject.CreateInstance<DestroySelfAbility>();
            AssetDatabase.CreateAsset(destroyAbility, $"{folderPath}/DestroySelf_Ability.asset");
            
            ScoreAbility scoreAbility = ScriptableObject.CreateInstance<ScoreAbility>();
            scoreAbility.ScoreAmount = 500;
            AssetDatabase.CreateAsset(scoreAbility, $"{folderPath}/Score_Ability.asset");

            SpeedBoostAbility speedAbility = ScriptableObject.CreateInstance<SpeedBoostAbility>();
            speedAbility.SpeedMultiplier = 2.0f;
            AssetDatabase.CreateAsset(speedAbility, $"{folderPath}/SpeedBoost_Ability.asset");

            // 5. Create Obstacle Rock
            GearConfig rockObstacle = ScriptableObject.CreateInstance<GearConfig>();
            SetPrivateData(rockObstacle, new GearConfigData
            {
                Id = "obstacle_rock",
                BaseRotationSpeed = 0f,
                IsInteractable = false, 
                MaxCharge = 30f,
                ChargeOverTimeAmount = 0f,
                ChargeOnTriggerAmount = 10f
            }, null, new List<GearAbilitySO> { destroyAbility });
            AssetDatabase.CreateAsset(rockObstacle, $"{folderPath}/ObstacleRockConfig.asset");

            // 6. Create Score Gear
            GearConfig scoreGear = ScriptableObject.CreateInstance<GearConfig>();
            SetPrivateData(scoreGear, new GearConfigData
            {
                Id = "score_gear",
                BaseRotationSpeed = 0f,
                IsInteractable = true,
                MaxCharge = 100f,
                ChargeOverTimeAmount = 0f,
                ChargeOnTriggerAmount = 50f
            }, null, new List<GearAbilitySO> { scoreAbility });
            AssetDatabase.CreateAsset(scoreGear, $"{folderPath}/ScoreGearConfig.asset");

            // 7. Create Speed Buff Gear
            GearConfig speedGear = ScriptableObject.CreateInstance<GearConfig>();
            SetPrivateData(speedGear, new GearConfigData
            {
                Id = "speed_buff_gear",
                BaseRotationSpeed = 0f,
                IsInteractable = true,
                MaxCharge = 0f, // Passive aura, doesn't need charge
                ChargeOverTimeAmount = 0f,
                ChargeOnTriggerAmount = 0f
            }, null, new List<GearAbilitySO> { speedAbility });
            AssetDatabase.CreateAsset(speedGear, $"{folderPath}/SpeedBuffGearConfig.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#33ff33>[GearEngine]</color> Basic Setup Configs successfully generated at {folderPath}!");
        }

        private static void SetPrivateData(GearConfig config, GearConfigData data, GearConfig nextLvl, List<GearAbilitySO> abilities)
        {
            SerializedObject so = new SerializedObject(config);
            
            // Set Data fields
            SerializedProperty dataProp = so.FindProperty("data");
            if (dataProp != null)
            {
                dataProp.FindPropertyRelative("Id").stringValue = data.Id;
                dataProp.FindPropertyRelative("BaseRotationSpeed").floatValue = data.BaseRotationSpeed;
                dataProp.FindPropertyRelative("TriggerPattern").enumValueIndex = (int)data.TriggerPattern;
                dataProp.FindPropertyRelative("IsInteractable").boolValue = data.IsInteractable;
                dataProp.FindPropertyRelative("MaxCharge").floatValue = data.MaxCharge;
                dataProp.FindPropertyRelative("ChargeOverTimeAmount").floatValue = data.ChargeOverTimeAmount;
                dataProp.FindPropertyRelative("ChargeOnTriggerAmount").floatValue = data.ChargeOnTriggerAmount;
            }

            // Set Next Level
            if (nextLvl != null)
            {
                so.FindProperty("nextLevel").objectReferenceValue = nextLvl;
            }

            // Set Abilities
            if (abilities != null && abilities.Count > 0)
            {
                SerializedProperty abProp = so.FindProperty("abilities");
                abProp.ClearArray();
                for (int i = 0; i < abilities.Count; i++)
                {
                    abProp.InsertArrayElementAtIndex(i);
                    abProp.GetArrayElementAtIndex(i).objectReferenceValue = abilities[i];
                }
            }

            so.ApplyModifiedProperties();
        }
    }
}
