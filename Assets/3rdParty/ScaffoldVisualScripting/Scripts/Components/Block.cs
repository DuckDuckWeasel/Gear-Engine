
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
    /// A container for a sequence of Scaffold comands.
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(Blackboard))]
    [AddComponentMenu("")]
    public class Block : Node
    {
        [SerializeField] protected int itemId = -1; // Invalid blackboard item id

        [FormerlySerializedAs("sequenceName")]
        [Tooltip("The name of the block node as displayed in the Blackboard window")]
        [SerializeField] protected string blockName = "New Block";

        [TextArea(2, 5)]
        [Tooltip("Description text to display under the block node")]
        [SerializeField] protected string description = "";

        [Tooltip("An optional Event Handler which can execute the block when an event occurs")]
        [SerializeField] protected EventHandler eventHandler;

        [HideInInspector]
        [SerializeField] protected List<Command> commandList = new List<Command>();

        [SerializeField] protected List<CommandTrack> tracks = new List<CommandTrack>();

        [Tooltip("Controls the shared composite behavior used to execute Commands.")]
        [SerializeField]
        protected CompositeExecutionMethod executionMethod =
            CompositeExecutionMethod.Sequence;

        [Tooltip("Controls how the Block waits for its parallel Commands to finish before completing.")]
        [SerializeField] protected CompositeAwaitMode awaitMode = CompositeAwaitMode.WaitAll;

        [Tooltip("Controls the Command order for Sequence and Selector.")]
        [SerializeField] protected CompositeOrderMode orderMode = CompositeOrderMode.Ordered;

        [Tooltip("Prevents Random and Shuffle from starting with the Command that executed last in the previous run.")]
        [SerializeField] protected bool avoidRepeatingLastCommand;

        protected ExecutionState executionState;

        protected Command activeCommand;

        // Tracks whose visible Commands are part of the in-flight Execute() call, if any.
        protected List<CommandTrack> activeTracks;

        private readonly List<ICompositeTask> compositeCommandTasks = new List<ICompositeTask>();
        private readonly List<Command> compositeCommands = new List<Command>();
        private readonly Dictionary<Command, int> compositeTaskIndexes =
            new Dictionary<Command, int>();
        private readonly Dictionary<Command, Action<CompositeExecutionStatus>> compositeCompletions =
            new Dictionary<Command, Action<CompositeExecutionStatus>>();
        private CompositeExecutionRunner compositeRunner;
        private Command lastExecutedCommand;
        private bool compositeExecutionCompleted;
        private CompositeExecutionStatus lastCompositeExecutionStatus =
            CompositeExecutionStatus.Success;

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
                CommandTrack track = (tracks != null && tracks.Count > 0) ? tracks[0] : null;
                return track != null ? track.PreviousActiveCommandIndex : previousActiveCommandIndex;
            }
        }

        protected int jumpToCommandIndex = -1;

        protected int executionCount;

        /// <summary>
        /// If set, blackboard will not auto select when it is next executed, used by eventhandlers.
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
                CommandTrack track = new CommandTrack("Track 0");
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
                foreach (CommandTrack track in tracks)
                {
                    int index = 0;
                    for (int i = 0; i < track.Commands.Count; i++)
                    {
                        Command command = track.Commands[i];
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

        protected virtual void Update()
        {
            compositeRunner?.Tick();
#if UNITY_EDITOR
            UpdateCommandIndexes();
#endif
        }

#if UNITY_EDITOR
        private void UpdateCommandIndexes()
        {
            if (tracks == null)
            {
                return;
            }

            foreach (CommandTrack track in tracks)
            {
                int index = 0;
                for (int i = 0; i < track.Commands.Count; i++)
                {
                    Command command = track.Commands[i];
                    if (command == null)// Null entry will be deleted automatically later
                    {
                        continue;
                    }
                    command.CommandIndex = index++;
                }
            }
        }
#endif
        //editor only state for speeding up blackboard window drawing
        public bool IsSelected { get; set; }    //local cache of selectedness
        public enum FilteredState { Full, Partial, None }
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
        /// The name of the block node as displayed in the Blackboard window.
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
                if (tracks != null && tracks.Count > 0)
                {
                    return tracks[0].Commands;
                }

                return commandList;
            }
        }

        public virtual List<CommandTrack> Tracks { get { return tracks; } }

        /// <summary>
        /// Controls how the Block's visible Commands execute.
        /// </summary>
        public virtual CompositeExecutionMethod ExecutionMethod { get { return executionMethod; } set { executionMethod = value; } }

        /// <summary>
        /// Controls how this Block waits for parallel Commands to finish.
        /// </summary>
        public virtual CompositeAwaitMode AwaitMode { get { return awaitMode; } set { awaitMode = value; } }

        public virtual CompositeOrderMode OrderMode { get { return orderMode; } set { orderMode = value; } }

        public virtual bool AvoidRepeatingLastCommand
        {
            get { return avoidRepeatingLastCommand; }
            set { avoidRepeatingLastCommand = value; }
        }

        public virtual CompositeExecutionStatus LastCompositeExecutionStatus
        {
            get { return lastCompositeExecutionStatus; }
        }

        /// <summary>
        /// Returns the effective percentage for a command in its containing track.
        /// Manual overrides reserve their percentages and enabled automatic commands
        /// share the remainder equally. Disabled commands contribute zero percent.
        /// </summary>
        public virtual float GetCommandWeight(Command command)
        {
            List<Command> commands = GetCommandWeightList(command);
            if (!IsCommandWeightEligible(command) || commands == null)
            {
                return 0f;
            }

            GetEnabledCommandWeightBalance(
                commands,
                out float overrideTotal,
                out int automaticCommandCount);
            if (overrideTotal >= 100f)
            {
                return command.HasCompositeWeightOverride && overrideTotal > 0f
                    ? command.CompositeWeight / overrideTotal * 100f
                    : 0f;
            }

            if (command.HasCompositeWeightOverride)
            {
                return command.CompositeWeight;
            }

            return automaticCommandCount > 0
                ? (100f - overrideTotal) / automaticCommandCount
                : 0f;
        }

        private List<Command> GetCommandWeightList(Command command)
        {
            if (command == null)
            {
                return null;
            }

            if (command.ParentTrack != null && command.ParentTrack.Commands.Contains(command))
            {
                return command.ParentTrack.Commands;
            }

            if (tracks != null)
            {
                foreach (CommandTrack track in tracks)
                {
                    if (track != null && track.Commands.Contains(command))
                    {
                        return track.Commands;
                    }
                }
            }

            return CommandList.Contains(command) ? CommandList : null;
        }

        private static void GetEnabledCommandWeightBalance(
            List<Command> commands,
            out float overrideTotal,
            out int automaticCommandCount)
        {
            overrideTotal = 0f;
            automaticCommandCount = 0;
            foreach (Command command in commands)
            {
                if (!IsCommandWeightEligible(command))
                {
                    continue;
                }

                if (command.HasCompositeWeightOverride)
                {
                    overrideTotal += command.CompositeWeight;
                    continue;
                }

                automaticCommandCount++;
            }
        }

        private static bool IsCommandWeightEligible(Command command)
        {
            return command != null && command.IsWeightEligible();
        }

        /// <summary>
        /// Controls the next command to execute in the block execution coroutine.
        /// Applies to the first (primary) track, for backward compatibility with single-track Blocks.
        /// </summary>
        public virtual int JumpToCommandIndex
        {
            set
            {
                CommandTrack track = (tracks != null && tracks.Count > 0) ? tracks[0] : null;
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
            Action<CompositeExecutionStatus> completion;
            if (command != null && compositeCompletions.TryGetValue(command, out completion))
            {
                compositeCompletions.Remove(command);
                command.IsExecuting = false;
                RequestOrderedCommandHandoff(command, nextCommandIndex);
                CompositeExecutionStatus status = GetCommandStatus(command);
                CompleteCompositeCommand(command, completion, status);
                return;
            }

            CommandTrack track = command != null ? command.ParentTrack : null;
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
        /// Returns the parent Blackboard for this Block.
        /// </summary>
        public virtual Blackboard GetBlackboard()
        {
            return GetComponent<Blackboard>();
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
        /// Executes the visible Commands through the same composite runtime used by Invoke Action.
        /// AwaitMode controls completion when commands start in parallel.
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

            ResetExecutionFeedback();

            lastOnCompleteAction = onComplete;

            // Always refresh (cheap): tracks/commands may have been added or reordered
            // (via script or the inspector) since Awake() last ran or SetExecutionInfo was called.
            SetExecutionInfo();

            executionCount++;
            int executionCountAtStart = executionCount;

            Blackboard blackboard = GetBlackboard();
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
                blackboard.SelectedBlock = this;
                if (CommandList.Count > 0)
                {
                    blackboard.ClearSelectedCommands();
                    blackboard.AddSelectedCommand(CommandList[0]);
                }
            }
#endif

            activeTracks = (tracks != null) ? new List<CommandTrack>(tracks) : new List<CommandTrack>();

            for (int t = 0; t < activeTracks.Count; t++)
            {
                CommandTrack track = activeTracks[t];
                track.ActiveCommand = null;
                track.PreviousActiveCommandIndex = -1;
                track.JumpToCommandIndex = -1;
            }

            BlockCompositeExecutionContext compositeContext =
                new BlockCompositeExecutionContext
                {
                    CommandIndex = commandIndex,
                    Blackboard = blackboard,
                    SuppressSelectionChanges = suppressSelectionChanges,
                };
            CreateCompositeCommandRunner(compositeContext);
            compositeExecutionCompleted = false;
            StartCompositeCommandRunner();

            while (!compositeExecutionCompleted &&
                   executionCountAtStart == executionCount &&
                   executionState == ExecutionState.Executing)
            {
                yield return null;
            }

            if (State == ExecutionState.Executing &&
                //ensure we aren't dangling from a previous stopage and stopping a future run
                executionCountAtStart == executionCount)
            {
                ReturnToIdle();
            }
        }

        protected virtual float GetCompositeRandomValue()
        {
            return UnityEngine.Random.value;
        }

        private void CreateCompositeCommandRunner(BlockCompositeExecutionContext compositeContext)
        {
            RememberLastExecutedCommand();
            compositeRunner?.Stop();
            compositeCommandTasks.Clear();
            compositeCommands.Clear();
            compositeTaskIndexes.Clear();
            compositeCompletions.Clear();

            for (int trackIndex = 0; trackIndex < activeTracks.Count; trackIndex++)
            {
                CommandTrack track = activeTracks[trackIndex];
                for (int commandListIndex = 0; commandListIndex < track.Commands.Count; commandListIndex++)
                {
                    Command command = track.Commands[commandListIndex];
                    if (command == null)
                    {
                        continue;
                    }

                    CommandExecutionContext context = new CommandExecutionContext
                    {
                        Block = this,
                        Command = command,
                        Track = track,
                        Blackboard = compositeContext.Blackboard,
                        IsIncluded = trackIndex != 0 || command.CommandIndex >= compositeContext.CommandIndex,
                        IsPrimaryTrack = trackIndex == 0,
                        SuppressSelectionChanges = compositeContext.SuppressSelectionChanges,
                    };
                    compositeTaskIndexes.Add(command, compositeCommandTasks.Count);
                    compositeCommands.Add(command);
                    compositeCommandTasks.Add(new CommandCompositeTask(context));
                }
            }

            compositeRunner = new CompositeExecutionRunner(
                compositeCommandTasks,
                GetCompositeRandomValue);
        }

        private void StartCompositeCommandRunner()
        {
            int lastExecutedCommandIndex = compositeCommands.IndexOf(lastExecutedCommand);
            if (ShouldAvoidRepeatingLastCommand())
            {
                compositeRunner.StartWithoutRepeatingLast(
                    executionMethod,
                    awaitMode,
                    orderMode,
                    lastExecutedCommandIndex,
                    OnCompositeCommandsComplete);
                return;
            }

            compositeRunner.Start(
                executionMethod,
                awaitMode,
                orderMode,
                OnCompositeCommandsComplete);
        }

        private void OnCompositeCommandsComplete(CompositeExecutionStatus status)
        {
            RememberLastExecutedCommand();
            lastCompositeExecutionStatus = status;
            compositeExecutionCompleted = true;
        }

        private bool ShouldAvoidRepeatingLastCommand()
        {
            return avoidRepeatingLastCommand &&
                   compositeCommandTasks.Count > 1 &&
                   CompositeExecutionDescription.SupportsOrder(executionMethod) &&
                   orderMode != CompositeOrderMode.Ordered;
        }

        private void RememberLastExecutedCommand()
        {
            if (compositeRunner == null)
            {
                return;
            }

            int lastExecutedCommandIndex = compositeRunner.LastStartedTaskIndex;
            if (lastExecutedCommandIndex >= 0 && lastExecutedCommandIndex < compositeCommands.Count)
            {
                lastExecutedCommand = compositeCommands[lastExecutedCommandIndex];
            }
        }

        internal void StartCompositeCommand(
            CommandExecutionContext context,
            Action<CompositeExecutionStatus> onComplete)
        {
            Command command = context.Command;
            CommandTrack track = context.Track;
            track.PreviousActiveCommandIndex = track.ActiveCommand == null
                ? -1
                : track.ActiveCommand.CommandIndex;
            track.ActiveCommand = command;
            activeCommand = command;
            if (context.IsPrimaryTrack)
            {
                previousActiveCommandIndex = track.PreviousActiveCommandIndex;
            }

            SelectExecutingCommand(context);
            command.IsExecuting = true;
            command.ExecutingIconTimer =
                Time.realtimeSinceStartup + ScaffoldConstants.ExecutingIconFadeTime;
            BlockSignals.DoCommandExecute(
                this,
                command,
                command.CommandIndex,
                track.Commands.Count);
            compositeCompletions[command] = onComplete;

            try
            {
                command.Execute();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[Block] Command execution failed at {command.GetLocationIdentifier()}: {exception}");
                compositeCompletions.Remove(command);
                command.IsExecuting = false;
                onComplete(CompositeExecutionStatus.Failure);
            }
        }

        internal void StopCompositeCommand(Command command)
        {
            if (command == null)
            {
                return;
            }

            compositeCompletions.Remove(command);
            if (!command.IsExecuting)
            {
                return;
            }

            command.IsExecuting = false;
            command.OnStopExecuting();
            if (activeCommand == command)
            {
                activeCommand = null;
            }
        }

        private void SelectExecutingCommand(CommandExecutionContext context)
        {
#if UNITY_EDITOR
            Blackboard blackboard = context.Blackboard;
            if (!blackboard.IsActive() ||
                context.SuppressSelectionChanges ||
                !context.IsPrimaryTrack)
            {
                return;
            }

            if ((blackboard.SelectedCommands.Count == 0 && context.Command.CommandIndex == 0) ||
                (blackboard.SelectedCommands.Count == 1 &&
                 blackboard.SelectedCommands[0].CommandIndex ==
                 context.Track.PreviousActiveCommandIndex))
            {
                blackboard.ClearSelectedCommands();
                blackboard.AddSelectedCommand(context.Command);
            }
#endif
        }

        private void RequestOrderedCommandHandoff(Command command, int nextCommandIndex)
        {
            if (executionMethod != CompositeExecutionMethod.Sequence ||
                orderMode != CompositeOrderMode.Ordered ||
                nextCommandIndex == command.CommandIndex + 1)
            {
                return;
            }

            int nextTaskIndex = FindCompositeTaskIndex(command.ParentTrack, nextCommandIndex);
            compositeRunner?.RequestNextTaskIndex(nextTaskIndex);
        }

        private int FindCompositeTaskIndex(CommandTrack track, int commandIndex)
        {
            int trackIndex = activeTracks.IndexOf(track);
            if (trackIndex < 0)
            {
                return compositeCommandTasks.Count;
            }

            for (int searchTrackIndex = trackIndex; searchTrackIndex < activeTracks.Count; searchTrackIndex++)
            {
                CommandTrack searchTrack = activeTracks[searchTrackIndex];
                foreach (Command candidate in searchTrack.Commands)
                {
                    if (candidate == null ||
                        (searchTrackIndex == trackIndex && candidate.CommandIndex < commandIndex))
                    {
                        continue;
                    }

                    int taskIndex;
                    if (compositeTaskIndexes.TryGetValue(candidate, out taskIndex))
                    {
                        return taskIndex;
                    }
                }

                commandIndex = 0;
            }

            return compositeCommandTasks.Count;
        }

        private static CompositeExecutionStatus GetCommandStatus(Command command)
        {
            ICompositeExecutionStatusProvider statusProvider =
                command as ICompositeExecutionStatusProvider;
            return statusProvider != null
                ? statusProvider.LastCompositeExecutionStatus
                : CompositeExecutionStatus.Success;
        }

        private void CompleteCompositeCommand(
            Command command,
            Action<CompositeExecutionStatus> completion,
            CompositeExecutionStatus status)
        {
#if UNITY_EDITOR
            Blackboard blackboard = GetBlackboard();
            if (blackboard.StepPause > 0f && isActiveAndEnabled)
            {
                StartCoroutine(CompleteCompositeCommandAfterPause(command, completion, status));
                return;
            }
#endif
            completion(status);
        }

#if UNITY_EDITOR
        private IEnumerator CompleteCompositeCommandAfterPause(
            Command command,
            Action<CompositeExecutionStatus> completion,
            CompositeExecutionStatus status)
        {
            yield return new WaitForSeconds(GetBlackboard().StepPause);
            if (!command.IsExecuting)
            {
                completion(status);
            }
        }
#endif

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
        /// Stop executing commands in this Block, including detached parallel commands.
        /// </summary>
        public virtual void Stop()
        {
            RememberLastExecutedCommand();
            compositeRunner?.Stop();
            compositeCompletions.Clear();
            ResetExecutionFeedback();

            // Legacy fallback field, harmless to also set for the no-tracks edge case.
            jumpToCommandIndex = int.MaxValue;

            //force idle here so other commands that rely on block not executing are informed this frame rather than next
            if (executionState == ExecutionState.Executing)
            {
                ReturnToIdle();
            }
        }

        public virtual bool TryGetCommandExecutionStatus(
            Command command,
            out CompositeExecutionStatus status)
        {
            status = default;
            return command != null &&
                   compositeRunner != null &&
                   compositeTaskIndexes.TryGetValue(command, out int taskIndex) &&
                   compositeRunner.TryGetTaskStatus(taskIndex, out status);
        }

        public virtual void ResetExecutionFeedback()
        {
            compositeRunner?.ResetTaskStatuses();
            if (tracks == null)
            {
                return;
            }

            foreach (CommandTrack track in tracks)
            {
                if (track == null)
                {
                    continue;
                }

                foreach (Command command in track.Commands)
                {
                    command?.ResetExecutionFeedback();
                }
            }
        }

        /// <summary>
        /// Returns a list of all Blocks connected to this one.
        /// </summary>
        public virtual List<Block> GetConnectedBlocks()
        {
            List<Block> connectedBlocks = new List<Block>();
            GetConnectedBlocks(ref connectedBlocks);
            return connectedBlocks;
        }

        public virtual void GetConnectedBlocks(ref List<Block> connectedBlocks)
        {
            if (tracks == null)
            {
                return;
            }

            foreach (CommandTrack track in tracks)
            {
                for (int i = 0; i < track.Commands.Count; i++)
                {
                    Command command = track.Commands[i];
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
            Command command = GetPreviousActiveCommand();
            return command != null ? command.GetType() : null;
        }

        public virtual int GetPreviousActiveCommandIndent()
        {
            Command command = GetPreviousActiveCommand();
            return command != null ? command.IndentLevel : -1;
        }

        public virtual Command GetPreviousActiveCommand()
        {
            CommandTrack track = (tracks != null && tracks.Count > 0) ? tracks[0] : null;
            if (track != null)
            {
                return track.GetPreviousActiveCommand();
            }

            int index = PreviousActiveCommandIndex;
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

            foreach (CommandTrack track in tracks)
            {
                int indentLevel = 0;
                for (int i = 0; i < track.Commands.Count; i++)
                {
                    Command command = track.Commands[i];
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
                Command command = CommandList[i];
                if (command.IsLabel())
                {
                    // TODO: ActionBase doesn't have Key. Skip for now.
                }
            }

            return -1;
        }

        #endregion
    }
}
