
using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Scaffold
{
    /// <summary>
    /// Execution state of a Block.
    /// </summary>
    public enum ExecutionState
    {
        /// <summary> No command executing </summary>
        Idle,
        /// <summary> Executing a command </summary>
        Executing,
    }

    /// <summary>
    /// Controls how a Block waits for its (potentially several, parallel) CommandTracks
    /// to finish before the Block itself is considered complete.
    /// </summary>
    public enum BlockAwaitMode
    {
        /// <summary> Block completes only once every track has finished. </summary>
        WaitAll,
        /// <summary> Block completes as soon as any single track finishes; other tracks keep running in the background. </summary>
        WaitAny,
        /// <summary> Block completes immediately after starting all tracks, without waiting on any of them. </summary>
        WaitNone,
    }

    /// <summary>
    /// Controls whether a Block starts its CommandTracks in sequence or together.
    /// </summary>
    public enum BlockExecutionMethod
    {
        /// <summary> Starts the next track after the current track has completed. </summary>
        Sequence,
        /// <summary> Starts every track together. </summary>
        AllAtSameTime,
    }

    /// <summary>
    /// A container for a sequence of Scaffold comands.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Flowchart))]
    [AddComponentMenu("")]
    public class Block : Node
    {
        [SerializeField] protected int itemId = -1; // Invalid flowchart item id

        [FormerlySerializedAs("sequenceName")]
        [Tooltip("The name of the block node as displayed in the Flowchart window")]
        [SerializeField] protected string blockName = "New Block";

        [TextArea(2, 5)]
        [Tooltip("Description text to display under the block node")]
        [SerializeField] protected string description = "";

        [Tooltip("An optional Event Handler which can execute the block when an event occurs")]
        [SerializeField] protected EventHandler eventHandler;

        [HideInInspector]
        [SerializeField] protected List<Command> commandList = new List<Command>();

        [SerializeField] protected List<CommandTrack> tracks = new List<CommandTrack>();

        [Tooltip("Controls whether Command Tracks run one after another or together.")]
        [SerializeField] protected BlockExecutionMethod executionMethod = BlockExecutionMethod.Sequence;

        [Tooltip("Controls how the Block waits for its parallel Command Tracks to finish before completing.")]
        [SerializeField] protected BlockAwaitMode awaitMode = BlockAwaitMode.WaitAll;

        protected ExecutionState executionState;

        protected Command activeCommand;

        // Tracks currently running as part of the in-flight Execute() call, if any.
        protected List<CommandTrack> activeTracks;

        protected Action lastOnCompleteAction;

        /// <summary>
        // Index of last command executed before the current one.
        // -1 indicates no previous command.
        /// </summary>
        protected int previousActiveCommandIndex = -1;

        public int PreviousActiveCommandIndex
        {
            get
            {
                var track = (tracks != null && tracks.Count > 0) ? tracks[0] : null;
                return track != null ? track.PreviousActiveCommandIndex : previousActiveCommandIndex;
            }
        }

        protected int jumpToCommandIndex = -1;

        protected int executionCount;

        /// <summary>
        /// If set, flowchart will not auto select when it is next executed, used by eventhandlers.
        /// Only effects the editor.
        /// </summary>
        public bool SuppressNextAutoSelection { get; set; }

        [SerializeField] bool suppressAllAutoSelections = false;
        

        protected virtual void Awake()
        {
            EnsureTracksInitialized();
            SetExecutionInfo();
        }

        /// <summary>
        /// Guarantees Tracks has at least one CommandTrack, migrating any legacy commandList
        /// data into it first. After this runs, CommandList and Tracks[0].Commands always
        /// refer to the same list, so every editor surface (classic Inspector, Scaffold
        /// Timeline) reads and writes the same underlying data instead of two disconnected ones.
        /// </summary>
        public virtual void EnsureTracksInitialized()
        {
            if (tracks == null)
            {
                tracks = new List<CommandTrack>();
            }

            if (tracks.Count == 0)
            {
                var track = new CommandTrack("Track 0");
                if (commandList != null && commandList.Count > 0)
                {
                    track.Commands.AddRange(commandList);
                    // Clear the legacy field directly, not through the CommandList property:
                    // once "track" is added to tracks below, CommandList resolves to
                    // track.Commands, so clearing via the property would wipe the data we
                    // just migrated instead of the old field.
                    commandList.Clear();
                }
                tracks.Add(track);
            }
        }

        /// <summary>
        /// Populate the command metadata used to control execution.
        /// </summary>
        protected virtual void SetExecutionInfo()
        {
            // Give each child command a reference back to its parent block and track,
            // and tell each command its index within its own track.
            if (tracks != null)
            {
                foreach (var track in tracks)
                {
                    int index = 0;
                    for (int i = 0; i < track.Commands.Count; i++)
                    {
                        var command = track.Commands[i];
                        if (command == null)
                        {
                            continue;
                        }
                        command.ParentBlock = this;
                        command.ParentTrack = track;
                        command.CommandIndex = index++;
                    }
                }
            }

            // Ensure all commands are at their correct indent level
            // This should have already happened in the editor, but may be necessary
            // if commands are added to the Block at runtime.
            UpdateIndentLevels();
        }

#if UNITY_EDITOR
        // The user can modify the command list order while playing in the editor,
        // so we keep the command indices updated every frame. There's no need to
        // do this in player builds so we compile this bit out for those builds.
        protected virtual void Update()
        {
            if (tracks == null)
            {
                return;
            }

            foreach (var track in tracks)
            {
                int index = 0;
                for (int i = 0; i < track.Commands.Count; i++)
                {
                    var command = track.Commands[i];
                    if (command == null)// Null entry will be deleted automatically later
                    {
                        continue;
                    }
                    command.CommandIndex = index++;
                }
            }
        }

#endif
        //editor only state for speeding up flowchart window drawing
        public bool IsSelected { get; set; }    //local cache of selectedness
        public enum FilteredState { Full, Partial, None}
        public FilteredState FilterState { get; set; }    //local cache of filteredness
        public bool IsControlSelected { get; set; } //local cache of being part of the control exclusion group

        #region Public members

        /// <summary>
        /// The execution state of the Block.
        /// </summary>
        public virtual ExecutionState State { get { return executionState; } }

        /// <summary>
        /// Unique identifier for the Block.
        /// </summary>
        public virtual int ItemId { get { return itemId; } set { itemId = value; } }

        /// <summary>
        /// The name of the block node as displayed in the Flowchart window.
        /// </summary>
        public virtual string BlockName { get { return blockName; } set { blockName = value; } }

        /// <summary>
        /// Description text to display under the block node
        /// </summary>
        public virtual string Description { get { return description; } }

        /// <summary>
        /// An optional Event Handler which can execute the block when an event occurs.
        /// Note: Using the concrete class instead of the interface here because of weird editor behaviour.
        /// </summary>
        public virtual EventHandler _EventHandler { get { return eventHandler; } set { eventHandler = value; } }

        /// <summary>
        /// The currently executing command.
        /// </summary>
        public virtual Command ActiveCommand { get { return activeCommand; } }

        /// <summary>
        /// Timer for fading Block execution icon.
        /// </summary>
        public virtual float ExecutingIconTimer { get; set; }

        /// <summary>
        /// The list of commands in the first track (for backward compatibility).
        /// </summary>
        public virtual List<Command> CommandList
        {
            get
            {
                if (tracks != null && tracks.Count > 0) return tracks[0].Commands;
                return commandList;
            }
        }

        public virtual List<CommandTrack> Tracks { get { return tracks; } }

        /// <summary>
        /// Controls whether CommandTracks start in sequence or all at the same time.
        /// </summary>
        public virtual BlockExecutionMethod ExecutionMethod { get { return executionMethod; } set { executionMethod = value; } }

        /// <summary>
        /// Controls how this Block waits for its (possibly several, parallel) CommandTracks to finish.
        /// </summary>
        public virtual BlockAwaitMode AwaitMode { get { return awaitMode; } set { awaitMode = value; } }

        /// <summary>
        /// Controls the next command to execute in the block execution coroutine.
        /// Applies to the first (primary) track, for backward compatibility with single-track Blocks.
        /// </summary>
        public virtual int JumpToCommandIndex
        {
            set
            {
                var track = (tracks != null && tracks.Count > 0) ? tracks[0] : null;
                if (track != null)
                {
                    track.JumpToCommandIndex = value;
                }
                else
                {
                    jumpToCommandIndex = value;
                }
            }
        }

        /// <summary>
        /// Called by a Command to signal it has finished and execution should continue at
        /// nextCommandIndex, within the track the command belongs to.
        /// </summary>
        public virtual void OnCommandCompleted(Command command, int nextCommandIndex)
        {
            var track = command != null ? command.ParentTrack : null;
            if (track != null)
            {
                track.JumpToCommandIndex = nextCommandIndex;
            }
            else
            {
                // Command not associated with a track (e.g. legacy/edge case), fall back
                // to the primary track so execution isn't silently dropped.
                JumpToCommandIndex = nextCommandIndex;
            }
        }

        /// <summary>
        /// Returns the parent Flowchart for this Block.
        /// </summary>
        public virtual Flowchart GetFlowchart()
        {
            return GetComponent<Flowchart>();
        }

        /// <summary>
        /// Returns true if the Block is executing a command.
        /// </summary>
        public virtual bool IsExecuting()
        {
            return (executionState == ExecutionState.Executing);
        }

        /// <summary>
        /// Returns the number of times this Block has executed.
        /// </summary>
        public virtual int GetExecutionCount()
        {
            return executionCount;
        }

        /// <summary>
        /// Start a coroutine which executes all commands in the Block. Only one running instance of each Block is permitted.
        /// </summary>
        public virtual void StartExecution()
        {
            StartCoroutine(Execute());
        }

        /// <summary>
        /// A coroutine method that executes all commands in the Block. Only one running instance of each Block is permitted.
        /// Starts CommandTracks according to ExecutionMethod. AwaitMode controls completion only when
        /// tracks start all at the same time.
        /// </summary>
        /// <param name="commandIndex">Index of command to start execution at, within the first (primary) track</param>
        /// <param name="onComplete">Delegate function to call when execution completes</param>
        public virtual IEnumerator Execute(int commandIndex = 0, Action onComplete = null)
        {
            if (executionState != ExecutionState.Idle)
            {
                Debug.LogWarning(BlockName + " cannot be executed, it is already running.");
                yield break;
            }

            lastOnCompleteAction = onComplete;

            // Always refresh (cheap): tracks/commands may have been added or reordered
            // (via script or the inspector) since Awake() last ran or SetExecutionInfo was called.
            SetExecutionInfo();

            executionCount++;
            var executionCountAtStart = executionCount;

            var flowchart = GetFlowchart();
            executionState = ExecutionState.Executing;
            BlockSignals.DoBlockStart(this);

            bool suppressSelectionChanges = false;

            #if UNITY_EDITOR
            // Select the executing block & the first command
            if (suppressAllAutoSelections || SuppressNextAutoSelection)
            {
                SuppressNextAutoSelection = false;
                suppressSelectionChanges = true;
            }
            else
            {
                flowchart.SelectedBlock = this;
                if (CommandList.Count > 0)
                {
                    flowchart.ClearSelectedCommands();
                    flowchart.AddSelectedCommand(CommandList[0]);
                }
            }
            #endif

            activeTracks = (tracks != null) ? new List<CommandTrack>(tracks) : new List<CommandTrack>();

            for (int t = 0; t < activeTracks.Count; t++)
            {
                var track = activeTracks[t];
                track.IsComplete = false;
                track.ActiveCommand = null;
                track.PreviousActiveCommandIndex = -1;
            }

            if (executionMethod == BlockExecutionMethod.Sequence)
            {
                for (int t = 0; t < activeTracks.Count; t++)
                {
                    var track = activeTracks[t];
                    int startIndex = t == 0 ? commandIndex : 0;
                    bool isPrimaryTrack = t == 0;
                    track.RunningCoroutine = StartCoroutine(ExecuteTrack(track, startIndex, flowchart, suppressSelectionChanges, isPrimaryTrack));
                    yield return track.RunningCoroutine;

                    if (executionCountAtStart != executionCount || executionState != ExecutionState.Executing)
                    {
                        yield break;
                    }
                }
            }
            else
            {
                for (int t = 0; t < activeTracks.Count; t++)
                {
                    var track = activeTracks[t];
                    // Only the primary (first) track honours the requested starting index,
                    // preserving the previous single-track Execute(commandIndex) contract.
                    int startIndex = (t == 0) ? commandIndex : 0;
                    bool isPrimaryTrack = (t == 0);
                    track.RunningCoroutine = StartCoroutine(ExecuteTrack(track, startIndex, flowchart, suppressSelectionChanges, isPrimaryTrack));
                }
            }

            if (executionMethod == BlockExecutionMethod.AllAtSameTime)
            {
                switch (awaitMode)
                {
                    case BlockAwaitMode.WaitNone:
                        break;

                    case BlockAwaitMode.WaitAny:
                        while (executionCountAtStart == executionCount &&
                               activeTracks.Count > 0 &&
                               !activeTracks.Exists(trk => trk.IsComplete))
                        {
                            yield return null;
                        }
                        break;

                    case BlockAwaitMode.WaitAll:
                    default:
                        while (executionCountAtStart == executionCount &&
                               activeTracks.Exists(trk => !trk.IsComplete))
                        {
                            yield return null;
                        }
                        break;
                }
            }

            if(State == ExecutionState.Executing &&
                //ensure we aren't dangling from a previous stopage and stopping a future run
                executionCountAtStart == executionCount)
            {
                ReturnToIdle();
            }
        }

        /// <summary>
        /// Executes the commands of a single CommandTrack in sequence. Multiple instances of this
        /// coroutine run concurrently (one per track) while the Block is executing.
        /// </summary>
        protected virtual IEnumerator ExecuteTrack(CommandTrack track, int startCommandIndex, Flowchart flowchart, bool suppressSelectionChanges, bool isPrimaryTrack)
        {
            var commands = track.Commands;
            track.JumpToCommandIndex = startCommandIndex;

            int i = 0;
            while (true)
            {
                // Executing commands specify the next command to skip to by setting JumpToCommandIndex via Command.Continue()
                if (track.JumpToCommandIndex > -1)
                {
                    i = track.JumpToCommandIndex;
                    track.JumpToCommandIndex = -1;
                }

                // Skip disabled commands, comments and labels
                while (i < commands.Count &&
                      (!commands[i].enabled ||
                        commands[i].GetType().Name == "CommentAction" ||
                        commands[i].GetType().Name == "LabelAction"))
                {
                    i = commands[i].CommandIndex + 1;
                }

                if (i >= commands.Count)
                {
                    break;
                }

                // The previous active command is needed for if / else / else if commands
                track.PreviousActiveCommandIndex = (track.ActiveCommand == null) ? -1 : track.ActiveCommand.CommandIndex;
                if (isPrimaryTrack)
                {
                    previousActiveCommandIndex = track.PreviousActiveCommandIndex;
                }

                var command = commands[i];
                track.ActiveCommand = command;
                activeCommand = command;

                if (flowchart.IsActive() && !suppressSelectionChanges && isPrimaryTrack)
                {
                    // Auto select a command in some situations
                    if ((flowchart.SelectedCommands.Count == 0 && i == 0) ||
                        (flowchart.SelectedCommands.Count == 1 && flowchart.SelectedCommands[0].CommandIndex == track.PreviousActiveCommandIndex))
                    {
                        flowchart.ClearSelectedCommands();
                        flowchart.AddSelectedCommand(commands[i]);
                    }
                }

                command.IsExecuting = true;
                // This icon timer is managed by the FlowchartWindow class, but we also need to
                // set it here in case a command starts and finishes execution before the next window update.
                command.ExecutingIconTimer = Time.realtimeSinceStartup + ScaffoldConstants.ExecutingIconFadeTime;
                BlockSignals.DoCommandExecute(this, command, i, commands.Count);

#if UNITY_EDITOR
                try
                {
                    command.Execute();
                }
                catch (Exception)
                {
                    Debug.LogError("Rethrowing Exception thrown by:" + command.GetLocationIdentifier());
                    throw;
                }
#else
                command.Execute();
#endif

                // Wait until the executing command sets another command to jump to via Command.Continue()
                while (track.JumpToCommandIndex == -1)
                {
                    yield return null;
                }

                #if UNITY_EDITOR
                if (flowchart.StepPause > 0f)
                {
                    yield return new WaitForSeconds(flowchart.StepPause);
                }
                #endif

                command.IsExecuting = false;
            }

            track.IsComplete = true;
            track.ActiveCommand = null;
            track.RunningCoroutine = null;
        }

        private void ReturnToIdle()
        {
            executionState = ExecutionState.Idle;
            activeCommand = null;
            BlockSignals.DoBlockEnd(this);

            if (lastOnCompleteAction != null)
            {
                lastOnCompleteAction();
            }
            lastOnCompleteAction = null;
        }

        /// <summary>
        /// Stop executing commands in this Block, including every currently running CommandTrack.
        /// </summary>
        public virtual void Stop()
        {
            if (activeTracks != null)
            {
                foreach (var track in activeTracks)
                {
                    // Tell the executing command to stop immediately
                    if (track.ActiveCommand != null)
                    {
                        track.ActiveCommand.IsExecuting = false;
                        track.ActiveCommand.OnStopExecuting();
                    }

                    if (track.RunningCoroutine != null)
                    {
                        StopCoroutine(track.RunningCoroutine);
                        track.RunningCoroutine = null;
                    }

                    track.IsComplete = true;
                }
            }

            // Legacy fallback field, harmless to also set for the no-tracks edge case.
            jumpToCommandIndex = int.MaxValue;

            //force idle here so other commands that rely on block not executing are informed this frame rather than next
            ReturnToIdle();
        }

        /// <summary>
        /// Returns a list of all Blocks connected to this one.
        /// </summary>
        public virtual List<Block> GetConnectedBlocks()
        {
            var connectedBlocks = new List<Block>();
            GetConnectedBlocks(ref connectedBlocks);
            return connectedBlocks;
        }

        public virtual void GetConnectedBlocks(ref List<Block> connectedBlocks)
        {
            if (tracks == null)
            {
                return;
            }

            foreach (var track in tracks)
            {
                for (int i = 0; i < track.Commands.Count; i++)
                {
                    var command = track.Commands[i];
                    if (command != null)
                    {
                        command.GetConnectedBlocks(ref connectedBlocks);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the type of the previously executing command (on the primary track).
        /// </summary>
        /// <returns>The previous active command type.</returns>
        public virtual System.Type GetPreviousActiveCommandType()
        {
            var command = GetPreviousActiveCommand();
            return command != null ? command.GetType() : null;
        }

        public virtual int GetPreviousActiveCommandIndent()
        {
            var command = GetPreviousActiveCommand();
            return command != null ? command.IndentLevel : -1;
        }

        public virtual Command GetPreviousActiveCommand()
        {
            var track = (tracks != null && tracks.Count > 0) ? tracks[0] : null;
            if (track != null)
            {
                return track.GetPreviousActiveCommand();
            }

            var index = PreviousActiveCommandIndex;
            if (index >= 0 && index < CommandList.Count)
            {
                return CommandList[index];
            }

            return null;
        }

        /// <summary>
        /// Recalculate the indent levels for all commands in every track.
        /// </summary>
        public virtual void UpdateIndentLevels()
        {
            if (tracks == null)
            {
                return;
            }

            foreach (var track in tracks)
            {
                int indentLevel = 0;
                for (int i = 0; i < track.Commands.Count; i++)
                {
                    var command = track.Commands[i];
                    if (command == null)
                    {
                        continue;
                    }
                    if (command.CloseBlock())
                    {
                        indentLevel--;
                    }
                    // Negative indent level is not permitted
                    indentLevel = Math.Max(indentLevel, 0);
                    command.IndentLevel = indentLevel;
                    if (command.OpenBlock())
                    {
                        indentLevel++;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the index of the Label command with matching key, or -1 if not found.
        /// </summary>
        public virtual int GetLabelIndex(string labelKey)
        {
            if (labelKey.Length == 0)
            {
                return -1;
            }

            for (int i = 0; i < CommandList.Count; i++)
            {
                var command = CommandList[i];
                if (command.GetType().Name == "LabelAction")
                {
                    // TODO: ActionBase doesn't have Key. Skip for now.
                }
            }

            return -1;
        }

        #endregion
    }
}
