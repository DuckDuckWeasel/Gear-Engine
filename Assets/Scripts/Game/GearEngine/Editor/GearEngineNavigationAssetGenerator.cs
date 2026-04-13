global using GearEngine.GearEngine;
global using GearEngine.GearEngine.Abilities;
global using GearEngine.GearEngine.Bootstrap;
global using GearEngine.GearEngine.Config;
global using GearEngine.GearEngine.Events;
global using GearEngine.GearEngine.Manager;
global using GearEngine.GearEngine.Merge;
global using GearEngine.GearEngine.Nodes;
global using GearEngine.GearEngine.Visuals;
global using GearEngine.GearEngine.Presentation;
global using GearEngine.GearEngine.Presentation.UI;
global using GearEngine.GearEngine.Presentation.UI.Tags;
global using GearEngine.GearEngine.Presentation.World;

using System.IO;
using GearEngine.GearEngine.Presentation;
using Scaffold.Navigation;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Editor
{
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

            GameObject root = BuildStubRootCanvas();
            PopulateStubChildViews(root);
            SaveAndDestroyStubRoot(root);
        }

        private static GameObject BuildStubRootCanvas()
        {
            GameObject root = new GameObject("GearEngineViewStub", typeof(RectTransform));
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            root.AddComponent<GearEngineView>();
            return root;
        }

        private static void PopulateStubChildViews(GameObject root)
        {
            SimulationControlView simView = CreateStubSimulationView(root.transform);
            GearInventoryView invView = CreateStubInventoryView(root.transform);
            BoardView boardView = CreateStubBoardPlaceholder(root.transform);
            WireGearEngineViewReferences(root.GetComponent<GearEngineView>(), simView, invView, boardView);
        }

        private static SimulationControlView CreateStubSimulationView(Transform parent)
        {
            GameObject simGo = new GameObject("SimulationControlView", typeof(RectTransform));
            simGo.transform.SetParent(parent, false);
            return simGo.AddComponent<SimulationControlView>();
        }

        private static GearInventoryView CreateStubInventoryView(Transform parent)
        {
            GameObject invGo = new GameObject("GearInventoryView", typeof(RectTransform));
            invGo.transform.SetParent(parent, false);
            GearInventoryView invView = invGo.AddComponent<GearInventoryView>();
            GameObject itemsGo = new GameObject("ItemsContainer", typeof(RectTransform));
            itemsGo.transform.SetParent(invGo.transform, false);
            RectTransform itemsRt = itemsGo.GetComponent<RectTransform>();
            SerializedObject invSo = new SerializedObject(invView);
            SerializedProperty containerProp = invSo.FindProperty("itemsContainer");
            if (containerProp != null)
            {
                containerProp.objectReferenceValue = itemsRt;
            }

            invSo.ApplyModifiedPropertiesWithoutUndo();
            return invView;
        }

        private static BoardView CreateStubBoardPlaceholder(Transform parent)
        {
            GameObject boardGo = new GameObject("BoardViewPlaceholder", typeof(RectTransform));
            boardGo.transform.SetParent(parent, false);
            return boardGo.AddComponent<BoardView>();
        }

        private static void WireGearEngineViewReferences(GearEngineView gearView, SimulationControlView simView, GearInventoryView invView, BoardView boardView)
        {
            SerializedObject gvSo = new SerializedObject(gearView);
            SetReference(gvSo, "simControlView", simView);
            SetReference(gvSo, "inventoryView", invView);
            SetReference(gvSo, "boardView", boardView);
            gvSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SaveAndDestroyStubRoot(GameObject root)
        {
            PrefabUtility.SaveAsPrefabAsset(root, StubPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void EnsureStubIsAddressable()
        {
            try
            {
                if (!TryGetAddressableSettings(out AddressableAssetSettings settings))
                {
                    return;
                }

                TryRegisterStubEntry(settings);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GearEngineNavigationAssetGenerator] Addressable registration failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static bool TryGetAddressableSettings(out AddressableAssetSettings settings)
        {
            settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning(
                    "[GearEngineNavigationAssetGenerator] AddressableAssetSettings not found. " +
                    "Open Window > Asset Management > Addressables > Groups, then run this menu again.");
                return false;
            }

            return true;
        }

        private static void TryRegisterStubEntry(AddressableAssetSettings settings)
        {
            string guid = AssetDatabase.AssetPathToGUID(StubPrefabPath);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning($"[GearEngineNavigationAssetGenerator] Stub prefab has no GUID yet ({StubPrefabPath}).");
                return;
            }

            if (settings.FindAssetEntry(guid) != null)
            {
                return;
            }

            CreateStubAddressableEntry(settings, guid);
        }

        private static void CreateStubAddressableEntry(AddressableAssetSettings settings, string guid)
        {
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);
            if (entry == null)
            {
                Debug.LogError("[GearEngineNavigationAssetGenerator] CreateOrMoveEntry returned null for navigation stub.");
                return;
            }

            entry.address = StubPrefabPath;
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
            ViewConfig existing = AssetDatabase.LoadAssetAtPath<ViewConfig>(ViewConfigPath);
            if (existing != null)
            {
                AssignStubAssetReference(existing);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            ViewConfig viewConfig = ScriptableObject.CreateInstance<ViewConfig>();
            AssetDatabase.CreateAsset(viewConfig, ViewConfigPath);
            AssignStubAssetReference(viewConfig);
            EditorUtility.SetDirty(viewConfig);
            return viewConfig;
        }

        private static void AssignStubAssetReference(ViewConfig viewConfig)
        {
            string guid = RequireStubPrefabGuid();
            SerializedObject so = new SerializedObject(viewConfig);
            SerializedProperty assetProp = so.FindProperty("asset") ?? so.FindProperty("viewAsset");
            if (assetProp == null)
            {
                throw new System.InvalidOperationException("ViewConfig: could not find serialized asset field.");
            }

            ApplyGuidToSerializedAsset(assetProp, guid);
            so.ApplyModifiedPropertiesWithoutUndo();
            viewConfig.SetType(typeof(GearEngineView));
            EditorUtility.SetDirty(viewConfig);
        }

        private static string RequireStubPrefabGuid()
        {
            string guid = AssetDatabase.AssetPathToGUID(StubPrefabPath);
            if (string.IsNullOrEmpty(guid))
            {
                throw new System.InvalidOperationException($"Stub prefab not found at {StubPrefabPath}.");
            }

            return guid;
        }

        private static void ApplyGuidToSerializedAsset(SerializedProperty assetProp, string guid)
        {
            SerializedProperty guidProp = assetProp.FindPropertyRelative("m_AssetGUID");
            if (guidProp != null)
            {
                guidProp.stringValue = guid;
            }
        }

        private static void RegisterViewConfigOnNavigationSettings(ViewConfig viewConfig)
        {
            NavigationSettings settings = AssetDatabase.LoadAssetAtPath<NavigationSettings>(NavigationSettingsPath);
            if (settings == null)
            {
                Debug.LogError($"[GearEngine] Missing Navigation Settings at {NavigationSettingsPath}. Create it in the editor (Scaffold/Core/Settings/Navigation).");
                return;
            }

            TryAppendViewConfigToNavigation(settings, viewConfig);
        }

        private static void TryAppendViewConfigToNavigation(NavigationSettings settings, ViewConfig viewConfig)
        {
            SerializedObject so = new SerializedObject(settings);
            SerializedProperty screens = so.FindProperty("screens");
            if (screens == null)
            {
                return;
            }

            if (!NavigationScreensContainView(screens, viewConfig))
            {
                AppendViewConfigToScreens(screens, viewConfig);
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(settings);
        }

        private static bool NavigationScreensContainView(SerializedProperty screens, ViewConfig viewConfig)
        {
            for (int i = 0; i < screens.arraySize; i++)
            {
                if (screens.GetArrayElementAtIndex(i).objectReferenceValue == viewConfig)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AppendViewConfigToScreens(SerializedProperty screens, ViewConfig viewConfig)
        {
            int idx = screens.arraySize;
            screens.InsertArrayElementAtIndex(idx);
            screens.GetArrayElementAtIndex(idx).objectReferenceValue = viewConfig;
        }
    }
}
