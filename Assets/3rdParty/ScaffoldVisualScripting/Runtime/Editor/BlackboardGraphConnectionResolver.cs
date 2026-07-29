using System;
using System.Collections.Generic;

namespace Scaffold.VisualScripting.Editor
{
    public sealed class BlackboardGraphConnectionResolver
    {
        public IReadOnlyList<BlackboardGraphConnection> Resolve(BlackboardDefinition definition)
        {
            List<BlackboardGraphConnection> connections = new List<BlackboardGraphConnection>();
            Dictionary<string, BlockDefinition> blocks = IndexBlocks(definition);
            for (int index = 0; index < definition.Blocks.Count; index++)
            {
                AddBlockConnections(definition.Blocks[index], blocks, connections);
            }

            return connections;
        }

        private Dictionary<string, BlockDefinition> IndexBlocks(BlackboardDefinition definition)
        {
            Dictionary<string, BlockDefinition> blocks = new Dictionary<string, BlockDefinition>(StringComparer.Ordinal);
            for (int index = 0; index < definition.Blocks.Count; index++)
            {
                BlockDefinition block = definition.Blocks[index];
                if (block != null && !string.IsNullOrWhiteSpace(block.Name) && !blocks.ContainsKey(block.Name))
                {
                    blocks.Add(block.Name, block);
                }
            }

            return blocks;
        }

        private void AddBlockConnections(BlockDefinition source, IReadOnlyDictionary<string, BlockDefinition> blocks, ICollection<BlackboardGraphConnection> result)
        {
            if (source == null)
            {
                return;
            }

            HashSet<string> names = CollectConnectionNames(source);
            foreach (string name in names)
            {
                if (blocks.TryGetValue(name, out BlockDefinition destination))
                {
                    result.Add(new BlackboardGraphConnection(source, destination));
                }
            }
        }

        private HashSet<string> CollectConnectionNames(BlockDefinition block)
        {
            HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
            for (int trackIndex = 0; trackIndex < block.Tracks.Count; trackIndex++)
            {
                List<IAction> actions = block.Tracks[trackIndex].ActionList.Actions;
                for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
                {
                    if (actions[actionIndex] is IBlockConnectionSource source)
                    {
                        source.GetConnectedBlockNames(names);
                    }
                }
            }

            return names;
        }
    }
}
