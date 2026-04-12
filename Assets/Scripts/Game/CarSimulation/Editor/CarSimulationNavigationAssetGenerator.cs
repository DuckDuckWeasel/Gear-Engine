using System.IO;
using Scaffold.Navigation;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.CarSimulation.Editor
{
    /// <summary>
    /// Creates the navigation stub prefab, <see cref="ViewConfig"/>, and registers it on <c>Assets/Data/Navigation/Navigation Settings.asset</c>.
    /// </summary>
    public static class CarSimulationNavigationAssetGenerator
    {
        public const string StubPrefabPath = "Assets/Game/CarSimulation/Prefabs/CarTrackView_NavigationStub.prefab";
        public const string ViewConfigPath = "Assets/Game/CarSimulation/Configs/CarTrackViewConfig.asset";
        public const string NavigationSettingsPath = "Assets/Data/Navigation/Navigation Settings.asset";

        [MenuItem("Game/Car Simulation/Generate Navigation Assets")]
        public static void Generate()
        {
            try
            {
                EnsurePrefabFolder();
                EnsureConfigsFolder();
                EnsureStubPrefab();
                EnsureStubIsAddressable();
                ViewConfig viewConfig = EnsureViewConfig();
                RegisterViewConfigOnNavigationSettings(viewConfig);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"<color=#33ff33>[CarSimulation]</color> Navigation assets ready. ViewConfig: {ViewConfigPath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CarSimulationNavigationAssetGenerator] {ex.Message}\n{ex.StackTrace}");
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

        private static void EnsureConfigsFolder()
        {
            string dir = Path.GetDirectoryName(ViewConfigPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private static void EnsureStubPrefab()
        {
            if (File.Exists(StubPrefabPath))
            {
                GameObject contents = PrefabUtility.LoadPrefabContents(StubPrefabPath);
                try
                {
                    if (contents.GetComponentInChildren<CarTrackTestView>(true) != null)
                    {
                        return;
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(contents);
                }

                AssetDatabase.DeleteAsset(StubPrefabPath);
            }

            var root = new GameObject("CarTrackViewStub");
            root.AddComponent<CarTrackTestView>();

            var trackGo = new GameObject("Track");
            trackGo.transform.SetParent(root.transform, false);
            var container = trackGo.AddComponent<SplineContainer>();
            var track = trackGo.AddComponent<Track>();

            var pathGo = new GameObject("Path");
            pathGo.transform.SetParent(trackGo.transform, false);
            var extrude = pathGo.AddComponent<SplineExtrude>();

            var trackSo = new SerializedObject(track);
            trackSo.FindProperty("splineContainer").objectReferenceValue = container;
            trackSo.FindProperty("splineExtrude").objectReferenceValue = extrude;
            trackSo.ApplyModifiedPropertiesWithoutUndo();

            var shellSo = new SerializedObject(root.GetComponent<CarTrackTestView>());
            shellSo.FindProperty("track").objectReferenceValue = track;
            shellSo.ApplyModifiedPropertiesWithoutUndo();

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
                        "[CarSimulationNavigationAssetGenerator] AddressableAssetSettings not found. " +
                        "Open Window > Asset Management > Addressables > Groups, then run this menu again.");
                    return;
                }

                string guid = AssetDatabase.AssetPathToGUID(StubPrefabPath);
                if (string.IsNullOrEmpty(guid))
                {
                    Debug.LogWarning($"[CarSimulationNavigationAssetGenerator] Stub prefab has no GUID yet ({StubPrefabPath}).");
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
                    Debug.LogError("[CarSimulationNavigationAssetGenerator] CreateOrMoveEntry returned null for navigation stub.");
                    return;
                }

                entry.address = StubPrefabPath;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CarSimulationNavigationAssetGenerator] Addressable registration failed: {ex.Message}\n{ex.StackTrace}");
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
            viewConfig.SetType(typeof(CarTrackTestView));
            EditorUtility.SetDirty(viewConfig);
        }

        private static void RegisterViewConfigOnNavigationSettings(ViewConfig viewConfig)
        {
            var settings = AssetDatabase.LoadAssetAtPath<NavigationSettings>(NavigationSettingsPath);
            if (settings == null)
            {
                Debug.LogError($"[CarSimulation] Missing Navigation Settings at {NavigationSettingsPath}. Create it in the editor (Scaffold/Core/Settings/Navigation).");
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
