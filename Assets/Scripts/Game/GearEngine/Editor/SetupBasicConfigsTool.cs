using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Abilities;
using GearEngine.GearEngine.Presentation;

namespace GearEngine.GearEngine.Editor
{
    public static class SetupBasicConfigsTool
    {
        [MenuItem("GearEngine/Step 1: Generate Basic Setup Configs")]
        public static void GenerateConfigs()
        {
            RunBasicGearConfigPipeline("Assets/Game/GearEngine/Configs", "Assets/Game/GearEngine/Prefabs");
        }

        private static void RunBasicGearConfigPipeline(string folderPath, string prefabPath)
        {
            EnsureGearFolderLayout(folderPath, prefabPath);
            GearSprites sprites = LoadGearSprites();
            CreateTagsAndBoardConfig(folderPath);
            CreatePrefabPrimitive("EmptySlotView", "", new Color(0.2f, 0.2f, 0.2f, 0.3f), prefabPath);
            GearConfig baseGearLv2 = EnsureBaseGearLevel2(folderPath, prefabPath, sprites);
            EnsureCoreGear(folderPath, prefabPath, sprites);
            EnsureBaseGearLevel1(folderPath, prefabPath, sprites, baseGearLv2);
            AbilityAssets abilities = CreateAbilityAssets(folderPath);
            EnsureRockObstacle(folderPath, prefabPath, sprites, abilities.DestroySelf);
            EnsureScoreGear(folderPath, prefabPath, sprites, abilities.Score);
            EnsureSpeedGear(folderPath, prefabPath, sprites, abilities.SpeedBoost);
            CreateOrUpdateDefaultInventoryLoadout(folderPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"<color=#33ff33>[GearEngine]</color> Basic Setup Configs successfully generated and merged at {folderPath}!");
        }

