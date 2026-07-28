using Scaffold.VisualScripting.Authoring;
using Scaffold.VisualScripting.Unity;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    public static class BlackboardAuthoringMenuItems
    {
        [MenuItem("GameObject/Scaffold/Blackboard", false, 10)]
        public static void CreateBlackboardBehaviour()
        {
            BlackboardBehaviour behaviour = CreateBlackboardHost();
            Selection.activeObject = behaviour;
            BlackboardDefinitionWindowLauncher.Open(behaviour);
        }

        /// <summary>
        /// Creates a Blackboard host with its required runtime scope.
        /// </summary>
        public static BlackboardBehaviour CreateBlackboardHost()
        {
            GameObject gameObject = new GameObject("Blackboard");
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Blackboard");
            Undo.AddComponent<BlackboardLifetimeScope>(gameObject);
            return Undo.AddComponent<BlackboardBehaviour>(gameObject);
        }

        [MenuItem("Assets/Create/Scaffold/Visual Scripting/Blackboard Definition")]
        public static void CreateDefinitionAsset()
        {
            BlackboardDefinitionAsset asset = ScriptableObject.CreateInstance<BlackboardDefinitionAsset>();
            string path = AssetDatabase.GenerateUniqueAssetPath("Assets/BlackboardDefinition.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
            BlackboardDefinitionWindowLauncher.Open(asset);
        }
    }
}
