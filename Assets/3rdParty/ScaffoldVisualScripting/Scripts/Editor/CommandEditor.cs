
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;

namespace Scaffold.EditorUtils
{
    [CustomEditor(typeof(Command), true)]
    public class CommandEditor : TriInspector.Editors.TriEditor
    {
        #region statics
        public static Command selectedCommand;
        public static bool SelectedCommandDataStale { get; set; }

        public static CommandInfoAttribute GetCommandInfo(System.Type commandType)
        {
            CommandInfoAttribute retval = null;

            object[] attributes = commandType.GetCustomAttributes(typeof(CommandInfoAttribute), false);
            foreach (object obj in attributes)
            {
                CommandInfoAttribute commandInfoAttr = obj as CommandInfoAttribute;
                if (commandInfoAttr != null)
                {
                    if (retval == null)
                    {
                        retval = commandInfoAttr;
                    }
                    else if (retval.Priority < commandInfoAttr.Priority)
                    {
                        retval = commandInfoAttr;
                    }
                }
            }

            return retval;
        }

        #endregion statics

        private Dictionary<string, ReorderableList> reorderableLists;

        public virtual void OnEnable()
        {
            base.OnEnable();
            if (NullTargetCheck()) // Check for an orphaned editor instance
            {
                return;
            }

            reorderableLists = new Dictionary<string, ReorderableList>();
        }

        public virtual void OnDisable()
        {
            base.OnDisable();
        }

        public virtual void DrawCommandInspectorGUI()
        {
            Command t = target as Command;
            if (t == null)
            {
                return;
            }

            Blackboard blackboard = (Blackboard)t.GetBlackboard();
            if (blackboard == null)
            {
                return;
            }

            CommandInfoAttribute commandInfoAttr = CommandEditor.GetCommandInfo(t.GetType());
            if (commandInfoAttr == null)
            {
                return;
            }

            GUILayout.BeginVertical(GUI.skin.box);

            if (t.enabled)
            {
                if (blackboard.ColorCommands)
                {
                    GUI.backgroundColor = t.GetButtonColor();
                }
                else
                {
                    GUI.backgroundColor = Color.white;
                }
            }
            else
            {
                GUI.backgroundColor = Color.grey;
            }
            GUILayout.BeginHorizontal(GUI.skin.button);

            string commandName = GetCommandDisplayName(t, commandInfoAttr);
            GUILayout.Label(commandName, GUILayout.MinWidth(80), GUILayout.ExpandWidth(true));

            GUILayout.FlexibleSpace();

            GUILayout.Label(new GUIContent("(" + t.ItemId + ")"));

            GUILayout.Space(10);

            GUI.backgroundColor = Color.white;
            DrawHeaderCompositeWeight(t);
            bool enabled = t.enabled;
            enabled = GUILayout.Toggle(enabled, new GUIContent());

            if (t.enabled != enabled)
            {
                Undo.RecordObject(t, "Set Enabled");
                t.enabled = enabled;
            }

            GUILayout.EndHorizontal();
            DrawHeaderContextMenu(GUILayoutUtility.GetLastRect(), t, blackboard);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();
            DrawBlockCompositeSettings(t);
            DrawCommandGUI();
            if (EditorGUI.EndChangeCheck())
            {
                SelectedCommandDataStale = true;
            }

            EditorGUILayout.Separator();

            if (t.ErrorMessage.Length > 0)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = new Color(1, 0, 0);
                EditorGUILayout.LabelField(new GUIContent("Error: " + t.ErrorMessage), style);
            }

            GUILayout.EndVertical();

            // Display help text
            CommandInfoAttribute infoAttr = CommandEditor.GetCommandInfo(t.GetType());
            if (infoAttr != null)
            {
                EditorGUILayout.HelpBox(infoAttr.HelpText, MessageType.Info, true);
            }
        }

