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
            string prefabPath = "Assets/Game/GearEngine/Prefabs";
            
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            if (!Directory.Exists(prefabPath)) Directory.CreateDirectory(prefabPath);
            AssetDatabase.Refresh();

            Sprite baseSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/BaseGear.png");
            Sprite coreSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/CoreGear.png");
            Sprite rockSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/RockObstacle.png");
            Sprite scoreSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/ScoreGear.png");
            Sprite speedSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/SpeedGear.png");

            // 1. Create Default Tags
            TagSO gridBoardTag = ScriptableObject.CreateInstance<TagSO>();
            gridBoardTag.Description = "Valid 2D surface area for gears to be dropped onto.";
            AssetDatabase.CreateAsset(gridBoardTag, $"{folderPath}/GridBoard_Tag.asset");

            TagSO inventoryTag = ScriptableObject.CreateInstance<TagSO>();
            inventoryTag.Description = "Marks an item inside the inventory that can be picked up.";
            AssetDatabase.CreateAsset(inventoryTag, $"{folderPath}/Inventory_Tag.asset");

            // 1.2 Create Board Config
            BoardConfigSO boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
            AssetDatabase.CreateAsset(boardConfig, $"{folderPath}/BasicBoardConfig.asset");

            // 1.5 Create Empty Slot Background View
            GameObject emptySlot = CreatePrefabPrimitive("EmptySlotView", "", new Color(0.2f, 0.2f, 0.2f, 0.3f), prefabPath);
            // We just generate the physical block prefab, no config file needed since it's just background visual!

            // 2. Create Core Gear
            GameObject coreView = CreatePrefabPrimitive("CoreGearView", "CoreGear", new Color(1f, 0.8f, 0.1f), prefabPath);
            GearConfig coreGear = ScriptableObject.CreateInstance<GearConfig>();
            SetPrivateData(coreGear, new GearConfigData
            {
                Id = "core_gear_1",
                Category = GearCategory.Core,
                BaseRotationSpeed = 30f,
                VisualPrefab = coreView,
                UIIcon = coreSpr,
                TriggerPattern = TriggerPattern.FourWay,
                IsInteractable = true,
                MaxCharge = 0f, // Core doesn't hold charge
                ChargeOverTimeAmount = 0f,
                ChargeOnTriggerAmount = 0f
            }, null, null);
            AssetDatabase.CreateAsset(coreGear, $"{folderPath}/CoreGearConfig.asset");

            // 3. Create Base Gear Level 2
            GameObject base2View = CreatePrefabPrimitive("BaseGear2View", "BaseGear", new Color(0.8f, 0.8f, 0.9f), prefabPath);
            GearConfig baseGearLv2 = ScriptableObject.CreateInstance<GearConfig>();
            SetPrivateData(baseGearLv2, new GearConfigData
            {
                Id = "base_gear_2",
                BaseRotationSpeed = 0f, 
                VisualPrefab = base2View,
                UIIcon = baseSpr,
                IsInteractable = true,
                MaxCharge = 200f,
                ChargeOverTimeAmount = 0f,
                ChargeOnTriggerAmount = 50f
            }, null, null);
            AssetDatabase.CreateAsset(baseGearLv2, $"{folderPath}/BaseGearConfig_Level2.asset");

            // 4. Create Base Gear Level 1 (Links to Level 2)
            GameObject base1View = CreatePrefabPrimitive("BaseGear1View", "BaseGear", new Color(0.6f, 0.6f, 0.65f), prefabPath);
            GearConfig baseGearLv1 = ScriptableObject.CreateInstance<GearConfig>();
            SetPrivateData(baseGearLv1, new GearConfigData
            {
                Id = "base_gear_1",
                BaseRotationSpeed = 0f,
                VisualPrefab = base1View,
                UIIcon = baseSpr,
                IsInteractable = true,
                MaxCharge = 100f,
                ChargeOverTimeAmount = 0f, 
                ChargeOnTriggerAmount = 25f 
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

            // 6. Create Obstacle Rock
            GameObject rockView = CreatePrefabPrimitive("RockObstacleView", "RockObstacle", new Color(0.4f, 0.4f, 0.35f), prefabPath);
            GearConfig rockObstacle = ScriptableObject.CreateInstance<GearConfig>();
            SetPrivateData(rockObstacle, new GearConfigData
            {
                Id = "obstacle_rock",
                BaseRotationSpeed = 0f,
                VisualPrefab = rockView,
                UIIcon = rockSpr,
                IsInteractable = false, 
                MaxCharge = 30f,
                ChargeOverTimeAmount = 0f,
                ChargeOnTriggerAmount = 10f
            }, null, new List<GearAbilitySO> { destroyAbility });
            AssetDatabase.CreateAsset(rockObstacle, $"{folderPath}/ObstacleRockConfig.asset");

            // 7. Create Score Gear
            GameObject scoreView = CreatePrefabPrimitive("ScoreGearView", "ScoreGear", new Color(1f, 0.6f, 0.0f), prefabPath);
            GearConfig scoreGear = ScriptableObject.CreateInstance<GearConfig>();
            SetPrivateData(scoreGear, new GearConfigData
            {
                Id = "score_gear",
                BaseRotationSpeed = 0f,
                VisualPrefab = scoreView,
                UIIcon = scoreSpr,
                IsInteractable = true,
                MaxCharge = 100f,
                ChargeOverTimeAmount = 0f,
                ChargeOnTriggerAmount = 50f
            }, null, new List<GearAbilitySO> { scoreAbility });
            AssetDatabase.CreateAsset(scoreGear, $"{folderPath}/ScoreGearConfig.asset");

            // 8. Create Speed Buff Gear
            GameObject speedView = CreatePrefabPrimitive("SpeedGearView", "SpeedGear", new Color(0.1f, 0.9f, 0.2f), prefabPath);
            GearConfig speedGear = ScriptableObject.CreateInstance<GearConfig>();
            SetPrivateData(speedGear, new GearConfigData
            {
                Id = "speed_buff_gear",
                BaseRotationSpeed = 0f,
                VisualPrefab = speedView,
                UIIcon = speedSpr,
                IsInteractable = true,
                MaxCharge = 0f, 
                ChargeOverTimeAmount = 0f,
                ChargeOnTriggerAmount = 0f
            }, null, new List<GearAbilitySO> { speedAbility });
            AssetDatabase.CreateAsset(speedGear, $"{folderPath}/SpeedBuffGearConfig.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#33ff33>[GearEngine]</color> Basic Setup Configs successfully generated at {folderPath}!");
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

            if (customSprite != null)
            {
                sr.sprite = customSprite;
                sr.color = Color.white;
            }
            else
            {
                // Fallback to Unity editor knob/square primitive sprite mapping
                sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                sr.color = tint;
            }

            if (name == "CoreGearView")
            {
                GameObject tipGo = new GameObject("TriggerTip");
                tipGo.transform.SetParent(tempGo.transform);
                tipGo.transform.localPosition = new Vector3(0, 0.45f, -0.1f);
                var tipSr = tipGo.AddComponent<SpriteRenderer>();
                tipSr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                tipSr.color = Color.green;
                tipGo.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
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
