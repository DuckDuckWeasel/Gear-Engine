using System;

namespace Scaffold.VisualScripting
{
    public sealed class ActionTrack : IDisposable
    {
        public ActionTrack(Blackboard blackboard, Block block, ActionTrackDefinition definition, Func<float> getRandomValue)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            ActionList = new ActionList(blackboard, block, this, definition.ActionList, getRandomValue);
        }

        public ActionTrackDefinition Definition { get; }

        public ActionList ActionList { get; }

        public ExecutionId ExecutionId { get; private set; }

        public void Dispose()
        {
            ActionList.Dispose();
        }

        internal void BeginExecution(ExecutionId blockExecutionId)
        {
            ExecutionId = ExecutionId.New();
            ActionList.BeginExecution(blockExecutionId, ExecutionId);
        }
    }
}
