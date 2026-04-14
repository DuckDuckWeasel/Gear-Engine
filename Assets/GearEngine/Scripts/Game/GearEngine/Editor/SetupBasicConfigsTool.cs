using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using GearEngine.GearEngine.Abilities;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI.Tags;

namespace GearEngine.GearEngine.Editor
{
    public static class SetupBasicConfigsTool
    {
        [MenuItem("GearEngine/Step 1: Generate Basic Setup Configs")]
        public static void GenerateConfigs()
        {
            string folderPath = "Assets/GearEngine/Data/Gear";
            string prefabPath = "Assets/GearEngine/Prefabs/Gears/Gears";
            
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
            if (!Directory.Exists(folderPath + "/Tag")) Directory.CreateDirectory(folderPath + "/Tag");
            if (!Directory.Exists(folderPath + "/Gear")) Directory.CreateDirectory(folderPath + "/Gear");
            if (!Directory.Exists(folderPath + "/Ability")) Directory.CreateDirectory(folderPath + "/Ability");
            
            if (!Directory.Exists(prefabPath)) Directory.CreateDirectory(prefabPath);
            AssetDatabase.Refresh();

            // Prefab sprites (board visuals)
            Sprite baseSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GearEngine/Art/Sprites/BaseGear.png");
            Sprite coreSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GearEngine/Art/Sprites/CoreGear.png");

            // UIIcon sprites (inventory icons) — each gear has its own distinct icon
            Sprite iconNumber1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GearEngine/Art/Sprites/Number1.png");
            Sprite iconNumber2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GearEngine/Art/Sprites/Number2.png");
            Sprite iconCoin = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GearEngine/Art/Sprites/Coin.png");
            Sprite iconArrowUp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GearEngine/Art/Sprites/ArrowUp.png");

            // 1. Create Default Tags
            TagSO gridBoardTag = ScriptableObject.CreateInstance<TagSO>();
            gridBoardTag.Description = "Valid 2D surface area for gears to be dropped onto.";
            AssetDatabase.CreateAsset(gridBoardTag, $"{folderPath}/Tag/GridBoard_Tag.asset");

            TagSO inventoryTag = ScriptableObject.CreateInstance<TagSO>();
            inventoryTag.Description = "Marks an item inside the inventory that can be picked up.";
            AssetDatabase.CreateAsset(inventoryTag, $"{folderPath}/Tag/Inventory_Tag.asset");

            TagSO trashZoneTag = ScriptableObject.CreateInstance<TagSO>();
            trashZoneTag.Description = "Marks the trash drop zone for gear deletion.";
            AssetDatabase.CreateAsset(trashZoneTag, $"{folderPath}/Tag/TrashZone_Tag.asset");

            // 1.2 Create or Update Board Config
            string boardCfgPath = $"{folderPath}/BasicBoardConfig.asset";
            BoardConfigSO boardConfig = AssetDatabase.LoadAssetAtPath<BoardConfigSO>(boardCfgPath);
            bool isNewBoardCfg = false;
            if (boardConfig == null)
            {
                boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
                isNewBoardCfg = true;
            }

            boardConfig.GridWidth = 7;
            boardConfig.GridHeight = 5;
            boardConfig.Spacing = 0.75f;
            boardConfig.MaxDragGrabDistance = 0.75f;
            boardConfig.StaggeredRotationOffset = 22.5f;
            boardConfig.TrashZoneYOffset = 160f;

            if (isNewBoardCfg)
            {
                AssetDatabase.CreateAsset(boardConfig, boardCfgPath);
            }
            else
            {
                EditorUtility.SetDirty(boardConfig);
            }

            // 1.3 Create Feature Toggle
            string togglePath = $"{folderPath}/GearEngineFeatureToggle.asset";
            GearEngineFeatureToggleSO featureToggle = AssetDatabase.LoadAssetAtPath<GearEngineFeatureToggleSO>(togglePath);

            // 1.4 Generate Trash Icon sprite as a project asset
            string trashIconTexPath = "Assets/GearEngine/Art/Sprites/TrashIcon.png";
            Sprite trashIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(trashIconTexPath);
            if (trashIconSprite == null)
            {
                Texture2D trashTex = CreateTrashIconTexture();
                byte[] pngBytes = trashTex.EncodeToPNG();
                string absolutePath = System.IO.Path.Combine(
                    System.IO.Directory.GetCurrentDirectory(), trashIconTexPath);
                System.IO.File.WriteAllBytes(absolutePath, pngBytes);
                Object.DestroyImmediate(trashTex);

                AssetDatabase.ImportAsset(trashIconTexPath, ImportAssetOptions.ForceUpdate);

                // Configure as sprite
                TextureImporter importer = AssetImporter.GetAtPath(trashIconTexPath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 64;
                    importer.filterMode = FilterMode.Point;
                    importer.SaveAndReimport();
                }

                trashIconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(trashIconTexPath);
            }

            if (featureToggle == null)
            {
                featureToggle = ScriptableObject.CreateInstance<GearEngineFeatureToggleSO>();
                featureToggle.EnableTrashDeletion = true;
                featureToggle.TrashAlignment = TrashZoneAlignment.Right;
                featureToggle.TrashZoneTag = trashZoneTag;
                featureToggle.TrashIcon = trashIconSprite;
                AssetDatabase.CreateAsset(featureToggle, togglePath);
            }
            else
            {
                bool dirty = false;
                if (!featureToggle.EnableTrashDeletion)
                {
                    featureToggle.EnableTrashDeletion = true;
                    dirty = true;
                }
                if (featureToggle.TrashZoneTag == null)
                {
                    featureToggle.TrashZoneTag = trashZoneTag;
                    dirty = true;
                }

                // Always update — a moved asset may leave a broken reference
                if (trashIconSprite != null && featureToggle.TrashIcon != trashIconSprite)
                {
                    featureToggle.TrashIcon = trashIconSprite;
                    dirty = true;
                }

                if (dirty)
                {
                    EditorUtility.SetDirty(featureToggle);
                }
            }

            // 1.5 Create Empty Slot Background View
            GameObject emptySlot = CreatePrefabPrimitive("EmptySlotView", "", new Color(0.2f, 0.2f, 0.2f, 0.3f), prefabPath);
            // We just generate the physical block prefab, no config file needed since it's just background visual!

            // 1.6 Create Gear Inventory Slot Prefab
            GameObject slotTemplate = new GameObject("GearSlot");
            var slotRect = slotTemplate.AddComponent<RectTransform>();
            slotRect.sizeDelta = new Vector2(120, 120);
            var slotImg = slotTemplate.AddComponent<UnityEngine.UI.Image>();
            slotImg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            PrefabUtility.SaveAsPrefabAsset(slotTemplate, $"{prefabPath}/GearSlot.prefab");
            Object.DestroyImmediate(slotTemplate);

            string[] allGearGuids = AssetDatabase.FindAssets("t:GearConfig");
            foreach (string guid in allGearGuids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                GearConfig gc = AssetDatabase.LoadAssetAtPath<GearConfig>(p);
                if (gc != null)
                {
                    SerializedObject serializedObj = new SerializedObject(gc);
                    SerializedProperty dataP = serializedObj.FindProperty("privateData");
                    if (dataP != null)
                    {
                        var categoryP = dataP.FindPropertyRelative("Category");
                        bool core = categoryP != null && categoryP.enumValueIndex == (int)GearCategory.Core;
                        
                        var mProp = dataP.FindPropertyRelative("IsMovable");
                        if (mProp != null && !mProp.boolValue && !core) mProp.boolValue = true;

                        var rProp = dataP.FindPropertyRelative("IsReturnable");
                        if (rProp != null && !rProp.boolValue && !core) rProp.boolValue = true;

                        var dProp = dataP.FindPropertyRelative("IsDeletable");
                        if (dProp != null && !dProp.boolValue && !core) dProp.boolValue = true;

                        var rwdProp = dataP.FindPropertyRelative("DeleteRewardAmount");
                        if (rwdProp != null && rwdProp.intValue <= 0 && !core) rwdProp.intValue = 25;
                        
                        var iProp = dataP.FindPropertyRelative("IsInteractable");
                        if (iProp != null && !iProp.boolValue && !core) iProp.boolValue = true;
                    }
                    serializedObj.ApplyModifiedProperties();
                    EditorUtility.SetDirty(gc);
                }
            }
            AssetDatabase.SaveAssets();

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
                    IsMovable = false,
                    IsReturnable = false,
                    RelativeScaleMultiplier = 1.0f,
                    MaxCharge = 0f, 
                    ChargeOverTimeAmount = 0f,
                    ChargeOnTriggerAmount = 0f,
                    SnapSlowdownDuration = 0.5f,
                    SnapSlowdownMultiplier = 0.15f,
                    TriggerSpinDegrees = 45f,
                    IsDeletable = false,
                    DeleteRewardAmount = 0
                }, null, null);
                AssetDatabase.CreateAsset(coreGear, $"{folderPath}/Gear/CoreGearConfig.asset");
            }
            // FORCE core gear constraints regardless of whether it already existed in the project
            SerializedObject coreSo = new SerializedObject(coreGear);
            SerializedProperty coreDataProp = coreSo.FindProperty("data");
            if (coreDataProp != null)
            {
                coreDataProp.FindPropertyRelative("IsMovable").boolValue = false;
                coreDataProp.FindPropertyRelative("IsReturnable").boolValue = false;
                coreDataProp.FindPropertyRelative("IsDeletable").boolValue = false;
                
                // Clear out UIIcon to fix legacy cyan bug completely
                var iconProp = coreDataProp.FindPropertyRelative("UIIcon");
                if (iconProp != null) iconProp.objectReferenceValue = null;
            }
            coreSo.ApplyModifiedProperties();

            UpdateVisualsAndMeta(coreGear, coreView, null, null, null, 0);
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
                    IsMovable = true,
                    IsReturnable = true,
                    RelativeScaleMultiplier = 1.0f,
                    MaxCharge = 200f,
                    ChargeOverTimeAmount = 0f,
                    ChargeOnTriggerAmount = 50f,
                    SnapSlowdownDuration = 0.5f,
                    SnapSlowdownMultiplier = 0.15f,
                    TriggerSpinDegrees = 45f,
                    IsDeletable = true,
                    DeleteRewardAmount = 75
                }, null, null);
                AssetDatabase.CreateAsset(baseGearLv2, $"{folderPath}/Gear/BaseGearConfig_Level2.asset");
            }
            UpdateVisualsAndMeta(baseGearLv2, base2View, iconNumber2, null, null, 75);

            // 4. Create Base Gear Level 1 (Links to Level 2)
            GameObject base1View = CreatePrefabPrimitive("BaseGear1View", "BaseGear", new Color(0.8f, 0.8f, 0.9f), prefabPath);
            bool isBase1New;
            GearConfig baseGearLv1 = GetOrCreateGearConfig($"{folderPath}/Gear/BaseGearConfig_Level1.asset", out isBase1New);
            if (isBase1New)
            {
                SetPrivateData(baseGearLv1, new GearConfigData
                {
                    Id = "base_gear_1",
                    BaseRotationSpeed = 0f,
                    IsInteractable = true,
                    IsMovable = true,
                    IsReturnable = true,
                    RelativeScaleMultiplier = 1.0f,
                    MaxCharge = 100f,
                    ChargeOverTimeAmount = 0f, 
                    ChargeOnTriggerAmount = 25f,
                    SnapSlowdownDuration = 0.5f,
                    SnapSlowdownMultiplier = 0.15f,
                    TriggerSpinDegrees = 45f,
                    IsDeletable = true,
                    DeleteRewardAmount = 25
                }, baseGearLv2, null);
                AssetDatabase.CreateAsset(baseGearLv1, $"{folderPath}/Gear/BaseGearConfig_Level1.asset");
            }
            UpdateVisualsAndMeta(baseGearLv1, base1View, iconNumber1, baseGearLv2, null, 25);

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
            GameObject rockView = CreatePrefabPrimitive("RockObstacleView", "BaseGear", new Color(0.4f, 0.4f, 0.35f), prefabPath);
            bool isRockNew;
            GearConfig rockObstacle = GetOrCreateGearConfig($"{folderPath}/Gear/ObstacleRockConfig.asset", out isRockNew);
            if (isRockNew)
            {
                SetPrivateData(rockObstacle, new GearConfigData
                {
                    Id = "obstacle_rock",
                    BaseRotationSpeed = 0f,
                    IsInteractable = false, 
                    IsMovable = true,
                    IsReturnable = true,
                    RelativeScaleMultiplier = 1.0f,
                    MaxCharge = 30f,
                    ChargeOverTimeAmount = 0f,
                    ChargeOnTriggerAmount = 10f,
                    SnapSlowdownDuration = 0.5f,
                    SnapSlowdownMultiplier = 0.15f,
                    TriggerSpinDegrees = 45f,
                    IsDeletable = false,
                    DeleteRewardAmount = 0
                }, null, new List<GearAbilitySO> { destroyAbility });
                AssetDatabase.CreateAsset(rockObstacle, $"{folderPath}/Gear/ObstacleRockConfig.asset");
            }
            UpdateVisualsAndMeta(rockObstacle, rockView, baseSpr, null, new List<GearAbilitySO> { destroyAbility }, 0);

            // 7. Create Score Gear
            GameObject scoreView = CreatePrefabPrimitive("ScoreGearView", "BaseGear", new Color(0.1f, 0.9f, 0.2f), prefabPath);
            bool isScoreNew;
            GearConfig scoreGear = GetOrCreateGearConfig($"{folderPath}/Gear/ScoreGearConfig.asset", out isScoreNew);
            if (isScoreNew)
            {
                SetPrivateData(scoreGear, new GearConfigData
                {
                    Id = "score_gear",
                    BaseRotationSpeed = 0f,
                    IsInteractable = true,
                    IsMovable = true,
                    IsReturnable = true,
                    RelativeScaleMultiplier = 1.0f,
                    MaxCharge = 100f,
                    ChargeOverTimeAmount = 0f,
                    ChargeOnTriggerAmount = 50f,
                    SnapSlowdownDuration = 0.5f,
                    SnapSlowdownMultiplier = 0.15f,
                    TriggerSpinDegrees = 45f,
                    IsDeletable = true,
                    DeleteRewardAmount = 100
                }, null, new List<GearAbilitySO> { scoreAbility });
                AssetDatabase.CreateAsset(scoreGear, $"{folderPath}/Gear/ScoreGearConfig.asset");
            }
            UpdateVisualsAndMeta(scoreGear, scoreView, iconCoin, null, new List<GearAbilitySO> { scoreAbility }, 100);

            // 8. Create Speed Buff Gear
            GameObject speedView = CreatePrefabPrimitive("SpeedGearView", "BaseGear", new Color(0.1f, 0.9f, 0.2f), prefabPath);
            bool isSpeedNew;
            GearConfig speedGear = GetOrCreateGearConfig($"{folderPath}/Gear/SpeedBuffGearConfig.asset", out isSpeedNew);
            if (isSpeedNew)
            {
                SetPrivateData(speedGear, new GearConfigData
                {
                    Id = "speed_buff_gear",
                    BaseRotationSpeed = 0f,
                    IsInteractable = true,
                    RelativeScaleMultiplier = 1.0f,
                    MaxCharge = 0f, 
                    ChargeOverTimeAmount = 0f,
                    ChargeOnTriggerAmount = 0f,
                    SnapSlowdownDuration = 0.5f,
                    SnapSlowdownMultiplier = 0.15f,
                    TriggerSpinDegrees = 45f,
                    IsDeletable = true,
                    DeleteRewardAmount = 50
                }, null, new List<GearAbilitySO> { speedAbility });
                AssetDatabase.CreateAsset(speedGear, $"{folderPath}/Gear/SpeedBuffGearConfig.asset");
            }
            UpdateVisualsAndMeta(speedGear, speedView, iconArrowUp, null, new List<GearAbilitySO> { speedAbility }, 50);

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

        private static void UpdateVisualsAndMeta(GearConfig config, GameObject visual, Sprite icon, GearConfig nextLvl, List<GearAbilitySO> abilities, int? forceReward = null)
        {
            SerializedObject so = new SerializedObject(config);
            SerializedProperty dataProp = so.FindProperty("data");
            if (dataProp != null)
            {
                var visualProp = dataProp.FindPropertyRelative("VisualPrefab");
                if (visualProp != null) visualProp.objectReferenceValue = visual;

                // Only overwrite icon if a valid sprite was provided — prevents
                // nulling out manually assigned icons when a sprite fails to load.
                var iconProp = dataProp.FindPropertyRelative("UIIcon");
                if (iconProp != null && icon != null) iconProp.objectReferenceValue = icon;

                // FORCE critically missing properties that might have defaulted to false upon deserialization
                var categoryProp = dataProp.FindPropertyRelative("Category");
                bool isCore = categoryProp != null && categoryProp.enumValueIndex == (int)GearCategory.Core;
                
                var isInteractableProp = dataProp.FindPropertyRelative("IsInteractable");
                if (isInteractableProp != null) isInteractableProp.boolValue = !isCore;

                var isMovableProp = dataProp.FindPropertyRelative("IsMovable");
                if (isMovableProp != null) isMovableProp.boolValue = !isCore;

                var isReturnableProp = dataProp.FindPropertyRelative("IsReturnable");
                if (isReturnableProp != null) isReturnableProp.boolValue = !isCore;

                var isDeletableProp = dataProp.FindPropertyRelative("IsDeletable");
                if (isDeletableProp != null) isDeletableProp.boolValue = !isCore;

                if (forceReward.HasValue)
                {
                    var deleteRewardProp = dataProp.FindPropertyRelative("DeleteRewardAmount");
                    if (deleteRewardProp != null) deleteRewardProp.intValue = forceReward.Value;
                }
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
                customSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/GearEngine/Art/Sprites/{spriteName}.png");
            }

            if (customSprite == null && !string.IsNullOrEmpty(spriteName))
            {
                // Fallback to BaseGear sprite if the preferred sprite didn't exist
                customSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GearEngine/Art/Sprites/BaseGear.png");
            }

            if (customSprite != null)
            {
                sr.sprite = customSprite;
                sr.color = tint;
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
                dataProp.FindPropertyRelative("IsMovable").boolValue = data.IsMovable;
                dataProp.FindPropertyRelative("IsReturnable").boolValue = data.IsReturnable;
                dataProp.FindPropertyRelative("MaxCharge").floatValue = data.MaxCharge;
                dataProp.FindPropertyRelative("ChargeOverTimeAmount").floatValue = data.ChargeOverTimeAmount;
                dataProp.FindPropertyRelative("ChargeOnTriggerAmount").floatValue = data.ChargeOnTriggerAmount;

                var uiScaleProp = dataProp.FindPropertyRelative("RelativeScaleMultiplier");
                if (uiScaleProp != null) uiScaleProp.floatValue = data.RelativeScaleMultiplier;

                var slowdownDurProp = dataProp.FindPropertyRelative("SnapSlowdownDuration");
                if (slowdownDurProp != null) slowdownDurProp.floatValue = data.SnapSlowdownDuration;

                var slowdownMultProp = dataProp.FindPropertyRelative("SnapSlowdownMultiplier");
                if (slowdownMultProp != null) slowdownMultProp.floatValue = data.SnapSlowdownMultiplier;

                var triggerSpinProp = dataProp.FindPropertyRelative("TriggerSpinDegrees");
                if (triggerSpinProp != null) triggerSpinProp.floatValue = data.TriggerSpinDegrees;

                var isDeletableProp = dataProp.FindPropertyRelative("IsDeletable");
                if (isDeletableProp != null) isDeletableProp.boolValue = data.IsDeletable;

                var deleteRewardProp = dataProp.FindPropertyRelative("DeleteRewardAmount");
                if (deleteRewardProp != null) deleteRewardProp.intValue = data.DeleteRewardAmount;
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

        private static Texture2D CreateTrashIconTexture()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color transparent = new Color(0, 0, 0, 0);
            Color fill = new Color(0.9f, 0.3f, 0.3f, 1f);

            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

            // Lid (top bar)
            for (int x = 10; x < 54; x++)
                for (int y = 50; y < 56; y++)
                    pixels[y * size + x] = fill;

            // Handle on lid
            for (int x = 24; x < 40; x++)
                for (int y = 56; y < 60; y++)
                    pixels[y * size + x] = fill;

            // Body (left wall)
            for (int x = 14; x < 18; x++)
                for (int y = 8; y < 50; y++)
                    pixels[y * size + x] = fill;

            // Body (right wall)
            for (int x = 46; x < 50; x++)
                for (int y = 8; y < 50; y++)
                    pixels[y * size + x] = fill;

            // Body (bottom)
            for (int x = 14; x < 50; x++)
                for (int y = 8; y < 12; y++)
                    pixels[y * size + x] = fill;

            // Vertical lines inside (ribs)
            for (int x = 24; x < 26; x++)
                for (int y = 14; y < 46; y++)
                    pixels[y * size + x] = fill;

            for (int x = 31; x < 33; x++)
                for (int y = 14; y < 46; y++)
                    pixels[y * size + x] = fill;

            for (int x = 38; x < 40; x++)
                for (int y = 14; y < 46; y++)
                    pixels[y * size + x] = fill;

            tex.SetPixels(pixels);
            tex.Apply();
            tex.filterMode = FilterMode.Point;

            return tex;
        }
    }
}
