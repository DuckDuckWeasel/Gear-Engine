using System;
using GearEngine.Core.Actions;
using UnityEditor;

namespace Scaffold.EditorUtils
{
    public static class InvokeActionEditorUtility
    {
        public static string GetDisplayName(IAction action)
        {
            return action == null ? "Empty Action" : GetDisplayName(action.GetType());
        }

        public static string GetDisplayName(Type actionType)
        {
            if (actionType == null)
            {
                return "Empty Action";
            }

            CommandInfoAttribute commandInfo = CommandEditor.GetCommandInfo(actionType);
            return commandInfo != null
                ? commandInfo.CommandName
                : ObjectNames.NicifyVariableName(actionType.Name);
        }
    }
}
