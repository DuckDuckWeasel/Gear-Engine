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
            SerializedProperty reference = serializedObject.FindProperty("definitionReference");
            EditorGUILayout.PropertyField(reference);
            DrawVariableSource(reference);
            DrawRuntimeState();
            serializedObject.ApplyModifiedProperties();
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
                EditorGUILayout.LabelField("Runtime ID", behaviour.Runtime.RuntimeInstanceId.ToString());
                EditorGUILayout.LabelField("Started", behaviour.Runtime.HasStarted.ToString());
                EditorGUILayout.LabelField("Enabled", behaviour.Runtime.IsEnabled.ToString());
            }
        }

        private void DrawOpenButton()
        {
            if (GUILayout.Button("Open Blackboard Window"))
            {
                BlackboardDefinitionWindowLauncher.Open(target);
            }
        }
    }
}