        private static void DrawHeaderContextMenu(Rect headerRect, Command command, Blackboard blackboard)
        {
            if (Event.current.type != EventType.ContextClick ||
                !headerRect.Contains(Event.current.mousePosition))
            {
                return;
            }

            Block block = command.ParentBlock;
            if (block == null)
            {
                block = command.GetComponent<Block>();
            }

            if (block == null)
            {
                return;
            }

            ShowCommandContextMenu(command, blackboard);
            Event.current.Use();
        }

        internal static void ShowCommandContextMenu(
            Command command,
            Blackboard blackboard,
            string additionalItemLabel = null,
            GenericMenu.MenuFunction additionalItemAction = null)
        {
            if (command == null || blackboard == null)
            {
                return;
            }

            Block block = command.ParentBlock;
            if (block == null)
            {
                block = command.GetComponent<Block>();
            }

            if (block == null)
            {
                return;
            }

            SelectCommandForContextMenu(command, blackboard);
            GenericMenu menu = new GenericMenu();
            bool hasSelection = blackboard.SelectedCommands.Count > 0;
            bool hasClipboardCommands = CommandCopyBuffer.GetInstance().HasCommands();
            AddContextMenuItem(menu, "Cut", hasSelection, () =>
            {
                CopySelectedCommands(block, blackboard);
                DeleteSelectedCommands(block, blackboard);
            });
            AddContextMenuItem(menu, "Copy", hasSelection, () => CopySelectedCommands(block, blackboard));
            AddContextMenuItem(menu, "Duplicate", hasSelection, () =>
            {
                CopySelectedCommands(block, blackboard);
                PasteCommands(block, blackboard);
            });
            AddContextMenuItem(menu, "Paste", hasClipboardCommands, () => PasteCommands(block, blackboard));
            AddContextMenuItem(menu, "Delete", hasSelection, () => DeleteSelectedCommands(block, blackboard));

            if (ShouldShowListSelectionItems(command))
            {
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Select All"), false, () => SelectAllCommands(block, blackboard));
                menu.AddItem(new GUIContent("Select None"), false, () => blackboard.ClearSelectedCommands());
            }

            if (!string.IsNullOrEmpty(additionalItemLabel) && additionalItemAction != null)
            {
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent(additionalItemLabel), false, additionalItemAction);
            }

