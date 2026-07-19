using System;

namespace Scaffold
{
    /// <summary>
    /// Adapts a visible Block command to the same composite task contract used by Invoke Action.
    /// </summary>
    public sealed class CommandCompositeTask : ICompositeTask
    {
        private readonly CommandExecutionContext context;

        public CommandCompositeTask(CommandExecutionContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            if (context.Block == null)
            {
                throw new ArgumentException("A Block is required.", nameof(context));
            }

            if (context.Command == null)
            {
                throw new ArgumentException("A Command is required.", nameof(context));
            }

            if (context.Track == null)
            {
                throw new ArgumentException("A CommandTrack is required.", nameof(context));
            }

            if (context.Blackboard == null)
            {
                throw new ArgumentException("A Blackboard is required.", nameof(context));
            }
        }

        public bool IsEnabled =>
            context.IsIncluded &&
            context.Command.enabled &&
            context.Command.GetType().Name != "CommentAction" &&
            context.Command.GetType().Name != "LabelAction";

        public float Utility => context.Command.CompositeUtility;

        public float Weight => context.Block.GetCommandWeight(context.Command);

        public bool BlockDuringExecution => context.Command.CompositeBlockDuringExecution;

        public void Execute(Action<CompositeExecutionStatus> onComplete)
        {
            context.Block.StartCompositeCommand(context, onComplete);
        }

        public void Interrupt()
        {
            context.Block.StopCompositeCommand(context.Command);
        }
    }
}
