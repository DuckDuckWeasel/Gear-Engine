using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using Scaffold.Events.Contracts;
using UnityEngine;

namespace GearEngine.GearEngine.Merge
{
    public class GridMergeService : IGridMergeService
    {
        public GridMergeService(
            IGridManager grid,
            IEventBus eventBus,
            Bootstrap.IGearNodeFactory nodeFactory,
            IInventoryService inventoryService)
        {
            this.grid = grid;
            this.eventBus = eventBus;
            this.nodeFactory = nodeFactory;
            this.inventoryService = inventoryService;
        }

        private readonly IGridManager grid;
        private readonly IEventBus eventBus;
        private readonly Bootstrap.IGearNodeFactory nodeFactory;
        private readonly IInventoryService inventoryService;

        public IGridNode MergeNodes(IGridNode draggedNode, IGridNode occupantNode, Vector2Int targetPos)
        {
            if (draggedNode == null || occupantNode == null || occupantNode.ConfigData?.NextLevelConfig == null)
            {
                return null;
            }

            grid.ExtractNode(draggedNode.Position);
            grid.ExtractNode(occupantNode.Position);

            GearItem nextConfig = occupantNode.ConfigData.NextLevelConfig;
            GearItemData upgradedData;
            if (draggedNode.ConfigData?.Owner != null && occupantNode.ConfigData?.Owner != null && inventoryService != null)
            {
                inventoryService.Remove(draggedNode.ConfigData.Owner);
                inventoryService.Remove(occupantNode.ConfigData.Owner);
                OwnedGear newOwner = inventoryService.Add(nextConfig);
                upgradedData = newOwner.Config.CreateRuntimeData();
                upgradedData.Owner = newOwner;
            }
            else
            {
                upgradedData = nextConfig.CreateRuntimeData();
            }

            IGridNode newNode = nodeFactory.CreateNode(targetPos, upgradedData);

            grid.AddNode(newNode);

            return newNode;
        }

        public bool TryMerge(Vector2Int posA, Vector2Int posB)
        {
            IGridNode nodeA = grid.GetNode(posA);
            IGridNode nodeB = grid.GetNode(posB);

            if (!TryGetMergeablePair(nodeA, nodeB, out string configId, out GearItem nextLevelConfig))
            {
                return false;
            }

            grid.RemoveNode(posA);
            grid.RemoveNode(posB);

            Debug.Log($"<color=#ff9900>[GearMergeService]</color> Merged {configId} -> Upgrade to Next Level at {posA}");

            eventBus.Raise(new GearMergedEvent(posA, nextLevelConfig.Id));
            return true;
        }

        private bool TryGetMergeablePair(IGridNode nodeA, IGridNode nodeB, out string configId, out GearItem nextLevelConfig)
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