        private static void EnsureGearFolderLayout(string folderPath, string prefabPath)
        {
            EnsureDirectoryExists(folderPath);
            EnsureDirectoryExists($"{folderPath}/Tag");
            EnsureDirectoryExists($"{folderPath}/Gear");
            EnsureDirectoryExists($"{folderPath}/Ability");
            EnsureDirectoryExists(prefabPath);
            AssetDatabase.Refresh();
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void CommitNewGearIfNeeded(GearConfig cfg, bool isNew, string assetPath, GearConfigData data, GearConfig nextLvl, List<GearAbilitySO> abilities)
        {
            if (!isNew)
            {
                return;
            }

            SetPrivateData(cfg, data, nextLvl, abilities);
            AssetDatabase.CreateAsset(cfg, assetPath);
        }

        private static GearSprites LoadGearSprites()
        {
            Sprite baseSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/BaseGear.png");
            Sprite coreSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/CoreGear.png");
            Sprite fallbackSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/FallbackGear.png");
            Sprite rockSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/RockObstacle.png") ?? fallbackSpr;
            Sprite scoreSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/ScoreGear.png") ?? fallbackSpr;
            Sprite speedSpr = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/SpeedGear.png") ?? fallbackSpr;
            return new GearSprites(baseSpr, coreSpr, fallbackSpr, rockSpr, scoreSpr, speedSpr);
        }

        private static void CreateTagsAndBoardConfig(string folderPath)
        {
            TagSO gridBoardTag = ScriptableObject.CreateInstance<TagSO>();
            gridBoardTag.Description = "Valid 2D surface area for gears to be dropped onto.";
            AssetDatabase.CreateAsset(gridBoardTag, $"{folderPath}/Tag/GridBoard_Tag.asset");

            TagSO inventoryTag = ScriptableObject.CreateInstance<TagSO>();
            inventoryTag.Description = "Marks an item inside the inventory that can be picked up.";
            AssetDatabase.CreateAsset(inventoryTag, $"{folderPath}/Tag/Inventory_Tag.asset");

            BoardConfigSO boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
            boardConfig.GridWidth = 7;
            boardConfig.GridHeight = 5;
            AssetDatabase.CreateAsset(boardConfig, $"{folderPath}/BasicBoardConfig.asset");
        }

        private static GearConfig EnsureBaseGearLevel2(string folderPath, string prefabPath, GearSprites sprites)
        {
            GameObject view = CreatePrefabPrimitive("BaseGear2View", "BaseGear", new Color(0.8f, 0.8f, 0.9f), prefabPath);
            GearConfig cfg = GetOrCreateGearConfig($"{folderPath}/Gear/BaseGearConfig_Level2.asset", out bool isNew);
            GearConfigData data = new GearConfigData { Id = "base_gear_2", BaseRotationSpeed = 0f, IsInteractable = true, MaxCharge = 200f, ChargeOverTimeAmount = 0f, ChargeOnTriggerAmount = 50f };
            CommitNewGearIfNeeded(cfg, isNew, $"{folderPath}/Gear/BaseGearConfig_Level2.asset", data, null, null);
            UpdateVisualsAndMeta(cfg, view, sprites.Base, null, null);
            return cfg;
        }

        private static void EnsureCoreGear(string folderPath, string prefabPath, GearSprites sprites)
        {
            GameObject view = CreatePrefabPrimitive("CoreGearView", "CoreGear", new Color(1f, 0.8f, 0.1f), prefabPath);
            GearConfig cfg = GetOrCreateGearConfig($"{folderPath}/Gear/CoreGearConfig.asset", out bool isNew);
            GearConfigData data = new GearConfigData { Id = "core_gear_1", Category = GearCategory.Core, BaseRotationSpeed = 70f, TriggerPattern = TriggerPattern.FourWay, IsInteractable = true, MaxCharge = 0f, ChargeOverTimeAmount = 0f, ChargeOnTriggerAmount = 0f };
            CommitNewGearIfNeeded(cfg, isNew, $"{folderPath}/Gear/CoreGearConfig.asset", data, null, null);
            UpdateVisualsAndMeta(cfg, view, sprites.Core, null, null);
        }

        private static void EnsureBaseGearLevel1(string folderPath, string prefabPath, GearSprites sprites, GearConfig nextLevel)
        {
            GameObject view = CreatePrefabPrimitive("BaseGear1View", "BaseGear", new Color(0.6f, 0.6f, 0.65f), prefabPath);
            GearConfig cfg = GetOrCreateGearConfig($"{folderPath}/Gear/BaseGearConfig_Level1.asset", out bool isNew);
            GearConfigData data = new GearConfigData { Id = "base_gear_1", BaseRotationSpeed = 0f, IsInteractable = true, MaxCharge = 100f, ChargeOverTimeAmount = 0f, ChargeOnTriggerAmount = 25f };
            CommitNewGearIfNeeded(cfg, isNew, $"{folderPath}/Gear/BaseGearConfig_Level1.asset", data, nextLevel, null);
            UpdateVisualsAndMeta(cfg, view, sprites.Base, nextLevel, null);
        }

        private static AbilityAssets CreateAbilityAssets(string folderPath)
        {
            DestroySelfAbility destroyAbility = ScriptableObject.CreateInstance<DestroySelfAbility>();
            AssetDatabase.CreateAsset(destroyAbility, $"{folderPath}/Ability/DestroySelf_Ability.asset");

            ScoreAbility scoreAbility = ScriptableObject.CreateInstance<ScoreAbility>();
            scoreAbility.ScoreAmount = 500;
            AssetDatabase.CreateAsset(scoreAbility, $"{folderPath}/Ability/Score_Ability.asset");

            SpeedBoostAbility speedAbility = ScriptableObject.CreateInstance<SpeedBoostAbility>();
            speedAbility.SpeedMultiplier = 2.0f;
            AssetDatabase.CreateAsset(speedAbility, $"{folderPath}/Ability/SpeedBoost_Ability.asset");
            return new AbilityAssets(destroyAbility, scoreAbility, speedAbility);
        }

        private static void EnsureRockObstacle(string folderPath, string prefabPath, GearSprites sprites, DestroySelfAbility destroyAbility)
        {
            GameObject view = CreatePrefabPrimitive("RockObstacleView", "RockObstacle", new Color(0.4f, 0.4f, 0.35f), prefabPath);
            GearConfig cfg = GetOrCreateGearConfig($"{folderPath}/Gear/ObstacleRockConfig.asset", out bool isNew);
            List<GearAbilitySO> abs = new List<GearAbilitySO> { destroyAbility };
            GearConfigData data = new GearConfigData { Id = "obstacle_rock", BaseRotationSpeed = 0f, IsInteractable = false, MaxCharge = 30f, ChargeOverTimeAmount = 0f, ChargeOnTriggerAmount = 10f };
            CommitNewGearIfNeeded(cfg, isNew, $"{folderPath}/Gear/ObstacleRockConfig.asset", data, null, abs);
            UpdateVisualsAndMeta(cfg, view, sprites.Rock, null, abs);
        }

        private static void EnsureScoreGear(string folderPath, string prefabPath, GearSprites sprites, ScoreAbility scoreAbility)
        {
            GameObject view = CreatePrefabPrimitive("ScoreGearView", "ScoreGear", new Color(1f, 0.6f, 0.0f), prefabPath);
            GearConfig cfg = GetOrCreateGearConfig($"{folderPath}/Gear/ScoreGearConfig.asset", out bool isNew);
            List<GearAbilitySO> abs = new List<GearAbilitySO> { scoreAbility };
            GearConfigData data = new GearConfigData { Id = "score_gear", BaseRotationSpeed = 0f, IsInteractable = true, MaxCharge = 100f, ChargeOverTimeAmount = 0f, ChargeOnTriggerAmount = 50f };
            CommitNewGearIfNeeded(cfg, isNew, $"{folderPath}/Gear/ScoreGearConfig.asset", data, null, abs);
            UpdateVisualsAndMeta(cfg, view, sprites.Score, null, abs);
        }

        private static void EnsureSpeedGear(string folderPath, string prefabPath, GearSprites sprites, SpeedBoostAbility speedAbility)
        {
            GameObject view = CreatePrefabPrimitive("SpeedGearView", "SpeedGear", new Color(0.1f, 0.9f, 0.2f), prefabPath);
            GearConfig cfg = GetOrCreateGearConfig($"{folderPath}/Gear/SpeedBuffGearConfig.asset", out bool isNew);
            List<GearAbilitySO> abs = new List<GearAbilitySO> { speedAbility };
            GearConfigData data = new GearConfigData { Id = "speed_buff_gear", BaseRotationSpeed = 0f, IsInteractable = true, MaxCharge = 0f, ChargeOverTimeAmount = 0f, ChargeOnTriggerAmount = 0f };
            CommitNewGearIfNeeded(cfg, isNew, $"{folderPath}/Gear/SpeedBuffGearConfig.asset", data, null, abs);
            UpdateVisualsAndMeta(cfg, view, sprites.Speed, null, abs);
        }

        private static void CreateOrUpdateDefaultInventoryLoadout(string folderPath)
        {
            GearConfig loadoutCore = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/CoreGearConfig.asset");
            GearConfig loadoutBase1 = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/BaseGearConfig_Level1.asset");
            GearConfig loadoutBase2 = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/BaseGearConfig_Level2.asset");
            GearConfig loadoutSpeed = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/SpeedBuffGearConfig.asset");
            GearConfig loadoutRock = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/ObstacleRockConfig.asset");
            GearConfig loadoutScore = AssetDatabase.LoadAssetAtPath<GearConfig>($"{folderPath}/Gear/ScoreGearConfig.asset");
            GearInventoryLoadoutSO loadout = GetOrCreateLoadout($"{folderPath}/GearInventoryLoadout.asset");
            List<GearConfig> gears = BuildDefaultStartingGearList(loadoutCore, loadoutBase1, loadoutBase2, loadoutSpeed, loadoutRock, loadoutScore);
            WriteStartingGearsSerialized(loadout, gears);
        }

        private static void AddIfNotNull(List<GearConfig> list, GearConfig cfg)
        {
            if (cfg != null)
            {
                list.Add(cfg);
            }
        }

        private static GearInventoryLoadoutSO GetOrCreateLoadout(string path)
        {
            GearInventoryLoadoutSO loadout = AssetDatabase.LoadAssetAtPath<GearInventoryLoadoutSO>(path);
            if (loadout == null)
            {
                loadout = ScriptableObject.CreateInstance<GearInventoryLoadoutSO>();
                AssetDatabase.CreateAsset(loadout, path);
            }

            return loadout;
        }

        private static List<GearConfig> BuildDefaultStartingGearList(GearConfig core, GearConfig baseGear, GearConfig baseLevel2, GearConfig speedGear, GearConfig rockObs, GearConfig scoreGear)
        {
            var gears = new List<GearConfig>();
            AddIfNotNull(gears, core);
            AddIfNotNull(gears, baseGear);
            AddIfNotNull(gears, baseLevel2);
            AddIfNotNull(gears, speedGear);
            AddIfNotNull(gears, rockObs);
            AddIfNotNull(gears, scoreGear);
            return gears;
        }

        private static void WriteStartingGearsSerialized(GearInventoryLoadoutSO loadout, List<GearConfig> gears)
        {
            SerializedObject so = new SerializedObject(loadout);
            SerializedProperty prop = so.FindProperty("startingGears");
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
            return ScriptableObject.CreateInstance<GearConfig>();
        }

        private static void UpdateVisualsAndMeta(GearConfig config, GameObject visual, Sprite icon, GearConfig nextLvl, List<GearAbilitySO> abilities)
        {
            SerializedObject so = new SerializedObject(config);
            ApplyVisualAndIcon(so, visual, icon);
            ApplyNextLevelSerialized(so, nextLvl);
            ApplyAbilitiesSerialized(so, abilities);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
        }

        private static void ApplyVisualAndIcon(SerializedObject so, GameObject visual, Sprite icon)
        {
            SerializedProperty dataProp = so.FindProperty("data");
            if (dataProp == null)
            {
                return;
            }

            SerializedProperty visualProp = dataProp.FindPropertyRelative("VisualPrefab");
            if (visualProp != null)
            {
                visualProp.objectReferenceValue = visual;
            }

            SerializedProperty iconProp = dataProp.FindPropertyRelative("UIIcon");
            if (iconProp != null)
            {
                iconProp.objectReferenceValue = icon;
            }
        }

        private static void ApplyNextLevelSerialized(SerializedObject so, GearConfig nextLvl)
        {
            if (nextLvl == null)
            {
                return;
            }

            so.FindProperty("nextLevel").objectReferenceValue = nextLvl;
        }

        private static void ApplyAbilitiesSerialized(SerializedObject so, List<GearAbilitySO> abilities)
        {
            if (abilities == null)
            {
                return;
            }

            SerializedProperty abProp = so.FindProperty("abilities");
            if (abilities.Count > 0)
            {
                WriteAbilityListToProperty(abProp, abilities);
            }
            else
            {
                abProp.ClearArray();
            }
        }

        private static void WriteAbilityListToProperty(SerializedProperty abProp, List<GearAbilitySO> abilities)
        {
            abProp.ClearArray();
            for (int i = 0; i < abilities.Count; i++)
            {
                abProp.InsertArrayElementAtIndex(i);
                abProp.GetArrayElementAtIndex(i).objectReferenceValue = abilities[i];
            }
        }

        private static GameObject CreatePrefabPrimitive(string name, string spriteName, Color tint, string destinationDir)
        {
            string fullPath = $"{destinationDir}/{name}.prefab";
            GameObject tempGo = new GameObject(name);
            SpriteRenderer sr = tempGo.AddComponent<SpriteRenderer>();
            ApplySpriteToRenderer(sr, spriteName, tint);
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(tempGo, fullPath);
            Object.DestroyImmediate(tempGo);
            return savedPrefab;
        }

        private static void ApplySpriteToRenderer(SpriteRenderer sr, string spriteName, Color tint)
        {
            Sprite customSprite = TryLoadGearSprite(spriteName);
            if (customSprite != null)
            {
                sr.sprite = customSprite;
                sr.color = tint != Color.white ? tint : Color.white;
                return;
            }

            sr.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            sr.color = tint;
        }

        private static Sprite TryLoadGearSprite(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                return null;
            }

            Sprite customSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Game/GearEngine/Sprites/{spriteName}.png");
            if (customSprite == null)
            {
                customSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/GearEngine/Sprites/FallbackGear.png");
            }

            return customSprite;
        }

