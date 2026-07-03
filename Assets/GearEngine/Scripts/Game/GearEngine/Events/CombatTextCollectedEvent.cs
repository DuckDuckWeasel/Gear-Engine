using Scaffold.Events.Contracts;

namespace GearEngine.GearEngine.Events
{
    public sealed record CombatTextCollectedEvent(int Score) : ContextEvent;
}
