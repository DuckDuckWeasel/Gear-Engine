using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting.Editor
{
    public sealed class BlackboardAuthoringClipboard
    {
        public BlackboardAuthoringClipboard(SerializedGraphCloner cloner, DefinitionIdRegenerator idRegenerator)
        {
            this.cloner = cloner ?? throw new ArgumentNullException(nameof(cloner));
            this.idRegenerator = idRegenerator ?? throw new ArgumentNullException(nameof(idRegenerator));
        }

        public bool HasBlock => blocks.Count > 0;

        public bool HasAction => action != null;

        private readonly SerializedGraphCloner cloner;
        private readonly DefinitionIdRegenerator idRegenerator;
        private readonly List<BlockDefinition> blocks = new List<BlockDefinition>();
        private IAction action;

        public void Copy(BlockDefinition source)
        {
            blocks.Clear();
            blocks.Add(cloner.CloneGraph(source ?? throw new ArgumentNullException(nameof(source))));
        }

        public void Copy(IReadOnlyList<BlockDefinition> source)
        {
            blocks.Clear();
            for (int index = 0; source != null && index < source.Count; index++)
            {
                if (source[index] != null)
                {
                    blocks.Add(cloner.CloneGraph(source[index]));
                }
            }
        }

        public void Copy(IAction source)
        {
            action = cloner.CloneGraph(source ?? throw new ArgumentNullException(nameof(source)));
        }

        public BlockDefinition PasteBlock()
        {
            if (blocks.Count == 0)
            {
                throw new InvalidOperationException("The Blackboard block clipboard is empty.");
            }

            BlockDefinition clone = cloner.CloneGraph(blocks[0]);
            idRegenerator.Regenerate(clone);
            return clone;
        }

        public IReadOnlyList<BlockDefinition> PasteBlocks()
        {
            List<BlockDefinition> pasted = new List<BlockDefinition>();
            for (int index = 0; index < blocks.Count; index++)
            {
                BlockDefinition clone = cloner.CloneGraph(blocks[index]);
                idRegenerator.Regenerate(clone);
                pasted.Add(clone);
            }

            return pasted;
        }

        public IAction PasteAction()
        {
            IAction clone = cloner.CloneGraph(action ?? throw new InvalidOperationException("The Blackboard action clipboard is empty."));
            idRegenerator.Regenerate(clone);
            return clone;
        }
    }
}
