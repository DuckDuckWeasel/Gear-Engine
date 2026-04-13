using Scaffold.Events.Contracts;
using UnityEngine;

namespace Scaffold.GearEngine.Events
{
    public record GearDestroyedEvent(Vector2Int Position) : ContextEvent;
}
