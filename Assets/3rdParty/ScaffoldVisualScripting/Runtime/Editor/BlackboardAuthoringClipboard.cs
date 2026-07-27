using System;

namespace Scaffold.VisualScripting.Editor
{
    public sealed class BlackboardAuthoringClipboard
    {
        public BlackboardAuthoringClipboard(SerializedGraphCloner cloner, DefinitionIdRegenerator idRegenerator)
        {
            this.cloner = cloner ?? throw new ArgumentNullException(nameof(cloner));
            this.idRegenerator = idRegenerator ?? throw new ArgumentNullException(nameof(idRegenerator));
        }

        public bool HasBlock => block != null;

        public bool HasAction => action != null;

        private readonly SerializedGraphCloner cloner;
        private readonly DefinitionIdRegenerator idRegenerator;
        private BlockDefinition block;
        private IAction action;

        public void Copy(BlockDefinition source)
        {
            block = cloner.CloneGraph(source ?? throw new ArgumentNullException(nameof(source)));
        }

        public void Copy(IAction source)
        {
            action = cloner.CloneGraph(source ?? throw new ArgumentNullException(nameof(source)));
        }

        public BlockDefinition PasteBlock()
        {
            BlockDefinition clone = cloner.CloneGraph(block ?? throw new InvalidOperationException("The Blackboard block clipboard is empty."));
            idRegenerator.Regenerate(clone);
            return clone;
        }

        public IAction PasteAction()
        {
            IAction clone = cloner.CloneGraph(action ?? throw new InvalidOperationException("The Blackboard action clipboard is empty."));
            idRegenerator.Regenerate(clone);
            return clone;
        }
    }
}
