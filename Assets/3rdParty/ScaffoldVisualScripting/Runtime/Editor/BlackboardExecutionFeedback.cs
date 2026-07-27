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
            Block block = FindActionBlock(behaviour, actionId, out ActionTrack track, out int actionIndex);
            status = default;
            return block != null && track.ActionList.TryGetActionStatus(actionIndex, out status);
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

        private Block FindActionBlock(BlackboardBehaviour behaviour, DefinitionId actionId, out ActionTrack track, out int actionIndex)
        {
            track = null;
            actionIndex = -1;
            if (behaviour == null || !behaviour.IsRuntimeAvailable)
            {
                return null;
            }

            return FindActionInBlocks(behaviour, actionId, out track, out actionIndex);
        }

        private Block FindActionInBlocks(BlackboardBehaviour behaviour, DefinitionId actionId, out ActionTrack track, out int actionIndex)
        {
            foreach (Block block in behaviour.Runtime.Blocks)
            {
                if (TryFindAction(block, actionId, out track, out actionIndex))
                {
                    return block;
                }
            }

            track = null;
            actionIndex = -1;
            return null;
        }

        private bool TryFindAction(Block block, DefinitionId actionId, out ActionTrack track, out int actionIndex)
        {
            foreach (ActionTrack candidate in block.Tracks)
            {
                actionIndex = candidate.Definition.ActionList.Actions.FindIndex(action => action != null && action.DefinitionId == actionId);
                if (actionIndex >= 0)
                {
                    track = candidate;
                    return true;
                }
            }

            track = null;
            actionIndex = -1;
            return false;
        }
    }
}
