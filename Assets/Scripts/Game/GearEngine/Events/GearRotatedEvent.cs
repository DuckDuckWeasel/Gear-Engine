using Scaffold.Events.Contracts;
using UnityEngine;

namespace Scaffold.GearEngine.Events
{
    public record GearRotatedEvent(Vector2Int Source) : ContextEvent;
}
