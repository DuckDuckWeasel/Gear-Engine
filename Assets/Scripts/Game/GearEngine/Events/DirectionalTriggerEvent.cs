using Scaffold.Events.Contracts;
using UnityEngine;

namespace Scaffold.GearEngine.Events
{
    public record DirectionalTriggerEvent(Vector2Int TargetPosition, float ChargeOnTriggerAmount, float SourceRotationSign = 1f) : ContextEvent;
}
