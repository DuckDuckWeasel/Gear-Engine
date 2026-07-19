
using UnityEngine;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;

namespace Scaffold
{
    /// <summary>
    /// Attribute class for Scaffold commands.
    /// </summary>
    /// 
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CommandInfoAttribute : Attribute
    {
        /// <summary>
        /// Metadata atribute for the Command class. 
        /// </summary>
        /// <param name="category">The category to place this command in.</param>
        /// <param name="commandName">The display name of the command.</param>
        /// <param name="helpText">Help information to display in the inspector.</param>
        /// <param name="priority">If two command classes have the same name, the one with highest priority is listed. Negative priority removess the command from the list.</param>///
        public CommandInfoAttribute(string category, string commandName, string helpText, int priority = 0)
        {
            this.Category = category;
            this.CommandName = commandName;
            this.HelpText = helpText;
            this.Priority = priority;
        }

        public string Category { get; set; }
        public string CommandName { get; set; }
        public string HelpText { get; set; }
        public int Priority { get; set; }
    }

    /// <summary>
    /// Base class for Commands. Commands can be added to Blocks to create an execution sequence.
    /// </summary>
    public abstract class Command : MonoBehaviour, IVariableReference
    {
        [FormerlySerializedAs("commandId")]
        [HideInInspector]
        [SerializeField] protected int itemId = -1; // Invalid blackboard item id

        [HideInInspector]
        [SerializeField] protected int indentLevel;

        [HideInInspector]
        [SerializeField] private float compositeUtility;

        [HideInInspector]
        [SerializeField] private bool compositeBlockDuringExecution;

        [HideInInspector]
        [Range(0f, 100f)]
        [SerializeField] private float compositeWeight;

        [HideInInspector]
        [SerializeField] private bool compositeWeightInitialized;

        [HideInInspector]
        [SerializeField] private bool compositeWeightOverride;

        protected string errorMessage = "";

        #region Editor caches
#if UNITY_EDITOR
        //
        protected List<Variable> referencedVariables = new List<Variable>();

        //used by var list adapter to highlight variables 
        public bool IsVariableReferenced(Variable variable)
        {
            return referencedVariables.Contains(variable) || HasReference(variable);
        }

        /// <summary>
        /// Called by OnValidate
        /// 
        /// Child classes to specialise to add variable references to referencedVariables, either directly or
        /// via the use of Blackboard.DetermineSubstituteVariables
        /// </summary>
        protected virtual void RefreshVariableCache()
        {
            referencedVariables.Clear();
        }
#endif
        #endregion Editor caches

        #region Public members

        /// <summary>
        /// Unique identifier for this command.
        /// Unique for this Blackboard.
        /// </summary>
        public virtual int ItemId { get { return itemId; } set { itemId = value; } }

        /// <summary>
        /// Error message to display in the command inspector.
        /// </summary>
        public virtual string ErrorMessage { get { return errorMessage; } }

        /// <summary>
        /// Indent depth of the current commands.
        /// Commands are indented inside If, While, etc. sections.
        /// </summary>
        public virtual int IndentLevel { get { return indentLevel; } set { indentLevel = value; } }

        /// <summary>
        /// Index of the command in the parent block's command list.
        /// </summary>
        public virtual int CommandIndex { get; set; }

        /// <summary>
        /// Utility score used when the parent Block executes commands as a Utility Selector.
        /// </summary>
        public virtual float CompositeUtility
        {
            get { return compositeUtility; }
            set { compositeUtility = value; }
        }

        /// <summary>
        /// Prevents utility reevaluation while this command is executing.
        /// </summary>
        public virtual bool CompositeBlockDuringExecution
        {
            get { return compositeBlockDuringExecution; }
            set { compositeBlockDuringExecution = value; }
        }

        /// <summary>
        /// Stored selection weight used when this command overrides automatic balancing.
        /// </summary>
        public virtual float CompositeWeight
        {
            get
            {
                return compositeWeightInitialized
                    ? Mathf.Clamp(compositeWeight, 0f, 100f)
                    : 100f;
            }
            set
            {
                compositeWeight = Mathf.Clamp(value, 0f, 100f);
                compositeWeightInitialized = true;
                compositeWeightOverride = true;
            }
        }

        /// <summary>
        /// True when this command reserves a manual percentage instead of sharing the
        /// percentage left by the other overrides in its command list.
        /// </summary>
        public virtual bool HasCompositeWeightOverride
        {
            get
            {
                return compositeWeightOverride ||
                       (compositeWeightInitialized && !Mathf.Approximately(compositeWeight, 100f));
            }
        }

        /// <summary>
        /// Restores automatic balancing for this command.
        /// </summary>
        public virtual void ClearCompositeWeightOverride()
        {
            compositeWeight = 0f;
            compositeWeightInitialized = false;
            compositeWeightOverride = false;
        }

        /// <summary>
        /// Set to true by the parent block while the command is executing.
        /// </summary>
        public virtual bool IsExecuting { get; set; }

        /// <summary>
        /// Clears transient editor feedback retained from the previous execution.
        /// Composite commands override this to clear their child results as well.
        /// </summary>
        public virtual void ResetExecutionFeedback()
        {
        }

        /// <summary>
        /// Timer used to control appearance of executing icon in inspector.
        /// </summary>
        public virtual float ExecutingIconTimer { get; set; }

        /// <summary>
        /// Reference to the Block object that this command belongs to.
        /// This reference is only populated at runtime and in the editor when the 
        /// block is selected.
        /// </summary>
        public virtual Block ParentBlock { get; set; }

