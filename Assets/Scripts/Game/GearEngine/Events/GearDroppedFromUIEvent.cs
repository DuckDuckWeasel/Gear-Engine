using UnityEngine;
using Scaffold.Events.Contracts;

namespace Game.GearEngine.Events
{
    public record GearDroppedFromUIEvent(Vector3 WorldPosition, GearConfigData GearData) : ContextEvent;
}
