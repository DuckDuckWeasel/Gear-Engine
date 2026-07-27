
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Scaffold.EditorUtils
{
    /// <summary>
    /// Show the variable selection window as a searchable popup
    /// </summary>
    public class VariableSelectPopupWindowContent : BasePopupWindowContent
    {
        static readonly int POPUP_WIDTH = 200, POPUP_HEIGHT = 200;
        static List<System.Type> _variableTypes;
        static List<System.Type> VariableTypes
        {
            get
            {
                if (_variableTypes == null || _variableTypes.Count == 0)
                    CacheVariableTypes();

                return _variableTypes;
            }
        }

        static void CacheVariableTypes()
        {
            var derivedType = typeof(Variable);
            _variableTypes = EditorExtensions.FindDerivedTypes(derivedType)
                .Where(x => !x.IsAbstract && derivedType.IsAssignableFrom(x))
                .ToList();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            CacheVariableTypes();
        }

        protected override void PrepareAllItems()
        {
            int i = 0;
            foreach (var item in VariableTypes)
            {
                VariableInfoAttribute variableInfo = VariableEditor.GetVariableInfo(item);
                if (variableInfo != null)
                {
                    allItems.Add(new FilteredListItem(i, (variableInfo.Category.Length > 0 ? variableInfo.Category + CATEGORY_CHAR : "") + variableInfo.VariableType));
                }

                i++;
            }
        }

        protected override void SelectByOrigIndex(int index)
        {
            AddVariable(VariableTypes[index]);
        }

        static public void DoAddVariable(Rect position, string currentHandlerName, Blackboard blackboard)
        {
            curBlackboard = blackboard;
            if (!ScaffoldEditorPreferences.useLegacyMenus)
            {
                //new method
                VariableSelectPopupWindowContent win = new VariableSelectPopupWindowContent(currentHandlerName, POPUP_WIDTH, POPUP_HEIGHT);
                PopupWindow.Show(position, win);
            }
            //old method
            DoOlderMenu(blackboard);
        }

        static protected void DoOlderMenu(Blackboard blackboard)
        {
            GenericMenu menu = new GenericMenu();

            // Add variable types without a category
            foreach (var type in VariableTypes)
            {
                VariableInfoAttribute variableInfo = VariableEditor.GetVariableInfo(type);
                if (variableInfo == null ||
                    variableInfo.Category != "")
                {
                    continue;
                }

                GUIContent typeName = new GUIContent(variableInfo.VariableType);

                menu.AddItem(typeName, false, AddVariable, type);
            }

            // Add types with a category
            foreach (var type in VariableTypes)
            {
                VariableInfoAttribute variableInfo = VariableEditor.GetVariableInfo(type);
                if (variableInfo == null ||
                    variableInfo.Category == "")
                {
                    continue;
                }
                
                GUIContent typeName = new GUIContent(variableInfo.Category + CATEGORY_CHAR + variableInfo.VariableType);

                menu.AddItem(typeName, false, AddVariable, type);
            }

            menu.ShowAsContext();
        }

        private static Blackboard curBlackboard;

        public VariableSelectPopupWindowContent(string currentHandlerName, int width, int height)
            : base(currentHandlerName, width, height)
        {
        }

        public static void AddVariable(object obj)
        {
            AddVariable(obj, string.Empty);
        }

        public static void AddVariable(object obj, string suggestedName)
        {
            System.Type t = obj as System.Type;
            if (t == null)
            {
                return;
            }

            var blackboard = curBlackboard != null ? curBlackboard : BlackboardWindow.GetBlackboard();
            Undo.RecordObject(blackboard, "Add Variable");
            Variable newVariable = blackboard.gameObject.AddComponent(t) as Variable;
            newVariable.Key = blackboard.GetUniqueVariableKey(suggestedName);

            //if suggested exists, then insert, if not just add
            var existingVariable = blackboard.GetVariable(suggestedName);
            if (existingVariable != null)
            {
                blackboard.Variables.Insert(blackboard.Variables.IndexOf(existingVariable)+1, newVariable);
            }
            else
            {
                blackboard.Variables.Add(newVariable);
            }

            // Because this is an async call, we need to force prefab instances to record changes
            PrefabUtility.RecordPrefabInstancePropertyModifications(blackboard);
        }
    }
}