
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEditorInternal;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Presentation.UI.Input;

namespace Scaffold.EditorUtils
{
    public class CommandListAdaptor
    {
        private const float k_CommandExtractionEdgeHeight = 8f;
        private const float k_CommandReorderHysteresis = 10f;
        private const float BlockExtractionHorizontalSpacing = 24f;
        private const float DefaultBlockWidth = 240f;
        private const float EnabledToggleWidth = 20f;
        private const float WeightFieldWidth = 58f;
        private const float WeightOverrideWidth = 20f;

        /// <summary>
        /// If true, scrolls to the currently selected command in the inspector when the editor is redrawn. A
        /// Automatically resets to false.
        /// </summary>
        public static bool ScrollToCommandOnDraw = false;

        protected SerializedProperty _arrayProperty;
        protected ReorderableList list;
        protected Block block;
        protected GUIStyle summaryStyle, commandLabelStyle;

        public Command ContextCommand { get; private set; }

        private readonly List<NestedActionDropTarget> _nestedActionDropTargets = new List<NestedActionDropTarget>();
        private readonly List<StandaloneActionDropTarget> _standaloneActionDropTargets = new List<StandaloneActionDropTarget>();
        private readonly List<InvokeActionDropTarget> _invokeActionDropTargets = new List<InvokeActionDropTarget>();
        private readonly HashSet<int> _collapsedInvokeActionGroups = new HashSet<int>();
        private NestedActionDrag _nestedActionDrag;
        private NestedActionDrag _pendingNestedActionDrag;
        private StandaloneActionDrag _standaloneActionDrag;
        private StandaloneActionDrag _pendingStandaloneActionDrag;
        private InvokeActionCommand _capturedReorderSource;
        private bool _commandReorderDragActive;
        private int _commandReorderAnchorIndex = -1;
        private float _commandReorderAnchorY;

        public float fixedItemHeight;

        public SerializedProperty this[int index]
        {
            get { return _arrayProperty.GetArrayElementAtIndex(index); }
        }

        public SerializedProperty arrayProperty
        {
            get { return _arrayProperty; }
        }

        public CommandListAdaptor(Block _block, SerializedProperty arrayProperty)
        {
            if (arrayProperty == null)
            {
                throw new ArgumentNullException("Array property was null.");
            }

            if (!arrayProperty.isArray)
            {
                throw new InvalidOperationException("Specified serialized propery is not an array.");
            }

            this._arrayProperty = arrayProperty;
            this.block = _block;

            list = new ReorderableList(arrayProperty.serializedObject, arrayProperty, true, true, false, false);
            list.drawHeaderCallback = DrawHeader;
            list.drawElementCallback = DrawItem;
            list.elementHeightCallback = GetItemHeight;
            list.onSelectCallback = SelectChanged;
            list.onMouseDragCallback = HandleCommandMouseDrag;
            list.onMouseUpCallback = HandleCommandMouseUp;
            list.onReorderCallbackWithDetails = HandleCommandReordered;
        }

        public void DrawCommandList()
        {
            if (summaryStyle == null)
            {
                summaryStyle = new GUIStyle(EditorStyles.label);
                summaryStyle.fontSize = 11;
                summaryStyle.richText = true;
                summaryStyle.wordWrap = false;
                summaryStyle.clipping = TextClipping.Clip;
                summaryStyle.alignment = TextAnchor.MiddleLeft;
                if (EditorGUIUtility.isProSkin)
                {
                    summaryStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                }
            }

            if (commandLabelStyle == null)
            {
                commandLabelStyle = new GUIStyle(GUI.skin.box); // Use Unity's rounded box
                commandLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                commandLabelStyle.fontStyle = FontStyle.Bold;
                commandLabelStyle.alignment = TextAnchor.MiddleLeft;
                commandLabelStyle.richText = true;
                commandLabelStyle.fontSize = 11;
                commandLabelStyle.padding = new RectOffset(6, 6, 0, 0);
                commandLabelStyle.margin = new RectOffset(0, 0, 0, 0);
            }

            if (block.CommandList.Count == 0)
            {
                EditorGUILayout.HelpBox("Press the + button below to add a command to the list.", MessageType.Info);
            }
            else
            {
                _nestedActionDropTargets.Clear();
                _standaloneActionDropTargets.Clear();
                _invokeActionDropTargets.Clear();
                Vector2 actualMousePosition = Event.current.mousePosition;
                bool restoreMousePosition = Event.current.type == EventType.MouseDrag;
                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                {
                    ResetCommandReorderDrag();
                    CancelActionDrags();
                }

                bool commandListWasDraggable = list.draggable;
                bool suppressCommandListDrag = InvokeActionEditorUtility.ShouldTemporarilySuppressParentDrag(
                    HasActionDrag(),
                    Event.current.rawType);
                if (suppressCommandListDrag)
                {
                    list.draggable = false;
                }

                try
                {
                    list.DoLayoutList();
                }
                finally
                {
                    list.draggable = commandListWasDraggable;
                }

                if (restoreMousePosition)
                {
                    Event.current.mousePosition = actualMousePosition;
                }
                HandleNestedActionDrag(Event.current);
                HandleStandaloneActionDrag(Event.current);
            }
        }

        private void SelectChanged(ReorderableList list)
        {
            _commandReorderAnchorIndex = list.index;
            _commandReorderAnchorY = Event.current.mousePosition.y;
            Command command = this[list.index].objectReferenceValue as Command;
            if (command is InvokeActionCommand invokeAction)
            {
                InvokeActionEditorSelection.Clear(invokeAction);
            }
            Blackboard blackboard = (Blackboard)command.GetBlackboard();
            BlockEditor.actionList.Add(delegate
            {
                blackboard.ClearSelectedCommands();
                blackboard.AddSelectedCommand(command);
            });
        }

        private void HandleCommandReordered(ReorderableList reorderedList, int oldIndex, int newIndex)
        {
            if (newIndex < 0 || newIndex >= block.CommandList.Count)
            {
                ResetCommandReorderDrag();
                return;
            }

            InvokeActionCommand commandAtOldIndex = oldIndex >= 0 && oldIndex < block.CommandList.Count
                ? block.CommandList[oldIndex] as InvokeActionCommand
                : null;
            InvokeActionCommand source = InvokeActionEditorUtility.ResolveReorderSource(
                _capturedReorderSource,
                commandAtOldIndex);
            ResetCommandReorderDrag();
            QueueInvokeActionMerge(source, reorderedList);
        }

        private void HandleCommandMouseDrag(ReorderableList draggedList)
        {
            if (HasActionDrag())
            {
                ResetCommandReorderDrag();
                return;
            }

            if (draggedList.index < 0 || draggedList.index >= _arrayProperty.arraySize)
            {
                ResetCommandReorderDrag();
                return;
            }

            if (!_commandReorderDragActive)
            {
                _commandReorderDragActive = true;
                _capturedReorderSource = this[draggedList.index].objectReferenceValue as InvokeActionCommand;
                if (_commandReorderAnchorIndex != draggedList.index)
                {
                    _commandReorderAnchorY = Event.current.mousePosition.y;
                }
            }

            // ReorderableList updates its private drag position after this callback. Biasing the
            // event toward its origin keeps the center of a destination row stable for merging.
            Vector2 mousePosition = Event.current.mousePosition;
            mousePosition.y = InvokeActionEditorUtility.GetReorderDragYWithHysteresis(
                _commandReorderAnchorY,
                mousePosition.y,
                k_CommandReorderHysteresis);
            Event.current.mousePosition = mousePosition;
        }

        private void HandleCommandMouseUp(ReorderableList mouseUpList)
        {
            if (!_commandReorderDragActive)
            {
                return;
            }

            InvokeActionCommand source = _capturedReorderSource;
            ResetCommandReorderDrag();
            QueueInvokeActionMerge(source, mouseUpList);
        }

