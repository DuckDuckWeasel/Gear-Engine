
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
        /// <summary>
        /// If true, scrolls to the currently selected command in the inspector when the editor is redrawn. A
        /// Automatically resets to false.
        /// </summary>
        public static bool ScrollToCommandOnDraw = false;

        protected SerializedProperty _arrayProperty;
        protected ReorderableList list;
        protected Block block;
        protected GUIStyle summaryStyle, commandLabelStyle;

        private readonly List<NestedActionDropTarget> _nestedActionDropTargets = new List<NestedActionDropTarget>();
        private readonly List<StandaloneGroupDropTarget> _standaloneGroupDropTargets = new List<StandaloneGroupDropTarget>();
        private readonly List<InvokeActionDropTarget> _invokeActionDropTargets = new List<InvokeActionDropTarget>();
        private readonly HashSet<int> _collapsedInvokeActionGroups = new HashSet<int>();
        private NestedActionDrag _nestedActionDrag;
        private NestedActionDrag _pendingNestedActionDrag;
        private StandaloneActionDrag _standaloneActionDrag;
        private StandaloneActionDrag _pendingStandaloneActionDrag;

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
                throw new ArgumentNullException("Array property was null.");
            if (!arrayProperty.isArray)
                throw new InvalidOperationException("Specified serialized propery is not an array.");

            this._arrayProperty = arrayProperty;
            this.block = _block;

            list = new ReorderableList(arrayProperty.serializedObject, arrayProperty, true, true, false, false);
            list.drawHeaderCallback = DrawHeader;
            list.drawElementCallback = DrawItem;
            list.elementHeightCallback = GetItemHeight;
            list.onSelectCallback = SelectChanged;
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
                    summaryStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f, 1f);
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
                _standaloneGroupDropTargets.Clear();
                _invokeActionDropTargets.Clear();
                list.DoLayoutList();
                HandleNestedActionDrag(Event.current);
                HandleStandaloneActionDrag(Event.current);
            }
        }

        private void SelectChanged(ReorderableList list)
        {
            Command command = this[list.index].objectReferenceValue as Command;
            if (command is InvokeActionCommand invokeAction)
            {
                InvokeActionEditorSelection.Clear(invokeAction);
            }
            var flowchart = (Flowchart)command.GetFlowchart();
            BlockEditor.actionList.Add(delegate
            {
                flowchart.ClearSelectedCommands();
                flowchart.AddSelectedCommand(command);
            });
        }

        private void DrawHeader(Rect rect)
        {
            if (rect.width < 0) return;
            EditorGUI.LabelField(rect, new GUIContent("Commands"));
        }

        public void DrawItem(Rect position, int index, bool selected, bool focused)
        {
            if (position.width < 0) return;

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

            var flowchart = (Flowchart)command.GetFlowchart();
            if (flowchart == null)
            {
                return;
            }

            bool commandIsSelected = flowchart.SelectedCommands.Contains(command);
            if (command is InvokeActionCommand invokeAction)
            {
                if (HasNestedActionGroup(invokeAction))
                {
                    DrawInvokeActionGroup(position, invokeAction, commandIsSelected, index, flowchart);
                }
                else
                {
                    DrawSingleInvokeAction(position, invokeAction, commandIsSelected, index, flowchart);
                }
                HandleCommandSelection(position, index, command, flowchart);
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
            foreach (Command selectedCommand in flowchart.SelectedCommands)
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

            // There's a weird incompatibility between the Reorderable list control used for the command list and 
            // the UnityEvent list control used in some commands. In play mode, if you click on the reordering grabber
            // for a command in the list it causes the UnityEvent list to spew null exception errors.
            // The workaround for now is to hide the reordering grabber from mouse clicks by extending the command
            // selection rectangle to cover it. We are planning to totally replace the command list display system.
            Rect clickRect = position;
            //clickRect.x -= 20;
            //clickRect.width += 20;

            HandleCommandSelection(clickRect, index, command, flowchart);

            Color commandLabelColor = Color.white;
            if (flowchart.ColorCommands)
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
            if (!commandIsSelected && flowchart.ColorCommands == false && EditorGUIUtility.isProSkin)
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
                if (flowchart.ShowLineNumbers)
                {
                    commandNameLabel = command.CommandIndex.ToString() + ": " + commandName;
                }
                else
                {
                    commandNameLabel = commandName;
                }

                GUI.Label(commandLabelRect, commandNameLabel, commandLabelStyle);
            }

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
            
            GUI.backgroundColor = Color.white;
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

        private void DrawInvokeActionGroup(Rect position, InvokeActionCommand invokeAction, bool isSelected, int commandIndex, Flowchart flowchart)
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
            bool allowsNestedActionDrag = isConditionalGroup || actionCount > 1;
            string executionMethod = invokeAction.ExecutionMethod == InvokeActionExecutionMethod.AllAtSameTime
                ? "All At Same Time"
                : "Sequence";
            CommandInfoAttribute commandInfo = CommandEditor.GetCommandInfo(invokeAction.GetType());
            string commandName = commandInfo != null ? commandInfo.CommandName : invokeAction.GetType().Name;
            string groupName = isConditionalGroup
                ? InvokeActionEditorUtility.GetDisplayName(invokeAction.actions[0])
                : commandName;
            bool isExpanded = !IsInvokeActionGroupCollapsed(invokeAction);

            Rect headerRect = new Rect(position.x, position.y, position.width, headerHeight);
            // The entire group is a destination. This keeps the target easy to acquire when
            // the group is collapsed or its action list is empty.
            _invokeActionDropTargets.Add(new InvokeActionDropTarget(position, invokeAction));
            DrawInvokeActionRowBackground(headerRect);
            Rect collapseRect = new Rect(headerRect.x + 24f, headerRect.y, 18f, headerRect.height);
            bool expanded = EditorGUI.Foldout(collapseRect, isExpanded, GUIContent.none, false);
            if (expanded != isExpanded)
            {
                SetInvokeActionGroupCollapsed(invokeAction, !expanded);
                isExpanded = expanded;
            }

            Rect parentToggleRect = new Rect(headerRect.xMax - 22f, headerRect.y + 2f, 18f, EditorGUIUtility.singleLineHeight);
            DrawInvokeActionEnabledToggle(invokeAction, parentToggleRect);
            string headerLabel = $"{groupName} • {executionMethod}";
            EditorGUI.LabelField(new Rect(headerRect.x + 44f, headerRect.y, headerRect.width - 70f, headerRect.height), headerLabel, EditorStyles.boldLabel);
            ShowInvokeActionContextMenu(headerRect, invokeAction, flowchart);

            // Dropping an action on the left side of a group creates a new standalone Invoke
            // Action before or after this command, which is the "move outside" affordance.
            Rect standaloneDropRect = new Rect(position.x, position.y, 36f, position.height);
            _standaloneGroupDropTargets.Add(new StandaloneGroupDropTarget(standaloneDropRect, commandIndex));
            if (_nestedActionDrag != null && standaloneDropRect.Contains(Event.current.mousePosition))
            {
                float insertionY = Event.current.mousePosition.y < standaloneDropRect.center.y
                    ? position.y
                    : position.yMax - 2f;
                EditorGUI.DrawRect(standaloneDropRect, new Color(1f, 1f, 1f, 0.15f));
                EditorGUI.DrawRect(new Rect(position.x, insertionY, position.width, 2f), Color.white);
            }

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
                _nestedActionDropTargets.Add(new NestedActionDropTarget(actionRect, invokeAction, actionIndex));
                Rect toggleRect = new Rect(actionRect.xMax - 20f, actionRect.y + 2f, 18f, EditorGUIUtility.singleLineHeight);

                string actionName = actionIndex < actionCount
                    ? InvokeActionEditorUtility.GetDisplayName(invokeAction.actions[actionIndex])
                    : string.Empty;
                bool enabled = actionIndex < actionCount &&
                               invokeAction.IsActionEnabled(actionIndex);
                if (actionIndex < actionCount)
                {
                    DrawNestedActionDragHandle(actionRect);
                    using (new EditorGUI.DisabledScope(!invokeAction.enabled))
                    {
                        DrawNestedActionEnabledToggle(invokeAction, actionIndex, enabled, toggleRect);
                    }
                    if (allowsNestedActionDrag)
                    {
                        TryBeginNestedActionDrag(invokeAction, actionIndex, actionRect, toggleRect, commandIndex, flowchart);
                    }
                    using (new EditorGUI.DisabledScope(!invokeAction.enabled || !enabled))
                    {
                        DrawNestedActionLabel(actionRect, actionName, isConditionalGroup, actionTextXOffset + 20f);
                    }
                }

                DrawNestedActionInsertionIndicator(actionRect);
                DrawStandaloneActionInsertionIndicator(actionRect, invokeAction);
            }

            DrawStandaloneActionDropHighlight(position, invokeAction);
        }

        private void DrawSingleInvokeAction(Rect position, InvokeActionCommand invokeAction, bool isSelected, int commandIndex, Flowchart flowchart)
        {
            bool hasAction = invokeAction.actions != null && invokeAction.actions.Count == 1;
            string actionName = hasAction
                ? InvokeActionEditorUtility.GetDisplayName(invokeAction.actions[0])
                : "Invoke Action";
            Rect actionRect = new Rect(position.x, position.y - 2f, position.width, position.height + 5f);
            _invokeActionDropTargets.Add(new InvokeActionDropTarget(actionRect, invokeAction));
            DrawInvokeActionRowBackground(actionRect);
            using (new EditorGUI.DisabledScope(!invokeAction.enabled))
            {
                EditorGUI.LabelField(new Rect(actionRect.x + 6f, actionRect.y, actionRect.width - 32f, actionRect.height), actionName, EditorStyles.boldLabel);
            }

            Rect parentToggleRect = new Rect(actionRect.xMax - 20f, actionRect.y + 2f, 18f, EditorGUIUtility.singleLineHeight);
            DrawInvokeActionEnabledToggle(invokeAction, parentToggleRect);
            TryBeginStandaloneActionDrag(invokeAction, actionRect, parentToggleRect, commandIndex, flowchart);
            DrawNestedActionInsertionIndicator(actionRect);
            DrawStandaloneActionDropHighlight(actionRect, invokeAction);

            Rect standaloneDropRect = new Rect(position.x, position.y, 36f, position.height);
            _standaloneGroupDropTargets.Add(new StandaloneGroupDropTarget(standaloneDropRect, commandIndex));
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
                   invokeAction.actions[0] is ActionBase action &&
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

        private void DrawNestedActionLabel(Rect actionRect, string actionName, bool useDefaultLayout, float labelOffset)
        {
            if (useDefaultLayout)
            {
                EditorGUI.LabelField(new Rect(actionRect.x + 26f, actionRect.y, actionRect.width - 26f, actionRect.height), actionName, EditorStyles.boldLabel);
                return;
            }

            EditorGUI.LabelField(new Rect(actionRect.x + labelOffset, actionRect.y, actionRect.width - labelOffset, actionRect.height), actionName);
        }

        private static void DrawNestedActionDragHandle(Rect actionRect)
        {
            Rect dragHandleRect = new Rect(actionRect.x + 5f, actionRect.y, 16f, actionRect.height);
            EditorGUI.LabelField(dragHandleRect, "≡", EditorStyles.centeredGreyMiniLabel);
            EditorGUIUtility.AddCursorRect(dragHandleRect, MouseCursor.Pan);
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

        private void ShowInvokeActionContextMenu(Rect headerRect, InvokeActionCommand invokeAction, Flowchart flowchart)
        {
            if (Event.current.type != EventType.MouseUp ||
                Event.current.button != 1 ||
                !headerRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Convert to Block"), false, () => ConvertInvokeActionToBlock(invokeAction, flowchart));
            menu.ShowAsContext();
            Event.current.Use();
        }

        private void ConvertInvokeActionToBlock(InvokeActionCommand invokeAction, Flowchart flowchart)
        {
            if (invokeAction == null || flowchart == null || block == null ||
                !block.CommandList.Contains(invokeAction))
            {
                return;
            }

            try
            {
                Undo.RecordObject(block, "Convert Invoke Action To Block");
                Undo.RecordObject(invokeAction, "Convert Invoke Action To Block");

                var newBlock = Undo.AddComponent<Block>(flowchart.gameObject);
                Undo.RecordObject(newBlock, "Convert Invoke Action To Block");
                newBlock._NodeRect = new Rect(block._NodeRect.position + new Vector2(240f, 0f), Vector2.zero);
                newBlock.BlockName = flowchart.GetUniqueBlockKey($"{block.BlockName} Actions", newBlock);
                newBlock.ItemId = flowchart.NextItemId();

                invokeAction.OnCommandRemoved(block);
                block.CommandList.Remove(invokeAction);
                newBlock.CommandList.Add(invokeAction);
                invokeAction.ParentBlock = newBlock;
                invokeAction.OnCommandAdded(newBlock);

                flowchart.ClearSelectedCommands();
                flowchart.SelectedBlock = newBlock;
                flowchart.AddSelectedCommand(invokeAction);

                PrefabUtility.RecordPrefabInstancePropertyModifications(block);
                PrefabUtility.RecordPrefabInstancePropertyModifications(newBlock);
                PrefabUtility.RecordPrefabInstancePropertyModifications(invokeAction);
                EditorUtility.SetDirty(block);
                EditorUtility.SetDirty(newBlock);
                EditorUtility.SetDirty(invokeAction);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to convert Invoke Action to a new Block: {exception}");
            }
        }

        private void TryBeginNestedActionDrag(InvokeActionCommand invokeAction, int actionIndex, Rect actionRect, Rect toggleRect, int commandIndex, Flowchart flowchart)
        {
            if (_nestedActionDrag != null)
            {
                return;
            }

            if (Event.current.type == EventType.MouseDrag &&
                _pendingNestedActionDrag != null &&
                _pendingNestedActionDrag.Matches(invokeAction, actionIndex))
            {
                _nestedActionDrag = _pendingNestedActionDrag;
                _pendingNestedActionDrag = null;
                Event.current.Use();
                return;
            }

            if (Event.current.type != EventType.MouseDown ||
                Event.current.button != 0 ||
                !actionRect.Contains(Event.current.mousePosition) ||
                toggleRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            SelectInvokeAction(invokeAction, actionIndex, commandIndex, flowchart);
            _pendingNestedActionDrag = new NestedActionDrag(invokeAction, actionIndex);
            Event.current.Use();
        }

        private void SelectInvokeAction(InvokeActionCommand invokeAction, int actionIndex, int commandIndex, Flowchart flowchart)
        {
            if (flowchart == null)
            {
                return;
            }

            InvokeActionEditorSelection.Select(invokeAction, actionIndex);
            list.index = commandIndex;
            BlockEditor.actionList.Add(delegate
            {
                flowchart.ClearSelectedCommands();
                flowchart.AddSelectedCommand(invokeAction);
            });
        }

        private void HandleNestedActionDrag(Event currentEvent)
        {
            if (_nestedActionDrag == null)
            {
                if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
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

            if (TryGetStandaloneGroupDropTarget(currentEvent.mousePosition, out int commandInsertIndex))
            {
                MoveNestedActionToStandaloneGroup(_nestedActionDrag, commandInsertIndex);
            }
            else if (TryGetNestedActionDropTarget(currentEvent.mousePosition, out InvokeActionCommand destination, out int destinationIndex))
            {
                MoveNestedAction(_nestedActionDrag, destination, destinationIndex);
            }
            else if (TryGetInvokeActionDropTarget(currentEvent.mousePosition, _nestedActionDrag.InvokeAction, out InvokeActionCommand groupDestination))
            {
                MoveNestedAction(_nestedActionDrag, groupDestination, groupDestination.actions?.Count ?? 0);
            }

            _nestedActionDrag = null;
            _pendingNestedActionDrag = null;
            currentEvent.Use();
        }

        private void TryBeginStandaloneActionDrag(InvokeActionCommand invokeAction, Rect actionRect, Rect toggleRect, int commandIndex, Flowchart flowchart)
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
                !actionRect.Contains(Event.current.mousePosition) ||
                toggleRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            SelectInvokeAction(invokeAction, 0, commandIndex, flowchart);
            _pendingStandaloneActionDrag = new StandaloneActionDrag(invokeAction, Event.current.mousePosition);
        }

        private void HandleStandaloneActionDrag(Event currentEvent)
        {
            if (_standaloneActionDrag == null)
            {
                if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
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
                TryGetInvokeActionDropTarget(currentEvent.mousePosition, _standaloneActionDrag.InvokeAction, out destination))
            {
                MoveStandaloneActionIntoInvokeGroup(_standaloneActionDrag.InvokeAction, destination, destination.actions?.Count ?? 0);
            }

            if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
            {
                _standaloneActionDrag = null;
                _pendingStandaloneActionDrag = null;
                currentEvent.Use();
            }
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
            GUI.Label(labelRect, "Drop action into Invoke Action", EditorStyles.centeredGreyMiniLabel);
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

        private bool TryGetInvokeActionDropTarget(Vector2 mousePosition, InvokeActionCommand source, out InvokeActionCommand destination)
        {
            foreach (var target in _invokeActionDropTargets)
            {
                if (target.InvokeAction == source || !target.Rect.Contains(mousePosition))
                {
                    continue;
                }

                destination = target.InvokeAction;
                return true;
            }

            destination = null;
            return false;
        }

        private bool TryGetNestedActionDropTarget(Vector2 mousePosition, out InvokeActionCommand destination, out int destinationIndex)
        {
            foreach (var target in _nestedActionDropTargets)
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

        private bool TryGetStandaloneGroupDropTarget(Vector2 mousePosition, out int commandInsertIndex)
        {
            foreach (var target in _standaloneGroupDropTargets)
            {
                if (!target.Rect.Contains(mousePosition))
                {
                    continue;
                }

                commandInsertIndex = mousePosition.y < target.Rect.center.y
                    ? target.CommandIndex
                    : target.CommandIndex + 1;
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
            if (!source.TryRemoveAction(0, out var action, out bool enabled))
            {
                return;
            }

            destinationIndex = Mathf.Clamp(destinationIndex, 0, destination.actions.Count);
            destination.InsertAction(destinationIndex, action, enabled);
            RemoveEmptyInvokeAction(source);
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

            if (!drag.InvokeAction.TryRemoveAction(drag.ActionIndex, out var action, out bool enabled))
            {
                return;
            }

            destination.InsertAction(destinationIndex, action, enabled);
            RemoveEmptyInvokeAction(drag.InvokeAction);
            RecordInvokeActionChanges(drag.InvokeAction, destination);
        }

        private void MoveNestedActionToStandaloneGroup(NestedActionDrag drag, int commandInsertIndex)
        {
            if (drag.InvokeAction == null ||
                drag.ActionIndex < 0 || drag.ActionIndex >= drag.InvokeAction.actions.Count)
            {
                return;
            }

            var flowchart = drag.InvokeAction.GetFlowchart();
            if (flowchart == null)
            {
                return;
            }

            Undo.RecordObject(block, "Move Invoke Action Outside Group");
            Undo.RecordObject(drag.InvokeAction, "Move Invoke Action Outside Group");
            if (!drag.InvokeAction.TryRemoveAction(drag.ActionIndex, out var action, out bool enabled))
            {
                return;
            }

            var standaloneGroup = Undo.AddComponent<InvokeActionCommand>(flowchart.gameObject);
            standaloneGroup.ItemId = flowchart.NextItemId();
            standaloneGroup.ExecutionMethod = drag.InvokeAction.ExecutionMethod;
            standaloneGroup.enabled = drag.InvokeAction.enabled;
            standaloneGroup.ParentBlock = block;
            standaloneGroup.InsertAction(0, action, enabled);

            commandInsertIndex = Mathf.Clamp(commandInsertIndex, 0, block.CommandList.Count);
            block.CommandList.Insert(commandInsertIndex, standaloneGroup);
            RemoveEmptyInvokeAction(drag.InvokeAction);
            RecordInvokeActionChanges(drag.InvokeAction, standaloneGroup);
        }

        private void RemoveEmptyInvokeAction(InvokeActionCommand invokeAction)
        {
            if (invokeAction == null || invokeAction.actions.Count > 0)
            {
                return;
            }

            block.CommandList.Remove(invokeAction);
            Undo.DestroyObjectImmediate(invokeAction);
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
            public NestedActionDrag(InvokeActionCommand invokeAction, int actionIndex)
            {
                InvokeAction = invokeAction;
                ActionIndex = actionIndex;
            }

            public InvokeActionCommand InvokeAction { get; }
            public int ActionIndex { get; }

            public bool Matches(InvokeActionCommand invokeAction, int actionIndex)
            {
                return InvokeAction == invokeAction && ActionIndex == actionIndex;
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
                Vector2 delta = currentPosition - StartPosition;
                return delta.sqrMagnitude >= 64f;
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

        private readonly struct StandaloneGroupDropTarget
        {
            public StandaloneGroupDropTarget(Rect rect, int commandIndex)
            {
                Rect = rect;
                CommandIndex = commandIndex;
            }

            public Rect Rect { get; }
            public int CommandIndex { get; }
        }

        private void HandleCommandSelection(Rect clickRect, int index, Command command, Flowchart flowchart)
        {
            if (Event.current.type != EventType.MouseDown ||
                Event.current.button != 0 ||
                !clickRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            if (flowchart.SelectedCommands.Contains(command))
            {
                if (!EditorGUI.actionKey && !Event.current.shift)
                {
                    BlockEditor.actionList.Add(delegate
                    {
                        flowchart.SelectedCommands.Remove(command);
                        flowchart.ClearSelectedCommands();
                    });
                }

                if (EditorGUI.actionKey)
                {
                    BlockEditor.actionList.Add(delegate { flowchart.SelectedCommands.Remove(command); });
                    Event.current.Use();
                }
            }
            else
            {
                bool shift = Event.current.shift;
                if (!shift && !EditorGUI.actionKey)
                {
                    BlockEditor.actionList.Add(delegate { flowchart.ClearSelectedCommands(); });
                    Event.current.Use();
                    list.index = index;
                }

                BlockEditor.actionList.Add(delegate { flowchart.AddSelectedCommand(command); });
                AddRangeSelection(command, flowchart, shift);
                Event.current.Use();
            }

            GUIUtility.keyboardControl = 0;
        }

        private static void AddRangeSelection(Command command, Flowchart flowchart, bool shift)
        {
            if (!shift || flowchart.SelectedBlock == null)
            {
                return;
            }

            int firstSelectedIndex = flowchart.SelectedBlock.CommandList.FindIndex(selectedCommand =>
                flowchart.SelectedCommands.Contains(selectedCommand));
            int lastSelectedIndex = flowchart.SelectedBlock.CommandList.FindLastIndex(selectedCommand =>
                flowchart.SelectedCommands.Contains(selectedCommand));

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
                Command selectedCommand = flowchart.SelectedBlock.CommandList[i];
                BlockEditor.actionList.Add(delegate { flowchart.AddSelectedCommand(selectedCommand); });
            }
        }
    }
}
