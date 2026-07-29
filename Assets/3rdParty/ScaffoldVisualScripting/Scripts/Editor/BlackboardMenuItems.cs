using Scaffold.VisualScripting.Unity;
using UnityEditor;
using UnityEngine;

namespace Scaffold.EditorUtils
{
    public static class BlackboardMenuItems
    {
        [MenuItem("Tools/Scaffold/Create/Blackboard", false, 0)]
        private static void CreateBlackboard()
        {
            GameObject instance = SpawnPrefab("Blackboard");
            if (instance == null)
            {
                Debug.LogError(
                    "[BlackboardMenuItems] The Blackboard prefab could not be loaded.");
                return;
            }

            if (instance.GetComponent<BlackboardBehaviour>() == null)
            {
                Debug.LogError(
                    "[BlackboardMenuItems] The Blackboard prefab has no BlackboardBehaviour.");
            }
        }

        public static GameObject SpawnPrefab(string prefabName)
        {
            GameObject prefab = Resources.Load<GameObject>(
                $"Prefabs/{prefabName}");
            if (prefab == null)
            {
                return null;
            }

            GameObject instance = Object.Instantiate(prefab);
            instance.name = prefab.name;
            PositionAtSceneView(instance);
            Selection.activeGameObject = instance;
            Undo.RegisterCreatedObjectUndo(instance, "Create Scaffold Object");
            return instance;
        }

        private static void PositionAtSceneView(GameObject instance)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null || sceneView.camera == null)
            {
                return;
            }

            Vector3 position = sceneView.camera.ViewportToWorldPoint(
                new Vector3(0.5f, 0.5f, 10f));
            position.z = 0f;
            instance.transform.position = position;
        }
    }
}