        private void QueueInvokeActionMerge(InvokeActionCommand source, ReorderableList reorderedList)
        {
            if (source == null || source.actions == null || source.actions.Count != 1)
            {
                return;
            }

            foreach (InvokeActionDropTarget target in _invokeActionDropTargets)
            {
                if (target.InvokeAction == source ||
                    !InvokeActionEditorUtility.IsMergeDrop(target.Rect, Event.current.mousePosition))
                {
                    continue;
                }

                int actionCount = target.InvokeAction.actions?.Count ?? 0;
                int destinationIndex = InvokeActionEditorUtility.GetInsertionIndex(
                    target.Rect,
                    Event.current.mousePosition,
                    actionCount);
                InvokeActionCommand destination = target.InvokeAction;
                BlockEditor.actionList.Add(delegate
                {
                    MoveStandaloneActionIntoInvokeGroup(source, destination, destinationIndex);
                    reorderedList.index = block.CommandList.IndexOf(destination);
                });
                return;
            }
        }

        private void ResetCommandReorderDrag()
        {
            _capturedReorderSource = null;
            _commandReorderDragActive = false;
            _commandReorderAnchorIndex = -1;
        }

        private void DrawHeader(Rect rect)
        {
            if (rect.width < 0)
            {
                return;
            }

            EditorGUI.LabelField(rect, new GUIContent("Commands"));
        }

        public void DrawItem(Rect position, int index, bool selected, bool focused)
        {
            if (position.width < 0)
            {
                return;
            }

            Command command = this[index].objectReferenceValue as Command;

            if (command == null)
            {
                return;
            }

            CommandInfoAttribute commandInfoAttr = CommandEditor.GetCommandInfo(command.GetType());
            if (commandInfoAttr == null)
            {
                return;
            }

            Blackboard blackboard = (Blackboard)command.GetBlackboard();
            if (blackboard == null)
            {
                return;
            }

            bool commandIsSelected = blackboard.SelectedCommands.Contains(command);
            if (command is InvokeActionCommand invokeAction)
            {
                if (HasNestedActionGroup(invokeAction))
                {
                    DrawInvokeActionGroup(position, invokeAction, commandIsSelected, index, blackboard);
                }
                else
                {
                    DrawSingleInvokeAction(position, invokeAction, commandIsSelected, index, blackboard);
                }

                if (InvokeActionEditorUtility.CanAcceptActionDrop(invokeAction))
                {
                    RegisterCommandEdgeExtractionDropTargets(position, index);
                }
                else
                {
                    RegisterCommandExtractionDropTargets(position, index);
                }

                HandleCommandSelection(position, index, command, blackboard);
                return;
            }

            bool isComment = command.GetType().Name == "CommentAction";
            bool isLabel = command.GetType().Name == "LabelAction";

            string summary = command.GetSummary();
            if (summary == null)
            {
                summary = "";
            }
            else
            {
                summary = summary.Replace("\n", "").Replace("\r", "");
            }
            if (summary.StartsWith("Error:"))
            {
                summary = "<color=red> " + summary + "</color>";
            }

            if (isComment || isLabel)
            {
                summary = "<b> " + summary + "</b>";
            }
            else
            {
                summary = "<i>" + summary + "</i>";
            }

            commandIsSelected = false;
            foreach (Command selectedCommand in blackboard.SelectedCommands)
            {
                if (selectedCommand == command)
                {
                    commandIsSelected = true;
                    if (ScrollToCommandOnDraw)
                    {
                        GUI.ScrollTo(position);
                        ScrollToCommandOnDraw = false;
                    }
                    break;
                }
            }

            string commandName = commandInfoAttr.CommandName;

            float indentSize = 20;
            for (int i = 0; i < command.IndentLevel; ++i)
            {
                Rect indentRect = position;
                indentRect.x += i * indentSize;// - 21;
                indentRect.width = indentSize + 1;
                indentRect.y -= 2;
                indentRect.height += 5;
                GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                GUI.Box(indentRect, "", commandLabelStyle);
            }

            float commandNameWidth = Mathf.Max(commandLabelStyle.CalcSize(new GUIContent(commandName)).x, 90f);
            float indentWidth = command.IndentLevel * indentSize;

            Rect commandLabelRect = position;
            commandLabelRect.x += indentWidth;// - 21;
            commandLabelRect.y -= 2;
            commandLabelRect.width -= (indentSize * command.IndentLevel);// - 22);
            commandLabelRect.height += 5;
            bool showsCommandWeight = ShouldShowCommandWeight(block, command);
            float commandControlsWidth = EnabledToggleWidth +
                                         (showsCommandWeight
                                             ? WeightFieldWidth + WeightOverrideWidth
                                             : 0f);
            commandLabelRect.width -= commandControlsWidth;

            // There's a weird incompatibility between the Reorderable list control used for the command list and 
            // the UnityEvent list control used in some commands. In play mode, if you click on the reordering grabber
            // for a command in the list it causes the UnityEvent list to spew null exception errors.
            // The workaround for now is to hide the reordering grabber from mouse clicks by extending the command
            // selection rectangle to cover it. We are planning to totally replace the command list display system.
            Rect clickRect = position;
            //clickRect.x -= 20;
            //clickRect.width += 20;

            HandleCommandSelection(clickRect, index, command, blackboard);

            Color commandLabelColor = Color.white;
            if (blackboard.ColorCommands)
            {
                commandLabelColor = command.GetButtonColor();
            }

            if (commandIsSelected)
            {
                // Modern Unity / Animora Selection Blue
                commandLabelColor = new Color(0.17f, 0.36f, 0.53f, 1f);
            }
            else if (!command.enabled)
            {
                commandLabelColor = new Color(0.4f, 0.4f, 0.4f, 1f);
            }

            // If not selected and we are in pro skin, darken the default white button slightly so it doesn't blind the user
            if (!commandIsSelected && blackboard.ColorCommands == false && EditorGUIUtility.isProSkin)
            {
                commandLabelColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            }

            GUI.backgroundColor = commandLabelColor;

            if (isComment)
            {
                GUI.Label(commandLabelRect, "", commandLabelStyle);
            }
            else
            {
                string commandNameLabel;
                if (blackboard.ShowLineNumbers)
                {
                    commandNameLabel = command.CommandIndex.ToString() + ": " + commandName;
                }
                else
                {
                    commandNameLabel = commandName;
                }

                GUI.Label(commandLabelRect, commandNameLabel, commandLabelStyle);
            }

            DrawBlockExecutionFeedback(commandLabelRect, command);

            if (command.ExecutingIconTimer > Time.realtimeSinceStartup)
            {
                Rect iconRect = new Rect(commandLabelRect);
                iconRect.x += iconRect.width - commandLabelRect.width - 20;
                iconRect.width = 20;
                iconRect.height = 20;

                Color storeColor = GUI.color;

                float alpha = (command.ExecutingIconTimer - Time.realtimeSinceStartup) / ScaffoldConstants.ExecutingIconFadeTime;
                alpha = Mathf.Clamp01(alpha);

                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.Label(iconRect, ScaffoldEditorResources.PlaySmall, new GUIStyle());

                GUI.color = storeColor;
            }

            Rect summaryRect = new Rect(commandLabelRect);
            if (isComment)
            {
                summaryRect.x += 5;
            }
            else
            {
                summaryRect.x += commandNameWidth + 5;
                summaryRect.width -= commandNameWidth + 5;
            }

            GUI.Label(summaryRect, summary, summaryStyle);

            Rect enabledRect = new Rect(
                position.xMax - EnabledToggleWidth,
                position.y + 2f,
                18f,
                EditorGUIUtility.singleLineHeight);
            if (showsCommandWeight)
            {
                Rect overrideRect = new Rect(
                    enabledRect.x - WeightOverrideWidth,
                    enabledRect.y,
                    WeightOverrideWidth,
                    enabledRect.height);
                Rect weightRect = new Rect(
                    overrideRect.x - WeightFieldWidth,
                    enabledRect.y,
                    WeightFieldWidth,
                    enabledRect.height);
                DrawCommandWeightControls(block, command, weightRect, overrideRect);
            }
            DrawCommandEnabledToggle(command, enabledRect);

            GUI.backgroundColor = Color.white;
            RegisterCommandExtractionDropTargets(position, index);
        }

