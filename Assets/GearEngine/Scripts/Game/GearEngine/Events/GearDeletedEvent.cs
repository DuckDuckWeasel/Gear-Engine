using Scaffold.Events.Contracts;
using UnityEngine;

namespace GearEngine.GearEngine.Events
{
    public record GearDeletedEvent(Vector2Int Position, int RewardAmount) : ContextEvent;
}
