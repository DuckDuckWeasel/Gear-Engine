using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GearEngine.App.Bootstrap.Editor
{
    /// <summary>Creates/updates the default layer publishers profile and links it to campaign/meta bootstrap roots in build settings.</summary>
    public static class LayerPublishersProjectSetup
    {
        private const int DelayCallBudget = 6;

        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            EditorApplication.delayCall += OnDelayCall;
        }

        [MenuItem("GearEngine/Bootstrap/Create or Update Layer Publishers & Assign to Scenes", priority = 0)]
        public static void CreateUpdateAndAssignFromMenu()
        {
            CreateOrUpdateProfileAsset(overwriteDefinitions: true);
            AssignProfileToOpenBuildScenes(force: true);
        }

        private static int _delayCount;

        private static void OnDelayCall()
        {
            if (EditorApplication.isCompiling)
            {
                if (_delayCount++ < DelayCallBudget)
                {
                    EditorApplication.delayCall += OnDelayCall;
                }

                return;
            }

            _delayCount = 0;
            bool hasProfile = File.Exists(LayerPublishersBuildUtility.DefaultProfileAssetPath) &&
                AssetDatabase.LoadAssetAtPath<LayerBootstrapPublishersProfile>(LayerPublishersBuildUtility.DefaultProfileAssetPath) != null;
            if (!hasProfile)
            {
                CreateOrUpdateProfileAsset(overwriteDefinitions: true);
            }
            else
            {
                var p = AssetDatabase.LoadAssetAtPath<LayerBootstrapPublishersProfile>(LayerPublishersBuildUtility.DefaultProfileAssetPath);
                if (p != null && (p.AssetPublisherDefinitions == null || p.AssetPublisherDefinitions.Count == 0))
                {
                    CreateOrUpdateProfileAsset(overwriteDefinitions: true);
                }
            }

            if (PlayerPrefs.GetInt(LayerPublishersBuildUtility.PlayerPrefScenesLinked, 0) == 0)
            {
                AssignProfileToOpenBuildScenes(force: false);
            }
        }

        private static void CreateOrUpdateProfileAsset(bool overwriteDefinitions)
        {
            string dir = Path.GetDirectoryName(LayerPublishersBuildUtility.DefaultProfileAssetPath);
            if (dir == null)
            {
                return;
            }

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var profile = AssetDatabase.LoadAssetAtPath<LayerBootstrapPublishersProfile>(LayerPublishersBuildUtility.DefaultProfileAssetPath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<LayerBootstrapPublishersProfile>();
                AssetDatabase.CreateAsset(profile, LayerPublishersBuildUtility.DefaultProfileAssetPath);
            }

            if (overwriteDefinitions || profile.AssetPublisherDefinitions == null || profile.AssetPublisherDefinitions.Count == 0)
            {
                profile.ReplaceDefinitionsForEditor(LayerPublishersBuildUtility.CreateDefaultCampaignDefinitions());
                EditorUtility.SetDirty(profile);
            }

            AssetDatabase.SaveAssetIfDirty(profile);
            AssetDatabase.Refresh();
        }

        private static void AssignProfileToOpenBuildScenes(bool force)
        {
            LayerBootstrapPublishersProfile prof = AssetDatabase.LoadAssetAtPath<LayerBootstrapPublishersProfile>(LayerPublishersBuildUtility.DefaultProfileAssetPath);
            if (prof == null)
            {
                return;
            }

            if (!force && PlayerPrefs.GetInt(LayerPublishersBuildUtility.PlayerPrefScenesLinked, 0) != 0)
            {
                return;
            }

            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
            if (scenes.Length == 0)
            {
                return;
            }

            Scene startScene = SceneManager.GetActiveScene();
            string startPath = startScene.IsValid() ? startScene.path : string.Empty;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i] == null || string.IsNullOrEmpty(scenes[i].path))
                {
                    continue;
                }

                string scenePath = scenes[i].path.Replace("\\", "/", StringComparison.Ordinal);
                if (!scenePath.EndsWith("/Main Scene.unity", StringComparison.OrdinalIgnoreCase) &&
                    !scenePath.EndsWith("/Meta.unity", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var scn = EditorSceneManager.OpenScene(scenes[i].path, OpenSceneMode.Single);
                GearAppFlowRoot[] roots = UnityEngine.Object.FindObjectsByType<GearAppFlowRoot>(FindObjectsSortMode.None);
                for (int r = 0; r < roots.Length; r++)
                {
                    SerializedObject so = new SerializedObject(roots[r]);
                    SerializedProperty prop = so.FindProperty("layerPublishersProfile");
                    if (prop == null)
                    {
                        continue;
                    }

                    prop.objectReferenceValue = prof;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(roots[r]);
                }

                if (scn.isDirty)
                {
                    EditorSceneManager.SaveScene(scn);
                }
            }

            if (!string.IsNullOrEmpty(startPath))
            {
                EditorSceneManager.OpenScene(startPath, OpenSceneMode.Single);
            }

            PlayerPrefs.SetInt(LayerPublishersBuildUtility.PlayerPrefScenesLinked, 1);
            PlayerPrefs.Save();
        }
    }
}
