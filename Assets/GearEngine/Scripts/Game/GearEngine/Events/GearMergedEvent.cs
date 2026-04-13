using Scaffold.Events.Contracts;
using UnityEngine;

namespace GearEngine.GearEngine.Events
{
    public record GearMergedEvent(Vector2Int MergePosition, string NewConfigId) : ContextEvent;
}
