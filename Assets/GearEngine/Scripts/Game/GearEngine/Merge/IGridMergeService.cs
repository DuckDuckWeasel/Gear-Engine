using UnityEngine;

namespace GearEngine.GearEngine.Merge
{
    public interface IGridMergeService
    {
        bool TryMerge(Vector2Int posA, Vector2Int posB);
        
        IGridNode MergeNodes(IGridNode draggedNode, IGridNode occupantNode, Vector2Int targetPos);
    }
}
