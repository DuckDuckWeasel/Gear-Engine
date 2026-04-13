using System.Collections.Generic;
using Scaffold.Events.Contracts;
using UnityEngine;

namespace Scaffold.GearEngine.Merge
{
    public class GearMergeService
    {
        private readonly IGridManager grid;
        private readonly IEventBus eventBus;

        public GearMergeService(IGridManager grid, IEventBus eventBus)
        {
            this.grid = grid;
            this.eventBus = eventBus;
        }

        public bool TryMerge(Vector2Int posA, Vector2Int posB)
        {
            var nodeA = grid.GetNode(posA);
            var nodeB = grid.GetNode(posB);

            if (nodeA == null || nodeB == null) return false;

            string configIdA = nodeA.ConfigData?.Id;
            string configIdB = nodeB.ConfigData?.Id;

            if (string.IsNullOrEmpty(configIdA) || configIdA != configIdB) return false;

            var nextLevelConfig = nodeA.ConfigData?.NextLevelConfig;
            if (nextLevelConfig == null)
            {
                Debug.Log("[GearMergeService] No next-level config defined. Merge rejected.");
                return false;
            }

            grid.RemoveNode(posA);
            grid.RemoveNode(posB);

            Debug.Log($"<color=#ff9900>[GearMergeService]</color> Merged {configIdA} -> Upgrade to Next Level at {posA}");

            eventBus.Raise(new GearMergedEvent(posA, nextLevelConfig.Id));
            return true;
        }
    }
}
