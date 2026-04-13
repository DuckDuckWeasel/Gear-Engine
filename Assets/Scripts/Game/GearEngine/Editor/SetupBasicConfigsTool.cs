using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation;

namespace GearEngine.GearEngine.Editor
{
    public static class SetupBasicConfigsTool
    {
        [MenuItem("GearEngine/Step 1: Generate Basic Setup Configs")]
        public static void GenerateConfigs()
        {
            string folderPath = "Assets/Game/GearEngine/Configs";
            string prefabPath = "Assets/Game/GearEngine/Prefabs";
            
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            if (!Directory.Exists(folderPath + "/Tag")) Directory.CreateDirectory(folderPath + "/Tag");
            if (!Directory.Exists(folderPath + "/Gear")) Directory.CreateDirectory(folderPath + "/Gear");
            if (!Directory.Exists(folderPath + "/Ability")) Directory.CreateDirectory(folderPath + "/Ability");
            
            if (!Directory.Exists(prefabPath)) Directory.CreateDirectory(prefabPath);
            AssetDatabase.Refresh();

            Sprite baseSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/BaseGear.png");
            Sprite coreSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/CoreGear.png");
            Sprite fallbackSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/FallbackGear.png");

            Sprite rockSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/RockObstacle.png") ?? fallbackSpr;
            Sprite scoreSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/ScoreGear.png") ?? fallbackSpr;
            Sprite speedSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/SpeedGear.png") ?? fallbackSpr;

            // 1. Create Default Tags
            TagSO gridBoardTag = ScriptableObject.CreateInstance<TagSO>();
            gridBoardTag.Description = "Valid 2D surface area for gears to be dropped onto.";
            AssetDatabase.CreateAsset(gridBoardTag, $"{folderPath}/Tag/GridBoard_Tag.asset");

            TagSO inventoryTag = ScriptableObject.CreateInstance<TagSO>();
            inventoryTag.Description = "Marks an item inside the inventory that can be picked up.";
            AssetDatabase.CreateAsset(inventoryTag, $"{folderPath}/Tag/Inventory_Tag.asset");

            // 1.2 Create Board Config
            BoardConfigSO boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
            boardConfig.GridWidth = 7;
            boardConfig.GridHeight = 5;
            AssetDatabase.CreateAsset(boardConfig, $"{folderPath}/BasicBoardConfig.asset");

            // 1.5 Create Empty Slot Background View
            GameObject emptySlot = CreatePrefabPrimitive("EmptySlotView", "", new Color(0.2f, 0.2f, 0.2f, 0.3f), prefabPath);
            // We just generate the physical block prefab, no config file needed since it's just background visual!

            // 2. Create Core Gear
            GameObject coreView = CreatePrefabPrimitive("CoreGearView", "CoreGear", new Color(1f, 0.8f, 0.1f), prefabPath);
            bool isCoreNew;
            GearConfig coreGear = GetOrCreateGearConfig($"{folderPath}/Gear/CoreGearConfig.asset", out isCoreNew);
            if (isCoreNew)
            {
                SetPrivateData(coreGear, new GearConfigData
                {
                    Id = "core_gear_1",
                    Category = GearCategory.Core,
                    BaseRotationSpeed = 70f,
                    TriggerPattern = TriggerPattern.FourWay,
                    IsInteractable = true,
                    MaxCharge = 0f, 
                    ChargeOverTimeAmount = 0f,
                    ChargeOnTriggerAmount = 0f
                }, null, null);
                AssetDatabase.CreateAsset(coreGear, $"{folderPath}/Gear/CoreGearConfig.asset");
            }
            UpdateVisualsAndMeta(coreGear, coreView, coreSpr, null, null);

            // 3. Create Base Gear Level 2
            GameObject base2View = CreatePrefabPrimitive("BaseGear2View", "BaseGear", new Color(0.8f, 0.8f, 0.9f), prefabPath);
            bool isBase2New;
            GearConfig baseGearLv2 = GetOrCreateGearConfig($"{folderPath}/Gear/BaseGearConfig_Level2.asset", out isBase2New);
            if (isBase2New)
            {
                SetPrivateData(baseGearLv2, new GearConfigData
                {
                    Id = "base_gear_2",
                    BaseRotationSpeed = 0f, 
                    IsInteractable = true,
                    MaxCharge = 200f,
                    ChargeOverTimeAmount = 0f,
                    ChargeOnTriggerAmount = 50f
                }, null, null);
                AssetDatabase.CreateAsset(baseGearLv2, $"{folderPath}/Gear/BaseGearConfig_Level2.asset");
            }
            UpdateVisualsAndMeta(baseGearLv2, base2View, baseSpr, null, null);

            // 4. Create Base Gear Level 1 (Links to Level 2)
            GameObject base1View = CreatePrefabPrimitive("BaseGear1View", "BaseGear", new Color(0.6f, 0.6f, 0.65f), prefabPath);
            bool isBase1New;
            GearConfig baseGearLv1 = GetOrCreateGearConfig($"{folderPath}/Gear/BaseGearConfig_Level1.asset", out isBase1New);
            if (isBase1New)
            {
                SetPrivateData(baseGearLv1, new GearConfigData
                {
                    Id = "base_gear_1",
                    BaseRotationSpeed = 0f,
                    IsInteractable = true,
                    MaxCharge = 100f,
                    ChargeOverTimeAmount = 0f, 
                    ChargeOnTriggerAmount = 25f 
                }, baseGearLv2, null);
                AssetDatabase.CreateAsset(baseGearLv1, $"{folderPath}/Gear/BaseGearConfig_Level1.asset");
            }
            UpdateVisualsAndMeta(baseGearLv1, base1View, baseSpr, baseGearLv2, null);

            // 4. Create Abilities
            DestroySelfAbility destroyAbility = ScriptableObject.CreateInstance<DestroySelfAbility>();
            AssetDatabase.CreateAsset(destroyAbility, $"{folderPath}/Ability/DestroySelf_Ability.asset");
            
            ScoreAbility scoreAbility = ScriptableObject.CreateInstance<ScoreAbility>();
            scoreAbility.ScoreAmount = 500;
            AssetDatabase.CreateAsset(scoreAbility, $"{folderPath}/Ability/Score_Ability.asset");

            SpeedBoostAbility speedAbility = ScriptableObject.CreateInstance<SpeedBoostAbility>();
            speedAbility.SpeedMultiplier = 2.0f;
            AssetDatabase.CreateAsset(speedAbility, $"{folderPath}/Ability/SpeedBoost_Ability.asset");

            // 6. Create Obstacle Rock
            GameObject rockView = CreatePrefabPrimitive("RockObstacleView", "RockObstacle", new Color(0.4f, 0.4f, 0.35f), prefabPath);
            bool isRockNew;
            GearConfig rockObstacle = GetOrCreateGearConfig($"{folderPath}/Gear/ObstacleRockConfig.asset", out isRockNew);
            if (isRockNew)
            {
                SetPrivateData(rockObstacle, new GearConfigData
                {
                    Id = "obstacle_rock",
                    BaseRotationSpeed = 0f,
                    IsInteractable = false, 
                    MaxCharge = 30f,
                    ChargeOverTimeAmount = 0f,
                    ChargeOnTriggerAmount = 10f
                }, null, new List<GearAbilitySO> { destroyAbility });
                AssetDatabase.CreateAsset(rockObstacle, $"{folderPath}/Gear/ObstacleRockConfig.asset");
            }
            UpdateVisualsAndMeta(rockObstacle, rockView, rockSpr, null, new List<GearAbilitySO> { destroyAbility });

            // 7. Create Score Gear
            GameObject scoreView = CreatePrefabPrimitive("ScoreGearView", "ScoreGear", new Color(1f, 0.6f, 0.0f), prefabPath);
            bool isScoreNew;
            GearConfig scoreGear = GetOrCreateGearConfig($"{folderPath}/Gear/ScoreGearConfig.asset", out isScoreNew);
            if (isScoreNew)
            {
                SetPrivateData(scoreGear, new GearConfigData
                {
                    Id = "score_gear",
                    BaseRotationSpeed = 0f,
                    IsInteractable = true,
                    MaxCharge = 100f,
                    ChargeOverTimeAmount = 0f,
                    ChargeOnTriggerAmount = 50f
                }, null, new List<GearAbilitySO> { scoreAbility });
                AssetDatabase.CreateAsset(scoreGear, $"{folderPath}/Gear/ScoreGearConfig.asset");
            }
            UpdateVisualsAndMeta(scoreGear, scoreView, scoreSpr, null, new List<GearAbilitySO> { scoreAbility });

            // 8. Create Speed Buff Gear
            GameObject speedView = CreatePrefabPrimitive("SpeedGearView", "SpeedGear", new Color(0.1f, 0.9f, 0.2f), prefabPath);
            bool isSpeedNew;
            GearConfig speedGear = GetOrCreateGearConfig($"{folderPath}/Gear/SpeedBuffGearConfig.asset", out isSpeedNew);
            if (isSpeedNew)
            {
                SetPrivateData(speedGear, new GearConfigData
                {
                    Id = "speed_buff_gear",
                    BaseRotationSpeed = 0f,
                    IsInteractable = true,
                    MaxCharge = 0f, 
                    ChargeOverTimeAmount = 0f,
                    ChargeOnTriggerAmount = 0f
                }, null, new List<GearAbilitySO> { speedAbility });
                AssetDatabase.CreateAsset(speedGear, $"{folderPath}/Gear/SpeedBuffGearConfig.asset");
            }
            UpdateVisualsAndMeta(speedGear, speedView, speedSpr, null, new List<GearAbilitySO> { speedAbility });

            GearConfig loadoutCore = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/CoreGearConfig.asset");
            GearConfig loadoutBase1 = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/BaseGearConfig_Level1.asset");
            GearConfig loadoutBase2 = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/BaseGearConfig_Level2.asset");
            GearConfig loadoutSpeed = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/SpeedBuffGearConfig.asset");
            GearConfig loadoutRock = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/ObstacleRockConfig.asset");
            GearConfig loadoutScore = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/ScoreGearConfig.asset");

            CreateOrUpdateDefaultInventoryLoadout(
                folderPath,
                loadoutCore,
                loadoutBase1,
                loadoutBase2,
                loadoutSpeed,
                loadoutRock,
                loadoutScore);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#33ff33>[GearEngine]</color> Basic Setup Configs successfully generated and merged at {folderPath}!");
        }