        private float GetItemHeight(int index)
        {
            Command command = this[index].objectReferenceValue as Command;
            if (command is not InvokeActionCommand invokeAction || !HasNestedActionGroup(invokeAction))
            {
                return 26f;
            }

            const float headerHeight = 22f;
            const float actionHeight = 22f;
            if (IsInvokeActionGroupCollapsed(invokeAction))
            {
                return headerHeight + 4f;
            }

            int actionCount = invokeAction.actions?.Count ?? 0;
            int childActionCount = IsConditionalActionGroup(invokeAction)
                ? Mathf.Max(1, actionCount - 1)
                : actionCount;
            return headerHeight + (childActionCount * actionHeight) + 4f;
        }

        private void DrawInvokeActionGroup(Rect position, InvokeActionCommand invokeAction, bool isSelected, int commandIndex, Blackboard blackboard)
        {
            const float headerHeight = 22f;
            const float actionHeight = 22f;
            const float actionTextXOffset = 14f;

            int actionCount = invokeAction.actions?.Count ?? 0;
            bool isConditionalGroup = IsConditionalActionGroup(invokeAction);
            int firstChildActionIndex = isConditionalGroup ? 1 : 0;
            int childActionCount = isConditionalGroup
                ? Mathf.Max(1, actionCount - firstChildActionIndex)
                : actionCount;
            CommandInfoAttribute commandInfo = CommandEditor.GetCommandInfo(invokeAction.GetType());
            string commandName = commandInfo != null ? commandInfo.CommandName : invokeAction.GetType().Name;
            string groupName = isConditionalGroup
                ? InvokeActionEditorUtility.GetDisplayName(invokeAction.actions[0].action)
                : commandName;
            bool isExpanded = !IsInvokeActionGroupCollapsed(invokeAction);

            Rect headerRect = new Rect(position.x, position.y, position.width, headerHeight);
            // The entire group is a destination. This keeps the target easy to acquire when
            // the group is collapsed or its action list is empty.
            _invokeActionDropTargets.Add(new InvokeActionDropTarget(position, invokeAction));
            DrawInvokeActionRowBackground(headerRect);
            bool blockShowsWaitingMessage = DrawBlockExecutionFeedback(headerRect, invokeAction);
            Rect collapseRect = new Rect(headerRect.x + 24f, headerRect.y, 18f, headerRect.height);
            bool expanded = EditorGUI.Foldout(collapseRect, isExpanded, GUIContent.none, false);
            if (expanded != isExpanded)
            {
                SetInvokeActionGroupCollapsed(invokeAction, !expanded);
                isExpanded = expanded;
            }

            bool showsCommandWeight = ShouldShowCommandWeight(block, invokeAction);
            Rect parentToggleRect = new Rect(
                headerRect.xMax - 22f,
                headerRect.y + 2f,
                18f,
                EditorGUIUtility.singleLineHeight);
            float commandWeightControlsWidth = showsCommandWeight
                ? WeightFieldWidth + WeightOverrideWidth
                : 0f;
            if (showsCommandWeight)
            {
                Rect overrideRect = new Rect(
                    parentToggleRect.x - WeightOverrideWidth,
                    parentToggleRect.y,
                    WeightOverrideWidth,
                    parentToggleRect.height);
                Rect weightRect = new Rect(
                    overrideRect.x - WeightFieldWidth,
                    parentToggleRect.y,
                    WeightFieldWidth,
                    parentToggleRect.height);
                DrawCommandWeightControls(block, invokeAction, weightRect, overrideRect);
            }
            DrawInvokeActionEnabledToggle(invokeAction, parentToggleRect);
            Rect labelRect = new Rect(
                headerRect.x + 44f,
                headerRect.y,
                Mathf.Max(
                    1f,
                    parentToggleRect.x - commandWeightControlsWidth - headerRect.x - 48f),
                headerRect.height);
            EditorGUI.LabelField(labelRect, groupName, EditorStyles.boldLabel);
            if (blockShowsWaitingMessage)
            {
                DrawBlockWaitingMessage(headerRect);
            }
            else
            {
                DrawNestedExecutionWaitingMessage(headerRect, invokeAction);
            }
            ShowInvokeActionContextMenu(headerRect, invokeAction, blackboard);

            if (!isExpanded)
            {
                DrawStandaloneActionDropHighlight(position, invokeAction);
                return;
            }

            float actionAreaTop = headerRect.yMax;
            const float actionAreaHorizontalOffset = 36f;
            Rect actionDropArea = new Rect(
                position.x + actionAreaHorizontalOffset,
                actionAreaTop,
                position.width - actionAreaHorizontalOffset,
                childActionCount * actionHeight);

            for (int childIndex = 0; childIndex < childActionCount; childIndex++)
            {
                int actionIndex = firstChildActionIndex + childIndex;
                float horizontalOffset = actionAreaHorizontalOffset;
                Rect actionRect = new Rect(position.x + horizontalOffset, actionAreaTop + (childIndex * actionHeight), position.width - horizontalOffset, actionHeight);
                DrawInvokeActionRowBackground(actionRect);
                DrawNestedActionExecutionFeedback(actionRect, invokeAction, actionIndex);
                _nestedActionDropTargets.Add(new NestedActionDropTarget(actionRect, invokeAction, actionIndex));
                bool showsWeight = CompositeExecutionDescription.SupportsWeight(
                    invokeAction.ExecutionMethod,
                    invokeAction.OrderMode);
                Rect toggleRect = new Rect(
                    actionRect.xMax - 20f,
                    actionRect.y + 2f,
                    18f,
                    EditorGUIUtility.singleLineHeight);
                Rect weightRect = new Rect(
                    toggleRect.x - WeightOverrideWidth - WeightFieldWidth,
                    actionRect.y + 2f,
                    WeightFieldWidth,
                    EditorGUIUtility.singleLineHeight);
                Rect overrideRect = new Rect(
                    weightRect.xMax,
                    actionRect.y + 2f,
                    WeightOverrideWidth,
                    EditorGUIUtility.singleLineHeight);

                string actionName = actionIndex < actionCount
                    ? InvokeActionEditorUtility.GetDisplayName(invokeAction.actions[actionIndex].action)
                    : string.Empty;
                bool enabled = actionIndex < actionCount &&
                               invokeAction.IsActionEnabled(actionIndex);
                if (actionIndex < actionCount)
                {
                    DrawNestedActionDragHandle(
                        actionRect,
                        showsWeight
                            ? WeightFieldWidth + WeightOverrideWidth
                            : 0f);
                    Rect dragRect = InvokeActionEditorUtility.GetActionRowDragRect(
                        actionRect,
                        22f + (showsWeight
                            ? WeightFieldWidth + WeightOverrideWidth
                            : 0f));
                    EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.Pan);
                    using (new EditorGUI.DisabledScope(!invokeAction.enabled))
                    {
                        DrawNestedActionEnabledToggle(invokeAction, actionIndex, enabled, toggleRect);
                        if (showsWeight)
                        {
                            DrawNestedActionWeightControls(
                                invokeAction,
                                actionIndex,
                                weightRect,
                                overrideRect);
                        }
                    }
                    TryBeginNestedActionDrag(invokeAction, actionIndex, dragRect, toggleRect, commandIndex, blackboard);
                    using (new EditorGUI.DisabledScope(!invokeAction.enabled || !enabled))
                    {
                        float weightOffset = showsWeight
                            ? WeightFieldWidth + WeightOverrideWidth
                            : 0f;
                        IAction action = invokeAction.actions[actionIndex].action;
                        const float issueWidth = 18f;
                        Rect issueRect = new Rect(
                            actionRect.xMax - 44f - weightOffset - issueWidth,
                            actionRect.y + 2f,
                            issueWidth,
                            EditorGUIUtility.singleLineHeight);
                        bool hasIssue = InvokeActionEditorUtility.DrawActionIssueBadge(
                            issueRect,
                            action);
                        DrawNestedActionLabel(
                            actionRect,
                            actionName,
                            isConditionalGroup,
                            actionTextXOffset + 20f,
                            weightOffset + (hasIssue ? issueWidth : 0f));
                    }
                }

                DrawNestedActionInsertionIndicator(actionRect);
                DrawStandaloneActionInsertionIndicator(actionRect, invokeAction);
            }

