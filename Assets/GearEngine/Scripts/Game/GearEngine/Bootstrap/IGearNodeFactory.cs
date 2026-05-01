using GearEngine.GearEngine.Nodes;
using UnityEngine;

namespace GearEngine.GearEngine.Bootstrap
{
    public interface IGearNodeFactory
    {
        IGridNode CreateNode(Vector2Int position, GearItemData configData);
    }
}
