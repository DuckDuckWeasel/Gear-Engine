using Scaffold;

namespace GearEngine.Core.Actions
{
    /// <summary>
    /// Implemented by IActions that require full access to the Command host 
    /// (e.g. for flow control, branching, loops).
    /// </summary>
    public interface ICommandContextConsumer
    {
        void SetCommandContext(Command hostCommand);
    }
}
