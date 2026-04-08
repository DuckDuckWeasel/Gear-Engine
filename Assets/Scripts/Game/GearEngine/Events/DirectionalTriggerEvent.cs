using Scaffold.Events.Contracts;
using UnityEngine;

namespace Game.GearEngine
{
    public record DirectionalTriggerEvent(Vector2Int TargetPosition, float ChargeOnTriggerAmount) : ContextEvent;
}
