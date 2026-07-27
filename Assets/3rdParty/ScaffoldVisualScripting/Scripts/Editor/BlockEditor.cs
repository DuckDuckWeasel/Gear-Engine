
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Scaffold.EditorUtils
{
    [CustomEditor(typeof(Block))]
    public class BlockEditor : Editor
    {
        public static List<Action> actionList = new List<Action>();

        public static bool SelectedBlockDataStale { get; set; }

        protected Texture2D upIcon;
        protected Texture2D downIcon;
        protected Texture2D addIcon;
        protected Texture2D duplicateIcon;
        protected Texture2D deleteIcon;


        private CommandListAdaptor commandListAdaptor;
        private SerializedProperty commandListProperty;

        private Rect lastCMDpopupPos;

        private string callersString;
        private bool callersFoldout;
        private bool behaviourAndTimingFoldout = true;
        private Vector2 descriptionScrollPosition;


        protected virtual void OnEnable()
        {
            //this appears to happen when leaving playmode
            try
            {
                if (serializedObject == null)
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }

            upIcon = ScaffoldEditorResources.Up;
            downIcon = ScaffoldEditorResources.Down;
            addIcon = ScaffoldEditorResources.Add;
            duplicateIcon = ScaffoldEditorResources.Duplicate;
            deleteIcon = ScaffoldEditorResources.Delete;

            Block block = target as Block;

            // The classic command list used to be backed directly by the "commandList" field,
            // but commands now live in Tracks[0] (see Block.CommandList / Block.Tracks). Ensure
            // that track exists and bind to it, so this Inspector and the Scaffold Timeline
            // window both read/write the same list instead of two disconnected ones.
            block.EnsureTracksInitialized();
            serializedObject.Update();

            commandListProperty = GetPrimaryTrackCommandsProperty();
            commandListAdaptor = new CommandListAdaptor(block, commandListProperty);
        }

        private SerializedProperty GetPrimaryTrackCommandsProperty()
        {
            SerializedProperty tracksProperty = serializedObject.FindProperty("tracks");
            return tracksProperty.GetArrayElementAtIndex(0).FindPropertyRelative("commands");
        }

        protected void CacheCallerString()
        {
            if (!string.IsNullOrEmpty(callersString))
            {
                return;
            }

            Block targetBlock = target as Block;

            string[] callers = FindObjectsOfType<MonoBehaviour>()
                .Where(x => x is IBlockCaller)
                .Select(x => x as IBlockCaller)
                .Where(x => x.MayCallBlock(targetBlock))
                .Select(x => x.GetLocationIdentifier()).ToArray();

            if (callers != null && callers.Length > 0)
            {
                callersString = string.Join("\n", callers);
            }
            else
            {
                callersString = "None";
            }
        }

        public virtual void DrawBlockName(Blackboard blackboard)
        {
            serializedObject.Update();

            SerializedProperty blockNameProperty = serializedObject.FindProperty("blockName");
            SerializedProperty useCustomTintProp = serializedObject.FindProperty("useCustomTint");
            SerializedProperty tintProp = serializedObject.FindProperty("tint");
            SerializedProperty descriptionProp = serializedObject.FindProperty("description");

            EditorGUILayout.LabelField("Block Inspector", BlockInspectorStyleSheet.Title);
            EditorGUILayout.BeginVertical(BlockInspectorStyleSheet.IdentityCard);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(ScaffoldEditorResources.FlowGraph), GUILayout.Width(38f), GUILayout.Height(38f));
            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Block Name", BlockInspectorStyleSheet.FieldHeader);
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            blockNameProperty.stringValue = EditorGUILayout.TextField(blockNameProperty.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                // Ensure block name is unique for this Blackboard
                Block block = target as Block;
                string uniqueName = blackboard.GetUniqueBlockKey(blockNameProperty.stringValue, block);
                if (uniqueName != block.BlockName)
                {
                    blockNameProperty.stringValue = uniqueName;
                }
            }
            EditorGUI.BeginChangeCheck();
            Color tint = EditorGUILayout.ColorField(GUIContent.none, tintProp.colorValue, true, true, false, GUILayout.Width(42f));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Set Block Tint");
                tintProp.colorValue = tint;
                useCustomTintProp.boolValue = true;
                EditorUtility.SetDirty(target);
                SelectedBlockDataStale = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(BlockInspectorStyleSheet.InnerSpacing);
            EditorGUILayout.LabelField("Description", BlockInspectorStyleSheet.FieldHeader);
            DrawAutoGrowingDescription(descriptionProp);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(BlockInspectorStyleSheet.OuterSpacing);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAutoGrowingDescription(SerializedProperty descriptionProperty)
        {
            GUIStyle textAreaStyle = BlockInspectorStyleSheet.DescriptionTextArea;
            float fieldWidth = Mathf.Max(1f, EditorGUIUtility.currentViewWidth - 100f);
            float contentHeight = textAreaStyle.CalcHeight(new GUIContent(descriptionProperty.stringValue), fieldWidth);
            float lineHeight = EditorGUIUtility.singleLineHeight + textAreaStyle.padding.top + textAreaStyle.padding.bottom;
            BlockInspectorStyleSheet.DescriptionLayout layout = BlockInspectorStyleSheet.CalculateDescriptionLayout(contentHeight, lineHeight);

            EditorGUI.BeginChangeCheck();
            string description;
            if (layout.RequiresScroll)
            {
                descriptionScrollPosition.x = 0f;
                descriptionScrollPosition = EditorGUILayout.BeginScrollView(descriptionScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Height(layout.Height));
                description = EditorGUILayout.TextArea(descriptionProperty.stringValue, textAreaStyle, GUILayout.MinHeight(contentHeight));
                EditorGUILayout.EndScrollView();
            }
            else
            {
                description = EditorGUILayout.TextArea(descriptionProperty.stringValue, textAreaStyle, GUILayout.Height(layout.Height));
            }

            if (EditorGUI.EndChangeCheck())
            {
                descriptionProperty.stringValue = description;
                SelectedBlockDataStale = true;
            }
        }

        public virtual void DrawBlockGUI(Blackboard blackboard)
        {
            serializedObject.Update();

            Block block = target as Block;

            // Execute any queued cut, copy, paste, etc. operations from the prevous GUI update
            // We need to defer applying these operations until the following update because
            // the ReorderableList control emits GUI errors if you clear the list in the same frame
            // as drawing the control (e.g. select all and then delete)
            if (Event.current.type == EventType.Layout)
            {
                foreach (Action action in actionList)
                {
                    if (action != null)
                    {
                        action();
                    }
                }
                actionList.Clear();
            }


            EditorGUI.BeginChangeCheck();

            if (block == blackboard.SelectedBlock)
            {
                SerializedProperty suppressProp = serializedObject.FindProperty("suppressAllAutoSelections");
                SerializedProperty executionMethodProp = serializedObject.FindProperty("executionMethod");
                SerializedProperty awaitModeProp = serializedObject.FindProperty("awaitMode");
                SerializedProperty orderModeProp = serializedObject.FindProperty("orderMode");
                SerializedProperty avoidRepeatProp =
                    serializedObject.FindProperty("avoidRepeatingLastCommand");

                DrawExecutionSummary(executionMethodProp, awaitModeProp, orderModeProp);
                EditorGUILayout.Space(BlockInspectorStyleSheet.OuterSpacing);

                EditorGUILayout.BeginVertical(BlockInspectorStyleSheet.SectionCard);
                behaviourAndTimingFoldout = EditorGUILayout.Foldout(behaviourAndTimingFoldout, "Behaviour & Timing", true, BlockInspectorStyleSheet.SectionFoldout);
                if (behaviourAndTimingFoldout)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(suppressProp, new GUIContent("Suppress All Auto Selections"));
                    DrawAvoidRepeatLastCommand(
                        block,
                        executionMethodProp,
                        orderModeProp,
                        avoidRepeatProp);
                    DrawEventHandlerProperties(block);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(BlockInspectorStyleSheet.OuterSpacing);
                EditorGUILayout.BeginVertical(BlockInspectorStyleSheet.SectionCard);
                if (callersFoldout = EditorGUILayout.Foldout(callersFoldout, "Callers", true, BlockInspectorStyleSheet.SectionFoldout))
                {
                    CacheCallerString();
                    GUI.enabled = false;
                    EditorGUILayout.TextArea(callersString);
                    GUI.enabled = true;
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(BlockInspectorStyleSheet.OuterSpacing);

                block.UpdateIndentLevels();

                // Make sure each command has a reference to its parent block
                foreach (Command command in block.CommandList)
                {
                    if (command == null) // Will be deleted from the list later on
                    {
                        continue;
                    }
                    command.ParentBlock = block;
                }

                commandListAdaptor.DrawCommandList();

                // EventType.contextClick doesn't register since we moved the Block Editor to be inside
                // a GUI Area, no idea why. As a workaround we just check for right click instead.
                if (Event.current.type == EventType.MouseUp &&
                    Event.current.button == 1)
                {
                    ShowContextMenu();
                    Event.current.Use();
                }

                if (GUIUtility.keyboardControl == 0) //Only call keyboard shortcuts when not typing in a text field
                {
                    Event e = Event.current;

                    // Copy keyboard shortcut
                    if (e.type == EventType.ValidateCommand && e.commandName == "Copy")
                    {
                        if (blackboard.SelectedCommands.Count > 0)
                        {
                            e.Use();
                        }
                    }

                    if (e.type == EventType.ExecuteCommand && e.commandName == "Copy")
                    {
                        actionList.Add(Copy);
                        e.Use();
                    }

                    // Cut keyboard shortcut
                    if (e.type == EventType.ValidateCommand && e.commandName == "Cut")
                    {
                        if (blackboard.SelectedCommands.Count > 0)
                        {
                            e.Use();
                        }
                    }

                    if (e.type == EventType.ExecuteCommand && e.commandName == "Cut")
                    {
                        actionList.Add(Cut);
                        e.Use();
                    }

                    // Paste keyboard shortcut
                    if (e.type == EventType.ValidateCommand && e.commandName == "Paste")
                    {
                        CommandCopyBuffer commandCopyBuffer = CommandCopyBuffer.GetInstance();
                        if (commandCopyBuffer.HasCommands())
                        {
                            e.Use();
                        }
                    }

                    if (e.type == EventType.ExecuteCommand && e.commandName == "Paste")
                    {
                        actionList.Add(Paste);
                        e.Use();
                    }

                    // Duplicate keyboard shortcut
                    if (e.type == EventType.ValidateCommand && e.commandName == "Duplicate")
                    {
                        if (blackboard.SelectedCommands.Count > 0)
                        {
                            e.Use();
                        }
                    }

                    if (e.type == EventType.ExecuteCommand && e.commandName == "Duplicate")
                    {
                        actionList.Add(Copy);
                        actionList.Add(Paste);
                        e.Use();
                    }

                    // Delete keyboard shortcut
                    if (e.type == EventType.ValidateCommand && e.commandName == "Delete")
                    {
                        if (blackboard.SelectedCommands.Count > 0)
                        {
                            e.Use();
                        }
                    }

                    if (e.type == EventType.ExecuteCommand && e.commandName == "Delete")
                    {
                        actionList.Add(Delete);
                        e.Use();
                    }

                    // SelectAll keyboard shortcut
                    if (e.type == EventType.ValidateCommand && e.commandName == "SelectAll")
                    {
                        e.Use();
                    }

                    if (e.type == EventType.ExecuteCommand && e.commandName == "SelectAll")
                    {
                        actionList.Add(SelectAll);
                        e.Use();
                    }
                }
            }

            // Remove any null entries in the command list.
            // This can happen when a command class is deleted or renamed.
            for (int i = commandListProperty.arraySize - 1; i >= 0; --i)
            {
                SerializedProperty commandProperty = commandListProperty.GetArrayElementAtIndex(i);
                if (commandProperty.objectReferenceValue == null)
                {
                    commandListProperty.DeleteArrayElementAtIndex(i);
                }
            }


            if (EditorGUI.EndChangeCheck())
            {
                SelectedBlockDataStale = true;
            }

            serializedObject.ApplyModifiedProperties();
        }

        public virtual void DrawButtonToolbar()
        {
            GUILayout.BeginHorizontal();


            // Previous Command
            if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.PageUp))
            {
                SelectPrevious();
                GUI.FocusControl("dummycontrol");
                Event.current.Use();
            }
            // Next Command
            if ((Event.current.type == EventType.KeyDown) && (Event.current.keyCode == KeyCode.PageDown))
            {
                SelectNext();
                GUI.FocusControl("dummycontrol");
                Event.current.Use();
            }

            if (GUILayout.Button(upIcon))
            {
                SelectPrevious();
            }

            // Down Button
            if (GUILayout.Button(downIcon))
            {
                SelectNext();
            }

            GUILayout.FlexibleSpace();


            //using false to prevent forcing a longer row than will fit on smallest inspector
            Rect pos = EditorGUILayout.GetControlRect(false, 0, EditorStyles.objectField);
            if (pos.x != 0)
            {
                lastCMDpopupPos = pos;
                lastCMDpopupPos.x += EditorGUIUtility.labelWidth;
                lastCMDpopupPos.y += EditorGUIUtility.singleLineHeight * 2;
            }
            // Add Button
            if (GUILayout.Button(addIcon))
            {
                //this may be less reliable for HDPI scaling but previous method using editor window height is now returning 
                //  null in 2019.2 suspect ongoing ui changes, so default to screen.height and then attempt to get the better result
                int h = Screen.height;
                if (EditorWindow.focusedWindow != null)
                {
                    h = (int)EditorWindow.focusedWindow.position.height;
                }
                else if (EditorWindow.mouseOverWindow != null)
                {
                    h = (int)EditorWindow.mouseOverWindow.position.height;
                }

                CommandSelectorPopupWindowContent.ShowCommandMenu(lastCMDpopupPos, "", target as Block,
                    (int)(EditorGUIUtility.currentViewWidth),
                    (int)(h - lastCMDpopupPos.y));
            }

            // Duplicate Button
            if (GUILayout.Button(duplicateIcon))
            {
                Copy();
                Paste();
            }

            // Delete Button
            if (GUILayout.Button(deleteIcon))
            {
                Delete();
            }

            GUILayout.EndHorizontal();

        }



        private void DrawExecutionSummary(
            SerializedProperty executionMethodProp,
            SerializedProperty awaitModeProp,
            SerializedProperty orderModeProp)
        {
            Block block = target as Block;
            CompositeExecutionMethod executionMethod =
                (CompositeExecutionMethod)executionMethodProp.enumValueIndex;
            CompositeAwaitMode awaitMode =
                (CompositeAwaitMode)awaitModeProp.enumValueIndex;
            CompositeOrderMode orderMode =
                (CompositeOrderMode)orderModeProp.enumValueIndex;
            string executionTooltip = CompositeExecutionDescription.GetExecutionTooltip(
                executionMethod,
                awaitMode,
                orderMode);
            bool useCompactLayout = BlockInspectorStyleSheet.UsesCompactSummaryLayout(EditorGUIUtility.currentViewWidth);
            float fieldHeight = EditorGUIUtility.singleLineHeight * 2f + BlockInspectorStyleSheet.InnerSpacing;
            float totalHeight = useCompactLayout
                ? (fieldHeight * 3f) + (BlockInspectorStyleSheet.InnerSpacing * 2f)
                : fieldHeight;

            EditorGUILayout.BeginVertical(BlockInspectorStyleSheet.SectionCard);
            Rect summaryRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(totalHeight), GUILayout.ExpandWidth(true));
            if (useCompactLayout)
            {
                DrawEventSummaryField(GetSummaryFieldRect(summaryRect, 0, 1, fieldHeight), block);
                DrawEnumSummaryField(GetSummaryFieldRect(summaryRect, 1, 1, fieldHeight), executionMethodProp, "Execution Mode", executionTooltip, false);
                DrawCompositeSecondarySummaryField(GetSummaryFieldRect(summaryRect, 2, 1, fieldHeight), executionMethod, awaitModeProp, orderModeProp);
            }
            else
            {
                DrawEventSummaryField(GetSummaryFieldRect(summaryRect, 0, 3, fieldHeight), block);
                DrawEnumSummaryField(GetSummaryFieldRect(summaryRect, 1, 3, fieldHeight), executionMethodProp, "Execution Mode", executionTooltip, false);
                DrawCompositeSecondarySummaryField(GetSummaryFieldRect(summaryRect, 2, 3, fieldHeight), executionMethod, awaitModeProp, orderModeProp);
            }
            EditorGUILayout.EndVertical();
        }

        private static void DrawAvoidRepeatLastCommand(
            Block block,
            SerializedProperty executionMethodProperty,
            SerializedProperty orderModeProperty,
            SerializedProperty avoidRepeatProperty)
        {
            CompositeExecutionMethod executionMethod =
                (CompositeExecutionMethod)executionMethodProperty.enumValueIndex;
            CompositeOrderMode orderMode =
                (CompositeOrderMode)orderModeProperty.enumValueIndex;
            if (!CompositeExecutionDescription.SupportsOrder(executionMethod) ||
                orderMode == CompositeOrderMode.Ordered ||
                CountCommands(block) <= 1)
            {
                return;
            }

            GUIContent label = new GUIContent(
                "Avoid Repeating Last Command",
                "Prevents the first Random or Shuffle choice from matching the Command that finished the previous execution.");
            EditorGUILayout.PropertyField(avoidRepeatProperty, label);
        }

        private static int CountCommands(Block block)
        {
            int commandCount = 0;
            foreach (CommandTrack track in block.Tracks)
            {
                foreach (Command command in track.Commands)
                {
                    if (command != null)
                    {
                        commandCount++;
                    }
                }
            }

            return commandCount;
        }

        private void DrawCompositeSecondarySummaryField(
            Rect fieldRect,
            CompositeExecutionMethod executionMethod,
            SerializedProperty awaitModeProperty,
            SerializedProperty orderModeProperty)
        {
            if (CompositeExecutionDescription.SupportsAwait(executionMethod))
            {
                CompositeAwaitMode awaitMode =
                    (CompositeAwaitMode)awaitModeProperty.enumValueIndex;
                string tooltip = CompositeExecutionDescription.GetAwaitTooltip(
                    executionMethod,
                    awaitMode);
                DrawEnumSummaryField(
                    fieldRect,
                    awaitModeProperty,
                    "Parallel Completion",
                    tooltip,
                    false);
                return;
            }

            if (CompositeExecutionDescription.SupportsOrder(executionMethod))
            {
                CompositeOrderMode orderMode =
                    (CompositeOrderMode)orderModeProperty.enumValueIndex;
                string tooltip = CompositeExecutionDescription.GetOrderTooltip(
                    executionMethod,
                    orderMode);
                DrawEnumSummaryField(
                    fieldRect,
                    orderModeProperty,
                    GetOrderLabel(executionMethod),
                    tooltip,
                    false);
                return;
            }

            DrawDisabledSummaryField(
                fieldRect,
                "Selection Mode",
                "Utility-driven",
                "Utility Selector derives execution from child utility settings.");
        }

        private static string GetOrderLabel(CompositeExecutionMethod executionMethod)
        {
            return executionMethod == CompositeExecutionMethod.Selector
                ? "Selector Order"
                : "Sequence Order";
        }

        private static void DrawDisabledSummaryField(
            Rect fieldRect,
            string label,
            string value,
            string tooltip)
        {
            EditorGUI.LabelField(
                new Rect(
                    fieldRect.x,
                    fieldRect.y,
                    fieldRect.width,
                    EditorGUIUtility.singleLineHeight),
                label,
                BlockInspectorStyleSheet.SummaryHeader);
            using (new EditorGUI.DisabledScope(true))
            {
                GUI.Button(
                    new Rect(
                        fieldRect.x,
                        fieldRect.yMax - EditorGUIUtility.singleLineHeight,
                        fieldRect.width,
                        EditorGUIUtility.singleLineHeight),
                    new GUIContent(value, tooltip),
                    BlockInspectorStyleSheet.SummaryPopup);
            }
        }

        private static Rect GetSummaryFieldRect(Rect summaryRect, int index, int columnCount, float fieldHeight)
        {
            if (columnCount == 1)
            {
                return new Rect(summaryRect.x, summaryRect.y + (index * (fieldHeight + BlockInspectorStyleSheet.InnerSpacing)), summaryRect.width, fieldHeight);
            }

            float gapWidth = BlockInspectorStyleSheet.InnerSpacing;
            float fieldWidth = Mathf.Max(1f, (summaryRect.width - (gapWidth * (columnCount - 1))) / columnCount);
            return new Rect(summaryRect.x + (index * (fieldWidth + gapWidth)), summaryRect.y, fieldWidth, fieldHeight);
        }

        private void DrawEnumSummaryField(Rect fieldRect, SerializedProperty property, string label, string tooltip, bool disabled)
        {
            EditorGUI.LabelField(new Rect(fieldRect.x, fieldRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight), label, BlockInspectorStyleSheet.SummaryHeader);
            using (new EditorGUI.DisabledScope(disabled))
            {
                DrawEnumSummaryPopup(new Rect(fieldRect.x, fieldRect.yMax - EditorGUIUtility.singleLineHeight, fieldRect.width, EditorGUIUtility.singleLineHeight), property, label, tooltip);
            }
        }

        private void DrawEnumSummaryPopup(Rect rect, SerializedProperty property, string label, string tooltip)
        {
            string value = property.enumDisplayNames[property.enumValueIndex];
            if (!GUI.Button(rect, new GUIContent(value, tooltip), BlockInspectorStyleSheet.SummaryPopup))
            {
                return;
            }

            string propertyPath = property.propertyPath;
            GenericMenu menu = new GenericMenu();
            for (int index = 0; index < property.enumDisplayNames.Length; index++)
            {
                int selectedIndex = index;
                menu.AddItem(new GUIContent(property.enumDisplayNames[selectedIndex]), selectedIndex == property.enumValueIndex, () => SetEnumValue(propertyPath, selectedIndex, label));
            }
            menu.DropDown(rect);
        }

        private void SetEnumValue(string propertyPath, int enumValueIndex, string label)
        {
            serializedObject.Update();
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property == null)
            {
                return;
            }

            Undo.RecordObject(target, "Set " + label);
            property.enumValueIndex = enumValueIndex;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            SelectedBlockDataStale = true;
        }

        private void DrawEventSummaryField(Rect fieldRect, Block block)
        {
            EditorGUI.LabelField(new Rect(fieldRect.x, fieldRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight), "Event", BlockInspectorStyleSheet.SummaryHeader);
            string currentHandlerName = GetEventHandlerName(block);
            Rect rect = new Rect(fieldRect.x, fieldRect.yMax - EditorGUIUtility.singleLineHeight, fieldRect.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(rect, new GUIContent(currentHandlerName, "Select the event that executes this Block."), BlockInspectorStyleSheet.SummaryPopup))
            {
                Rect popupPosition = new Rect(rect.x, rect.yMax, rect.width, 0f);
                EventSelectorPopupWindowContent.DoEventHandlerPopUp(popupPosition, currentHandlerName, block, (int)rect.width, 200);
            }
        }

        private static string GetEventHandlerName(Block block)
        {
            if (block == null || block._EventHandler == null)
            {
                return "<None>";
            }

            EventHandlerInfoAttribute info = EventHandlerEditor.GetEventHandlerInfo(block._EventHandler.GetType());
            return info != null ? info.EventHandlerName : block._EventHandler.GetType().Name;
        }

        private void DrawEventHandlerProperties(Block block)
        {
            if (block._EventHandler != null)
            {
                EventHandlerEditor eventHandlerEditor = Editor.CreateEditor(block._EventHandler) as EventHandlerEditor;
                if (eventHandlerEditor != null)
                {
                    EditorGUI.BeginChangeCheck();
                    eventHandlerEditor.DrawInspectorGUI();

                    if (EditorGUI.EndChangeCheck())
                    {
                        SelectedBlockDataStale = true;
                    }

                    DestroyImmediate(eventHandlerEditor);
                }
            }
        }


        public static void BlockField(SerializedProperty property, GUIContent label, GUIContent nullLabel, Blackboard blackboard)
        {
            if (blackboard == null)
            {
                return;
            }

            Block block = property.objectReferenceValue as Block;

            // Build dictionary of child blocks
            List<GUIContent> blockNames = new List<GUIContent>();

            int selectedIndex = 0;
            blockNames.Add(nullLabel);
            Block[] blocks = blackboard.GetComponents<Block>();
            blocks = blocks.OrderBy(x => x.BlockName).ToArray();

            for (int i = 0; i < blocks.Length; ++i)
            {
                blockNames.Add(new GUIContent(blocks[i].BlockName));

                if (block == blocks[i])
                {
                    selectedIndex = i + 1;
                }
            }

            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, blockNames.ToArray());
            if (selectedIndex == 0)
            {
                block = null; // Option 'None'
            }
            else
            {
                block = blocks[selectedIndex - 1];
            }

            property.objectReferenceValue = block;
        }

        public static Block BlockField(Rect position, GUIContent nullLabel, Blackboard blackboard, Block block)
        {
            if (blackboard == null)
            {
                return null;
            }

            Block result = block;

            // Build dictionary of child blocks
            List<GUIContent> blockNames = new List<GUIContent>();

            int selectedIndex = 0;
            blockNames.Add(nullLabel);
            Block[] blocks = blackboard.GetComponents<Block>();
            blocks = blocks.OrderBy(x => x.BlockName).ToArray();

            for (int i = 0; i < blocks.Length; ++i)
            {
                blockNames.Add(new GUIContent(blocks[i].BlockName));

                if (block == blocks[i])
                {
                    selectedIndex = i + 1;
                }
            }

            selectedIndex = EditorGUI.Popup(position, selectedIndex, blockNames.ToArray());
            if (selectedIndex == 0)
            {
                result = null; // Option 'None'
            }
            else
            {
                result = blocks[selectedIndex - 1];
            }

            return result;
        }

        public virtual void ShowContextMenu()
        {
            Block block = target as Block;
            Blackboard blackboard = (Blackboard)block.GetBlackboard();

            if (blackboard == null)
            {
                return;
            }

            bool showCut = false;
            bool showCopy = false;
            bool showDuplicate = false;
            bool showDelete = false;
            bool showPaste = false;
            bool showPlay = false;
            Command contextCommand = commandListAdaptor.ContextCommand;

            if (blackboard.SelectedCommands.Count > 0)
            {
                showCut = true;
                showCopy = true;
                showDuplicate = true;
                showDelete = true;
            }

            if (contextCommand != null && Application.isPlaying)
            {
                showPlay = true;
            }



            CommandCopyBuffer commandCopyBuffer = CommandCopyBuffer.GetInstance();

            if (commandCopyBuffer.HasCommands())
            {
                showPaste = true;
            }

            GenericMenu commandMenu = new GenericMenu();

            if (showCut)
            {
                commandMenu.AddItem(new GUIContent("Cut"), false, Cut);
            }
            else
            {
                commandMenu.AddDisabledItem(new GUIContent("Cut"));
            }

            if (showCopy)
            {
                commandMenu.AddItem(new GUIContent("Copy"), false, Copy);
            }
            else
            {
                commandMenu.AddDisabledItem(new GUIContent("Copy"));
            }

            if (showDuplicate)
            {
                commandMenu.AddItem(new GUIContent("Duplicate"), false, Duplicate);
            }
            else
            {
                commandMenu.AddDisabledItem(new GUIContent("Duplicate"));
            }

            if (showPaste)
            {
                commandMenu.AddItem(new GUIContent("Paste"), false, Paste);
            }
            else
            {
                commandMenu.AddDisabledItem(new GUIContent("Paste"));
            }

            if (showDelete)
            {
                commandMenu.AddItem(new GUIContent("Delete"), false, Delete);
            }
            else
            {
                commandMenu.AddDisabledItem(new GUIContent("Delete"));
            }

            if (showPlay)
            {
                commandMenu.AddItem(
                    new GUIContent("Play From Selected Command"),
                    false,
                    () => PlayCommand(contextCommand));
                commandMenu.AddItem(
                    new GUIContent("Stop All Blocks & Play From Selected"),
                    false,
                    () => StopAllPlayCommand(contextCommand));
            }

            commandMenu.AddSeparator("");

            commandMenu.AddItem(new GUIContent("Select All"), false, SelectAll);
            commandMenu.AddItem(new GUIContent("Select None"), false, SelectNone);

            commandMenu.ShowAsContext();
        }

        protected void SelectAll()
        {
            Block block = target as Block;
            Blackboard blackboard = (Blackboard)block.GetBlackboard();

            if (blackboard == null ||
                blackboard.SelectedBlock == null)
            {
                return;
            }

            blackboard.ClearSelectedCommands();
            Undo.RecordObject(blackboard, "Select All");
            foreach (Command command in blackboard.SelectedBlock.CommandList)
            {
                blackboard.AddSelectedCommand(command);
            }

            Repaint();
        }

        protected void SelectNone()
        {
            Block block = target as Block;
            Blackboard blackboard = (Blackboard)block.GetBlackboard();

            if (blackboard == null ||
                blackboard.SelectedBlock == null)
            {
                return;
            }

            Undo.RecordObject(blackboard, "Select None");
            blackboard.ClearSelectedCommands();

            Repaint();
        }

        protected void Cut()
        {
            Copy();
            Delete();
        }

        protected void Duplicate()
        {
            Copy();
            Paste();
        }

        protected void Copy()
        {
            Block block = target as Block;
            Blackboard blackboard = (Blackboard)block.GetBlackboard();

            if (blackboard == null ||
                blackboard.SelectedBlock == null)
            {
                return;
            }

            CommandCopyBuffer commandCopyBuffer = CommandCopyBuffer.GetInstance();
            commandCopyBuffer.Clear();

            // Scan through all commands in execution order to see if each needs to be copied
            foreach (Command command in blackboard.SelectedBlock.CommandList)
            {
                if (blackboard.SelectedCommands.Contains(command))
                {
                    Type type = command.GetType();
                    Command newCommand = Undo.AddComponent(commandCopyBuffer.gameObject, type) as Command;
                    FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                    foreach (FieldInfo field in fields)
                    {
                        // Copy all public fields
                        bool copy = field.IsPublic;

                        // Copy non-public fields that have the SerializeField attribute
                        object[] attributes = field.GetCustomAttributes(typeof(SerializeField), true);
                        if (attributes.Length > 0)
                        {
                            copy = true;
                        }

                        if (copy)
                        {
                            field.SetValue(newCommand, field.GetValue(command));
                        }
                    }
                }
            }
        }

        protected void Paste()
        {
            Block block = target as Block;
            Blackboard blackboard = (Blackboard)block.GetBlackboard();

            if (blackboard == null ||
                blackboard.SelectedBlock == null)
            {
                return;
            }

            CommandCopyBuffer commandCopyBuffer = CommandCopyBuffer.GetInstance();

            // Find where to paste commands in block (either at end or after last selected command)
            int pasteIndex = blackboard.SelectedBlock.CommandList.Count;
            if (blackboard.SelectedCommands.Count > 0)
            {
                for (int i = 0; i < blackboard.SelectedBlock.CommandList.Count; ++i)
                {
                    Command command = blackboard.SelectedBlock.CommandList[i];

                    foreach (Command selectedCommand in blackboard.SelectedCommands)
                    {
                        if (command == selectedCommand)
                        {
                            pasteIndex = i + 1;
                        }
                    }
                }
            }

            foreach (Command command in commandCopyBuffer.GetCommands())
            {
                // Using the Editor copy / paste functionality instead instead of reflection
                // because this does a deep copy of the command properties.
                if (ComponentUtility.CopyComponent(command))
                {
                    if (ComponentUtility.PasteComponentAsNew(blackboard.gameObject))
                    {
                        Command[] commands = blackboard.GetComponents<Command>();
                        Command pastedCommand = commands.Last<Command>();
                        if (pastedCommand != null)
                        {
                            pastedCommand.ItemId = blackboard.NextItemId();
                            blackboard.SelectedBlock.CommandList.Insert(pasteIndex++, pastedCommand);
                        }
                    }

                    // This stops the user pasting the command manually into another game object.
                    ComponentUtility.CopyComponent(blackboard.transform);
                }
            }

            // Because this is an async call, we need to force prefab instances to record changes
            PrefabUtility.RecordPrefabInstancePropertyModifications(block);

            Repaint();
        }

        protected void Delete()
        {
            Block block = target as Block;
            Blackboard blackboard = (Blackboard)block.GetBlackboard();

            if (blackboard == null ||
                blackboard.SelectedBlock == null)
            {
                return;
            }
            int lastSelectedIndex = 0;
            for (int i = blackboard.SelectedBlock.CommandList.Count - 1; i >= 0; --i)
            {
                Command command = blackboard.SelectedBlock.CommandList[i];
                foreach (Command selectedCommand in blackboard.SelectedCommands)
                {
                    if (command == selectedCommand)
                    {
                        command.OnCommandRemoved(block);

                        // Order of destruction is important here for undo to work
                        Undo.DestroyObjectImmediate(command);

                        Undo.RecordObject((Block)blackboard.SelectedBlock, "Delete");
                        blackboard.SelectedBlock.CommandList.RemoveAt(i);

                        lastSelectedIndex = i;

                        break;
                    }
                }
            }

            Undo.RecordObject(blackboard, "Delete");
            blackboard.ClearSelectedCommands();

            if (lastSelectedIndex < blackboard.SelectedBlock.CommandList.Count)
            {
                Command nextCommand = blackboard.SelectedBlock.CommandList[lastSelectedIndex];
                block.GetBlackboard().AddSelectedCommand(nextCommand);
            }

            Repaint();
        }

        protected void PlayCommand(Command command)
        {
            Block targetBlock = target as Block;
            Blackboard blackboard = (Blackboard)targetBlock.GetBlackboard();
            if (targetBlock.IsExecuting())
            {
                blackboard.RestartBlock(targetBlock, command.CommandIndex);
            }
            else
            {
                // Block isn't executing yet so can start it now.
                blackboard.ExecuteBlock(targetBlock, command.CommandIndex);
            }
        }

        protected void StopAllPlayCommand(Command command)
        {
            Block targetBlock = target as Block;
            Blackboard blackboard = (Blackboard)targetBlock.GetBlackboard();

            // Stop all active blocks then run the selected block.
            blackboard.StopAllBlocksAndRestartBlock(targetBlock, command.CommandIndex);
        }

        protected void SelectPrevious()
        {
            Block block = target as Block;
            Blackboard blackboard = (Blackboard)block.GetBlackboard();

            int firstSelectedIndex = blackboard.SelectedBlock.CommandList.Count;
            bool firstSelectedCommandFound = false;
            if (blackboard.SelectedCommands.Count > 0)
            {
                for (int i = 0; i < blackboard.SelectedBlock.CommandList.Count; i++)
                {
                    Command commandInBlock = blackboard.SelectedBlock.CommandList[i];

                    foreach (Command selectedCommand in blackboard.SelectedCommands)
                    {
                        if (commandInBlock == selectedCommand)
                        {
                            if (!firstSelectedCommandFound)
                            {
                                firstSelectedIndex = i;
                                firstSelectedCommandFound = true;
                                break;
                            }
                        }
                    }
                    if (firstSelectedCommandFound)
                    {
                        break;
                    }
                }
            }
            if (firstSelectedIndex > 0)
            {
                blackboard.ClearSelectedCommands();
                blackboard.AddSelectedCommand(blackboard.SelectedBlock.CommandList[firstSelectedIndex - 1]);
            }

            Repaint();
        }

        protected void SelectNext()
        {
            Block block = target as Block;
            Blackboard blackboard = (Blackboard)block.GetBlackboard();

            int lastSelectedIndex = -1;
            if (blackboard.SelectedCommands.Count > 0)
            {
                for (int i = 0; i < blackboard.SelectedBlock.CommandList.Count; i++)
                {
                    Command commandInBlock = blackboard.SelectedBlock.CommandList[i];

                    foreach (Command selectedCommand in blackboard.SelectedCommands)
                    {
                        if (commandInBlock == selectedCommand)
                        {
                            lastSelectedIndex = i;
                        }
                    }
                }
            }
            if (lastSelectedIndex < blackboard.SelectedBlock.CommandList.Count - 1)
            {
                blackboard.ClearSelectedCommands();
                blackboard.AddSelectedCommand(blackboard.SelectedBlock.CommandList[lastSelectedIndex + 1]);
            }

            Repaint();
        }



        public static List<KeyValuePair<System.Type, CommandInfoAttribute>> GetFilteredCommandInfoAttribute(List<System.Type> menuTypes)
        {
            Dictionary<string, KeyValuePair<System.Type, CommandInfoAttribute>> filteredAttributes = new Dictionary<string, KeyValuePair<System.Type, CommandInfoAttribute>>();

            foreach (System.Type type in menuTypes)
            {
                object[] attributes = type.GetCustomAttributes(false);
                foreach (object obj in attributes)
                {
                    CommandInfoAttribute infoAttr = obj as CommandInfoAttribute;
                    if (infoAttr != null)
                    {
                        string dictionaryName = string.Format("{0}/{1}", infoAttr.Category, infoAttr.CommandName);

                        int existingItemPriority = -1;
                        if (filteredAttributes.ContainsKey(dictionaryName))
                        {
                            existingItemPriority = filteredAttributes[dictionaryName].Value.Priority;
                        }

                        if (infoAttr.Priority > existingItemPriority)
                        {
                            KeyValuePair<System.Type, CommandInfoAttribute> keyValuePair = new KeyValuePair<System.Type, CommandInfoAttribute>(type, infoAttr);
                            filteredAttributes[dictionaryName] = keyValuePair;
                        }
                    }
                }
            }
            return filteredAttributes.Values.ToList<KeyValuePair<System.Type, CommandInfoAttribute>>();
        }

        // Compare delegate for sorting the list of command attributes
        public static int CompareCommandAttributes(KeyValuePair<System.Type, CommandInfoAttribute> x, KeyValuePair<System.Type, CommandInfoAttribute> y)
        {
            int compare = (x.Value.Category.CompareTo(y.Value.Category));
            if (compare == 0)
            {
                compare = (x.Value.CommandName.CompareTo(y.Value.CommandName));
            }
            return compare;
        }
    }
}
