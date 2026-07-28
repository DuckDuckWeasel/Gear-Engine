using Scaffold.VisualScripting.Unity;

namespace Scaffold.VisualScripting.Editor
{
    public sealed class BlackboardExecutionFeedback
    {
        public bool TryGetBlockState(BlackboardBehaviour behaviour, DefinitionId blockId, out BlockExecutionState state)
        {
            Block block = FindBlock(behaviour, blockId);
            state = block?.State ?? default;
            return block != null;
        }

        public bool TryGetActionStatus(BlackboardBehaviour behaviour, DefinitionId actionId, out ActionExecutionStatus status)
        {
            Block block = FindActionBlock(
                behaviour,
                actionId,
                out int taskIndex);
            status = default;
            return block != null &&
                block.TryGetActionStatus(taskIndex, out status);
        }

        public bool IsActionRunning(BlackboardBehaviour behaviour, DefinitionId actionId)
        {
            Block block = FindActionBlock(
                behaviour,
                actionId,
                out int taskIndex);
            return block != null &&
                block.IsActionRunning(taskIndex);
        }

        private Block FindBlock(BlackboardBehaviour behaviour, DefinitionId blockId)
        {
            if (behaviour == null || !behaviour.IsRuntimeAvailable)
            {
                return null;
            }

            foreach (Block block in behaviour.Runtime.Blocks)
            {
                if (block.Definition.DefinitionId == blockId)
                {
                    return block;
                }
            }

            return null;
        }

        private Block FindActionBlock(
            BlackboardBehaviour behaviour,
            DefinitionId actionId,
            out int taskIndex)
        {
            taskIndex = -1;
            if (behaviour == null || !behaviour.IsRuntimeAvailable)
            {
                return null;
            }

            return FindActionInBlocks(
                behaviour,
                actionId,
                out taskIndex);
        }

        private Block FindActionInBlocks(
            BlackboardBehaviour behaviour,
            DefinitionId actionId,
            out int taskIndex)
        {
            foreach (Block block in behaviour.Runtime.Blocks)
            {
                if (TryFindAction(
                    block,
                    actionId,
                    out taskIndex))
                {
                    return block;
                }
            }

            taskIndex = -1;
            return null;
        }

        private bool TryFindAction(
            Block block,
            DefinitionId actionId,
            out int taskIndex)
        {
            int trackOffset = 0;
            foreach (ActionTrack candidate in block.Tracks)
            {
                int actionIndex =
                    candidate.Definition.ActionList.Actions.FindIndex(
                        action =>
                            action != null &&
                            action.DefinitionId == actionId);
                if (actionIndex >= 0)
                {
                    taskIndex = trackOffset + actionIndex;
                    return true;
                }

                trackOffset +=
                    candidate.Definition.ActionList.Actions.Count;
            }

            taskIndex = -1;
            return false;
        }
    }
}
