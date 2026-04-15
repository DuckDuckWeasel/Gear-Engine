using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Manager
{
    public interface IGridManager
    {
        float GlobalSpeedModifier { get; set; }

        IEnumerable<IGridNode> GetAllNodes();

        void AddNode(IGridNode node);
        void RemoveNode(Vector2Int pos);
        IGridNode ExtractNode(Vector2Int pos);
        IGridNode GetNode(Vector2Int pos);

        // Atomic Domain Operations
        void SwapNodes(Vector2Int posA, Vector2Int posB);
        void SwapNodes(IGridNode nodeA, IGridNode nodeB);
        void MergeNode(Vector2Int targetPos, IGridNode newNode);

        bool IsRunning { get; }
        void Play();
        void Stop();
    }
}
