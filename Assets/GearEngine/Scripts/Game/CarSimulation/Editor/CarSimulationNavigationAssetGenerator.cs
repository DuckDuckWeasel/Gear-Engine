using System.IO;
using GearEngine.CarSimulation.Presentation;
using TrackViewComponent = GearEngine.CarSimulation.Tracks.Track;
using Scaffold.Navigation;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Editor
{
    /// <summary>Sample: Creates the navigation stub prefab, <see cref="ViewConfig"/>, and registers it on Navigation Settings.</summary>
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
                RunNavigationPipeline();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CarSimulationNavigationAssetGenerator] {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void RunNavigationPipeline()
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
            if (File.Exists(StubPrefabPath) && StubPrefabHasCarTrackView())
            {
                return;
            }

            if (File.Exists(StubPrefabPath))
            {
                AssetDatabase.DeleteAsset(StubPrefabPath);
            }

            BuildAndSaveStubPrefab();
        }

        private static bool StubPrefabHasCarTrackView()
        {
            GameObject contents = PrefabUtility.LoadPrefabContents(StubPrefabPath);
            try
            {
                return contents.GetComponentInChildren<CarTrackTestView>(true) != null;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        private static void BuildAndSaveStubPrefab()
        {
            var root = new GameObject("CarTrackViewStub");
            root.AddComponent<CarTrackTestView>();

            var trackGo = new GameObject("Track");
            trackGo.transform.SetParent(root.transform, false);
            var container = trackGo.AddComponent<SplineContainer>();
            var track = trackGo.AddComponent<TrackViewComponent>();

            var pathGo = new GameObject("Path");
            pathGo.transform.SetParent(trackGo.transform, false);
            pathGo.AddComponent<SplineExtrude>();

            WireTrackSerializedFields(track, container, pathGo.GetComponent<SplineExtrude>());
            WireShellTrackReference(root, track);

            PrefabUtility.SaveAsPrefabAsset(root, StubPrefabPath);
            Object.DestroyImmediate(root);
        }

        private static void WireTrackSerializedFields(TrackViewComponent track, SplineContainer container, SplineExtrude extrude)
        {
            var trackSo = new SerializedObject(track);
            trackSo.FindProperty("splineContainer").objectReferenceValue = container;
            trackSo.FindProperty("splineExtrude").objectReferenceValue = extrude;
            trackSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireShellTrackReference(GameObject root, TrackViewComponent track)
        {
            var shellSo = new SerializedObject(root.GetComponent<CarTrackTestView>());
            shellSo.FindProperty("track").objectReferenceValue = track;
            shellSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureStubIsAddressable()
        {
            try
            {
                TryRegisterStubAsAddressable();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CarSimulationNavigationAssetGenerator] Addressable registration failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void TryRegisterStubAsAddressable()
        {
            if (!TryGetAddressableSettings(out AddressableAssetSettings settings))
            {
                return;
            }

            if (!TryGetStubPrefabGuid(out string guid))
            {
                return;
            }

            EnsureStubAddressableEntry(settings, guid);
        }

        private static bool TryGetAddressableSettings(out AddressableAssetSettings settings)
        {
            settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                return true;
            }

            Debug.LogWarning(
                "[CarSimulationNavigationAssetGenerator] AddressableAssetSettings not found. " +
                "Open Window > Asset Management > Addressables > Groups, then run this menu again.");
            return false;
        }

        private static bool TryGetStubPrefabGuid(out string guid)
        {
            guid = AssetDatabase.AssetPathToGUID(StubPrefabPath);
            if (!string.IsNullOrEmpty(guid))
            {
                return true;
            }

            Debug.LogWarning($"[CarSimulationNavigationAssetGenerator] Stub prefab has no GUID yet ({StubPrefabPath}).");
            return false;
        }

        private static void EnsureStubAddressableEntry(AddressableAssetSettings settings, string guid)
        {
            if (settings.FindAssetEntry(guid) != null)
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
            string guid = RequireStubGuid();
            ApplyStubGuidToSerializedViewConfig(viewConfig, guid);
            viewConfig.SetType(typeof(CarTrackTestView));
            EditorUtility.SetDirty(viewConfig);
        }

        private static string RequireStubGuid()
        {
            string guid = AssetDatabase.AssetPathToGUID(StubPrefabPath);
            if (string.IsNullOrEmpty(guid))
            {
                throw new System.InvalidOperationException($"Stub prefab not found at {StubPrefabPath}.");
            }

            return guid;
        }

        private static void ApplyStubGuidToSerializedViewConfig(ViewConfig viewConfig, string guid)
        {
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
        }

        private static void RegisterViewConfigOnNavigationSettings(ViewConfig viewConfig)
        {
            var settings = AssetDatabase.LoadAssetAtPath<NavigationSettings>(NavigationSettingsPath);
            if (settings == null)
            {
                Debug.LogError($"[CarSimulation] Missing Navigation Settings at {NavigationSettingsPath}. Create it in the editor (Scaffold/Core/Settings/Navigation).");
                return;
            }

            if (TryAppendScreenIfMissing(settings, viewConfig))
            {
                EditorUtility.SetDirty(settings);
            }
        }

        private static bool TryAppendScreenIfMissing(NavigationSettings settings, ViewConfig viewConfig)
        {
            var so = new SerializedObject(settings);
            SerializedProperty screens = so.FindProperty("screens");
            if (screens == null)
            {
                return false;
            }

            return SyncScreensProperty(so, screens, viewConfig);
        }

        private static bool SyncScreensProperty(SerializedObject so, SerializedProperty screens, ViewConfig viewConfig)
        {
            if (ArrayContainsReference(screens, viewConfig))
            {
                so.ApplyModifiedPropertiesWithoutUndo();
                return true;
            }

            int idx = screens.arraySize;
            screens.InsertArrayElementAtIndex(idx);
            screens.GetArrayElementAtIndex(idx).objectReferenceValue = viewConfig;
            so.ApplyModifiedPropertiesWithoutUndo();
            return true;
        }

        private static bool ArrayContainsReference(SerializedProperty screens, UnityEngine.Object reference)
        {
            for (int i = 0; i < screens.arraySize; i++)
            {
                if (screens.GetArrayElementAtIndex(i).objectReferenceValue == reference)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
