using System.Collections.Generic;
using Scaffold.VisualScripting.Authoring;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    [CustomEditor(typeof(BlackboardDefinitionAsset))]
    public sealed class BlackboardDefinitionAssetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("definition"), true);
            serializedObject.ApplyModifiedProperties();
            DrawValidation();
            DrawButtons();
        }

        private void DrawValidation()
        {
            BlackboardDefinitionAsset asset = target as BlackboardDefinitionAsset;
            IReadOnlyList<BlackboardValidationIssue> issues = new BlackboardDefinitionValidator().Validate(asset.Definition);
            for (int index = 0; index < issues.Count; index++)
            {
                EditorGUILayout.HelpBox(issues[index].ToString(), MessageType.Error);
            }
        }

        private void DrawButtons()
        {
            if (GUILayout.Button("Open Blackboard Window"))
            {
                BlackboardDefinitionWindowLauncher.Open(target);
            }

            if (GUILayout.Button("Duplicate With New Definition IDs"))
            {
                DuplicateAsset();
            }
        }

        private void DuplicateAsset()
        {
            BlackboardDefinitionAsset asset = target as BlackboardDefinitionAsset;
            string path = EditorUtility.SaveFilePanelInProject("Duplicate Blackboard Definition", $"{asset.name}Copy", "asset", "Choose a destination.");
            if (!string.IsNullOrWhiteSpace(path))
            {
                Selection.activeObject = BlackboardDefinitionDuplicationUtility.DuplicateAsset(asset, path);
            }
        }
    }
}
