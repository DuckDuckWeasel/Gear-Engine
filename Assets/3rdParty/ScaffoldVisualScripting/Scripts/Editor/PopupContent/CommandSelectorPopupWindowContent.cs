
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scaffold.EditorUtils
{
    /// <summary>
    /// Searchable Popup Window for adding a command to a block
    /// </summary>
    public class CommandSelectorPopupWindowContent : BasePopupWindowContent
    {
        static List<System.Type> _commandTypes;
        static List<System.Type> CommandTypes
        {
            get
            {
                if (_commandTypes == null || _commandTypes.Count == 0)
                    CacheCommandTypes();

                return _commandTypes;
            }
        }

        static void CacheCommandTypes()
        {
            _commandTypes = EditorExtensions.FindDerivedTypes(typeof(Command)).Where(x => !x.IsAbstract).ToList();
            
            // Also find all IAction types that have the CommandInfoAttribute
            var allTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => {
                try { return a.GetTypes(); } catch { return new Type[0]; }
            });
            
            Type actionType = Type.GetType("GearEngine.Core.Actions.IAction, Game.GearEngine");
            if (actionType != null)
            {
                var actionTypes = allTypes.Where(t => t != null && actionType.IsAssignableFrom(t) && !t.IsAbstract && !typeof(Command).IsAssignableFrom(t));
                foreach(var t in actionTypes)
                {
                    if (t.GetCustomAttributes(typeof(CommandInfoAttribute), false).Length > 0)
                    {
                        _commandTypes.Add(t);
                    }
                }
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            CacheCommandTypes();
        }

        static Block curBlock;
        static protected List<KeyValuePair<System.Type, CommandInfoAttribute>> filteredAttributes;

        public CommandSelectorPopupWindowContent(string currentHandlerName, int width, int height)
            : base(currentHandlerName, width, height)
        {
        }

        protected override void SelectByOrigIndex(int index)
        {
            var commandType = (index >= 0 && index < CommandTypes.Count) ? CommandTypes[index] : null;
            AddCommandCallback(commandType);
        }

        protected override void PrepareAllItems()
        {
            filteredAttributes = GetFilteredSupportedCommands(curBlock.GetBlackboard());

            foreach (var item in filteredAttributes)
            {
                //force lookup to orig index here to account for commmand lists being filtered by users
                var newFilteredItem = new FilteredListItem(CommandTypes.IndexOf(item.Key), (item.Value.Category.Length > 0 ? item.Value.Category + CATEGORY_CHAR : "") + item.Value.CommandName, item.Value.HelpText);
                allItems.Add(newFilteredItem);
            }
        }

        static public void ShowCommandMenu(Rect position, string currentHandlerName, Block block, int width, int height)
        {
            curBlock = block;


            if (!ScaffoldEditorPreferences.useLegacyMenus)
            {
                var win = new CommandSelectorPopupWindowContent(currentHandlerName,
                    width, (int)(height - EditorGUIUtility.singleLineHeight * 3));
                PopupWindow.Show(position, win);
            }
            else
            {
                //need to ensure we have filtered data 
                filteredAttributes = GetFilteredSupportedCommands(curBlock.GetBlackboard());
            }

            //old method
            DoOlderMenu();
        }

        protected static List<KeyValuePair<System.Type, CommandInfoAttribute>> GetFilteredSupportedCommands(Blackboard blackboard)
        {
            List<KeyValuePair<System.Type, CommandInfoAttribute>> filteredAttributes = BlockEditor.GetFilteredCommandInfoAttribute(CommandTypes);

            filteredAttributes.Sort(BlockEditor.CompareCommandAttributes);

            filteredAttributes = filteredAttributes.Where(x => blackboard.IsCommandSupported(x.Value)).ToList();

            return filteredAttributes;
        }
        

        static protected void DoOlderMenu()
        {
            GenericMenu commandMenu = new GenericMenu();

            // Build menu list

            foreach (var keyPair in filteredAttributes)
            {
                GUIContent menuItem;
                if (keyPair.Value.Category == "")
                {
                    menuItem = new GUIContent(keyPair.Value.CommandName);
                }
                else
                {
                    menuItem = new GUIContent(keyPair.Value.Category + CATEGORY_CHAR + keyPair.Value.CommandName);
                }

                commandMenu.AddItem(menuItem, false, AddCommandCallback, keyPair.Key);
            }

            commandMenu.ShowAsContext();
        }

        //Used by GenericMenu Delegate
        static protected void AddCommandCallback(object obj)
        {
            Type command = obj as Type;
            if (command != null)
            {
                AddCommandCallback(command);
            }
        }


        static protected void AddCommandCallback(Type commandType)
        {
            var block = curBlock;
            if (block == null || commandType == null)
            {
                return;
            }

            var blackboard = (Blackboard)block.GetBlackboard();

            // Use index of last selected command in list, or end of list if nothing selected.
            int index = -1;
            foreach (var command in blackboard.SelectedCommands)
            {
                if (command.CommandIndex + 1 > index)
                {
                    index = command.CommandIndex + 1;
                }
            }
            if (index == -1)
            {
                index = block.CommandList.Count;
            }

            Command newCommand = null;
            if (typeof(Command).IsAssignableFrom(commandType))
            {
                newCommand = Undo.AddComponent(block.gameObject, commandType) as Command;
            }
            else
            {
                // Pure C# Action (IAction), wrap it in InvokeActionCommand
                Type wrapperType = Type.GetType("GearEngine.GearEngine.Presentation.UI.Input.InvokeActionCommand, Game.GearEngine");
                if (wrapperType != null)
                {
                    newCommand = Undo.AddComponent(block.gameObject, wrapperType) as Command;
                    var newAction = Activator.CreateInstance(commandType);
                    
                    var actionsField = wrapperType.GetField("actions", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (actionsField != null)
                    {
                        var list = actionsField.GetValue(newCommand) as System.Collections.IList;
                        if (list != null)
                        {
                            list.Add(newAction);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Could not find InvokeActionCommand wrapper type.");
                    return;
                }
            }

            block.GetBlackboard().AddSelectedCommand(newCommand);
            newCommand.ParentBlock = block;
            newCommand.ItemId = blackboard.NextItemId();

            // Let command know it has just been added to the block
            newCommand.OnCommandAdded(block);

            Undo.RecordObject(block, "Set command type");
            if (index < block.CommandList.Count - 1)
            {
                block.CommandList.Insert(index, newCommand);
            }
            else
            {
                block.CommandList.Add(newCommand);
            }

            // Because this is an async call, we need to force prefab instances to record changes
            PrefabUtility.RecordPrefabInstancePropertyModifications(block);

            //clear commands just in case there was a selection made prior, 
            // this way, only one command is selected at the end; the new one.
            blackboard.ClearSelectedCommands();

            CommandListAdaptor.ScrollToCommandOnDraw = true;
            blackboard.AddSelectedCommand(newCommand); //select the new command.
        }
    }
}
