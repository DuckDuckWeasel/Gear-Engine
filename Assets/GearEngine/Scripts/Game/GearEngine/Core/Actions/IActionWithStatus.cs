using System;

namespace GearEngine.Core.Actions
{
    /// <summary>
    /// Allows an action to report success or failure to a composite execution host.
    /// </summary>
    public interface IActionWithStatus
    {
        void ExecuteWithStatus(Action<ActionExecutionStatus> onComplete);
    }
}
