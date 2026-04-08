using Scaffold.Events.Contracts;
using UnityEngine;

namespace Game.GearEngine
{
    public record GearMergedEvent(Vector2Int MergePosition, string NewConfigId) : ContextEvent;
}
