using System;
using System.Linq;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Presentation.UI.Input;
using Scaffold;
using Scaffold.EditorUtils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GearEngine.GearEngine.Editor
{
    [CustomEditor(typeof(InvokeActionCommand))]
    public sealed class InvokeActionCommandEditor : CommandEditor
    {
        private ReorderableList _actionsList;
        private SerializedProperty _actionsProperty;
        private SerializedProperty _actionEnabledProperty;

        public override void OnEnable()
        {
            base.OnEnable();
            if (target == null)
            {
                return;
            }

            _actionsProperty = serializedObject.FindProperty("actions");
            _actionEnabledProperty = serializedObject.FindProperty("actionEnabled");
            SynchronizeEnabledStates();
            CreateActionsList();
        }

        public override void DrawCommandGUI()
        {
            serializedObject.Update();
            SynchronizeEnabledStates();
            SynchronizeSelectedAction();

            if (RequiresExecutionMethod())
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("executionMethod"));
                EditorGUILayout.Space();
            }
            _actionsList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        protected override string GetCommandDisplayName(Command command, CommandInfoAttribute commandInfo)
        {
            var invokeAction = command as InvokeActionCommand;
            if (invokeAction != null && invokeAction.actions.Count == 1 && invokeAction.actions[0] != null)
            {
                return InvokeActionEditorUtility.GetDisplayName(invokeAction.actions[0]);
            }

            return base.GetCommandDisplayName(command, commandInfo);
        }

        private void CreateActionsList()
        {
            _actionsList = new ReorderableList(serializedObject, _actionsProperty, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Actions"),
                drawElementCallback = DrawActionElement,
                elementHeightCallback = GetActionElementHeight,
                onAddDropdownCallback = ShowAddActionMenu,
                onRemoveCallback = RemoveAction,
                onReorderCallbackWithDetails = ReorderEnabledStates,
                onSelectCallback = SelectAction,
            };
        }

        private void DrawActionElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var actionProperty = _actionsProperty.GetArrayElementAtIndex(index);
            var enabledProperty = _actionEnabledProperty.GetArrayElementAtIndex(index);
            const float ToggleWidth = 20f;
            var actionRect = new Rect(rect.x, rect.y, rect.width - ToggleWidth, rect.height);
            var toggleRect = new Rect(actionRect.xMax, rect.y, ToggleWidth, EditorGUIUtility.singleLineHeight);
            enabledProperty.boolValue = EditorGUI.Toggle(toggleRect, enabledProperty.boolValue);
            if (actionProperty.managedReferenceValue == null)
            {
                if (GUI.Button(actionRect, "Select Action", EditorStyles.popup))
                {
                    ShowActionMenu(actionRect, index);
                }
                return;
            }

            var action = actionProperty.managedReferenceValue as IAction;
            var actionLabel = new GUIContent(InvokeActionEditorUtility.GetDisplayName(action));
            EditorGUI.PropertyField(actionRect, actionProperty, actionLabel, true);
        }

        private float GetActionElementHeight(int index)
        {
            var actionProperty = _actionsProperty.GetArrayElementAtIndex(index);
            return actionProperty.managedReferenceValue == null
                ? EditorGUIUtility.singleLineHeight
                : EditorGUI.GetPropertyHeight(actionProperty, true) + EditorGUIUtility.standardVerticalSpacing;
        }

        private void ShowAddActionMenu(Rect rect, ReorderableList list)
        {
            ShowActionMenu(rect, _actionsProperty.arraySize);
        }

        private void ShowActionMenu(Rect rect, int insertIndex)
        {
            var menu = new GenericMenu();
            var actionTypes = TypeCache.GetTypesDerivedFrom<IAction>()
                .Where(type => type.IsClass && type.IsSerializable && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(type => type.FullName)
                .ToList();

            foreach (var actionType in actionTypes)
            {
                var capturedType = actionType;
                menu.AddItem(new GUIContent(capturedType.FullName), false, () => AddAction(capturedType, insertIndex));
            }

            if (actionTypes.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No IAction implementations available"));
            }

            menu.DropDown(rect);
        }

        private void AddAction(Type actionType, int insertIndex)
        {
            Undo.RecordObject(target, "Add Invoke Action");
            serializedObject.Update();

            _actionsProperty.InsertArrayElementAtIndex(insertIndex);
            _actionEnabledProperty.InsertArrayElementAtIndex(insertIndex);
            _actionEnabledProperty.GetArrayElementAtIndex(insertIndex).boolValue = true;
            var actionProperty = _actionsProperty.GetArrayElementAtIndex(insertIndex);
            actionProperty.managedReferenceValue = Activator.CreateInstance(actionType);
            actionProperty.isExpanded = true;
            if (_actionsProperty.arraySize > 1)
            {
                serializedObject.FindProperty("displayAsGroup").boolValue = true;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            InvokeActionEditorSelection.Select(target as InvokeActionCommand, insertIndex);
        }

        private void RemoveAction(ReorderableList list)
        {
            if (list.index < 0 || list.index >= _actionsProperty.arraySize)
            {
                return;
            }

            Undo.RecordObject(target, "Remove Invoke Action");
            if (_actionsProperty.arraySize > 1)
            {
                serializedObject.FindProperty("displayAsGroup").boolValue = true;
            }
            _actionsProperty.DeleteArrayElementAtIndex(list.index);
            _actionEnabledProperty.DeleteArrayElementAtIndex(list.index);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            SelectClosestAction(list.index);
        }

        private void ReorderEnabledStates(ReorderableList list, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= _actionEnabledProperty.arraySize ||
                newIndex < 0 || newIndex >= _actionEnabledProperty.arraySize)
            {
                return;
            }

            bool enabled = _actionEnabledProperty.GetArrayElementAtIndex(oldIndex).boolValue;
            _actionEnabledProperty.DeleteArrayElementAtIndex(oldIndex);
            _actionEnabledProperty.InsertArrayElementAtIndex(newIndex);
            _actionEnabledProperty.GetArrayElementAtIndex(newIndex).boolValue = enabled;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            InvokeActionEditorSelection.Select(target as InvokeActionCommand, newIndex);
        }

        private void SelectAction(ReorderableList list)
        {
            InvokeActionEditorSelection.Select(target as InvokeActionCommand, list.index);
        }

        private void SelectClosestAction(int removedIndex)
        {
            int closestIndex = Mathf.Min(removedIndex, _actionsProperty.arraySize - 1);
            _actionsList.index = closestIndex;
            if (closestIndex >= 0)
            {
                InvokeActionEditorSelection.Select(target as InvokeActionCommand, closestIndex);
                return;
            }

            InvokeActionEditorSelection.Clear(target as InvokeActionCommand);
        }

        private void SynchronizeSelectedAction()
        {
            int selectedIndex = InvokeActionEditorSelection.GetSelectedIndex(target as InvokeActionCommand);
            if (selectedIndex < 0 || selectedIndex >= _actionsProperty.arraySize)
            {
                return;
            }

            _actionsList.index = selectedIndex;
            _actionsProperty.GetArrayElementAtIndex(selectedIndex).isExpanded = true;
        }

        private void SynchronizeEnabledStates()
        {
            while (_actionEnabledProperty.arraySize < _actionsProperty.arraySize)
            {
                _actionEnabledProperty.InsertArrayElementAtIndex(_actionEnabledProperty.arraySize);
                _actionEnabledProperty.GetArrayElementAtIndex(_actionEnabledProperty.arraySize - 1).boolValue = true;
            }

            while (_actionEnabledProperty.arraySize > _actionsProperty.arraySize)
            {
                _actionEnabledProperty.DeleteArrayElementAtIndex(_actionEnabledProperty.arraySize - 1);
            }
        }

        private bool RequiresExecutionMethod()
        {
            if (_actionsProperty.arraySize > 1)
            {
                return true;
            }

            var invokeAction = target as InvokeActionCommand;
            return invokeAction != null &&
                   invokeAction.actions.Count > 0 &&
                   invokeAction.actions[0] is ActionBase action &&
                   action.OpenBlock();
        }
    }
}
