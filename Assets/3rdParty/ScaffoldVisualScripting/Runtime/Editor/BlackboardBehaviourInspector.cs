using System.Collections.Generic;
using Scaffold.VisualScripting.Authoring;
using Scaffold.VisualScripting.Unity;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    [CustomEditor(typeof(BlackboardBehaviour))]
    public sealed class BlackboardBehaviourInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty reference =
                serializedObject.FindProperty("definitionReference");
            EditorGUILayout.PropertyField(reference);
            DrawVariableSource(reference);
            serializedObject.ApplyModifiedProperties();
            DrawDefinitionStatus();
            DrawRuntimeState();
            DrawOpenButton();
        }

        private void DrawVariableSource(SerializedProperty reference)
        {
            SerializedProperty source = reference.FindPropertyRelative("source");
            if ((BlackboardDefinitionSource)source.enumValueIndex == BlackboardDefinitionSource.BlackboardVariable)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sourceBehaviour"));
            }
        }

        private void DrawRuntimeState()
        {
            BlackboardBehaviour behaviour = target as BlackboardBehaviour;
            if (behaviour != null && behaviour.IsRuntimeAvailable)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "Runtime",
                    EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(
                    "Runtime ID",
                    behaviour.Runtime.RuntimeInstanceId.ToString());
                EditorGUILayout.LabelField(
                    "Started",
                    behaviour.Runtime.HasStarted.ToString());
                EditorGUILayout.LabelField(
                    "Enabled",
                    behaviour.Runtime.IsEnabled.ToString());
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawOpenButton()
        {
            EditorGUILayout.Space();
            if (GUILayout.Button(
                "Open Blackboard Window",
                GUILayout.Height(28f)))
            {
                BlackboardDefinitionWindowLauncher.Open(target);
            }
        }

        private void DrawDefinitionStatus()
        {
            BlackboardBehaviour behaviour = target as BlackboardBehaviour;
            if (behaviour == null)
            {
                return;
            }

            BlackboardDefinition definition = GetVisibleDefinition(behaviour);
            if (definition == null)
            {
                DrawUnavailableDefinitionStatus(behaviour);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Definition",
                EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(
                string.IsNullOrWhiteSpace(definition.Name)
                    ? "Unnamed Blackboard"
                    : definition.Name,
                EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                $"{definition.Blocks.Count} " +
                $"{GetCountLabel(definition.Blocks.Count, "Block")} · " +
                $"{definition.Variables.Count} " +
                GetCountLabel(definition.Variables.Count, "Variable"),
                EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            DrawValidation(definition);
        }

        private BlackboardDefinition GetVisibleDefinition(
            BlackboardBehaviour behaviour)
        {
            BlackboardDefinitionReference reference =
                behaviour.DefinitionReference;
            if (reference.Source == BlackboardDefinitionSource.Direct)
            {
                return reference.DirectDefinition;
            }

            return reference.Source == BlackboardDefinitionSource.ScriptableObject &&
                reference.DefinitionAsset != null
                    ? reference.DefinitionAsset.Definition
                    : null;
        }

        private void DrawUnavailableDefinitionStatus(
            BlackboardBehaviour behaviour)
        {
            string message =
                behaviour.DefinitionReference.Source ==
                BlackboardDefinitionSource.BlackboardVariable
                    ? "This definition is resolved from the source Blackboard at runtime."
                    : "Assign a Blackboard Definition Asset to continue.";
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }

        private void DrawValidation(BlackboardDefinition definition)
        {
            IReadOnlyList<BlackboardValidationIssue> issues =
                new BlackboardDefinitionValidator().Validate(definition);
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Definition is valid.",
                    MessageType.Info);
                return;
            }

            string message = issues.Count == 1
                ? issues[0].ToString()
                : $"{issues.Count} validation issues. {issues[0]}";
            EditorGUILayout.HelpBox(message, MessageType.Error);
        }

        private string GetCountLabel(int count, string singular)
        {
            return count == 1
                ? singular
                : $"{singular}s";
        }
    }
}
