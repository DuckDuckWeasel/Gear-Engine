namespace Scaffold
{
    /// <summary>
    /// Supported target types for messages.
    /// </summary>
    public enum MessageTarget
    {
        /// <summary>
        /// Send a message to the Blackboard containing the action.
        /// </summary>
        SameBlackboard,

        /// <summary>
        /// Broadcast a message to all Blackboards.
        /// </summary>
        AllBlackboards
    }
}
