using System.Collections.Generic;
using UnityEngine;

namespace Game.GearEngine
{
    public interface IGridManager
    {
        float GlobalSpeedModifier { get; set; }
        
        void AddNode(IGridNode node);
        void RemoveNode(Vector2Int pos);
        IGridNode GetNode(Vector2Int pos);

        bool IsRunning { get; }
        void Play();
        void Stop();
    }
}
