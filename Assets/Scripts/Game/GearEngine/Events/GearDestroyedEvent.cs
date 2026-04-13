using Scaffold.Events.Contracts;
using UnityEngine;

namespace GearEngine.GearEngine.Events
{
    public record GearDestroyedEvent(Vector2Int Position) : ContextEvent;
}