        private static void SetPrivateData(GearConfig config, GearConfigData data, GearConfig nextLvl, List<GearAbilitySO> abilities)
        {
            SerializedObject so = new SerializedObject(config);
            ApplyGearDataFields(so, data);
            ApplyNextLevelSerialized(so, nextLvl);
            CopyAbilitiesToConfig(so, abilities);
            so.ApplyModifiedProperties();
        }

        private static void ApplyGearDataFields(SerializedObject so, GearConfigData data)
        {
            SerializedProperty dataProp = so.FindProperty("data");
            if (dataProp == null)
            {
                return;
            }

            dataProp.FindPropertyRelative("Id").stringValue = data.Id;
            dataProp.FindPropertyRelative("BaseRotationSpeed").floatValue = data.BaseRotationSpeed;
            CopyOptionalObjectRefs(dataProp, data);
            CopyGearEnumsAndScalars(dataProp, data);
        }

        private static void CopyOptionalObjectRefs(SerializedProperty dataProp, GearConfigData data)
        {
            SerializedProperty visualProp = dataProp.FindPropertyRelative("VisualPrefab");
            if (visualProp != null)
            {
                visualProp.objectReferenceValue = data.VisualPrefab;
            }

            SerializedProperty iconProp = dataProp.FindPropertyRelative("UIIcon");
            if (iconProp != null)
            {
                iconProp.objectReferenceValue = data.UIIcon;
            }

            SerializedProperty categoryProp = dataProp.FindPropertyRelative("Category");
            if (categoryProp != null)
            {
                categoryProp.enumValueIndex = (int)data.Category;
            }
        }

