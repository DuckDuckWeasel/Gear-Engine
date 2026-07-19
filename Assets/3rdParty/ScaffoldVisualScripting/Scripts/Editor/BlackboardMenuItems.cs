
﻿using UnityEngine;
using UnityEditor;

namespace Scaffold.EditorUtils
{
    public class BlackboardMenuItems
    {
        [MenuItem("Tools/Scaffold/Create/Blackboard", false, 0)]
        static void CreateBlackboard()
        {
            GameObject go = SpawnPrefab("Blackboard");
            go.transform.position = Vector3.zero;

            // This is the latest version of Blackboard, so no need to update.
            var blackboard = go.GetComponent<Blackboard>();
            if (blackboard != null)
            {
                blackboard.Version = ScaffoldConstants.CurrentVersion;
            }

            // Only the first created Blackboard in the scene should have a default GameStarted block
            if (GameObject.FindObjectsOfType<Blackboard>().Length > 1)
            {
                var block = go.GetComponent<Block>();
                GameObject.DestroyImmediate(block._EventHandler);
                block._EventHandler = null;
            }
        }

        [MenuItem("Tools/Scaffold/Create/Scaffold Logo", false, 1000)]
        static void CreateScaffoldLogo()
        {
            SpawnPrefab("ScaffoldLogo");
        }

        [MenuItem("Tools/Scaffold/Utilities/Export Scaffold Package")]
        static void ExportScaffoldPackageFull()
        {
            ExportScaffoldPackage( new string[] {"Assets/Scaffold", "Assets/ScaffoldExamples" });
        }

        [MenuItem("Tools/Scaffold/Utilities/Export Scaffold Package - Lite")]
        static void ExportScaffoldPackageLite()
        {
            ExportScaffoldPackage(new string[] { "Assets/Scaffold" });
        }

        static void ExportScaffoldPackage(string[] folders)
        {
            string path = EditorUtility.SaveFilePanel("Export Scaffold Package", "", "Scaffold", "unitypackage");
            if (path.Length == 0)
            {
                return;
            }

            AssetDatabase.ExportPackage(folders, path, ExportPackageOptions.Recurse);
        }

        public static GameObject SpawnPrefab(string prefabName)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + prefabName);
            if (prefab == null)
            {
                return null;
            }

            GameObject go = GameObject.Instantiate(prefab) as GameObject;
            go.name = prefab.name;

            SceneView view = SceneView.lastActiveSceneView;
            if (view != null)
            {
                Camera sceneCam = view.camera;
                Vector3 pos = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
                pos.z = 0f;
                go.transform.position = pos;
            }

            Selection.activeGameObject = go;
            
            Undo.RegisterCreatedObjectUndo(go, "Create Object");

            return go;
        }
    }
}