            menu.ShowAsContext();
        }

        private static void AddContextMenuItem(
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

        private static bool ShouldShowListSelectionItems(Command command)
        {
            PropertyInfo displayAsGroupProperty = command.GetType().GetProperty("DisplayAsGroup");
            if (displayAsGroupProperty != null &&
                displayAsGroupProperty.PropertyType == typeof(bool) &&
                (bool)displayAsGroupProperty.GetValue(command))
            {
                return true;
            }

            FieldInfo actionsField = command.GetType().GetField("actions");
            return actionsField?.GetValue(command) is System.Collections.ICollection actions &&
                   actions.Count > 1;
        }

        private static void SelectCommandForContextMenu(Command command, Blackboard blackboard)
        {
            if (blackboard.SelectedCommands.Contains(command))
            {
                return;
            }

            Undo.RecordObject(blackboard, "Select Command");
            blackboard.ClearSelectedCommands();
            blackboard.AddSelectedCommand(command);
        }

        private static void CopySelectedCommands(Block block, Blackboard blackboard)
        {
            CommandCopyBuffer commandCopyBuffer = CommandCopyBuffer.GetInstance();
            commandCopyBuffer.Clear();
            foreach (Command command in block.CommandList)
            {
                if (!blackboard.SelectedCommands.Contains(command))
                {
                    continue;
                }

                Type type = command.GetType();
                Command copiedCommand = Undo.AddComponent(commandCopyBuffer.gameObject, type) as Command;
                foreach (FieldInfo field in type.GetFields(
                             BindingFlags.Instance |
                             BindingFlags.Public |
                             BindingFlags.NonPublic |
                             BindingFlags.FlattenHierarchy))
                {
                    bool shouldCopy = field.IsPublic ||
                                      field.GetCustomAttributes(typeof(SerializeField), true).Length > 0;
                    if (shouldCopy)
                    {
                        field.SetValue(copiedCommand, field.GetValue(command));
                    }
                }
            }
        }

        private static void PasteCommands(Block block, Blackboard blackboard)
        {
            CommandCopyBuffer commandCopyBuffer = CommandCopyBuffer.GetInstance();
            int pasteIndex = block.CommandList.Count;
            for (int commandIndex = 0; commandIndex < block.CommandList.Count; commandIndex++)
            {
                if (blackboard.SelectedCommands.Contains(block.CommandList[commandIndex]))
                {
                    pasteIndex = commandIndex + 1;
                }
            }

            foreach (Command copiedCommand in commandCopyBuffer.GetCommands())
            {
                if (!ComponentUtility.CopyComponent(copiedCommand) ||
                    !ComponentUtility.PasteComponentAsNew(blackboard.gameObject))
                {
                    continue;
                }

                Command pastedCommand = blackboard.GetComponents<Command>().LastOrDefault();
                if (pastedCommand == null)
                {
                    continue;
                }

                pastedCommand.ItemId = blackboard.NextItemId();
                pastedCommand.ParentBlock = block;
                pastedCommand.OnCommandAdded(block);
                block.CommandList.Insert(pasteIndex++, pastedCommand);
                ComponentUtility.CopyComponent(blackboard.transform);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(block);
            EditorUtility.SetDirty(block);
        }

        private static void DeleteSelectedCommands(Block block, Blackboard blackboard)
        {
            int lastSelectedIndex = 0;
            for (int commandIndex = block.CommandList.Count - 1; commandIndex >= 0; commandIndex--)
            {
                Command command = block.CommandList[commandIndex];
                if (!blackboard.SelectedCommands.Contains(command))
                {
                    continue;
                }

                command.OnCommandRemoved(block);
                Undo.DestroyObjectImmediate(command);
                Undo.RecordObject(block, "Delete Command");
                block.CommandList.RemoveAt(commandIndex);
                lastSelectedIndex = commandIndex;
            }

            Undo.RecordObject(blackboard, "Delete Command");
            blackboard.ClearSelectedCommands();
            if (lastSelectedIndex < block.CommandList.Count)
            {
                blackboard.AddSelectedCommand(block.CommandList[lastSelectedIndex]);
            }

            EditorUtility.SetDirty(block);
        }

        private static void SelectAllCommands(Block block, Blackboard blackboard)
        {
            Undo.RecordObject(blackboard, "Select All Commands");
            blackboard.ClearSelectedCommands();
            foreach (Command command in block.CommandList)
            {
                blackboard.AddSelectedCommand(command);
            }
        }

        private static void DrawHeaderCompositeWeight(Command command)
        {
            Block block = command.ParentBlock;
            if (block == null)
            {
                block = command.GetComponent<Block>();
            }

            if (block == null ||
                !CompositeExecutionDescription.SupportsWeight(
                    block.ExecutionMethod,
                    block.OrderMode) ||
                command.GetType().Name == "CommentAction" ||
                command.GetType().Name == "LabelAction")
            {
                return;
            }

            bool hasOverride = command.HasCompositeWeightOverride;
            float displayedWeight = block.GetCommandWeight(command);
            EditorGUI.BeginChangeCheck();
            using (new EditorGUI.DisabledScope(!hasOverride))
            {
                displayedWeight = EditorGUILayout.DelayedFloatField(
                    displayedWeight,
                    GUILayout.Width(48f));
            }

            if (EditorGUI.EndChangeCheck())
            {
                SetCommandWeightOverride(command, displayedWeight);
            }

            bool requestedOverride = GUILayout.Toggle(
                hasOverride,
                new GUIContent(
                    "%",
                    hasOverride
                        ? "Click to restore automatic balancing."
                        : "Click to edit a manual percentage."),
                EditorStyles.miniButton,
                GUILayout.Width(20f));
            if (requestedOverride == hasOverride)
            {
                return;
            }

            Undo.RecordObject(command, requestedOverride
                ? "Enable Command Weight Override"
                : "Disable Command Weight Override");
            if (requestedOverride)
            {
                command.CompositeWeight = displayedWeight;
            }
            else
            {
                command.ClearCompositeWeightOverride();
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(command);
            EditorUtility.SetDirty(command);
            SelectedCommandDataStale = true;
        }

        private static void SetCommandWeightOverride(Command command, float weight)
        {
            Undo.RecordObject(command, "Set Command Weight Override");
            command.CompositeWeight = Mathf.Clamp(weight, 0f, 100f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(command);
            EditorUtility.SetDirty(command);
            SelectedCommandDataStale = true;
        }

        private void DrawBlockCompositeSettings(Command command)
        {
            Block block = command.ParentBlock;
            if (block == null)
            {
                block = command.GetComponent<Block>();
            }

            if (block == null)
            {
                return;
            }

            if (block.ExecutionMethod != CompositeExecutionMethod.UtilitySelector)
            {
                return;
            }

            serializedObject.Update();
            GUIContent utilityLabel = new GUIContent(
                "Block Utility",
                CompositeExecutionDescription.GetExecutionTooltip(
                    block.ExecutionMethod,
                    block.AwaitMode,
                    block.OrderMode));
            GUIContent blockLabel = new GUIContent(
                "Block During Execution",
                "Keeps this command selected until it finishes instead of reevaluating utility every frame.");
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("compositeUtility"),
                utilityLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("compositeBlockDuringExecution"),
                blockLabel);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space();
        }

        public virtual void DrawCommandGUI()
        {
            base.OnInspectorGUI();
        }

        protected virtual string GetCommandDisplayName(Command command, CommandInfoAttribute commandInfo)
        {
            return commandInfo.CommandName;
        }


        public static void ObjectField<T>(SerializedProperty property, GUIContent label, GUIContent nullLabel, List<T> objectList) where T : UnityEngine.Object
        {
            if (property == null)
            {
                return;
            }

            List<GUIContent> objectNames = new List<GUIContent>();

            T selectedObject = property.objectReferenceValue as T;

            int selectedIndex = -1; // Invalid index

            // First option in list is <None>
            objectNames.Add(nullLabel);
            if (selectedObject == null)
            {
                selectedIndex = 0;
            }

            for (int i = 0; i < objectList.Count; ++i)
            {
                if (objectList[i] == null)
                {
                    continue;
                }

                objectNames.Add(new GUIContent(objectList[i].name));

                if (selectedObject == objectList[i])
                {
                    selectedIndex = i + 1;
                }
            }

            T result;

            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, objectNames.ToArray());

            if (selectedIndex == -1)
            {
                // Currently selected object is not in list, but nothing else was selected so no change.
                return;
            }
            else if (selectedIndex == 0)
            {
                result = null; // Null option
            }
            else
            {
                result = objectList[selectedIndex - 1];
            }

            property.objectReferenceValue = result;
        }

        // When modifying custom editor code you can occasionally end up with orphaned editor instances.
        // When this happens, you'll get a null exception error every time the scene serializes / deserialized.
        // Once this situation occurs, the only way to fix it is to restart the Unity editor.
        // 
        // As a workaround, this function detects if this command editor is an orphan and deletes it. 
        // To use it, just call this function at the top of the OnEnable() method in your custom editor.
        protected virtual bool NullTargetCheck()
        {
            try
            {
                // The serializedObject accessor create a new SerializedObject if needed.
                // However, this will fail with a null exception if the target object no longer exists.
#pragma warning disable 0219
                SerializedObject so = serializedObject;
            }
            catch (System.NullReferenceException)
            {
                DestroyImmediate(this);
                return true;
            }

            return false;
        }
    }
}