            DrawStandaloneActionDropHighlight(position, invokeAction);
        }

        private static void DrawNestedActionWeightControls(
            InvokeActionCommand invokeAction,
            int actionIndex,
            Rect weightRect,
            Rect overrideRect)
        {
            float weight = invokeAction.GetActionWeight(actionIndex);
            bool hasOverride = invokeAction.HasActionWeightOverride(actionIndex);
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
                Undo.RecordObject(invokeAction, "Set Action Weight Override");
                invokeAction.SetActionWeight(actionIndex, requestedWeight);
                PrefabUtility.RecordPrefabInstancePropertyModifications(invokeAction);
                EditorUtility.SetDirty(invokeAction);
                CommandEditor.SelectedCommandDataStale = true;
            }

            bool requestedOverride = GUI.Toggle(
                overrideRect,
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

            Undo.RecordObject(invokeAction, requestedOverride
                ? "Enable Action Weight Override"
                : "Disable Action Weight Override");
            if (requestedOverride)
            {
                invokeAction.SetActionWeight(actionIndex, weight);
            }
            else
            {
                invokeAction.ClearActionWeightOverride(actionIndex);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(invokeAction);
            EditorUtility.SetDirty(invokeAction);
            CommandEditor.SelectedCommandDataStale = true;
        }

        private void DrawSingleInvokeAction(Rect position, InvokeActionCommand invokeAction, bool isSelected, int commandIndex, Blackboard blackboard)
        {
            bool hasAction = invokeAction.actions != null && invokeAction.actions.Count == 1;
            bool acceptsActionDrop = InvokeActionEditorUtility.CanAcceptActionDrop(invokeAction);
            string actionName = hasAction
                ? InvokeActionEditorUtility.GetDisplayName(invokeAction.actions[0].action)
                : "Action Invoker";
            Rect actionRect = new Rect(position.x, position.y - 2f, position.width, position.height + 5f);
            bool showsCommandWeight = ShouldShowCommandWeight(block, invokeAction);
            float commandWeightControlsWidth = showsCommandWeight
                ? WeightFieldWidth + WeightOverrideWidth
                : 0f;
            if (acceptsActionDrop)
            {
                _invokeActionDropTargets.Add(new InvokeActionDropTarget(actionRect, invokeAction));
            }
            DrawInvokeActionRowBackground(actionRect);
            bool showsWaitingMessage = DrawBlockExecutionFeedback(actionRect, invokeAction);
            Rect dragHandleRect = hasAction
                ? DrawNestedActionDragHandle(actionRect, commandWeightControlsWidth)
                : new Rect();
            using (new EditorGUI.DisabledScope(!invokeAction.enabled))
            {
                const float labelOffset = 6f;
                float reservedWidth = (hasAction ? 48f : 26f) + commandWeightControlsWidth;
                if (hasAction)
                {
                    const float issueWidth = 18f;
                    Rect issueRect = new Rect(
                        actionRect.xMax - reservedWidth - issueWidth,
                        actionRect.y + 2f,
                        issueWidth,
                        EditorGUIUtility.singleLineHeight);
                    if (InvokeActionEditorUtility.DrawActionIssueBadge(
                            issueRect,
                            invokeAction.actions[0].action))
                    {
                        reservedWidth += issueWidth;
                    }
                }
                EditorGUI.LabelField(new Rect(actionRect.x + labelOffset, actionRect.y, actionRect.width - labelOffset - reservedWidth, actionRect.height), actionName, EditorStyles.boldLabel);
            }
            if (showsWaitingMessage)
            {
                DrawBlockWaitingMessage(actionRect);
            }

            Rect parentToggleRect = new Rect(actionRect.xMax - 20f, actionRect.y + 2f, 18f, EditorGUIUtility.singleLineHeight);
            if (showsCommandWeight)
            {
                Rect overrideRect = new Rect(
                    parentToggleRect.x - WeightOverrideWidth,
                    parentToggleRect.y,
                    WeightOverrideWidth,
                    parentToggleRect.height);
                Rect weightRect = new Rect(
                    overrideRect.x - WeightFieldWidth,
                    parentToggleRect.y,
                    WeightFieldWidth,
                    parentToggleRect.height);
                DrawCommandWeightControls(block, invokeAction, weightRect, overrideRect);
            }
            DrawInvokeActionEnabledToggle(invokeAction, parentToggleRect);
            if (hasAction)
            {
                TryBeginStandaloneActionDrag(invokeAction, dragHandleRect, parentToggleRect, commandIndex, blackboard);
            }
            if (acceptsActionDrop)
            {
                DrawNestedActionInsertionIndicator(actionRect);
                DrawStandaloneActionDropHighlight(actionRect, invokeAction);
            }
        }

        private void RegisterCommandExtractionDropTargets(Rect commandRect, int commandIndex)
        {
            RegisterCommandExtractionDropTargets(commandRect, commandIndex, commandRect.height * 0.5f);
        }

        private void RegisterCommandEdgeExtractionDropTargets(Rect commandRect, int commandIndex)
        {
            float targetHeight = Mathf.Min(k_CommandExtractionEdgeHeight, commandRect.height * 0.5f);
            RegisterCommandExtractionDropTargets(commandRect, commandIndex, targetHeight);
        }

        private void RegisterCommandExtractionDropTargets(Rect commandRect, int commandIndex, float targetHeight)
        {
            Rect beforeRect = InvokeActionEditorUtility.GetCommandBeforeDropRect(commandRect, targetHeight);
            Rect afterRect = InvokeActionEditorUtility.GetCommandAfterDropRect(commandRect, targetHeight);
            RegisterStandaloneActionDropTarget(new StandaloneActionDropTarget(
                beforeRect,
                commandIndex,
                commandRect.y));
            RegisterStandaloneActionDropTarget(new StandaloneActionDropTarget(
                afterRect,
                commandIndex + 1,
                commandRect.yMax));
        }

        private void RegisterStandaloneActionDropTarget(StandaloneActionDropTarget target)
        {
            _standaloneActionDropTargets.Add(target);
            if (_nestedActionDrag == null || !target.Rect.Contains(Event.current.mousePosition))
            {
                return;
            }

            EditorGUI.DrawRect(target.Rect, new Color(1f, 1f, 1f, 0.08f));
            EditorGUI.DrawRect(
                new Rect(target.Rect.x, target.IndicatorY - 1f, target.Rect.width, 2f),
                Color.white);
        }

        private bool IsInvokeActionGroupCollapsed(InvokeActionCommand invokeAction)
        {
            return invokeAction != null && _collapsedInvokeActionGroups.Contains(invokeAction.ItemId);
        }

        private void SetInvokeActionGroupCollapsed(InvokeActionCommand invokeAction, bool collapsed)
        {
            if (invokeAction == null)
            {
                return;
            }

            if (collapsed)
            {
                _collapsedInvokeActionGroups.Add(invokeAction.ItemId);
            }
            else
            {
                _collapsedInvokeActionGroups.Remove(invokeAction.ItemId);
            }
        }

        private static bool HasNestedActionGroup(InvokeActionCommand invokeAction)
        {
            return invokeAction.actions != null &&
                   (invokeAction.DisplayAsGroup ||
                    invokeAction.actions.Count > 1 ||
                    IsConditionalActionGroup(invokeAction));
        }

        private static bool IsConditionalActionGroup(InvokeActionCommand invokeAction)
        {
            return invokeAction.actions != null &&
                   invokeAction.actions.Count > 0 &&
                   invokeAction.actions[0].action is ActionBase action &&
                   action.OpenBlock();
        }

        private void DrawInvokeActionRowBackground(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, commandLabelStyle);
            if (rect.Contains(Event.current.mousePosition))
            {
                EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.06f));
            }
        }

        private bool DrawBlockExecutionFeedback(Rect rect, Command command)
        {
            if (!Application.isPlaying ||
                block == null ||
                block.CommandList == null ||
                block.CommandList.Count <= 1 ||
                command == null)
            {
                return false;
            }

            if (block.TryGetCommandExecutionStatus(
                    command,
                    out CompositeExecutionStatus status))
            {
                InvokeActionEditorUtility.DrawExecutionResult(rect, status);
                return false;
            }

            if (!block.IsExecuting() || !command.IsExecuting)
            {
                return false;
            }

            if (InvokeActionEditorUtility.IsDeterministicExecution(
                    block.ExecutionMethod,
                    block.OrderMode))
            {
                if (command is InvokeActionCommand invokeAction &&
                    invokeAction.TryGetExecutionProgress(out float progress))
                {
                    InvokeActionEditorUtility.DrawExecutionProgress(rect, progress);
                }
                else
                {
                    InvokeActionEditorUtility.DrawExecutingHighlight(rect);
                }

                return false;
            }

            string waitingMessage = InvokeActionEditorUtility.GetExecutionWaitingMessage(
                block.ExecutionMethod,
                block.AwaitMode,
                block.OrderMode);
            InvokeActionEditorUtility.DrawWaitingMessage(rect, waitingMessage);
            return !string.IsNullOrEmpty(waitingMessage);
        }

        private void DrawBlockWaitingMessage(Rect rect)
        {
            string waitingMessage = InvokeActionEditorUtility.GetExecutionWaitingMessage(
                block.ExecutionMethod,
                block.AwaitMode,
                block.OrderMode);
            InvokeActionEditorUtility.DrawWaitingMessage(rect, waitingMessage);
        }

        private static void DrawNestedExecutionWaitingMessage(
            Rect rect,
            InvokeActionCommand invokeAction)
        {
            if (!Application.isPlaying ||
                invokeAction == null ||
                !invokeAction.IsExecuting ||
                invokeAction.actions == null ||
                invokeAction.actions.Count <= 1 ||
                InvokeActionEditorUtility.IsDeterministicExecution(
                    invokeAction.ExecutionMethod,
                    invokeAction.OrderMode))
            {
                return;
            }

            string waitingMessage = InvokeActionEditorUtility.GetExecutionWaitingMessage(
                invokeAction.ExecutionMethod,
                invokeAction.AwaitMode,
                invokeAction.OrderMode);
            InvokeActionEditorUtility.DrawWaitingMessage(rect, waitingMessage);
        }

        private static void DrawNestedActionExecutionFeedback(
            Rect rect,
            InvokeActionCommand invokeAction,
            int actionIndex)
        {
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
                InvokeActionEditorUtility.DrawExecutionResult(rect, status);
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
                InvokeActionEditorUtility.DrawExecutionProgress(rect, progress);
            }
            else
            {
                InvokeActionEditorUtility.DrawExecutingHighlight(rect);
            }
        }

        private void DrawNestedActionLabel(
            Rect actionRect,
            string actionName,
            bool useDefaultLayout,
            float labelOffset,
            float rightOffset)
        {
            if (useDefaultLayout)
            {
                EditorGUI.LabelField(
                    new Rect(
                        actionRect.x + 6f,
                        actionRect.y,
                        actionRect.width - 54f - rightOffset,
                        actionRect.height),
                    actionName,
                    EditorStyles.boldLabel);
                return;
            }

            EditorGUI.LabelField(
                new Rect(
                    actionRect.x + labelOffset,
                    actionRect.y,
                    actionRect.width - labelOffset - 44f - rightOffset,
                    actionRect.height),
                actionName);
        }

        private static Rect DrawNestedActionDragHandle(Rect actionRect, float rightOffset = 0f)
        {
            Rect dragHandleRect = new Rect(
                actionRect.xMax - 42f - rightOffset,
                actionRect.y,
                16f,
                actionRect.height);
            EditorGUI.LabelField(dragHandleRect, "≡", EditorStyles.centeredGreyMiniLabel);
            EditorGUIUtility.AddCursorRect(dragHandleRect, MouseCursor.Pan);
            return dragHandleRect;
        }

        private static bool ShouldShowCommandWeight(Block parentBlock, Command command)
        {
            return parentBlock != null &&
                   command != null &&
                   command.GetType().Name != "CommentAction" &&
                   command.GetType().Name != "LabelAction" &&
                   CompositeExecutionDescription.SupportsWeight(
                       parentBlock.ExecutionMethod,
                       parentBlock.OrderMode);
        }

        private static void DrawCommandWeightControls(
            Block parentBlock,
            Command command,
            Rect weightRect,
            Rect overrideRect)
        {
            bool hasOverride = command.HasCompositeWeightOverride;
            float weight = parentBlock.GetCommandWeight(command);
            EditorGUI.BeginChangeCheck();
            float requestedWeight;
            using (new EditorGUI.DisabledScope(!hasOverride))
            {
                requestedWeight = InvokeActionEditorUtility.DelayedPercentageField(
                    weightRect,
                    new GUIContent(
                        string.Empty,
                        hasOverride
                            ? "Manual command weight override."
                            : "Automatically balanced command weight."),
                    weight);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(command, "Set Command Weight Override");
                command.CompositeWeight = requestedWeight;
                PrefabUtility.RecordPrefabInstancePropertyModifications(command);
                EditorUtility.SetDirty(command);
                CommandEditor.SelectedCommandDataStale = true;
            }

            bool requestedOverride = GUI.Toggle(
                overrideRect,
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

            Undo.RecordObject(command, requestedOverride
                ? "Enable Command Weight Override"
                : "Disable Command Weight Override");
            if (requestedOverride)
            {
                command.CompositeWeight = weight;
            }
            else
            {
                command.ClearCompositeWeightOverride();
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(command);
            EditorUtility.SetDirty(command);
            CommandEditor.SelectedCommandDataStale = true;
        }

        private static void DrawCommandEnabledToggle(Command command, Rect toggleRect)
        {
            EditorGUI.BeginChangeCheck();
            bool value = EditorGUI.Toggle(toggleRect, command.enabled);
            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            Undo.RecordObject(command, value ? "Enable Command" : "Disable Command");
            command.enabled = value;
            PrefabUtility.RecordPrefabInstancePropertyModifications(command);
            EditorUtility.SetDirty(command);
            CommandEditor.SelectedCommandDataStale = true;
        }

        private void DrawNestedActionEnabledToggle(InvokeActionCommand invokeAction, int actionIndex, bool enabled, Rect toggleRect)
        {
            EditorGUI.BeginChangeCheck();
            bool value = EditorGUI.Toggle(toggleRect, enabled);
            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            Undo.RecordObject(invokeAction, value ? "Enable Invoke Action" : "Disable Invoke Action");
            invokeAction.SetActionEnabled(actionIndex, value);
            PrefabUtility.RecordPrefabInstancePropertyModifications(invokeAction);
            EditorUtility.SetDirty(invokeAction);
        }

        private void DrawInvokeActionEnabledToggle(InvokeActionCommand invokeAction, Rect toggleRect)
        {
            EditorGUI.BeginChangeCheck();
            bool value = EditorGUI.Toggle(toggleRect, invokeAction.enabled);
            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            Undo.RecordObject(invokeAction, value ? "Enable Invoke Action" : "Disable Invoke Action");
            invokeAction.enabled = value;
            PrefabUtility.RecordPrefabInstancePropertyModifications(invokeAction);
            EditorUtility.SetDirty(invokeAction);
        }

        private void ShowInvokeActionContextMenu(Rect headerRect, InvokeActionCommand invokeAction, Blackboard blackboard)
        {
            if (Event.current.type != EventType.MouseUp ||
                Event.current.button != 1 ||
                !headerRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            CommandEditor.ShowCommandContextMenu(
                invokeAction,
                blackboard,
                "Convert to Block",
                () => ConvertInvokeActionToBlock(invokeAction, blackboard));
            Event.current.Use();
        }

        private void ConvertInvokeActionToBlock(InvokeActionCommand invokeAction, Blackboard blackboard)
        {
            if (invokeAction == null || blackboard == null || block == null ||
                !block.CommandList.Contains(invokeAction))
            {
                return;
            }

            try
            {
                Undo.RecordObject(block, "Convert Invoke Action To Block");
                Undo.RecordObject(invokeAction, "Convert Invoke Action To Block");

                Block newBlock = Undo.AddComponent<Block>(blackboard.gameObject);
                Undo.RecordObject(newBlock, "Convert Invoke Action To Block");
                float sourceBlockWidth = Mathf.Max(block._NodeRect.width, DefaultBlockWidth);
                Vector2 newBlockPosition = new Vector2(
                    block._NodeRect.x + sourceBlockWidth + BlockExtractionHorizontalSpacing,
                    block._NodeRect.y);
                newBlock._NodeRect = new Rect(newBlockPosition, Vector2.zero);
                newBlock.BlockName = blackboard.GetUniqueBlockKey($"{block.BlockName} Actions", newBlock);
                newBlock.ItemId = blackboard.NextItemId();

                invokeAction.OnCommandRemoved(block);
                block.CommandList.Remove(invokeAction);
                newBlock.CommandList.Add(invokeAction);
                invokeAction.ParentBlock = newBlock;
                invokeAction.OnCommandAdded(newBlock);

                blackboard.ClearSelectedCommands();
                blackboard.SelectedBlock = newBlock;
                blackboard.AddSelectedCommand(invokeAction);

                PrefabUtility.RecordPrefabInstancePropertyModifications(block);
                PrefabUtility.RecordPrefabInstancePropertyModifications(newBlock);
                PrefabUtility.RecordPrefabInstancePropertyModifications(invokeAction);
                EditorUtility.SetDirty(block);
                EditorUtility.SetDirty(newBlock);
                EditorUtility.SetDirty(invokeAction);
                BlackboardWindow.RefreshForBlackboard(blackboard);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to convert Invoke Action to a new Block: {exception}");
            }
        }

        private void TryBeginNestedActionDrag(InvokeActionCommand invokeAction, int actionIndex, Rect dragRect, Rect toggleRect, int commandIndex, Blackboard blackboard)
        {
            if (_nestedActionDrag != null)
            {
                return;
            }

            if (Event.current.type == EventType.MouseDrag &&
                _pendingNestedActionDrag != null &&
                _pendingNestedActionDrag.Matches(invokeAction, actionIndex) &&
                _pendingNestedActionDrag.HasStartedDrag(Event.current.mousePosition))
            {
                _nestedActionDrag = _pendingNestedActionDrag;
                _pendingNestedActionDrag = null;
                Event.current.Use();
                return;
            }

            if (Event.current.type != EventType.MouseDown ||
                Event.current.button != 0 ||
                !dragRect.Contains(Event.current.mousePosition) ||
                toggleRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            SelectInvokeAction(invokeAction, actionIndex, commandIndex, blackboard);
            _pendingNestedActionDrag = new NestedActionDrag(
                invokeAction,
                actionIndex,
                Event.current.mousePosition);
            PrepareActionDrag();
            Event.current.Use();
        }

        private void SelectInvokeAction(InvokeActionCommand invokeAction, int actionIndex, int commandIndex, Blackboard blackboard)
        {
            if (blackboard == null)
            {
                return;
            }

            InvokeActionEditorSelection.Select(invokeAction, actionIndex);
            list.index = commandIndex;
            BlockEditor.actionList.Add(delegate
            {
                blackboard.ClearSelectedCommands();
                blackboard.AddSelectedCommand(invokeAction);
            });
        }

        private void HandleNestedActionDrag(Event currentEvent)
        {
            if (_nestedActionDrag == null)
            {
                if (currentEvent.rawType == EventType.MouseUp && currentEvent.button == 0)
                {
                    _pendingNestedActionDrag = null;
                }

                return;
            }

            if (currentEvent.type == EventType.MouseDrag)
            {
                currentEvent.Use();
                return;
            }

            if (currentEvent.type != EventType.MouseUp || currentEvent.button != 0)
            {
                return;
            }

            if (TryGetStandaloneActionDropTarget(currentEvent.mousePosition, out int commandInsertIndex))
            {
                MoveNestedActionToStandaloneGroup(_nestedActionDrag, commandInsertIndex);
            }
            else if (TryGetNestedActionDropTarget(currentEvent.mousePosition, out InvokeActionCommand destination, out int destinationIndex))
            {
                MoveNestedAction(_nestedActionDrag, destination, destinationIndex);
            }
            else if (TryGetInvokeActionDropTarget(
                currentEvent.mousePosition,
                _nestedActionDrag.InvokeAction,
                out InvokeActionCommand groupDestination,
                out int groupDestinationIndex))
            {
                MoveNestedAction(_nestedActionDrag, groupDestination, groupDestinationIndex);
            }

            _nestedActionDrag = null;
            _pendingNestedActionDrag = null;
            currentEvent.Use();
        }

        private void TryBeginStandaloneActionDrag(InvokeActionCommand invokeAction, Rect dragHandleRect, Rect toggleRect, int commandIndex, Blackboard blackboard)
        {
            if (invokeAction.actions == null || invokeAction.actions.Count != 1 || _standaloneActionDrag != null)
            {
                return;
            }

            if (Event.current.type == EventType.MouseDrag &&
                _pendingStandaloneActionDrag != null &&
                _pendingStandaloneActionDrag.InvokeAction == invokeAction &&
                _pendingStandaloneActionDrag.HasStartedDrag(Event.current.mousePosition))
            {
                _standaloneActionDrag = _pendingStandaloneActionDrag;
                _pendingStandaloneActionDrag = null;
                Event.current.Use();
                return;
            }

            if (Event.current.type != EventType.MouseDown ||
                Event.current.button != 0 ||
                !dragHandleRect.Contains(Event.current.mousePosition) ||
                toggleRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            SelectInvokeAction(invokeAction, 0, commandIndex, blackboard);
            _pendingStandaloneActionDrag = new StandaloneActionDrag(invokeAction, Event.current.mousePosition);
            PrepareActionDrag();
            Event.current.Use();
        }

        private void HandleStandaloneActionDrag(Event currentEvent)
        {
            if (_standaloneActionDrag == null)
            {
                if (currentEvent.rawType == EventType.MouseUp && currentEvent.button == 0)
                {
                    _pendingStandaloneActionDrag = null;
                }

                return;
            }

            if (currentEvent.type == EventType.MouseDrag)
            {
                currentEvent.Use();
                return;
            }

            if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0 &&
                TryGetNestedActionDropTarget(currentEvent.mousePosition, out InvokeActionCommand destination, out int destinationIndex))
            {
                MoveStandaloneActionIntoInvokeGroup(_standaloneActionDrag.InvokeAction, destination, destinationIndex);
            }
            else if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0 &&
                TryGetInvokeActionDropTarget(
                    currentEvent.mousePosition,
                    _standaloneActionDrag.InvokeAction,
                    out destination,
                    out destinationIndex))
            {
                MoveStandaloneActionIntoInvokeGroup(_standaloneActionDrag.InvokeAction, destination, destinationIndex);
            }

            if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
            {
                _standaloneActionDrag = null;
                _pendingStandaloneActionDrag = null;
                currentEvent.Use();
            }
        }

        private bool HasActionDrag()
        {
            return _nestedActionDrag != null ||
                   _pendingNestedActionDrag != null ||
                   _standaloneActionDrag != null ||
                   _pendingStandaloneActionDrag != null;
        }

        private void PrepareActionDrag()
        {
            ResetCommandReorderDrag();
        }

        private void CancelActionDrags()
        {
            _nestedActionDrag = null;
            _pendingNestedActionDrag = null;
            _standaloneActionDrag = null;
            _pendingStandaloneActionDrag = null;
        }

        private void DrawStandaloneActionDropHighlight(Rect rect, InvokeActionCommand invokeAction)
        {
            if (_standaloneActionDrag == null ||
                _standaloneActionDrag.InvokeAction == invokeAction ||
                !rect.Contains(Event.current.mousePosition))
            {
                return;
            }

            const float borderWidth = 2f;
            Color borderColor = new Color(0.39f, 0.68f, 1f, 0.95f);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, borderWidth), borderColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - borderWidth, rect.width, borderWidth), borderColor);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, borderWidth, rect.height), borderColor);
            EditorGUI.DrawRect(new Rect(rect.xMax - borderWidth, rect.y, borderWidth, rect.height), borderColor);

            Rect labelRect = new Rect(rect.x + 40f, rect.center.y - (EditorGUIUtility.singleLineHeight * 0.5f), rect.width - 80f, EditorGUIUtility.singleLineHeight);
            GUI.Label(labelRect, "Drop action into Action Invoker", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawStandaloneActionInsertionIndicator(Rect actionRect, InvokeActionCommand destination)
        {
            if (_standaloneActionDrag == null ||
                _standaloneActionDrag.InvokeAction == destination ||
                !actionRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            float insertionY = Event.current.mousePosition.y < actionRect.center.y
                ? actionRect.y
                : actionRect.yMax - 3f;
            EditorGUI.DrawRect(new Rect(actionRect.x, insertionY, actionRect.width, 3f), new Color(0.39f, 0.68f, 1f, 1f));
        }

        private void DrawNestedActionInsertionIndicator(Rect actionRect)
        {
            if (_nestedActionDrag == null || !actionRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            float insertionY = Event.current.mousePosition.y < actionRect.center.y
                ? actionRect.y
                : actionRect.yMax - 3f;
            EditorGUI.DrawRect(new Rect(actionRect.x, insertionY, actionRect.width, 3f), Color.white);
        }

        private bool TryGetInvokeActionDropTarget(
            Vector2 mousePosition,
            InvokeActionCommand source,
            out InvokeActionCommand destination,
            out int destinationIndex)
        {
            foreach (InvokeActionDropTarget target in _invokeActionDropTargets)
            {
                if (target.InvokeAction == source || !target.Rect.Contains(mousePosition))
                {
                    continue;
                }

                destination = target.InvokeAction;
                int actionCount = destination.actions?.Count ?? 0;
                destinationIndex = InvokeActionEditorUtility.GetInsertionIndex(
                    target.Rect,
                    mousePosition,
                    actionCount);
                return true;
            }

            destination = null;
            destinationIndex = -1;
            return false;
        }

        private bool TryGetNestedActionDropTarget(Vector2 mousePosition, out InvokeActionCommand destination, out int destinationIndex)
        {
            foreach (NestedActionDropTarget target in _nestedActionDropTargets)
            {
                if (!target.Rect.Contains(mousePosition))
                {
                    continue;
                }

                destination = target.InvokeAction;
                destinationIndex = mousePosition.y < target.Rect.center.y
                    ? target.ActionIndex
                    : target.ActionIndex + 1;
                return true;
            }

            destination = null;
            destinationIndex = -1;
            return false;
        }

        private bool TryGetStandaloneActionDropTarget(Vector2 mousePosition, out int commandInsertIndex)
        {
            foreach (StandaloneActionDropTarget target in _standaloneActionDropTargets)
            {
                if (!target.Rect.Contains(mousePosition))
                {
                    continue;
                }

                commandInsertIndex = target.CommandInsertIndex;
                return true;
            }

            commandInsertIndex = -1;
            return false;
        }

        private void MoveStandaloneActionIntoInvokeGroup(InvokeActionCommand source, InvokeActionCommand destination, int destinationIndex)
        {
            if (source == null || destination == null || source == destination ||
                source.actions == null || source.actions.Count != 1)
            {
                return;
            }

            Undo.RecordObject(block, "Move Action Into Invoke Action");
            Undo.RecordObject(source, "Move Action Into Invoke Action");
            Undo.RecordObject(destination, "Move Action Into Invoke Action");
            if (!source.TryRemoveAction(
                    0,
                    out IAction action,
                    out bool enabled,
                    out InvokeActionUtilitySettings utilitySettings))
            {
                return;
            }

            destinationIndex = Mathf.Clamp(destinationIndex, 0, destination.actions.Count);
            destination.InsertActionInGroup(destinationIndex, action, enabled, utilitySettings);
            if (RemoveEmptyInvokeAction(source))
            {
                SynchronizeCommandListAfterStructuralChange(destination);
            }
            RecordInvokeActionChanges(source, destination);
        }

        private void MoveNestedAction(NestedActionDrag drag, InvokeActionCommand destination, int destinationIndex)
        {
            if (drag.InvokeAction == null || destination == null ||
                drag.ActionIndex < 0 || drag.ActionIndex >= drag.InvokeAction.actions.Count)
            {
                return;
            }

            if (drag.InvokeAction == destination && destinationIndex > drag.ActionIndex)
            {
                destinationIndex--;
            }

            if (drag.InvokeAction == destination)
            {
                Undo.RecordObject(destination, "Reorder Invoke Action");
                if (destination.TryMoveAction(drag.ActionIndex, destinationIndex))
                {
                    RecordInvokeActionChanges(destination, destination);
                }

                return;
            }

            Undo.RecordObject(block, "Move Invoke Action");
            Undo.RecordObject(drag.InvokeAction, "Move Invoke Action");
            if (destination != drag.InvokeAction)
            {
                Undo.RecordObject(destination, "Move Invoke Action");
            }

            if (!drag.InvokeAction.TryRemoveAction(
                    drag.ActionIndex,
                    out IAction action,
                    out bool enabled,
                    out InvokeActionUtilitySettings utilitySettings))
            {
                return;
            }

            destination.InsertActionInGroup(destinationIndex, action, enabled, utilitySettings);
            RecordInvokeActionChanges(drag.InvokeAction, destination);
        }

        private void MoveNestedActionToStandaloneGroup(NestedActionDrag drag, int commandInsertIndex)
        {
            if (drag.InvokeAction == null ||
                drag.ActionIndex < 0 || drag.ActionIndex >= drag.InvokeAction.actions.Count)
            {
                return;
            }

            Blackboard blackboard = drag.InvokeAction.GetBlackboard();
            if (blackboard == null)
            {
                return;
            }

            Undo.RecordObject(block, "Move Invoke Action Outside Group");
            Undo.RecordObject(drag.InvokeAction, "Move Invoke Action Outside Group");
            if (!drag.InvokeAction.TryRemoveAction(
                    drag.ActionIndex,
                    out IAction action,
                    out bool enabled,
                    out InvokeActionUtilitySettings utilitySettings))
            {
                return;
            }

            InvokeActionCommand standaloneGroup = Undo.AddComponent<InvokeActionCommand>(blackboard.gameObject);
            standaloneGroup.ItemId = blackboard.NextItemId();
            standaloneGroup.ExecutionMethod = drag.InvokeAction.ExecutionMethod;
            standaloneGroup.AwaitMode = drag.InvokeAction.AwaitMode;
            standaloneGroup.OrderMode = drag.InvokeAction.OrderMode;
            standaloneGroup.enabled = drag.InvokeAction.enabled;
            standaloneGroup.ParentBlock = block;
            standaloneGroup.InsertAction(0, action, enabled, utilitySettings);
            standaloneGroup.OnCommandAdded(block);

            InsertCommandReference(commandInsertIndex, standaloneGroup);
            SynchronizeCommandListAfterStructuralChange(standaloneGroup);
            RecordInvokeActionChanges(drag.InvokeAction, standaloneGroup);
        }

        private void InsertCommandReference(int commandInsertIndex, Command command)
        {
            SerializedObject serializedBlock = _arrayProperty.serializedObject;
            serializedBlock.Update();
            int clampedIndex = Mathf.Clamp(commandInsertIndex, 0, _arrayProperty.arraySize);
            _arrayProperty.InsertArrayElementAtIndex(clampedIndex);
            _arrayProperty.GetArrayElementAtIndex(clampedIndex).objectReferenceValue = command;
            serializedBlock.ApplyModifiedProperties();
        }

        private bool RemoveEmptyInvokeAction(InvokeActionCommand invokeAction)
        {
            if (invokeAction == null ||
                invokeAction.actions.Count > 0 ||
                invokeAction.DisplayAsGroup)
            {
                return false;
            }

            invokeAction.OnCommandRemoved(block);
            block.CommandList.Remove(invokeAction);
            InvokeActionEditorSelection.Clear(invokeAction);
            Undo.DestroyObjectImmediate(invokeAction);
            return true;
        }

        private void SynchronizeCommandListAfterStructuralChange(Command selectedCommand)
        {
            _arrayProperty.serializedObject.Update();
            list.index = block.CommandList.IndexOf(selectedCommand);

            Blackboard blackboard = block.GetBlackboard();
            if (blackboard == null || selectedCommand == null)
            {
                return;
            }

            blackboard.ClearSelectedCommands();
            blackboard.AddSelectedCommand(selectedCommand);
        }

        private void RecordInvokeActionChanges(InvokeActionCommand source, InvokeActionCommand destination)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(block);
            if (source != null)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(source);
                EditorUtility.SetDirty(source);
            }
            if (destination != null)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(destination);
                EditorUtility.SetDirty(destination);
            }
            EditorUtility.SetDirty(block);
        }

        private sealed class NestedActionDrag
        {
            public NestedActionDrag(InvokeActionCommand invokeAction, int actionIndex, Vector2 startPosition)
            {
                InvokeAction = invokeAction;
                ActionIndex = actionIndex;
                StartPosition = startPosition;
            }

            public InvokeActionCommand InvokeAction { get; }
            public int ActionIndex { get; }
            private Vector2 StartPosition { get; }

            public bool Matches(InvokeActionCommand invokeAction, int actionIndex)
            {
                return InvokeAction == invokeAction && ActionIndex == actionIndex;
            }

            public bool HasStartedDrag(Vector2 currentPosition)
            {
                return InvokeActionEditorUtility.HasDragStarted(StartPosition, currentPosition, 8f);
            }
        }

        private sealed class StandaloneActionDrag
        {
            public StandaloneActionDrag(InvokeActionCommand invokeAction, Vector2 startPosition)
            {
                InvokeAction = invokeAction;
                StartPosition = startPosition;
            }

            public InvokeActionCommand InvokeAction { get; }
            private Vector2 StartPosition { get; }

            public bool HasStartedDrag(Vector2 currentPosition)
            {
                return InvokeActionEditorUtility.HasDragStarted(StartPosition, currentPosition, 8f);
            }
        }

        private readonly struct InvokeActionDropTarget
        {
            public InvokeActionDropTarget(Rect rect, InvokeActionCommand invokeAction)
            {
                Rect = rect;
                InvokeAction = invokeAction;
            }

            public Rect Rect { get; }
            public InvokeActionCommand InvokeAction { get; }
        }

        private readonly struct NestedActionDropTarget
        {
            public NestedActionDropTarget(Rect rect, InvokeActionCommand invokeAction, int actionIndex)
            {
                Rect = rect;
                InvokeAction = invokeAction;
                ActionIndex = actionIndex;
            }

            public Rect Rect { get; }
            public InvokeActionCommand InvokeAction { get; }
            public int ActionIndex { get; }
        }

        private readonly struct StandaloneActionDropTarget
        {
            public StandaloneActionDropTarget(Rect rect, int commandInsertIndex, float indicatorY)
            {
                Rect = rect;
                CommandInsertIndex = commandInsertIndex;
                IndicatorY = indicatorY;
            }

            public Rect Rect { get; }
            public int CommandInsertIndex { get; }
            public float IndicatorY { get; }
        }

        private void HandleCommandSelection(Rect clickRect, int index, Command command, Blackboard blackboard)
        {
            if (Event.current.type != EventType.MouseDown ||
                !clickRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            if (Event.current.button == 1)
            {
                ContextCommand = command;
                return;
            }

            if (Event.current.button != 0)
            {
                return;
            }

            if (blackboard.SelectedCommands.Contains(command))
            {
                if (!EditorGUI.actionKey && !Event.current.shift)
                {
                    BlockEditor.actionList.Add(delegate
                    {
                        blackboard.SelectedCommands.Remove(command);
                        blackboard.ClearSelectedCommands();
                    });
                }

                if (EditorGUI.actionKey)
                {
                    BlockEditor.actionList.Add(delegate { blackboard.SelectedCommands.Remove(command); });
                    Event.current.Use();
                }
            }
            else
            {
                bool shift = Event.current.shift;
                if (!shift && !EditorGUI.actionKey)
                {
                    BlockEditor.actionList.Add(delegate { blackboard.ClearSelectedCommands(); });
                    Event.current.Use();
                    list.index = index;
                }

                BlockEditor.actionList.Add(delegate { blackboard.AddSelectedCommand(command); });
                AddRangeSelection(command, blackboard, shift);
                Event.current.Use();
            }

            GUIUtility.keyboardControl = 0;
        }

        private static void AddRangeSelection(Command command, Blackboard blackboard, bool shift)
        {
            if (!shift || blackboard.SelectedBlock == null)
            {
                return;
            }

            int firstSelectedIndex = blackboard.SelectedBlock.CommandList.FindIndex(selectedCommand =>
                blackboard.SelectedCommands.Contains(selectedCommand));
            int lastSelectedIndex = blackboard.SelectedBlock.CommandList.FindLastIndex(selectedCommand =>
                blackboard.SelectedCommands.Contains(selectedCommand));

            if (firstSelectedIndex < 0 || lastSelectedIndex < 0)
            {
                firstSelectedIndex = 0;
                lastSelectedIndex = command.CommandIndex;
            }
            else
            {
                firstSelectedIndex = Mathf.Min(firstSelectedIndex, command.CommandIndex);
                lastSelectedIndex = Mathf.Max(lastSelectedIndex, command.CommandIndex);
            }

            for (int i = firstSelectedIndex; i < lastSelectedIndex; i++)
            {
                Command selectedCommand = blackboard.SelectedBlock.CommandList[i];
                BlockEditor.actionList.Add(delegate { blackboard.AddSelectedCommand(selectedCommand); });
            }
        }
    }
}
