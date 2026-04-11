using System.IO;
using Game.GearEngine.Presentation;
using Scaffold.Navigation;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GearEngine.Editor
{
    /// <summary>
    /// Creates the navigation stub prefab, <see cref="ViewConfig"/>, and registers it on <c>Assets/Data/Navigation/Navigation Settings.asset</c>.
    /// </summary>
    public static class GearEngineNavigationAssetGenerator
    {
        public const string StubPrefabPath = "Assets/Game/GearEngine/Prefabs/GearEngineView_NavigationStub.prefab";
        public const string ViewConfigPath = "Assets/Game/GearEngine/Configs/GearEngineViewConfig.asset";
        public const string NavigationSettingsPath = "Assets/Data/Navigation/Navigation Settings.asset";

        [MenuItem("GearEngine/Generate Navigation Assets")]
        public static void Generate()
        {
            try
            {
                EnsurePrefabFolder();
                EnsureStubPrefab();
                EnsureStubIsAddressable();
                ViewConfig viewConfig = EnsureViewConfig();
                RegisterViewConfigOnNavigationSettings(viewConfig);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"<color=#33ff33>[GearEngine]</color> Navigation assets ready. ViewConfig: {ViewConfigPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GearEngineNavigationAssetGenerator] {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void EnsurePrefabFolder()
        {
            string dir = Path.GetDirectoryName(StubPrefabPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private static void EnsureStubPrefab()
        {
            if (File.Exists(StubPrefabPath))
            {
                return;
            }

            var root = new GameObject("GearEngineViewStub", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            var gearView = root.AddComponent<GearEngineView>();

            GameObject simGo = new GameObject("SimulationControlView", typeof(RectTransform));
            simGo.transform.SetParent(root.transform, false);
            var simView = simGo.AddComponent<SimulationControlView>();

            GameObject invGo = new GameObject("GearInventoryView", typeof(RectTransform));
            invGo.transform.SetParent(root.transform, false);
            var invView = invGo.AddComponent<GearInventoryView>();
            GameObject itemsGo = new GameObject("ItemsContainer", typeof(RectTransform));
            itemsGo.transform.SetParent(invGo.transform, false);
            var itemsRt = itemsGo.GetComponent<RectTransform>();
            var invSo = new SerializedObject(invView);
            SerializedProperty containerProp = invSo.FindProperty("itemsContainer");
            if (containerProp != null)
            {
                containerProp.objectReferenceValue = itemsRt;
            }

            invSo.ApplyModifiedPropertiesWithoutUndo();

            GameObject boardGo = new GameObject("BoardViewPlaceholder", typeof(RectTransform));
            boardGo.transform.SetParent(root.transform, false);
            var boardView = boardGo.AddComponent<BoardView>();

            var gvSo = new SerializedObject(gearView);
            SetReference(gvSo, "simControlView", simView);
            SetReference(gvSo, "inventoryView", invView);
            SetReference(gvSo, "boardView", boardView);
            gvSo.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, StubPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void EnsureStubIsAddressable()
        {
            try
            {
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                {
                    Debug.LogWarning(
                        "[GearEngineNavigationAssetGenerator] AddressableAssetSettings not found. " +
                        "Open Window > Asset Management > Addressables > Groups, then run this menu again.");
                    return;
                }

                string guid = AssetDatabase.AssetPathToGUID(StubPrefabPath);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning($"[GearEngineNavigationAssetGenerator] Stub prefab has no GUID yet ({StubPrefabPath}).");
                    return;
                }

                AddressableAssetEntry existing = settings.FindAssetEntry(guid);
                if (existing != null)
                {
                    return;
                }

                AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
                if (entry == null)
                {
                    Debug.LogError("[GearEngineNavigationAssetGenerator] CreateOrMoveEntry returned null for navigation stub.");
                    return;
                }

                entry.address = StubPrefabPath;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GearEngineNavigationAssetGenerator] Addressable registration failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void SetReference(SerializedObject so, string propertyName, Object value)
        {
            SerializedProperty p = so.FindProperty(propertyName);
            if (p != null)
            {
                p.objectReferenceValue = value;
            }
        }

        private static ViewConfig EnsureViewConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ViewConfig>(ViewConfigPath);
            if (existing != null)
            {
                AssignStubAssetReference(existing);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var viewConfig = ScriptableObject.CreateInstance<ViewConfig>();
            AssetDatabase.CreateAsset(viewConfig, ViewConfigPath);
            AssignStubAssetReference(viewConfig);
            EditorUtility.SetDirty(viewConfig);
            return viewConfig;
        }

        private static void AssignStubAssetReference(ViewConfig viewConfig)
        {
            string guid = AssetDatabase.AssetPathToGUID(StubPrefabPath);
            if (string.IsNullOrEmpty(guid))
            {
                throw new System.InvalidOperationException($"Stub prefab not found at {StubPrefabPath}.");
            }

            var so = new SerializedObject(viewConfig);
            SerializedProperty assetProp = so.FindProperty("asset") ?? so.FindProperty("viewAsset");
            if (assetProp == null)
            {
                throw new System.InvalidOperationException("ViewConfig: could not find serialized asset field.");
            }

            SerializedProperty guidProp = assetProp.FindPropertyRelative("m_AssetGUID");
            if (guidProp != null)
            {
                guidProp.stringValue = guid;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            viewConfig.SetType(typeof(GearEngineView));
            EditorUtility.SetDirty(viewConfig);
        }

        private static void RegisterViewConfigOnNavigationSettings(ViewConfig viewConfig)
        {
            var settings = AssetDatabase.LoadAssetAtPath<NavigationSettings>(NavigationSettingsPath);
            if (settings == null)
            {
                Debug.LogError($"[GearEngine] Missing Navigation Settings at {NavigationSettingsPath}. Create it in the editor (Scaffold/Core/Settings/Navigation).");
                return;
            }

            var so = new SerializedObject(settings);
            SerializedProperty screens = so.FindProperty("screens");
            if (screens == null)
            {
                return;
            }

            for (int i = 0; i < screens.arraySize; i++)
            {
                if (screens.GetArrayElementAtIndex(i).objectReferenceValue == viewConfig)
                {
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(settings);
                    return;
                }
            }

            int idx = screens.arraySize;
            screens.InsertArrayElementAtIndex(idx);
            screens.GetArrayElementAtIndex(idx).objectReferenceValue = viewConfig;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
        }
    }
}
