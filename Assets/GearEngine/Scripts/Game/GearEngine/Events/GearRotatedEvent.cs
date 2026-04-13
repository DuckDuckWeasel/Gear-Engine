using Scaffold.Events.Contracts;
using UnityEngine;

namespace GearEngine.GearEngine.Events
{
    public record GearRotatedEvent(Vector2Int Source) : ContextEvent;
}
