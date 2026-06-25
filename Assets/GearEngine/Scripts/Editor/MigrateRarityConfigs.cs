using UnityEditor;
using UnityEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;
using GearEngine.Perks.Config;

public class MigrateRarityConfigs
{
    [MenuItem("GearEngine/Migrate Rarity Configs")]
    public static void Migrate()
    {
        string folderPath = "Assets/GearEngine/Data/Config/Rarities";
        if (!AssetDatabase.IsValidFolder("Assets/GearEngine/Data/Config"))
        {
            AssetDatabase.CreateFolder("Assets/GearEngine/Data", "Config");
        }
        if (!AssetDatabase.IsValidFolder("Assets/GearEngine/Data/Config/Rarities"))
        {
            AssetDatabase.CreateFolder("Assets/GearEngine/Data/Config", "Rarities");
        }

        RarityConfigSO common = GetOrCreateRarity(folderPath, ItemRarity.Common, "AAAAAA", "Assets/GearEngine/Art/Sprites/UI/RarityCards/card_bg_common.png");
        RarityConfigSO rare = GetOrCreateRarity(folderPath, ItemRarity.Rare, "0070FF", "Assets/GearEngine/Art/Sprites/UI/RarityCards/card_bg_rare.png");
        RarityConfigSO epic = GetOrCreateRarity(folderPath, ItemRarity.Epic, "A335EE", "Assets/GearEngine/Art/Sprites/UI/RarityCards/card_bg_epic.png");
        RarityConfigSO legendary = GetOrCreateRarity(folderPath, ItemRarity.Legendary, "FF8000", "Assets/GearEngine/Art/Sprites/UI/RarityCards/card_bg_legendary.png");

        // Migrate GearItems
        string[] gearGuids = AssetDatabase.FindAssets("t:GearItem");
        int migratedGears = 0;
        foreach (var guid in gearGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GearItem gear = AssetDatabase.LoadAssetAtPath<GearItem>(path);
            if (gear != null)
            {
                SerializedObject so = new SerializedObject(gear);
                SerializedProperty dataProp = so.FindProperty("data");
                if (dataProp != null)
                {
                    SerializedProperty rarityProp = dataProp.FindPropertyRelative("rarity");
                    SerializedProperty configProp = dataProp.FindPropertyRelative("rarityConfig");

                    if (rarityProp != null && configProp != null)
                    {
                        ItemRarity r = (ItemRarity)rarityProp.enumValueIndex;
                        RarityConfigSO targetConfig = r switch
                        {
                            ItemRarity.Common => common,
                            ItemRarity.Rare => rare,
                            ItemRarity.Epic => epic,
                            ItemRarity.Legendary => legendary,
                            _ => common
                        };
                        
                        configProp.objectReferenceValue = targetConfig;
                        so.ApplyModifiedProperties();
                        migratedGears++;
                    }
                }
            }
        }

        // Migrate PerkItems
        string[] perkGuids = AssetDatabase.FindAssets("t:PerkItem");
        int migratedPerks = 0;
        foreach (var guid in perkGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            PerkItem perk = AssetDatabase.LoadAssetAtPath<PerkItem>(path);
            if (perk != null)
            {
                SerializedObject so = new SerializedObject(perk);
                SerializedProperty dataProp = so.FindProperty("data");
                if (dataProp != null)
                {
                    SerializedProperty rarityProp = dataProp.FindPropertyRelative("rarity");
                    SerializedProperty configProp = dataProp.FindPropertyRelative("rarityConfig");

                    if (rarityProp != null && configProp != null)
                    {
                        ItemRarity r = (ItemRarity)rarityProp.enumValueIndex;
                        RarityConfigSO targetConfig = r switch
                        {
                            ItemRarity.Common => common,
                            ItemRarity.Rare => rare,
                            ItemRarity.Epic => epic,
                            ItemRarity.Legendary => legendary,
                            _ => common
                        };
                        
                        configProp.objectReferenceValue = targetConfig;
                        so.ApplyModifiedProperties();
                        migratedPerks++;
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Migrated {migratedGears} Gears and {migratedPerks} Perks to use individual Rarity Configs!");
    }

    private static RarityConfigSO GetOrCreateRarity(string folderPath, ItemRarity rarity, string hexColor, string spritePath)
    {
        string assetPath = $"{folderPath}/{rarity}Rarity.asset";
        RarityConfigSO config = AssetDatabase.LoadAssetAtPath<RarityConfigSO>(assetPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<RarityConfigSO>();
            AssetDatabase.CreateAsset(config, assetPath);
        }

        config.Rarity = rarity;
        config.DisplayName = rarity.ToString();
        ColorUtility.TryParseHtmlString("#" + hexColor, out Color c);
        config.Color = c;
        config.CardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        EditorUtility.SetDirty(config);

        return config;
    }
}
