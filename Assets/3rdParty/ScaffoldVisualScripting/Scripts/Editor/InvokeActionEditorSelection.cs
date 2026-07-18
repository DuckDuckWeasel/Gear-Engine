using System.Collections.Generic;
using GearEngine.GearEngine.Presentation.UI.Input;

namespace Scaffold.EditorUtils
{
    public static class InvokeActionEditorSelection
    {
        private static readonly Dictionary<int, int> selectedActionIndices = new Dictionary<int, int>();

        public static void Select(InvokeActionCommand command, int actionIndex)
        {
            if (command == null)
            {
                return;
            }

            selectedActionIndices[command.GetInstanceID()] = actionIndex;
        }

        public static int GetSelectedIndex(InvokeActionCommand command)
        {
            if (command == null || command.actions == null)
            {
                return -1;
            }

            if (!selectedActionIndices.TryGetValue(command.GetInstanceID(), out int actionIndex))
            {
                return -1;
            }

            return actionIndex >= 0 && actionIndex < command.actions.Count ? actionIndex : -1;
        }

        public static void Clear(InvokeActionCommand command)
        {
            if (command != null)
            {
                selectedActionIndices.Remove(command.GetInstanceID());
            }
        }
    }
}
