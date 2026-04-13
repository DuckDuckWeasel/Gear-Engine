using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.GearEngine.Manager
{
    public interface IGridManager
    {
        float GlobalSpeedModifier { get; set; }

        IEnumerable<IGridNode> GetAllNodes();

        void AddNode(IGridNode node);
        void RemoveNode(Vector2Int pos);
        IGridNode ExtractNode(Vector2Int pos);
        IGridNode GetNode(Vector2Int pos);

        bool IsRunning { get; }
        void Play();
        void Stop();
    }
}
