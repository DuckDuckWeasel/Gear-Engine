using System;
using System.Collections.Generic;
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
        private const float ReorderHandleWidth = 18f;

        private ReorderableList _actionsList;
        private SerializedProperty _actionsProperty;
        private int _lastSynchronizedActionIndex = -1;
        private static readonly List<ActionClipboardEntry> ActionClipboard =
            new List<ActionClipboardEntry>();

        private sealed class ActionClipboardEntry : ScriptableObject
        {
            [SerializeReference] public IAction action;
            [SerializeField] public bool isEnabled;
            [SerializeField] public InvokeActionUtilitySettings utilitySettings;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (target == null)
            {
                return;
            }

            _actionsProperty = serializedObject.FindProperty("actions");
            SynchronizeActionMetadata();
            CreateActionsList();
        }

        public override void DrawCommandGUI()
        {
            serializedObject.Update();
            SynchronizeActionMetadata();
            InvokeActionCommand invokeAction = target as InvokeActionCommand;
            if (ShouldShowActionsList(invokeAction))
            {
                DrawExecutionSettings(invokeAction);
                SynchronizeSelectedAction();
                _actionsList.DoLayoutList();
            }
            else
            {
                DrawStandaloneAction();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawExecutionSettings(InvokeActionCommand invokeAction)
        {
            SerializedProperty executionMethodProperty =
                serializedObject.FindProperty("executionMethod");
            SerializedProperty awaitModeProperty = serializedObject.FindProperty("awaitMode");
            SerializedProperty orderModeProperty = serializedObject.FindProperty("orderMode");
            SerializedProperty avoidRepeatProperty =
                serializedObject.FindProperty("avoidRepeatingLastAction");

            CompositeExecutionMethod executionMethod =
                (CompositeExecutionMethod)executionMethodProperty.enumValueIndex;
            CompositeAwaitMode awaitMode =
                (CompositeAwaitMode)awaitModeProperty.enumValueIndex;
            CompositeOrderMode orderMode =
                (CompositeOrderMode)orderModeProperty.enumValueIndex;
            EditorGUILayout.PropertyField(
                executionMethodProperty,
                new GUIContent(
                    "Execution",
                    CompositeExecutionDescription.GetExecutionTooltip(
                        executionMethod,
                        awaitMode,
                        orderMode)));

            executionMethod = (CompositeExecutionMethod)executionMethodProperty.enumValueIndex;
            if (CompositeExecutionDescription.SupportsAwait(executionMethod))
            {
                awaitMode = (CompositeAwaitMode)awaitModeProperty.enumValueIndex;
                EditorGUILayout.PropertyField(
                    awaitModeProperty,
                    new GUIContent(
                        "Await",
                        CompositeExecutionDescription.GetAwaitTooltip(executionMethod, awaitMode)));
            }
            else if (CompositeExecutionDescription.SupportsOrder(executionMethod))
            {
                orderMode = (CompositeOrderMode)orderModeProperty.enumValueIndex;
                EditorGUILayout.PropertyField(
                    orderModeProperty,
                    new GUIContent(
                        "Order",
                        CompositeExecutionDescription.GetOrderTooltip(executionMethod, orderMode)));
                if (orderMode != CompositeOrderMode.Ordered && invokeAction.actions.Count > 1)
                {
                    EditorGUILayout.PropertyField(
                        avoidRepeatProperty,
                        new GUIContent(
                            "Avoid Repeating Last Action",
                            "Prevents the first Random or Shuffle choice from matching the action that finished the previous execution."));
                }
            }

            EditorGUILayout.Space();
        }

        protected override string GetCommandDisplayName(
            Command command,
            CommandInfoAttribute commandInfo)
        {
            if (TryGetStandaloneAction(command as InvokeActionCommand, out IAction action))
            {
                return InvokeActionEditorUtility.GetDisplayName(action);
            }

            return base.GetCommandDisplayName(command, commandInfo);
        }

        private void CreateActionsList()
        {
            _actionsList = new ReorderableList(serializedObject, _actionsProperty, true, true, true, true)
            {
                drawHeaderCallback = DrawActionsHeader,
                drawElementCallback = DrawActionElement,
                elementHeightCallback = GetActionElementHeight,
                onAddDropdownCallback = ShowAddActionMenu,
                onRemoveCallback = RemoveAction,
                onReorderCallbackWithDetails = ReorderEnabledStates,
                onSelectCallback = SelectAction,
            };
        }

        private void DrawStandaloneAction()
        {
            SerializedProperty actionProperty = _actionsProperty.GetArrayElementAtIndex(0).FindPropertyRelative("action");
            SerializedProperty property = actionProperty.Copy();
            SerializedProperty endProperty = property.GetEndProperty();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren) &&
                   !SerializedProperty.EqualContents(property, endProperty))
            {
                enterChildren = false;
                if (!InvokeActionEditorUtility.IsPropertyVisible(
                        actionProperty.managedReferenceValue as IAction,
                        property.name))
                {
                    continue;
                }

                EditorGUILayout.PropertyField(property, true);
            }
        }

        private static bool ShouldShowActionsList(InvokeActionCommand invokeAction)
        {
            return !TryGetStandaloneAction(invokeAction, out _);
        }

        private static bool TryGetStandaloneAction(
            InvokeActionCommand invokeAction,
            out IAction action)
        {
            action = null;
            if (invokeAction == null ||
                invokeAction.DisplayAsGroup ||
                invokeAction.actions == null ||
                invokeAction.actions.Count != 1)
            {
                return false;
            }

            action = invokeAction.actions[0].action;
            return action != null;
        }

        private void DrawActionElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty wrapperProperty = _actionsProperty.GetArrayElementAtIndex(index);
            SerializedProperty actionProperty = wrapperProperty.FindPropertyRelative("action");
            SerializedProperty enabledProperty = wrapperProperty.FindPropertyRelative("enabled");
            const float ToggleWidth = 20f;
            const float WeightWidth = 58f;
            const float PercentageToggleWidth = 20f;
            bool showWeight = IsRandomOrder();
            float controlsWidth = ToggleWidth +
                                  (showWeight ? WeightWidth + PercentageToggleWidth : 0f);
            Rect headerRect = InvokeActionEditorUtility.GetActionRowContentRect(
                rect,
                ReorderHandleWidth,
                controlsWidth,
                EditorGUIUtility.singleLineHeight);
            Rect weightRect = new Rect(
                headerRect.xMax,
                rect.y,
                WeightWidth,
                EditorGUIUtility.singleLineHeight);
            Rect percentageToggleRect = new Rect(
                weightRect.xMax,
                rect.y,
                PercentageToggleWidth,
                EditorGUIUtility.singleLineHeight);
            Rect toggleRect = new Rect(
                showWeight ? percentageToggleRect.xMax : headerRect.xMax,
                rect.y,
                ToggleWidth,
                EditorGUIUtility.singleLineHeight);
            DrawActionExecutionFeedback(headerRect, index);
            enabledProperty.boolValue = EditorGUI.Toggle(toggleRect, enabledProperty.boolValue);
            if (showWeight)
            {
                DrawWeightControls(weightRect, percentageToggleRect, index);
            }
            DrawActionContextMenu(headerRect, index);
            if (actionProperty.managedReferenceValue == null)
            {
                Rect issueRect = new Rect(
                    headerRect.xMax - EditorGUIUtility.singleLineHeight,
                    headerRect.y,
                    EditorGUIUtility.singleLineHeight,
                    headerRect.height);
                Rect selectRect = headerRect;
                selectRect.width -= issueRect.width;
                if (GUI.Button(selectRect, "Select Action", EditorStyles.popup))
                {
                    ShowActionMenu(selectRect, index);
                }
                InvokeActionEditorUtility.DrawActionIssueBadge(issueRect, null);
                return;
            }

            // The foldout and type picker will be drawn by TriInspector!
            // We just need to check if it's expanded to draw the rest of the utility settings.
            if (!actionProperty.isExpanded)
            {
                return; // Actually, if we don't draw utility settings when collapsed, return is fine.
            }

            float propertiesHeight = GetActionPropertiesHeight(actionProperty);
            Rect propertiesRect = new Rect(
                headerRect.x + 14f,
                headerRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                headerRect.width - 14f,
                propertiesHeight);
            DrawActionProperties(actionProperty, propertiesRect);

            if (!IsUtilitySelector())
            {
                return;
            }

            SerializedProperty settingsProperty = wrapperProperty.FindPropertyRelative("utilitySettings");
            SerializedProperty utilityProperty = settingsProperty.FindPropertyRelative("utility");
            SerializedProperty blockProperty = settingsProperty.FindPropertyRelative("blockDuringExecution");
            float utilityHeight = EditorGUI.GetPropertyHeight(utilityProperty, true);
            Rect utilityRect = new Rect(
                rect.x + 14f,
                propertiesRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                rect.width - 14f,
                utilityHeight);
            EditorGUI.PropertyField(utilityRect, utilityProperty, new GUIContent("Utility"), true);

            Rect blockRect = new Rect(
                utilityRect.x,
                utilityRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                utilityRect.width,
                EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(blockRect, blockProperty, new GUIContent("Block During Execution"));
        }

        private void DrawWeightControls(Rect weightRect, Rect percentageToggleRect, int index)
        {
            InvokeActionCommand invokeAction = target as InvokeActionCommand;
            if (invokeAction == null)
            {
                return;
            }

            float weight = invokeAction.GetActionWeight(index);
            bool hasOverride = invokeAction.HasActionWeightOverride(index);
            GUIContent tooltip = new GUIContent(
                string.Empty,
                hasOverride
                    ? "Manual weight override."
                    : "Automatically balanced weight.");
            EditorGUI.BeginChangeCheck();
            float requestedWeight;
            using (new EditorGUI.DisabledScope(!hasOverride))
            {
                requestedWeight = InvokeActionEditorUtility.DelayedPercentageField(
                    weightRect,
                    tooltip,
                    weight);
            }
            if (EditorGUI.EndChangeCheck())
            {
                SerializedProperty settingsProperty = _actionsProperty.GetArrayElementAtIndex(index).FindPropertyRelative("utilitySettings");
                settingsProperty.FindPropertyRelative("weight").floatValue =
                    Mathf.Clamp(requestedWeight, 0f, 100f);
                settingsProperty.FindPropertyRelative("weightInitialized").boolValue = true;
                settingsProperty.FindPropertyRelative("weightOverride").boolValue = true;
            }

            bool requestedOverride = GUI.Toggle(
                percentageToggleRect,
                hasOverride,
                new GUIContent(
                    "%",
                    hasOverride
                        ? "Click to restore automatic balancing."
                        : "Click to edit a manual percentage."),
                EditorStyles.miniButton);
            if (requestedOverride == hasOverride)
            {
                return;
            }

            SerializedProperty actionSettings = _actionsProperty.GetArrayElementAtIndex(index).FindPropertyRelative("utilitySettings");
            if (requestedOverride)
            {
                actionSettings.FindPropertyRelative("weight").floatValue = weight;
                actionSettings.FindPropertyRelative("weightInitialized").boolValue = true;
            }
            else
            {
                actionSettings.FindPropertyRelative("weight").floatValue = 0f;
                actionSettings.FindPropertyRelative("weightInitialized").boolValue = false;
            }
            actionSettings.FindPropertyRelative("weightOverride").boolValue = requestedOverride;
        }

        private void DrawActionContextMenu(Rect headerRect, int actionIndex)
        {
            if (Event.current.type != EventType.ContextClick ||
                !headerRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            InvokeActionCommand invokeAction = target as InvokeActionCommand;
            if (invokeAction == null)
            {
                return;
            }

            List<int> selectedIndices = InvokeActionEditorSelection.GetSelectedIndices(invokeAction);
            if (!selectedIndices.Contains(actionIndex))
            {
                InvokeActionEditorSelection.Select(invokeAction, actionIndex);
                selectedIndices = InvokeActionEditorSelection.GetSelectedIndices(invokeAction);
            }

            GenericMenu menu = new GenericMenu();
            bool hasSelection = selectedIndices.Count > 0;
            AddActionContextMenuItem(menu, "Cut", hasSelection, () =>
            {
                CopySelectedActions(invokeAction);
                DeleteSelectedActions(invokeAction);
            });
            AddActionContextMenuItem(
                menu,
                "Copy",
                hasSelection,
                () => CopySelectedActions(invokeAction));
            AddActionContextMenuItem(
                menu,
                "Paste",
                ActionClipboard.Count > 0,
                () => PasteActionClipboard(invokeAction));
            AddActionContextMenuItem(
                menu,
                "Delete",
                hasSelection,
                () => DeleteSelectedActions(invokeAction));
            menu.AddSeparator(string.Empty);
            menu.AddItem(
                new GUIContent("Select All"),
                false,
                () =>
                {
                    InvokeActionEditorSelection.SelectAll(invokeAction);
                    _actionsList.index = 0;
                });
            menu.AddItem(
                new GUIContent("Select None"),
                false,
                () =>
                {
                    InvokeActionEditorSelection.Clear(invokeAction);
                    _actionsList.index = -1;
                });
            menu.ShowAsContext();
            Event.current.Use();
        }

        private static void AddActionContextMenuItem(
            GenericMenu menu,
            string label,
            bool enabled,
            GenericMenu.MenuFunction callback)
        {
            if (enabled)
            {
                menu.AddItem(new GUIContent(label), false, callback);
                return;
            }

            menu.AddDisabledItem(new GUIContent(label));
        }

        private void CopySelectedActions(InvokeActionCommand invokeAction)
        {
            ClearActionClipboard();
            serializedObject.Update();
            foreach (int actionIndex in InvokeActionEditorSelection.GetSelectedIndices(invokeAction))
            {
                ActionClipboardEntry entry = ScriptableObject.CreateInstance<ActionClipboardEntry>();
                entry.hideFlags = HideFlags.HideAndDontSave;
                entry.action = invokeAction.actions[actionIndex].action;
                entry.isEnabled = invokeAction.IsActionEnabled(actionIndex);
                entry.utilitySettings = invokeAction.actions[actionIndex].utilitySettings;
                ActionClipboard.Add(entry);
            }
        }

        private void DrawActionsHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Actions");
            InvokeActionCommand invokeAction = target as InvokeActionCommand;
            if (invokeAction == null ||
                !invokeAction.IsExecuting ||
                invokeAction.actions == null ||
                invokeAction.actions.Count <= 1)
            {
                return;
            }

            string waitingMessage = InvokeActionEditorUtility.GetExecutionWaitingMessage(
                invokeAction.ExecutionMethod,
                invokeAction.AwaitMode,
                invokeAction.OrderMode);
            InvokeActionEditorUtility.DrawWaitingMessage(rect, waitingMessage);
            Repaint();
        }

        private void DrawActionExecutionFeedback(Rect headerRect, int actionIndex)
        {
            InvokeActionCommand invokeAction = target as InvokeActionCommand;
            if (!Application.isPlaying ||
                invokeAction == null ||
                invokeAction.actions == null ||
                invokeAction.actions.Count <= 1)
            {
                return;
            }

            if (invokeAction.TryGetActionExecutionStatus(
                    actionIndex,
                    out CompositeExecutionStatus status))
            {
                InvokeActionEditorUtility.DrawExecutionResult(headerRect, status);
                Repaint();
                return;
            }

            if (!invokeAction.IsExecuting ||
                !InvokeActionEditorUtility.IsDeterministicExecution(
                    invokeAction.ExecutionMethod,
                    invokeAction.OrderMode) ||
                !invokeAction.IsActionRunning(actionIndex))
            {
                return;
            }

            if (invokeAction.TryGetActionExecutionProgress(actionIndex, out float progress))
            {
                InvokeActionEditorUtility.DrawExecutionProgress(headerRect, progress);
            }
            else
            {
                InvokeActionEditorUtility.DrawExecutingHighlight(headerRect);
            }

            Repaint();
        }

        private void PasteActionClipboard(InvokeActionCommand invokeAction)
        {
            if (ActionClipboard.Count == 0)
            {
                return;
            }

            Undo.RecordObject(invokeAction, "Paste Invoke Actions");
            serializedObject.Update();
            List<int> selectedIndices = InvokeActionEditorSelection.GetSelectedIndices(invokeAction);
            int insertIndex = selectedIndices.Count > 0
                ? selectedIndices[selectedIndices.Count - 1] + 1
                : _actionsProperty.arraySize;
            int firstInsertedIndex = insertIndex;
            foreach (ActionClipboardEntry entry in ActionClipboard)
            {
                _actionsProperty.InsertArrayElementAtIndex(insertIndex);
                SerializedProperty wrapperProp = _actionsProperty.GetArrayElementAtIndex(insertIndex);
                wrapperProp.FindPropertyRelative("action").managedReferenceValue = entry.action;
                wrapperProp.FindPropertyRelative("action").isExpanded = true;
                wrapperProp.FindPropertyRelative("enabled").boolValue = entry.isEnabled;
                wrapperProp.FindPropertyRelative("id").stringValue = System.Guid.NewGuid().ToString("N");
                wrapperProp.FindPropertyRelative("utilitySettings").boxedValue = entry.utilitySettings;
                insertIndex++;
            }

            serializedObject.FindProperty("displayAsGroup").boolValue = true;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(invokeAction);
            InvokeActionEditorSelection.Select(invokeAction, firstInsertedIndex);
        }

        private void DeleteSelectedActions(InvokeActionCommand invokeAction)
        {
            List<int> selectedIndices = InvokeActionEditorSelection.GetSelectedIndices(invokeAction);
            if (selectedIndices.Count == 0)
            {
                return;
            }

            Undo.RecordObject(invokeAction, "Delete Invoke Actions");
            serializedObject.Update();
            int closestIndex = selectedIndices[0];
            for (int selectionIndex = selectedIndices.Count - 1; selectionIndex >= 0; selectionIndex--)
            {
                int actionIndex = selectedIndices[selectionIndex];
                _actionsProperty.DeleteArrayElementAtIndex(actionIndex);
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(invokeAction);
            SelectClosestAction(closestIndex);
        }

        private static void ClearActionClipboard()
        {
            foreach (ActionClipboardEntry entry in ActionClipboard)
            {
                UnityEngine.Object.DestroyImmediate(entry);
            }

            ActionClipboard.Clear();
        }

        private static void DrawActionProperties(SerializedProperty actionProperty, Rect propertiesRect)
        {
            SerializedProperty property = actionProperty.Copy();
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Mathf.Min(180f, propertiesRect.width * 0.4f);
            try
            {
                bool enterChildren = true;
                float propertyY = propertiesRect.y;
                while (property.NextVisible(enterChildren) &&
                       IsActionChildProperty(actionProperty, property))
                {
                    enterChildren = false;
                    if (!InvokeActionEditorUtility.IsPropertyVisible(
                            actionProperty.managedReferenceValue as IAction,
                            property.name))
                    {
                        continue;
                    }

                    float propertyHeight = EditorGUI.GetPropertyHeight(property, true);
                    Rect propertyRect = new Rect(
                        propertiesRect.x,
                        propertyY,
                        propertiesRect.width,
                        propertyHeight);
                    EditorGUI.PropertyField(propertyRect, property, true);
                    propertyY += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
                }
            }
            finally
            {
                EditorGUIUtility.labelWidth = originalLabelWidth;
            }
        }

        private float GetActionElementHeight(int index)
        {
            SerializedProperty wrapperProperty = _actionsProperty.GetArrayElementAtIndex(index);
            SerializedProperty actionProperty = wrapperProperty.FindPropertyRelative("action");
            float height = EditorGUIUtility.singleLineHeight;
            if (actionProperty.managedReferenceValue != null && actionProperty.isExpanded)
            {
                height += EditorGUIUtility.standardVerticalSpacing +
                          GetActionPropertiesHeight(actionProperty);
            }
            if (!IsUtilitySelector() ||
                actionProperty.managedReferenceValue == null ||
                !actionProperty.isExpanded)
            {
                return height;
            }

            SerializedProperty settingsProperty = wrapperProperty.FindPropertyRelative("utilitySettings");
            SerializedProperty utilityProperty = settingsProperty.FindPropertyRelative("utility");
            height += EditorGUI.GetPropertyHeight(utilityProperty, true);
            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing * 2f;
            return height;
        }

        private static float GetActionPropertiesHeight(SerializedProperty actionProperty)
        {
            SerializedProperty property = actionProperty.Copy();
            bool enterChildren = true;
            float height = 0f;
            while (property.NextVisible(enterChildren) &&
                   IsActionChildProperty(actionProperty, property))
            {
                enterChildren = false;
                if (!InvokeActionEditorUtility.IsPropertyVisible(
                        actionProperty.managedReferenceValue as IAction,
                        property.name))
                {
                    continue;
                }

                height += EditorGUI.GetPropertyHeight(property, true) +
                          EditorGUIUtility.standardVerticalSpacing;
            }

            return Mathf.Max(0f, height - EditorGUIUtility.standardVerticalSpacing);
        }

        private static bool IsActionChildProperty(
            SerializedProperty actionProperty,
            SerializedProperty candidateProperty)
        {
            return candidateProperty.propertyPath.StartsWith(
                actionProperty.propertyPath + ".",
                StringComparison.Ordinal);
        }

        private void ShowAddActionMenu(Rect rect, ReorderableList list)
        {
            ShowActionMenu(rect, _actionsProperty.arraySize);
        }

        private void ShowActionMenu(Rect rect, int insertIndex)
        {
            GenericMenu menu = new GenericMenu();
            List<Type> actionTypes = TypeCache.GetTypesDerivedFrom<IAction>()
                .Where(type => type.IsClass && type.IsSerializable && !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(type => type.FullName)
                .ToList();

            foreach (Type actionType in actionTypes)
            {
                Type capturedType = actionType;
                string menuPath = InvokeActionEditorUtility.GetMenuPath(capturedType);
                menu.AddItem(new GUIContent(menuPath), false, () => AddAction(capturedType, insertIndex));
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
            SerializedProperty wrapperProp = _actionsProperty.GetArrayElementAtIndex(insertIndex);
            wrapperProp.FindPropertyRelative("enabled").boolValue = true;
            wrapperProp.FindPropertyRelative("id").stringValue = Guid.NewGuid().ToString("N");
            wrapperProp.FindPropertyRelative("utilitySettings").boxedValue = new InvokeActionUtilitySettings(0f, false);
            SerializedProperty actionProp = wrapperProp.FindPropertyRelative("action");
            actionProp.managedReferenceValue = Activator.CreateInstance(actionType);
            wrapperProp.isExpanded = true;
            // Actions added through this Inspector belong to an explicit Action Invoker,
            // even when it currently contains only one action.
            serializedObject.FindProperty("displayAsGroup").boolValue = true;

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
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            SelectClosestAction(list.index);
        }

        private void ReorderEnabledStates(ReorderableList list, int oldIndex, int newIndex)
        {
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
                _lastSynchronizedActionIndex = -1;
                return;
            }

            _actionsList.index = selectedIndex;
            if (_lastSynchronizedActionIndex == selectedIndex)
            {
                return;
            }

            _actionsProperty.GetArrayElementAtIndex(selectedIndex).isExpanded = true;
            _lastSynchronizedActionIndex = selectedIndex;
        }

        private void SynchronizeActionMetadata()
        {
            for (int actionIndex = 0; actionIndex < _actionsProperty.arraySize; actionIndex++)
            {
                SerializedProperty wrapperProp = _actionsProperty.GetArrayElementAtIndex(actionIndex);
                SerializedProperty actionIdProperty = wrapperProp.FindPropertyRelative("id");
                if (string.IsNullOrEmpty(actionIdProperty.stringValue))
                {
                    actionIdProperty.stringValue = Guid.NewGuid().ToString("N");
                }
            }
        }

        private bool IsUtilitySelector()
        {
            InvokeActionCommand invokeAction = target as InvokeActionCommand;
            return invokeAction != null &&
                   invokeAction.ExecutionMethod == CompositeExecutionMethod.UtilitySelector;
        }

        private bool IsRandomOrder()
        {
            InvokeActionCommand invokeAction = target as InvokeActionCommand;
            return invokeAction != null &&
                   CompositeExecutionDescription.SupportsWeight(
                       invokeAction.ExecutionMethod,
                       invokeAction.OrderMode);
        }
    }
}
