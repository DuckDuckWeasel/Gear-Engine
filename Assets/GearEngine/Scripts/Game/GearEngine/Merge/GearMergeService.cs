using GearEngine.GearEngine.Config;
using Scaffold.Events.Contracts;
using UnityEngine;

namespace GearEngine.GearEngine.Merge
{
    public class GearMergeService
    {
        public GearMergeService(IGridManager grid, IEventBus eventBus)
        {
            this.grid = grid;
            this.eventBus = eventBus;
        }

        private readonly IGridManager grid;
        private readonly IEventBus eventBus;

        public bool TryMerge(Vector2Int posA, Vector2Int posB)
        {
            IGridNode nodeA = grid.GetNode(posA);
            IGridNode nodeB = grid.GetNode(posB);

            if (!TryGetMergeablePair(nodeA, nodeB, out string configId, out GearConfig nextLevelConfig))
            {
                return false;
            }

            grid.RemoveNode(posA);
            grid.RemoveNode(posB);

            Debug.Log($"<color=#ff9900>[GearMergeService]</color> Merged {configId} -> Upgrade to Next Level at {posA}");

            eventBus.Raise(new GearMergedEvent(posA, nextLevelConfig.Id));
            return true;
        }

        private bool TryGetMergeablePair(IGridNode nodeA, IGridNode nodeB, out string configId, out GearConfig nextLevelConfig)
        {
            configId = null;
            nextLevelConfig = null;

            if (!TryParseMatchingConfigIds(nodeA, nodeB, out string configIdA))
            {
                return false;
            }

            nextLevelConfig = nodeA.ConfigData?.NextLevelConfig;
            if (nextLevelConfig == null)
            {
                Debug.Log("[GearMergeService] No next-level config defined. Merge rejected.");
                return false;
            }

            configId = configIdA;
            return true;
        }

        private static bool TryParseMatchingConfigIds(IGridNode nodeA, IGridNode nodeB, out string configIdA)
        {
            configIdA = null;

            if (nodeA == null || nodeB == null)
            {
                return false;
            }

            configIdA = nodeA.ConfigData?.Id;
            string configIdB = nodeB.ConfigData?.Id;

            return !string.IsNullOrEmpty(configIdA) && configIdA == configIdB;
        }
    }
}
