using UnityEngine;
using System.Collections.Generic;

namespace Scaffold
{
    /// <summary>
    /// Represents a sequence of commands that execute sequentially.
    /// A Block can have multiple CommandTracks executing in parallel.
    /// </summary>
    [System.Serializable]
    public class CommandTrack
    {
        [SerializeField]
        protected string trackName = "Track";

        [SerializeField]
        protected List<Command> commands = new List<Command>();

        public string TrackName
        {
            get { return trackName; }
            set { trackName = value; }
        }

        public List<Command> Commands
        {
            get { return commands; }
            set { commands = value; }
        }

        public CommandTrack() { }

        public CommandTrack(string name)
        {
            trackName = name;
        }

        #region Runtime execution state

        // Populated while the parent Block is executing this track. Not serialized:
        // purely transient state for the currently running (or last run) coroutine.
        [System.NonSerialized] public Command ActiveCommand;
        [System.NonSerialized] public int PreviousActiveCommandIndex = -1;
        [System.NonSerialized] public int JumpToCommandIndex = -1;
        [System.NonSerialized] public bool IsComplete;
        [System.NonSerialized] public UnityEngine.Coroutine RunningCoroutine;

        /// <summary>
        /// Returns the command that was active before the current one, within this track.
        /// </summary>
        public Command GetPreviousActiveCommand()
        {
            if (PreviousActiveCommandIndex >= 0 && PreviousActiveCommandIndex < commands.Count)
            {
                return commands[PreviousActiveCommandIndex];
            }

            return null;
        }

        #endregion
    }
}
