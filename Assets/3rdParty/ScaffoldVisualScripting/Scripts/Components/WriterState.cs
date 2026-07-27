namespace Scaffold
{
    /// <summary>
    /// Current state of the writing process.
    /// </summary>
    public enum WriterState
    {
        /// <summary> Invalid state. </summary>
        Invalid,

        /// <summary> Writer has started writing. </summary>
        Start,

        /// <summary> Writing has been paused. </summary>
        Pause,

        /// <summary> Writing has resumed after a pause. </summary>
        Resume,

        /// <summary> Writing has ended. </summary>
        End
    }
}
