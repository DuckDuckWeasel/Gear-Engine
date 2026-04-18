using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Manager
{
    public interface IGridManager
    {
        IEnumerable<IGridNode> GetAllNodes();

        void AddNode(IGridNode node);
        void RemoveNode(Vector2Int pos);
        IGridNode ExtractNode(Vector2Int pos);
        IGridNode GetNode(Vector2Int pos);

        /// <summary>Stops simulation, disposes every node, and clears the grid.</summary>
        void ClearAll();

        /// <summary>Resets rotation, charge, and other per-run state on all nodes (does not remove nodes).</summary>
        void ResetAllNodeSimulationState();

        bool IsRunning { get; }
        void Play();
        void Stop();
    }
}
