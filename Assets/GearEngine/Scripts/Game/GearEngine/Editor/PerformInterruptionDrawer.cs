using GearEngine.GearEngine.Presentation.UI.Input;
using Scaffold;
using Scaffold.EditorUtils;
using UnityEditor;
using UnityEngine;
using IAction = global::GearEngine.Core.Actions.IAction;

namespace GearEngine.GearEngine.Editor
{
    [CustomPropertyDrawer(typeof(PerformInterruption))]
    public sealed class PerformInterruptionDrawer : PropertyDrawer
    {
        private const float k_HelpBoxHeight = 38f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Rect lineRect = GetNextLine(position);
            property.isExpanded = EditorGUI.Foldout(lineRect, property.isExpanded, label, true);
            if (!property.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;
            SerializedProperty targetCommandProperty = property.FindPropertyRelative("targetCommand");
            SerializedProperty targetActionIdsProperty = property.FindPropertyRelative("targetActionIds");
            SerializedProperty interruptSuccessProperty = property.FindPropertyRelative("interruptSuccess");

            lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(
                lineRect,
                targetCommandProperty,
                new GUIContent("Target Invoke Action", "Leave empty to target the current Invoke Action."));

            lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float statusHeight = EditorGUI.GetPropertyHeight(interruptSuccessProperty, true);
            lineRect.height = statusHeight;
            EditorGUI.PropertyField(
                lineRect,
                interruptSuccessProperty,
                new GUIContent("Interrupt Success"),
                true);

            lineRect.y += statusHeight + EditorGUIUtility.standardVerticalSpacing;
            lineRect.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(lineRect, "Tasks", EditorStyles.boldLabel);

            InvokeActionCommand targetCommand = ResolveTargetCommand(property, targetCommandProperty);
            if (targetCommand == null)
            {
                lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                lineRect.height = k_HelpBoxHeight;
                EditorGUI.HelpBox(
                    lineRect,
                    "Select a target Invoke Action, or edit this action inside an Invoke Action.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
                EditorGUI.EndProperty();
                return;
            }

            if (targetCommand.EnsureActionMetadata())
            {
                EditorUtility.SetDirty(targetCommand);
            }

            object currentAction = property.managedReferenceValue;
            for (int actionIndex = 0; actionIndex < targetCommand.actions.Count; actionIndex++)
            {
                if (ReferenceEquals(targetCommand.actions[actionIndex], currentAction))
                {
                    continue;
                }

                string actionId = targetCommand.GetActionId(actionIndex);
                bool isSelected = ContainsId(targetActionIdsProperty, actionId);
                lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                string actionName = InvokeActionEditorUtility.GetDisplayName(targetCommand.actions[actionIndex]);
                bool shouldSelect = EditorGUI.ToggleLeft(lineRect, actionName, isSelected);
                if (shouldSelect != isSelected)
                {
                    SetSelected(targetActionIdsProperty, actionId, shouldSelect);
                }
            }

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded)
            {
                return lineHeight;
            }

            SerializedProperty targetCommandProperty = property.FindPropertyRelative("targetCommand");
            SerializedProperty interruptSuccessProperty = property.FindPropertyRelative("interruptSuccess");
            InvokeActionCommand targetCommand = ResolveTargetCommand(property, targetCommandProperty);
            float height = lineHeight * 3f;
            height += EditorGUI.GetPropertyHeight(interruptSuccessProperty, true);
            height += EditorGUIUtility.standardVerticalSpacing * 3f;
            if (targetCommand == null)
            {
                return height + k_HelpBoxHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            object currentAction = property.managedReferenceValue;
            int selectableActionCount = 0;
            foreach (IAction action in targetCommand.actions)
            {
                if (!ReferenceEquals(action, currentAction))
                {
                    selectableActionCount++;
                }
            }

            return height +
                   (selectableActionCount *
                    (lineHeight + EditorGUIUtility.standardVerticalSpacing));
        }

        private static InvokeActionCommand ResolveTargetCommand(
            SerializedProperty property,
            SerializedProperty targetCommandProperty)
        {
            InvokeActionCommand explicitTarget = targetCommandProperty.objectReferenceValue as InvokeActionCommand;
            return explicitTarget ?? property.serializedObject.targetObject as InvokeActionCommand;
        }

        private static bool ContainsId(SerializedProperty targetActionIdsProperty, string actionId)
        {
            for (int targetIndex = 0; targetIndex < targetActionIdsProperty.arraySize; targetIndex++)
            {
                if (targetActionIdsProperty.GetArrayElementAtIndex(targetIndex).stringValue == actionId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetSelected(
            SerializedProperty targetActionIdsProperty,
            string actionId,
            bool selected)
        {
            for (int targetIndex = 0; targetIndex < targetActionIdsProperty.arraySize; targetIndex++)
            {
                if (targetActionIdsProperty.GetArrayElementAtIndex(targetIndex).stringValue != actionId)
                {
                    continue;
                }

                if (!selected)
                {
                    targetActionIdsProperty.DeleteArrayElementAtIndex(targetIndex);
                }

                return;
            }

            if (!selected)
            {
                return;
            }

            int newTargetIndex = targetActionIdsProperty.arraySize;
            targetActionIdsProperty.InsertArrayElementAtIndex(newTargetIndex);
            targetActionIdsProperty.GetArrayElementAtIndex(newTargetIndex).stringValue = actionId;
        }

        private static Rect GetNextLine(Rect position)
        {
            return new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);
        }
    }
}
