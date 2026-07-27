using System;
using System.Collections.Generic;
using GearEngine.Core.Architecture.References;

namespace GearEngine.Actions.Input
{
    [Serializable]
    public sealed class DropTargetConfig
    {
        public TargetReference Target = new TargetReference();
        public List<int> AllowedNodeIndices = new List<int>();
    }
}
