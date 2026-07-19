using System.Collections.Generic;
using GearEngine.GearEngine.Presentation.UI.Input;

namespace Scaffold.EditorUtils
{
    public static class InvokeActionEditorSelection
    {
        private static readonly Dictionary<InvokeActionCommand, int> selectedActionIndices = new Dictionary<InvokeActionCommand, int>();
        private static readonly Dictionary<InvokeActionCommand, HashSet<int>> selectedActionIndexSets =
            new Dictionary<InvokeActionCommand, HashSet<int>>();

        public static void Select(InvokeActionCommand command, int actionIndex)
        {
            if (command == null)
            {
                return;
            }

            selectedActionIndices[command] = actionIndex;
            selectedActionIndexSets[command] = new HashSet<int> { actionIndex };
        }

        public static int GetSelectedIndex(InvokeActionCommand command)
        {
            if (command == null || command.actions == null)
            {
                return -1;
            }

            if (!selectedActionIndices.TryGetValue(command, out int actionIndex))
            {
                return -1;
            }

            return actionIndex >= 0 && actionIndex < command.actions.Count ? actionIndex : -1;
        }

        public static void Clear(InvokeActionCommand command)
        {
            if (command != null)
            {
                selectedActionIndices.Remove(command);
                selectedActionIndexSets.Remove(command);
            }
        }

        public static void SelectAll(InvokeActionCommand command)
        {
            if (command == null || command.actions == null || command.actions.Count == 0)
            {
                Clear(command);
                return;
            }

            HashSet<int> indices = new HashSet<int>();
            for (int index = 0; index < command.actions.Count; index++)
            {
                indices.Add(index);
            }

            selectedActionIndices[command] = 0;
            selectedActionIndexSets[command] = indices;
        }

        public static List<int> GetSelectedIndices(InvokeActionCommand command)
        {
            if (command == null || command.actions == null ||
                !selectedActionIndexSets.TryGetValue(command, out HashSet<int> indices))
            {
                return new List<int>();
            }

            List<int> validIndices = new List<int>();
            foreach (int index in indices)
            {
                if (index >= 0 && index < command.actions.Count)
                {
                    validIndices.Add(index);
                }
            }

            validIndices.Sort();
            return validIndices;
        }
    }
}