        private static void CopyGearEnumsAndScalars(SerializedProperty dataProp, GearConfigData data)
        {
            int triggerIndex = data.TriggerPattern == TriggerPattern.FourWay ? 0 : 1;
            dataProp.FindPropertyRelative("TriggerPattern").enumValueIndex = triggerIndex;
            dataProp.FindPropertyRelative("IsInteractable").boolValue = data.IsInteractable;
            dataProp.FindPropertyRelative("MaxCharge").floatValue = data.MaxCharge;
            dataProp.FindPropertyRelative("ChargeOverTimeAmount").floatValue = data.ChargeOverTimeAmount;
            dataProp.FindPropertyRelative("ChargeOnTriggerAmount").floatValue = data.ChargeOnTriggerAmount;
            CopyOptionalSlowdownFields(dataProp, data);
        }

        private static void CopyOptionalSlowdownFields(SerializedProperty dataProp, GearConfigData data)
        {
            SerializedProperty slowdownDurProp = dataProp.FindPropertyRelative("SnapSlowdownDuration");
            if (slowdownDurProp != null)
            {
                slowdownDurProp.floatValue = data.SnapSlowdownDuration;
            }

            SerializedProperty slowdownMultProp = dataProp.FindPropertyRelative("SnapSlowdownMultiplier");
            if (slowdownMultProp != null)
            {
                slowdownMultProp.floatValue = data.SnapSlowdownMultiplier;
            }

            SerializedProperty triggerSpinProp = dataProp.FindPropertyRelative("TriggerSpinDegrees");
            if (triggerSpinProp != null)
            {
                triggerSpinProp.floatValue = data.TriggerSpinDegrees;
            }
        }

        private static void CopyAbilitiesToConfig(SerializedObject so, List<GearAbilitySO> abilities)
        {
            if (abilities == null || abilities.Count == 0)
            {
                return;
            }

            SerializedProperty abProp = so.FindProperty("abilities");
            WriteAbilityListToProperty(abProp, abilities);
        }
    }
}