        private static void CreateOrUpdateDefaultInventoryLoadout(
            string folderPath,
            GearConfig core,
            GearConfig baseGear,
            GearConfig baseLevel2,
            GearConfig speedGear,
            GearConfig rockObs,
            GearConfig scoreGear)
        {
            string path = $"{folderPath}/GearInventoryLoadout.asset";
            var loadout = AssetDatabase.LoadAssetAtPath<GearInventoryLoadoutSO>(path);
            if (loadout == null)
            {
                loadout = ScriptableObject.CreateInstance<GearInventoryLoadoutSO>();
                AssetDatabase.CreateAsset(loadout, path);
            }

            var gears = new List<GearConfig>();
            if (core != null)
            {
                gears.Add(core);
            }

            if (baseGear != null)
            {
                gears.Add(baseGear);
            }

            if (baseLevel2 != null)
            {
                gears.Add(baseLevel2);
            }

            if (speedGear != null)
            {
                gears.Add(speedGear);
            }

            if (rockObs != null)
            {
                gears.Add(rockObs);
            }

            if (scoreGear != null)
            {
                gears.Add(scoreGear);
            }

            var so = new SerializedObject(loadout);
            var prop = so.FindProperty("startingGears");
            if (prop != null)
            {
                prop.ClearArray();
                for (int i = 0; i < gears.Count; i++)
                {
                    prop.InsertArrayElementAtIndex(i);
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = gears[i];
                }
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(loadout);
        }

        private static GearConfig GetOrCreateGearConfig(string path, out bool isNew)
        {
            GearConfig cfg = AssetDatabase.LoadAssetAtPath<GearConfig>(path);
            if (cfg != null)
            {
                isNew = false;
                return cfg;
            }
            
            isNew = true;
            cfg = ScriptableObject.CreateInstance<GearConfig>();
            return cfg;
        }

        private static void UpdateVisualsAndMeta(GearConfig config, GameObject visual, Sprite icon, GearConfig nextLvl, List<GearAbilitySO> abilities)
        {
            SerializedObject so = new SerializedObject(config);
            SerializedProperty dataProp = so.FindProperty("data");
            if (dataProp != null)
            {
                var visualProp = dataProp.FindPropertyRelative("VisualPrefab");
                if (visualProp != null) visualProp.objectReferenceValue = visual;

                var iconProp = dataProp.FindPropertyRelative("UIIcon");
                if (iconProp != null) iconProp.objectReferenceValue = icon;
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
            else if (abilities != null && abilities.Count == 0)
            {
                so.FindProperty("abilities").ClearArray();
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
        }

        private static GameObject CreatePrefabPrimitive(string name, string spriteName, Color tint, string destinationDir)
        {
            string fullPath = $"{destinationDir}/{name}.prefab";

            GameObject tempGo = new GameObject(name);
            var sr = tempGo.AddComponent<SpriteRenderer>();
            
            // Try fetch HD custom sprite if available
            Sprite customSprite = null;
            if (!string.IsNullOrEmpty(spriteName))
            {
                customSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Game/GearEngine/Sprites/{spriteName}.png");
            }

            if (customSprite == null && !string.IsNullOrEmpty(spriteName))
            {
                // Extra safety: Try fallback if preferred sprite didn't exist
                customSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/FallbackGear.png");
            }

            if (customSprite != null)
            {
                sr.sprite = customSprite;
                sr.color = tint != Color.white ? tint : Color.white;
            }
            else
            {
                // Fallback to Unity editor knob/square primitive sprite mapping for empty spots
                sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                sr.color = tint;
            }
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(tempGo, fullPath);
            Object.DestroyImmediate(tempGo);

            return savedPrefab;
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
                
                var visualProp = dataProp.FindPropertyRelative("VisualPrefab");
                if (visualProp != null) visualProp.objectReferenceValue = data.VisualPrefab;

                var iconProp = dataProp.FindPropertyRelative("UIIcon");
                if (iconProp != null) iconProp.objectReferenceValue = data.UIIcon;

                var categoryProp = dataProp.FindPropertyRelative("Category");
                if (categoryProp != null) categoryProp.enumValueIndex = (int)data.Category;

                // Map the Enum cleanly to EnumIndex for SerializedProperties
                int triggerIndex = data.TriggerPattern == TriggerPattern.FourWay ? 0 : 1;
                dataProp.FindPropertyRelative("TriggerPattern").enumValueIndex = triggerIndex;

                dataProp.FindPropertyRelative("IsInteractable").boolValue = data.IsInteractable;
                dataProp.FindPropertyRelative("MaxCharge").floatValue = data.MaxCharge;
                dataProp.FindPropertyRelative("ChargeOverTimeAmount").floatValue = data.ChargeOverTimeAmount;
                dataProp.FindPropertyRelative("ChargeOnTriggerAmount").floatValue = data.ChargeOnTriggerAmount;

                var slowdownDurProp = dataProp.FindPropertyRelative("SnapSlowdownDuration");
                if (slowdownDurProp != null) slowdownDurProp.floatValue = data.SnapSlowdownDuration;

                var slowdownMultProp = dataProp.FindPropertyRelative("SnapSlowdownMultiplier");
                if (slowdownMultProp != null) slowdownMultProp.floatValue = data.SnapSlowdownMultiplier;

                var triggerSpinProp = dataProp.FindPropertyRelative("TriggerSpinDegrees");
                if (triggerSpinProp != null) triggerSpinProp.floatValue = data.TriggerSpinDegrees;
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
