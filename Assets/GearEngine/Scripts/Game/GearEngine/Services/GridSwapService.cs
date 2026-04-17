using GearEngine.GearEngine.Manager;
using UnityEngine;

namespace GearEngine.GearEngine.Services
{
    public class GridSwapService : IGridSwapService
    {
        public GridSwapService(IGridManager gridManager)
        {
            this.gridManager = gridManager;
        }

        private readonly IGridManager gridManager;

        public void SwapNodes(Vector2Int posA, Vector2Int posB)
        {
            IGridNode nodeA = gridManager.ExtractNode(posA);
            IGridNode nodeB = gridManager.ExtractNode(posB);

            if (nodeA != null)
            {
                nodeA.SetPosition(posB);
                gridManager.AddNode(nodeA);
            }

            if (nodeB != null)
            {
                nodeB.SetPosition(posA);
                gridManager.AddNode(nodeB);
            }
        }

        public void SwapNodes(IGridNode nodeA, IGridNode nodeB)
        {
            if (nodeA == null || nodeB == null)
            {
                Debug.LogWarning("[GridSwapService] Attempted to swap null nodes.");
                return;
            }

            Vector2Int posA = nodeA.Position;
            Vector2Int posB = nodeB.Position;

            Debug.Log($"[GridSwapService] Executing swap between '{nodeA.ConfigData?.Id}' at {posA} and '{nodeB.ConfigData?.Id}' at {posB}.");

            // Extract them safely in case they are still tracked by the grid
            gridManager.ExtractNode(posA);
            gridManager.ExtractNode(posB);

            // Reassign opposite positions and re-insert
            nodeA.SetPosition(posB);
            gridManager.AddNode(nodeA);

            nodeB.SetPosition(posA);
            gridManager.AddNode(nodeB);
            
            Debug.Log($"[GridSwapService] Swap Complete! '{nodeA.ConfigData?.Id}' is now at {nodeA.Position}. '{nodeB.ConfigData?.Id}' is now at {nodeB.Position}.");
        }
    }
}
