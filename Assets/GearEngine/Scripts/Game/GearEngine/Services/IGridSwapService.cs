using UnityEngine;

namespace GearEngine.GearEngine.Services
{
    public interface IGridSwapService
    {
        void SwapNodes(Vector2Int posA, Vector2Int posB);
        void SwapNodes(IGridNode nodeA, IGridNode nodeB);
    }
}
