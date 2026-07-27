using Scaffold;

namespace GearEngine.Core.Actions
{
    /// <summary>
    /// Implemented by IActions that require access to the parent Blackboard 
    /// (e.g. for variable resolution).
    /// </summary>
    public interface IBlackboardConsumer
    {
        void SetBlackboard(Blackboard blackboard);
    }
}