        /// <summary>
        /// Reference to the CommandTrack that this command belongs to within its parent Block.
        /// This reference is only populated at runtime and in the editor when the
        /// block is selected. Used to scope flow-control (If/Else/End, loops, jumps) to the
        /// track the command actually runs on, since a Block can run multiple tracks in parallel.
        /// </summary>
        public virtual CommandTrack ParentTrack { get; set; }

        /// <summary>
        /// Returns the Blackboard that this command belongs to.
        /// </summary>
        public virtual Blackboard GetBlackboard()
        {
            Blackboard blackboard = GetComponent<Blackboard>();
            if (blackboard == null &&
                transform.parent != null)
            {
                blackboard = transform.parent.GetComponent<Blackboard>();
            }
            return blackboard;
        }

        /// <summary>
        /// Execute the command.
        /// </summary>
        public virtual void Execute()
        {
            OnEnter();
        }

        /// <summary>
        /// End execution of this command and continue execution at the next command.
        /// </summary>
        public virtual void Continue()
        {
            // This is a noop if the Block has already been stopped
            if (IsExecuting)
            {
                Continue(CommandIndex + 1);
            }
        }

        /// <summary>
        /// End execution of this command and continue execution at a specific command index.
        /// </summary>
        /// <param name="nextCommandIndex">Next command index.</param>
        public virtual void Continue(int nextCommandIndex)
        {
            OnExit();
            if (ParentBlock != null)
            {
                ParentBlock.OnCommandCompleted(this, nextCommandIndex);
            }
        }

        /// <summary>
        /// Stops the parent Block executing.
        /// </summary>
        public virtual void StopParentBlock()
        {
            OnExit();
            if (ParentBlock != null)
            {
                ParentBlock.Stop();
            }
        }

        /// <summary>
        /// Called when the parent block has been requested to stop executing, and
        /// this command is the currently executing command.
        /// Use this callback to terminate any asynchronous operations and 
        /// cleanup state so that the command is ready to execute again later on.
        /// </summary>
        public virtual void OnStopExecuting()
        { }

        /// <summary>
        /// Called when the new command is added to a block in the editor.
        /// </summary>
        public virtual void OnCommandAdded(Block parentBlock)
        { }

        /// <summary>
        /// Called when the command is deleted from a block in the editor.
        /// </summary>
        public virtual void OnCommandRemoved(Block parentBlock)
        { }

        /// <summary>
        /// Called when this command starts execution.
        /// </summary>
        public virtual void OnEnter()
        { }

        /// <summary>
        /// Called when this command ends execution.
        /// </summary>
        public virtual void OnExit()
        { }

        /// <summary>
        /// Called when this command is reset. This happens when the Reset command is used.
        /// </summary>
        public virtual void OnReset()
        { }

        /// <summary>
        /// Populates a list with the Blocks that this command references.
        /// </summary>
        public virtual void GetConnectedBlocks(ref List<Block> connectedBlocks)
        { }

        /// <summary>
        /// Returns true if this command references the variable.
        /// Used to highlight variables in the variable list when a command is selected.
        /// </summary>
        public virtual bool HasReference(Variable variable)
        {
            return false;
        }

        public virtual string GetLocationIdentifier()
        {
            return ParentBlock.GetBlackboard().GetName() + ":" + ParentBlock.BlockName + "." + this.GetType().Name + "#" + CommandIndex.ToString();
        }

        /// <summary>
        /// Called by unity when script is loaded or its data changed by editor
        /// </summary>
        public virtual void OnValidate()
        {
#if UNITY_EDITOR
            RefreshVariableCache();
#endif
        }

        /// <summary>
        /// Returns the summary text to display in the command inspector.
        /// </summary>
        public virtual string GetSummary()
        {
            return "";
        }

        /// <summary>
        /// Returns the searchable content for searches on the blackboard window.
        /// </summary>
        public virtual string GetSearchableContent()
        {
            return GetSummary();
        }

        /// <summary>
        /// Returns the help text to display for this command.
        /// </summary>
        public virtual string GetHelpText()
        {
            return "";
        }

        /// <summary>
        /// Return true if this command opens a block of commands. Used for indenting commands.
        /// </summary>
        public virtual bool OpenBlock()
        {
            return false;
        }

        /// <summary>
        /// Return true if this command closes a block of commands. Used for indenting commands.
        /// </summary>
        public virtual bool CloseBlock()
        {
            return false;
        }

        /// <summary>
        /// Return the color for the command background in inspector.
        /// </summary>
        /// <returns>The button color.</returns>
        public virtual Color GetButtonColor()
        {
            return Color.white;
        }

        /// <summary>
        /// Returns true if the specified property should be displayed in the inspector. 
        /// This is useful for hiding certain properties based on the value of another property.
        /// </summary>
        public virtual bool IsPropertyVisible(string propertyName)
        {
            return true;
        }

        /// <summary>
        /// Returns true if the specified property should be displayed as a reorderable list in the inspector.
        /// This only applies for array properties and has no effect for non-array properties.
        /// </summary>
        public virtual bool IsReorderableArray(string propertyName)
        {
            return false;
        }

        /// <summary>
        /// Returns the localization id for the Blackboard that contains this command.
        /// </summary>
        public virtual string GetBlackboardLocalizationId()
        {
            // If no localization id has been set then use the Blackboard name
            Blackboard blackboard = GetBlackboard();
            if (blackboard == null)
            {
                return "";
            }

            string localizationId = GetBlackboard().LocalizationId;
            if (localizationId.Length == 0)
            {
                localizationId = blackboard.GetName();
            }

            return localizationId;
        }

        #endregion
    }
}
