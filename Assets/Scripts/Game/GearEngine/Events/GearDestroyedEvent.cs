using Scaffold.Events.Contracts;
using UnityEngine;

namespace Game.GearEngine
{
    public record GearDestroyedEvent(Vector2Int Position) : ContextEvent;
}
