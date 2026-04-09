using Scaffold.Events.Contracts;
using UnityEngine;

namespace Game.GearEngine
{
    public record DirectionalTriggerEvent(Vector2Int TargetPosition, float ChargeOnTriggerAmount, float SourceRotationSign = 1f) : ContextEvent;
}
