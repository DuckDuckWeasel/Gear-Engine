using Scaffold.Events.Contracts;
using UnityEngine;

namespace Game.GearEngine
{
    public record GearRotatedEvent(Vector2Int Source) : ContextEvent;
}
