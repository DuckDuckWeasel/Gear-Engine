using System;
using System.Collections.Generic;
using Scaffold.VisualScripting.Authoring;
using Scaffold.VisualScripting.Unity;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    public sealed class BlackboardDetailPanel
    {
        private static readonly string[] s_tabs = { "Block", "Variables" };
        private static readonly string[] s_actionHiddenFields = { "enabled", "utility", "weight", "hasWeightOverride", "blockDuringExecution", "indentLevel", "targetActionIds" };
        private static readonly string[] s_variableHiddenFields = { "key", "scope" };
        private const string k_actionDragKey = "Scaffold.Blackboard.Action";
        private readonly AdvancedDropdownState dropdownState = new AdvancedDropdownState();
        private readonly BlackboardEditorExecutionController execution = new BlackboardEditorExecutionController();
        private readonly BlackboardExecutionFeedback feedback = new BlackboardExecutionFeedback();
        private Vector2 mainScrollPosition;
        private Vector2 previewScrollPosition;
        private Vector2 variableScrollPosition;
        private Vector2 pendingDragStart;
        private string pendingActionDragId;
        private string pendingActionDragName;
        private IAction hoveredAction;
        private string previewActionId;
        private int selectedTab;

        public void DrawAuthoring(
            BlackboardAuthoringController controller,
            BlackboardBehaviour behaviour)
        {
            if (Event.current.type == EventType.MouseDown ||
                Event.current.type == EventType.MouseMove ||
                Event.current.type == EventType.MouseLeaveWindow)
            {
                hoveredAction = null;
                EditorWindow.focusedWindow?.Repaint();
            }

            selectedTab = GUILayout.Toolbar(selectedTab, s_tabs, EditorStyles.toolbarButton);
            if (selectedTab == 0)
            {
                DrawSelectedBlock(controller, behaviour);
            }
            else
            {
                variableScrollPosition = EditorGUILayout.BeginScrollView(
                    variableScrollPosition,
                    GUIStyle.none,
                    GUI.skin.verticalScrollbar,
                    GUILayout.ExpandHeight(true));
                DrawVariables(controller);
                EditorGUILayout.EndScrollView();
            }
        }

        public void DrawInspector(BlackboardAuthoringController controller)
        {
            previewScrollPosition = EditorGUILayout.BeginScrollView(
                previewScrollPosition,
                GUIStyle.none,
                GUI.skin.verticalScrollbar,
                GUILayout.ExpandHeight(true));
            DrawSelectedActionPreview(controller);
            EditorGUILayout.EndScrollView();
        }

        public void ShowVariables()
        {
            selectedTab = 1;
        }

        public static bool TryGetSelectedActionPreview(BlackboardAuthoringController controller, out ActionTrackDefinition selectedTrack, out IAction selectedAction)
        {
            selectedTrack = null;
            selectedAction = null;
            if (controller == null || controller.Metadata.SelectedActionIds.Count != 1)
            {
                return false;
            }

            DefinitionId selectedId = controller.Metadata.SelectedActionIds[0];
            for (int blockIndex = 0; blockIndex < controller.Definition.Blocks.Count; blockIndex++)
            {
                BlockDefinition block = controller.Definition.Blocks[blockIndex];
                if (block == null)
                {
                    continue;
                }

                for (int trackIndex = 0; trackIndex < block.Tracks.Count; trackIndex++)
                {
                    ActionTrackDefinition track = block.Tracks[trackIndex];
                    if (track == null)
                    {
                        continue;
                    }

                    for (int actionIndex = 0; actionIndex < track.ActionList.Actions.Count; actionIndex++)
                    {
                        IAction action = track.ActionList.Actions[actionIndex];
                        if (action != null && action.DefinitionId == selectedId)
                        {
                            selectedTrack = track;
                            selectedAction = action;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool ShouldReleasePreviewTextFocus(string previousActionId, string nextActionId)
        {
            return !string.Equals(previousActionId, nextActionId, StringComparison.Ordinal);
        }

        public static float GetActionRowAlpha(
            bool selected,
            bool hovered)
        {
            if (selected)
            {
                return hovered ? 0.62f : 0.55f;
            }

            return hovered ? 0.38f : 0.26f;
        }

        public static bool ShouldShowActionControls(bool hovered)
        {
            return hovered;
        }

        private void DrawSelectedActionPreview(BlackboardAuthoringController controller)
        {
            if (TryGetSelectedActionPreview(controller, out ActionTrackDefinition track, out IAction action))
            {
                UpdatePreviewTextFocus(action.DefinitionId.Value);
                EditorGUILayout.LabelField($"{BlackboardEditorDisplay.GetName(action.GetType())} Inspector", EditorStyles.boldLabel);
                string help = BlackboardEditorDisplay.GetHelp(action.GetType());
                if (!string.IsNullOrWhiteSpace(help))
                {
                    EditorGUILayout.HelpBox(help, MessageType.None);
                }

                DrawActionDetails(controller, track, action);
                return;
            }

            UpdatePreviewTextFocus(null);
            string message = controller.Metadata.SelectedActionIds.Count > 1
                ? "Multiple actions selected. Select one action to edit its properties."
                : "Select an action in the list to preview and edit it.";
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }

        private void UpdatePreviewTextFocus(string nextActionId)
        {
            if (!ShouldReleasePreviewTextFocus(previewActionId, nextActionId))
            {
                return;
            }

            ReleasePreviewTextFocus();
            previewActionId = nextActionId;
        }

        private static void ReleasePreviewTextFocus()
        {
            GUI.FocusControl(null);
            GUIUtility.keyboardControl = 0;
            EditorGUIUtility.editingTextField = false;
            GUI.changed = true;
            EditorWindow.focusedWindow?.Repaint();
        }

        private void DrawSelectedBlock(BlackboardAuthoringController controller, BlackboardBehaviour behaviour)
        {
            BlockDefinition block = controller.GetBlock(controller.Metadata.SelectedBlockId);
            if (block == null)
            {
                EditorGUILayout.HelpBox("Select a Block in the graph to edit it.", MessageType.Info);
                return;
            }

            DrawBlockHeader(controller, block);
            DrawBlockExecution(controller, block);
            DrawTrigger(controller, block);
            DrawTracks(controller, behaviour, block);
        }

        private void DrawBlockHeader(BlackboardAuthoringController controller, BlockDefinition block)
        {
            EditorGUILayout.BeginHorizontal();
            Texture2D icon = BlackboardEditorStyles.FlowGraph;
            if (icon != null)
            {
                GUILayout.Label(icon, GUILayout.Width(38f), GUILayout.Height(38f));
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Block Inspector", EditorStyles.boldLabel);
            string name = EditorGUILayout.TextField("Block Name", block.Name);
            if (!string.Equals(name, block.Name, StringComparison.Ordinal))
            {
                controller.RenameBlock(block.DefinitionId, name);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            DrawBlockAuthoringMetadata(controller, block);
        }

        private void DrawBlockAuthoringMetadata(BlackboardAuthoringController controller, BlockDefinition block)
        {
            BlockAuthoringMetadata layout = controller.GetLayout(block.DefinitionId);
            bool useTint = EditorGUILayout.Toggle("Custom Tint", layout.UseCustomTint);
            Color tint = EditorGUILayout.ColorField("Tint", layout.Tint);
            if (useTint != layout.UseCustomTint || tint != layout.Tint)
            {
                controller.SetBlockTint(block.DefinitionId, useTint, tint);
            }

            string description = EditorGUILayout.TextArea(layout.Description, GUILayout.MinHeight(42f));
            if (!string.Equals(description, layout.Description, StringComparison.Ordinal))
            {
                controller.SetBlockDescription(block.DefinitionId, description);
            }
        }

        private void DrawBlockExecution(BlackboardAuthoringController controller, BlockDefinition block)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Execution", EditorStyles.boldLabel);
            ActionListExecutionMethod method = (ActionListExecutionMethod)EditorGUILayout.EnumPopup("Method", block.ExecutionMethod);
            ActionListAwaitMode awaitMode = (ActionListAwaitMode)EditorGUILayout.EnumPopup("Await", block.AwaitMode);
            ActionListOrderMode order = (ActionListOrderMode)EditorGUILayout.EnumPopup("Order", block.OrderMode);
            bool avoid = EditorGUILayout.Toggle("Avoid Repeat", block.AvoidRepeatingLastAction);
            if (method != block.ExecutionMethod || awaitMode != block.AwaitMode || order != block.OrderMode || avoid != block.AvoidRepeatingLastAction)
            {
                ApplyBlockExecution(controller, block, method, awaitMode, order, avoid);
            }
        }

        private void ApplyBlockExecution(BlackboardAuthoringController controller, BlockDefinition block, ActionListExecutionMethod method, ActionListAwaitMode awaitMode, ActionListOrderMode order, bool avoid)
        {
            controller.RecordSerializedChange("Edit Blackboard Block Execution");
            block.ExecutionMethod = method;
            block.AwaitMode = awaitMode;
            block.OrderMode = order;
            block.AvoidRepeatingLastAction = avoid;
            controller.CompleteSerializedChange();
        }

        private void DrawTrigger(BlackboardAuthoringController controller, BlockDefinition block)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Trigger", EditorStyles.boldLabel);
            string triggerName = block.Trigger == null ? "None" : BlackboardEditorDisplay.GetName(block.Trigger.GetType());
            if (GUILayout.Button(triggerName, EditorStyles.popup))
            {
                ShowTriggerDropdown(controller, block, GUILayoutUtility.GetLastRect());
            }

            if (block.Trigger != null && GUILayout.Button("Clear", GUILayout.Width(48f)))
            {
                controller.ClearTrigger(block.DefinitionId);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
            if (block.Trigger != null && BlackboardSerializedPropertyRenderer.DrawManagedReference(controller.Owner, block.Trigger, Array.Empty<string>(), null, controller.Definition))
            {
                controller.CompleteSerializedChange();
            }
        }

        private void ShowTriggerDropdown(BlackboardAuthoringController controller, BlockDefinition block, Rect buttonRect)
        {
            IReadOnlyList<Type> types = BlackboardManagedTypeCatalog.GetTriggerTypes();
            BlackboardTypeDropdown dropdown = new BlackboardTypeDropdown(
                dropdownState,
                "Select Trigger",
                types,
                type =>
                {
                    if (type == null)
                    {
                        controller.ClearTrigger(block.DefinitionId);
                    }
                    else
                    {
                        controller.SetTrigger(block.DefinitionId, type);
                    }
                },
                true);
            dropdown.Show(buttonRect);
        }

        private void DrawTracks(BlackboardAuthoringController controller, BlackboardBehaviour behaviour, BlockDefinition block)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Action Tracks", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Track", GUILayout.Width(82f)))
            {
                controller.AddTrack(block.DefinitionId);
            }

            EditorGUILayout.EndHorizontal();
            mainScrollPosition = EditorGUILayout.BeginScrollView(
                mainScrollPosition,
                GUIStyle.none,
                GUI.skin.verticalScrollbar,
                GUILayout.ExpandHeight(true));
            for (int index = 0; index < block.Tracks.Count; index++)
            {
                ActionTrackDefinition track = block.Tracks[index];
                if (track != null)
                {
                    DrawTrack(controller, behaviour, block, track, index);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawTrack(BlackboardAuthoringController controller, BlackboardBehaviour behaviour, BlockDefinition block, ActionTrackDefinition track, int trackIndex)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawTrackHeader(controller, block, track, trackIndex);
            DrawTrackExecution(controller, track);
            DrawActions(controller, behaviour, track);
            DrawTrackFooter(controller, track);
            EditorGUILayout.EndVertical();
        }

        private void DrawTrackHeader(BlackboardAuthoringController controller, BlockDefinition block, ActionTrackDefinition track, int trackIndex)
        {
            EditorGUILayout.BeginHorizontal();
            string name = EditorGUILayout.TextField(track.Name, EditorStyles.boldLabel);
            if (!string.Equals(name, track.Name, StringComparison.Ordinal))
            {
                controller.RecordSerializedChange("Rename Blackboard Track");
                track.Name = name;
                controller.CompleteSerializedChange();
            }

            DrawMoveTrackButtons(controller, block, track, trackIndex);
            if (GUILayout.Button("×", GUILayout.Width(24f)))
            {
                controller.RemoveTrack(track.DefinitionId);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawMoveTrackButtons(BlackboardAuthoringController controller, BlockDefinition block, ActionTrackDefinition track, int trackIndex)
        {
            using (new EditorGUI.DisabledScope(trackIndex == 0))
            {
                if (GUILayout.Button("↑", GUILayout.Width(24f)))
                {
                    controller.MoveTrack(track.DefinitionId, trackIndex - 1);
                }
            }

            using (new EditorGUI.DisabledScope(trackIndex + 1 >= block.Tracks.Count))
            {
                if (GUILayout.Button("↓", GUILayout.Width(24f)))
                {
                    controller.MoveTrack(track.DefinitionId, trackIndex + 1);
                }
            }
        }

        private void DrawTrackExecution(BlackboardAuthoringController controller, ActionTrackDefinition track)
        {
            EditorGUILayout.BeginHorizontal();
            ActionListExecutionMethod method = (ActionListExecutionMethod)EditorGUILayout.EnumPopup(track.ActionList.ExecutionMethod);
            ActionListAwaitMode awaitMode = (ActionListAwaitMode)EditorGUILayout.EnumPopup(track.ActionList.AwaitMode);
            ActionListOrderMode order = (ActionListOrderMode)EditorGUILayout.EnumPopup(track.ActionList.OrderMode);
            EditorGUILayout.EndHorizontal();
            if (method != track.ActionList.ExecutionMethod || awaitMode != track.ActionList.AwaitMode || order != track.ActionList.OrderMode)
            {
                ApplyTrackExecution(controller, track, method, awaitMode, order);
            }
        }

        private void ApplyTrackExecution(BlackboardAuthoringController controller, ActionTrackDefinition track, ActionListExecutionMethod method, ActionListAwaitMode awaitMode, ActionListOrderMode order)
        {
            controller.RecordSerializedChange("Edit Blackboard Track Execution");
            track.ActionList.ExecutionMethod = method;
            track.ActionList.AwaitMode = awaitMode;
            track.ActionList.OrderMode = order;
            controller.CompleteSerializedChange();
        }

        private void DrawActions(BlackboardAuthoringController controller, BlackboardBehaviour behaviour, ActionTrackDefinition track)
        {
            for (int index = 0; index < track.ActionList.Actions.Count; index++)
            {
                IAction action = track.ActionList.Actions[index];
                if (action != null)
                {
                    DrawAction(controller, behaviour, track, action, index);
                }
            }

            HandlePreparedActionDrag();
            DrawTrackDropTarget(controller, track);
        }

        private void DrawAction(BlackboardAuthoringController controller, BlackboardBehaviour behaviour, ActionTrackDefinition track, IAction action, int index)
        {
            Rect row = GUILayoutUtility.GetRect(0f, 28f, GUILayout.ExpandWidth(true));
            UpdateHoveredAction(action, row);
            bool hovered = ReferenceEquals(hoveredAction, action);
            DrawActionBackground(
                row,
                action,
                controller.Metadata.SelectedActionIds.Contains(
                    action.DefinitionId),
                hovered);
            DrawActionRow(
                controller,
                behaviour,
                track,
                action,
                index,
                row,
                hovered);
            HandleActionRowEvent(controller, behaviour, track, action, index, row);
        }

        private void UpdateHoveredAction(IAction action, Rect row)
        {
            EventType eventType = Event.current.type;
            if ((eventType == EventType.MouseDown ||
                 eventType == EventType.MouseMove) &&
                IsMouseOver(row))
            {
                hoveredAction = action;
            }
        }

        private static bool IsMouseOver(Rect localRect)
        {
            Rect screenRect = GUIUtility.GUIToScreenRect(localRect);
            Vector2 screenMousePosition = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            return screenRect.Contains(screenMousePosition);
        }

        private void DrawActionBackground(
            Rect row,
            IAction action,
            bool selected,
            bool hovered)
        {
            Color background = EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f, 1f)
                : new Color(0.78f, 0.78f, 0.78f, 1f);
            EditorGUI.DrawRect(row, background);
            Color tint = BlackboardEditorDisplay.GetTint(action);
            tint.a = GetActionRowAlpha(selected, hovered);
            EditorGUI.DrawRect(row, tint);
            GUI.Box(row, GUIContent.none);
        }

        private void DrawActionRow(
            BlackboardAuthoringController controller,
            BlackboardBehaviour behaviour,
            ActionTrackDefinition track,
            IAction action,
            int index,
            Rect row,
            bool hovered)
        {
            Rect toggleRect = new Rect(row.x + 4f, row.y + 5f, 18f, 18f);
            DrawActionEnabled(controller, action, toggleRect);
            Rect labelRect = new Rect(
                toggleRect.xMax + 3f,
                row.y + 4f,
                row.width - (hovered ? 134f : 68f),
                20f);
            GUI.Label(labelRect, GetActionLabel(action), EditorStyles.boldLabel);
            DrawActionStatus(behaviour, action, row, hovered);
            DrawActionButtons(
                controller,
                track,
                action,
                index,
                row,
                ShouldShowActionControls(hovered));
        }

        private void DrawActionEnabled(BlackboardAuthoringController controller, IAction action, Rect toggleRect)
        {
            if (!(action is ActionDefinition definition))
            {
                return;
            }

            bool enabled = EditorGUI.Toggle(toggleRect, definition.Enabled);
            if (enabled != definition.Enabled)
            {
                controller.RecordSerializedChange("Toggle Blackboard Action");
                definition.Enabled = enabled;
                controller.CompleteSerializedChange();
            }
        }

        private string GetActionLabel(IAction action)
        {
            string name = BlackboardEditorDisplay.GetName(action.GetType());
            string summary = BlackboardEditorDisplay.GetSummary(action);
            return string.Equals(name, summary, StringComparison.OrdinalIgnoreCase) ? name : $"{name}: {summary}";
        }

        private void DrawActionStatus(
            BlackboardBehaviour behaviour,
            IAction action,
            Rect row,
            bool hovered)
        {
            if (behaviour != null && feedback.TryGetActionStatus(behaviour, action.DefinitionId, out ActionExecutionStatus status))
            {
                Rect statusRect = new Rect(
                    row.xMax - (hovered ? 114f : 48f),
                    row.y + 5f,
                    46f,
                    18f);
                GUI.Label(statusRect, status.ToString(), EditorStyles.miniLabel);
            }
        }

        private void DrawActionButtons(
            BlackboardAuthoringController controller,
            ActionTrackDefinition track,
            IAction action,
            int index,
            Rect row,
            bool visible)
        {
            Rect upRect = new Rect(row.xMax - 66f, row.y + 4f, 20f, 20f);
            Rect downRect = new Rect(row.xMax - 44f, row.y + 4f, 20f, 20f);
            Rect deleteRect = new Rect(row.xMax - 22f, row.y + 4f, 20f, 20f);
            GUIStyle buttonStyle = visible ? EditorStyles.miniButton : GUIStyle.none;
            GUIContent upContent = visible
                ? new GUIContent("↑", "Move Action Up")
                : GUIContent.none;
            GUIContent downContent = visible
                ? new GUIContent("↓", "Move Action Down")
                : GUIContent.none;
            GUIContent deleteContent = visible
                ? new GUIContent("×", "Delete Action")
                : GUIContent.none;
            using (new EditorGUI.DisabledScope(!visible || index == 0))
            {
                if (GUI.Button(
                    upRect,
                    upContent,
                    buttonStyle))
                {
                    controller.MoveAction(action.DefinitionId, index - 1);
                }
            }

            using (new EditorGUI.DisabledScope(
                       !visible ||
                       index + 1 >= track.ActionList.Actions.Count))
            {
                if (GUI.Button(
                    downRect,
                    downContent,
                    buttonStyle))
                {
                    controller.MoveAction(action.DefinitionId, index + 1);
                }
            }

            using (new EditorGUI.DisabledScope(!visible))
            {
                if (GUI.Button(
                    deleteRect,
                    deleteContent,
                    buttonStyle))
                {
                    controller.RemoveAction(action.DefinitionId);
                    GUIUtility.ExitGUI();
                }
            }
        }

        private void HandleActionRowEvent(BlackboardAuthoringController controller, BlackboardBehaviour behaviour, ActionTrackDefinition track, IAction action, int index, Rect row)
        {
            Event current = Event.current;
            if (current.type == EventType.MouseDown && current.button == 0 && row.Contains(current.mousePosition))
            {
                SelectAction(controller, track, action, current);
                PrepareActionDrag(action, current.mousePosition);
                current.Use();
            }
            else if (current.type == EventType.ContextClick && row.Contains(current.mousePosition))
            {
                ShowActionContextMenu(controller, behaviour, track, action);
                current.Use();
            }
            else if ((current.type == EventType.DragUpdated || current.type == EventType.DragPerform) && row.Contains(current.mousePosition))
            {
                AcceptActionDrop(controller, track, index, current);
            }
        }

        private void SelectAction(BlackboardAuthoringController controller, ActionTrackDefinition track, IAction action, Event current)
        {
            ReleasePreviewTextFocus();
            if (current.shift)
            {
                controller.SelectActionRange(track.DefinitionId, action.DefinitionId);
            }
            else if (current.command || current.control)
            {
                controller.ToggleActionSelection(track.DefinitionId, action.DefinitionId);
            }
            else
            {
                controller.SelectOnlyAction(track.DefinitionId, action.DefinitionId);
            }
        }

        private void PrepareActionDrag(IAction action, Vector2 mousePosition)
        {
            pendingActionDragId = action.DefinitionId.Value;
            pendingActionDragName = BlackboardEditorDisplay.GetName(action.GetType());
            pendingDragStart = mousePosition;
        }

        private void HandlePreparedActionDrag()
        {
            if (string.IsNullOrWhiteSpace(pendingActionDragId))
            {
                return;
            }

            Event current = Event.current;
            if (current.type == EventType.MouseUp)
            {
                ClearPreparedActionDrag();
                return;
            }

            if (current.type != EventType.MouseDrag || Vector2.Distance(pendingDragStart, current.mousePosition) < 4f)
            {
                return;
            }

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData(k_actionDragKey, pendingActionDragId);
            DragAndDrop.StartDrag(pendingActionDragName);
            ClearPreparedActionDrag();
            current.Use();
        }

        private void ClearPreparedActionDrag()
        {
            pendingActionDragId = null;
            pendingActionDragName = null;
        }

        private void AcceptActionDrop(BlackboardAuthoringController controller, ActionTrackDefinition track, int index, Event current)
        {
            string id = DragAndDrop.GetGenericData(k_actionDragKey) as string;
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            if (current.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                controller.MoveActionToTrack(new DefinitionId(id), track.DefinitionId, index);
                DragAndDrop.SetGenericData(k_actionDragKey, null);
            }

            current.Use();
        }

        private void DrawTrackDropTarget(BlackboardAuthoringController controller, ActionTrackDefinition track)
        {
            Rect target = GUILayoutUtility.GetRect(0f, 8f, GUILayout.ExpandWidth(true));
            Event current = Event.current;
            if ((current.type == EventType.DragUpdated || current.type == EventType.DragPerform) && target.Contains(current.mousePosition))
            {
                AcceptActionDrop(controller, track, track.ActionList.Actions.Count, current);
            }
        }

        private void DrawActionDetails(BlackboardAuthoringController controller, ActionTrackDefinition track, IAction action)
        {
            EditorGUI.indentLevel++;
            DrawActionMetadata(controller, track, action);
            DrawInterruptionTargets(controller, track, action);
            if (BlackboardSerializedPropertyRenderer.DrawManagedReference(controller.Owner, action, s_actionHiddenFields, action, controller.Definition))
            {
                controller.CompleteSerializedChange();
            }

            EditorGUI.indentLevel--;
        }

        private void DrawActionMetadata(BlackboardAuthoringController controller, ActionTrackDefinition track, IAction action)
        {
            if (!(action is ActionDefinition definition))
            {
                return;
            }

            float utility = definition.Utility;
            float weight = definition.Weight;
            bool weightOverride = definition.HasWeightOverride;
            bool block = definition.BlockDuringExecution;
            if (track.ActionList.ExecutionMethod == ActionListExecutionMethod.UtilitySelector)
            {
                utility = EditorGUILayout.FloatField("Utility", utility);
                block = EditorGUILayout.Toggle(
                    "Block During Execution",
                    block);
            }

            if (track.ActionList.OrderMode == ActionListOrderMode.Random)
            {
                weightOverride = EditorGUILayout.Toggle("Override Weight", weightOverride);
                weight = EditorGUILayout.Slider("Weight", weight, 0f, 100f);
            }

            ApplyActionMetadata(controller, definition, utility, weight, weightOverride, block);
        }

        private void ApplyActionMetadata(BlackboardAuthoringController controller, ActionDefinition definition, float utility, float weight, bool weightOverride, bool block)
        {
            if (Mathf.Approximately(utility, definition.Utility) && Mathf.Approximately(weight, definition.Weight) && weightOverride == definition.HasWeightOverride && block == definition.BlockDuringExecution)
            {
                return;
            }

            controller.RecordSerializedChange("Edit Blackboard Action Metadata");
            definition.Utility = utility;
            definition.Weight = weight;
            definition.HasWeightOverride = weightOverride;
            definition.BlockDuringExecution = block;
            controller.CompleteSerializedChange();
        }

        private void DrawInterruptionTargets(BlackboardAuthoringController controller, ActionTrackDefinition track, IAction action)
        {
            System.Reflection.PropertyInfo property = action.GetType().GetProperty("TargetActionIds");
            if (!(property?.GetValue(action) is IList<string> targets))
            {
                return;
            }

            EditorGUILayout.LabelField("Interrupt Actions", EditorStyles.boldLabel);
            for (int index = 0; index < track.ActionList.Actions.Count; index++)
            {
                IAction candidate = track.ActionList.Actions[index];
                if (candidate == null || candidate.DefinitionId == action.DefinitionId)
                {
                    continue;
                }

                bool selected = targets.Contains(candidate.DefinitionId.Value);
                bool next = EditorGUILayout.ToggleLeft(BlackboardEditorDisplay.GetName(candidate.GetType()), selected);
                if (next != selected)
                {
                    controller.RecordSerializedChange("Edit Interruption Targets");
                    if (next)
                    {
                        targets.Add(candidate.DefinitionId.Value);
                    }
                    else
                    {
                        targets.Remove(candidate.DefinitionId.Value);
                    }

                    controller.CompleteSerializedChange();
                }
            }

            for (int index = targets.Count - 1; index >= 0; index--)
            {
                string id = targets[index];
                if (!ContainsAction(track, id))
                {
                    EditorGUILayout.HelpBox($"Missing interruption target: {id}", MessageType.Warning);
                }
            }
        }

        private bool ContainsAction(ActionTrackDefinition track, string definitionId)
        {
            for (int index = 0; index < track.ActionList.Actions.Count; index++)
            {
                IAction action = track.ActionList.Actions[index];
                if (action != null && action.DefinitionId.Value == definitionId)
                {
                    return true;
                }
            }

            return false;
        }

        private void ShowActionContextMenu(BlackboardAuthoringController controller, BlackboardBehaviour behaviour, ActionTrackDefinition track, IAction action)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy"), false, () => controller.CopyAction(action.DefinitionId));
            menu.AddItem(new GUIContent("Duplicate"), false, () => controller.DuplicateAction(action.DefinitionId));
            bool deleteSelection = controller.Metadata.SelectedActionIds.Contains(action.DefinitionId) &&
                controller.Metadata.SelectedActionIds.Count > 1;
            menu.AddItem(
                new GUIContent(deleteSelection ? "Delete Selected" : "Delete"),
                false,
                deleteSelection
                    ? controller.RemoveSelectedActions
                    : () => controller.RemoveAction(action.DefinitionId));
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Select All"), false, () => controller.SelectAllActions(track.DefinitionId));
            menu.AddItem(new GUIContent("Select None"), false, controller.ClearActionSelection);
            menu.AddSeparator(string.Empty);
            BlockDefinition block = controller.GetOwningBlockForAction(action.DefinitionId);
            if (block != null && execution.CanControl(behaviour, out _))
            {
                menu.AddItem(
                    new GUIContent("Play From Start"),
                    false,
                    () => execution.Execute(
                        behaviour,
                        block.DefinitionId));
                if (BlackboardEditorExecutionController
                    .TryResolveActionStart(
                        controller,
                        action.DefinitionId,
                        out DefinitionId selectedBlockId,
                        out int taskIndex))
                {
                    menu.AddItem(
                        new GUIContent("Play From Selected"),
                        false,
                        () => execution.ExecuteFromAction(
                            behaviour,
                            selectedBlockId,
                            taskIndex));
                }
                else
                {
                    menu.AddDisabledItem(
                        new GUIContent("Play From Selected"));
                }
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Play From Start"));
                menu.AddDisabledItem(new GUIContent("Play From Selected"));
            }

            menu.ShowAsContext();
        }

        private void DrawTrackFooter(BlackboardAuthoringController controller, ActionTrackDefinition track)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Action"))
            {
                ShowActionDropdown(controller, track, GUILayoutUtility.GetLastRect());
            }

            using (new EditorGUI.DisabledScope(!controller.Clipboard.HasAction))
            {
                if (GUILayout.Button("Paste"))
                {
                    controller.PasteAction(track.DefinitionId);
                }
            }

            using (new EditorGUI.DisabledScope(controller.Metadata.SelectedActionIds.Count == 0))
            {
                if (GUILayout.Button("Group"))
                {
                    controller.GroupActions(track.DefinitionId, controller.Metadata.SelectedActionIds, "Group");
                }
            }

            EditorGUILayout.EndHorizontal();
            DrawGroups(controller, track);
        }

        private void ShowActionDropdown(BlackboardAuthoringController controller, ActionTrackDefinition track, Rect buttonRect)
        {
            IReadOnlyList<Type> types = BlackboardManagedTypeCatalog.GetActionTypes();
            BlackboardTypeDropdown dropdown = new BlackboardTypeDropdown(dropdownState, "Add Action", types, type => controller.AddAction(track.DefinitionId, type));
            dropdown.Show(buttonRect);
        }

        private void DrawGroups(BlackboardAuthoringController controller, ActionTrackDefinition track)
        {
            for (int index = 0; index < controller.Metadata.ActionGroups.Count; index++)
            {
                ActionGroupAuthoringMetadata group = controller.Metadata.ActionGroups[index];
                if (group.TrackId == track.DefinitionId)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"Group: {group.Name} ({group.ActionIds.Count})", EditorStyles.miniLabel);
                    if (GUILayout.Button("Ungroup", EditorStyles.miniButton, GUILayout.Width(60f)))
                    {
                        controller.UngroupActions(group.GroupId);
                        GUIUtility.ExitGUI();
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void DrawVariables(BlackboardAuthoringController controller)
        {
            DrawVariableHeader(controller);
            for (int index = 0; index < controller.Definition.Variables.Count; index++)
            {
                VariableDefinitionBase variable = controller.Definition.Variables[index];
                if (variable != null)
                {
                    DrawVariable(controller, variable, index);
                }
            }
        }

        private void DrawVariableHeader(BlackboardAuthoringController controller)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Variables", EditorStyles.boldLabel);
            if (GUILayout.Button("Add", GUILayout.Width(50f)))
            {
                ShowVariableDropdown(controller, GUILayoutUtility.GetLastRect());
            }

            if (GUILayout.Button("Sort", GUILayout.Width(50f)))
            {
                ShowVariableSortMenu(controller);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ShowVariableDropdown(BlackboardAuthoringController controller, Rect buttonRect)
        {
            IReadOnlyList<Type> types = BlackboardManagedTypeCatalog.GetVariableTypes();
            BlackboardTypeDropdown dropdown = new BlackboardTypeDropdown(dropdownState, "Add Variable", types, type => controller.AddVariable(type));
            dropdown.Show(buttonRect);
        }

        private void ShowVariableSortMenu(BlackboardAuthoringController controller)
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("By Name"), false, controller.SortVariablesByName);
            menu.AddItem(new GUIContent("By Type"), false, controller.SortVariablesByType);
            menu.ShowAsContext();
        }

        private void DrawVariable(BlackboardAuthoringController controller, VariableDefinitionBase variable, int index)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(BlackboardEditorDisplay.GetName(variable.GetType()), EditorStyles.boldLabel);
            string key = EditorGUILayout.TextField("Name", variable.Key);
            VariableScope scope = (VariableScope)EditorGUILayout.EnumPopup("Scope", variable.Scope);
            ApplyVariableHeader(controller, variable, key, scope);
            BlackboardSerializedPropertyRenderer.DrawManagedReference(controller.Owner, variable, s_variableHiddenFields);
            DrawVariableButtons(controller, variable, index);
            EditorGUILayout.EndVertical();
        }

        private void ApplyVariableHeader(BlackboardAuthoringController controller, VariableDefinitionBase variable, string key, VariableScope scope)
        {
            if (string.Equals(key, variable.Key, StringComparison.Ordinal) && scope == variable.Scope)
            {
                return;
            }

            controller.RecordSerializedChange("Edit Blackboard Variable");
            variable.Key = key;
            variable.Scope = scope;
            controller.CompleteSerializedChange();
        }

        private void DrawVariableButtons(BlackboardAuthoringController controller, VariableDefinitionBase variable, int index)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Duplicate"))
            {
                controller.DuplicateVariable(variable.DefinitionId);
            }

            using (new EditorGUI.DisabledScope(index == 0))
            {
                if (GUILayout.Button("↑", GUILayout.Width(30f)))
                {
                    controller.MoveVariable(variable.DefinitionId, index - 1);
                }
            }

            using (new EditorGUI.DisabledScope(index + 1 >= controller.Definition.Variables.Count))
            {
                if (GUILayout.Button("↓", GUILayout.Width(30f)))
                {
                    controller.MoveVariable(variable.DefinitionId, index + 1);
                }
            }

            if (GUILayout.Button("Delete"))
            {
                controller.RemoveVariable(variable.DefinitionId);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
