using UnityEngine;
using UnityEditor;
using GearEngine.Perks.Config;
using System.IO;

namespace GearEngine.Perks.Editor
{
    public static class PerkTestDataGenerator
    {
        [MenuItem("Tools/Gear Engine/Generate Test Perks")]
        public static void GenerateTestPerks()
        {
            string folderPath = "Assets/GearEngine/Data/Perks";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "GearEngine/Data/Perks"));
                AssetDatabase.Refresh();
            }

            // Create some test perks
            PerkItem speedPerk = CreatePerk("SpeedBoost", "Speed Boost", "Increases movement speed by 10%.", folderPath);
            PerkItem healthPerk = CreatePerk("ExtraHealth", "Extra Health", "Grants an additional 20 max HP.", folderPath);
            PerkItem damagePerk = CreatePerk("DamageUp", "Damage Up", "Increases base damage by 5.", folderPath);

            // Create catalog
            string catalogPath = $"{folderPath}/TestPerkCatalog.asset";
            PerkCatalogSO catalog = AssetDatabase.LoadAssetAtPath<PerkCatalogSO>(catalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<PerkCatalogSO>();
                AssetDatabase.CreateAsset(catalog, catalogPath);
            }

            SerializedObject catalogSo = new SerializedObject(catalog);
            SerializedProperty configsProp = catalogSo.FindProperty("items");

            if (configsProp != null)
            {
                configsProp.arraySize = 3;
                configsProp.GetArrayElementAtIndex(0).objectReferenceValue = speedPerk;
                configsProp.GetArrayElementAtIndex(1).objectReferenceValue = healthPerk;
                configsProp.GetArrayElementAtIndex(2).objectReferenceValue = damagePerk;
                catalogSo.ApplyModifiedProperties();
            }
            
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created test perks and catalog at {folderPath}");
        }

        private static PerkItem CreatePerk(string id, string name, string description, string folderPath)
        {
            string path = $"{folderPath}/Perk_{id}.asset";
            PerkItem perk = AssetDatabase.LoadAssetAtPath<PerkItem>(path);
            if (perk == null)
            {
                perk = ScriptableObject.CreateInstance<PerkItem>();
                AssetDatabase.CreateAsset(perk, path);
            }

            SerializedObject so = new SerializedObject(perk);
            SerializedProperty dataProp = so.FindProperty("data");
            if (dataProp != null)
            {
                dataProp.FindPropertyRelative("id").stringValue = id;
                dataProp.FindPropertyRelative("perkName").stringValue = name;
                dataProp.FindPropertyRelative("description").stringValue = description;
                so.ApplyModifiedProperties();
            }
            else 
            {
                // Just in case it's not nested in data
                SerializedProperty nameProp = so.FindProperty("perkName");
                if (nameProp == null) nameProp = so.FindProperty("Name");
                if (nameProp != null) nameProp.stringValue = name;
                
                so.ApplyModifiedProperties();
            }
            
            EditorUtility.SetDirty(perk);
            return perk;
        }
        [MenuItem("Tools/Gear Engine/Generate Gameplay Perks")]
        public static void GenerateGameplayPerks()
        {
            string folderPath = "Assets/GearEngine/Data/Perks/Gameplay";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "GearEngine/Data/Perks/Gameplay"));
                AssetDatabase.Refresh();
            }

            PerkItem maxSpeedPerk = CreatePerkWithModifier<GearEngine.Perks.Powerups.MaxSpeedMultiplierModifierSO>(
                "MaxSpeed", "Max Speed", "Increase speed limit of the car on a race", folderPath, m => { });
                
            PerkItem gearSizePerk = CreatePerkWithModifier<GearEngine.Perks.Powerups.ExtraCarGearsModifierSO>(
                "GearSize", "Gear Size", "Increase how many gears you can have in the car", folderPath, m => { });
                
            PerkItem stationsPerk = CreatePerkWithModifier<GearEngine.Perks.Powerups.ExtraInventoryGearsModifierSO>(
                "Stations", "Stations", "Increase how many gears you can have in inventory", folderPath, m => { });
                
            PerkItem boostPerk = CreatePerkWithModifier<GearEngine.Perks.Powerups.ExtraMaxNitroModifierSO>(
                "Boost", "Boost", "Increase the maximum nitro-boost you can have in a race", folderPath, m => { });
                
            PerkItem hotTiresPerk = CreatePerkWithModifier<GearEngine.Perks.Powerups.InitialSpeedBoostModifierSO>(
                "HotTires", "Hot Tires", "Initial speed increase", folderPath, m => { });

            AssetDatabase.SaveAssets();
            Debug.Log($"Created gameplay perks at {folderPath}");
        }

        private static PerkItem CreatePerkWithModifier<T>(string id, string name, string description, string folderPath, System.Action<T> modifierSetup) where T : GearEngine.Perks.Powerups.CarPowerupModifierSO
        {
            PerkItem perk = CreatePerk(id, name, description, folderPath);
            
            string modPath = $"{folderPath}/Mod_{id}.asset";
            T modifier = AssetDatabase.LoadAssetAtPath<T>(modPath);
            if (modifier == null)
            {
                modifier = ScriptableObject.CreateInstance<T>();
                modifierSetup?.Invoke(modifier);
                AssetDatabase.CreateAsset(modifier, modPath);
            }
            else
            {
                modifierSetup?.Invoke(modifier);
                EditorUtility.SetDirty(modifier);
            }

            SerializedObject so = new SerializedObject(perk);
            SerializedProperty dataProp = so.FindProperty("data");
            if (dataProp != null)
            {
                SerializedProperty modifiersProp = dataProp.FindPropertyRelative("modifiers");
                if (modifiersProp != null)
                {
                    bool hasModifier = false;
                    for (int i = 0; i < modifiersProp.arraySize; i++)
                    {
                        if (modifiersProp.GetArrayElementAtIndex(i).objectReferenceValue == modifier)
                        {
                            hasModifier = true;
                            break;
                        }
                    }
                    
                    if (!hasModifier)
                    {
                        modifiersProp.arraySize++;
                        modifiersProp.GetArrayElementAtIndex(modifiersProp.arraySize - 1).objectReferenceValue = modifier;
                        so.ApplyModifiedProperties();
                    }
                }
            }
            
            EditorUtility.SetDirty(perk);
            return perk;
        }
    }
}
