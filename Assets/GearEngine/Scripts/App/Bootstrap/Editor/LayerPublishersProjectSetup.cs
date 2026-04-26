using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GearEngine.App.Bootstrap.Editor
{
    /// <summary>Fills empty inline layer publisher lists on bootstrap roots in build settings scenes.</summary>
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
            FillEmptyInlinePublishersInBuildScenes();
            PlayerPrefs.SetInt(LayerPublishersBuildUtility.PlayerPrefInlineSeeded, 1);
            PlayerPrefs.Save();
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
            if (PlayerPrefs.GetInt(LayerPublishersBuildUtility.PlayerPrefInlineSeeded, 0) != 0)
            {
                return;
            }

            FillEmptyInlinePublishersInBuildScenes();
            PlayerPrefs.SetInt(LayerPublishersBuildUtility.PlayerPrefInlineSeeded, 1);
            PlayerPrefs.Save();
        }

        private static void FillEmptyInlinePublishersInBuildScenes()
        {
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
                    SerializedProperty listProp = so.FindProperty("layerAssetPublishers");
                    if (listProp == null || listProp.arraySize > 0)
                    {
                        continue;
                    }
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
        }
    }
}
