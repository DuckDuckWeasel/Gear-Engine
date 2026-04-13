using Scaffold.Events.Contracts;
using UnityEngine;

namespace Game.GearEngine
{
    public record GearDeletedEvent(Vector2Int Position, int RewardAmount) : ContextEvent;
}
