using System;

namespace Scaffold.VisualScripting
{
    internal sealed class BlockFlowController : IActionFlowController
    {
        public BlockFlowController(Block block, ActionTrack track)
        {
            this.block = block ?? throw new ArgumentNullException(nameof(block));
            this.track = track ?? throw new ArgumentNullException(nameof(track));
        }

        private readonly Block block;
        private readonly ActionTrack track;

        public void JumpTo(int actionIndex)
        {
            block.JumpTo(track, actionIndex);
        }

        public void StopBlock()
        {
            block.Stop();
        }
    }
}
