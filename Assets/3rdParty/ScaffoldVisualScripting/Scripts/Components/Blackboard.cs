
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TriInspector;

namespace Scaffold
{
    /// <summary>
    /// Visual scripting controller for the Blackboard programming language.
    /// Blackboard objects may be edited visually using the Blackboard editor window.
    /// </summary>
    [RequireComponent(typeof(EventDispatcher))]
    [ExecuteInEditMode]
    public partial class Blackboard : MonoBehaviour, ISubstitutionHandler
    {
        public static HashSet<Blackboard> AllActive = new HashSet<Blackboard>();

        public const string SubstituteVariableRegexString = "{\\$.*?}";

        [HideInInspector]
        [SerializeField] protected int version = 0; // Default to 0 to always trigger an update for older versions of Scaffold.

        [HideInInspector]
        [SerializeField] protected Vector2 scrollPos;

        [HideInInspector]
        [SerializeField] protected Vector2 variablesScrollPos;

        [HideInInspector]
        [SerializeField] protected bool variablesExpanded = true;

        [HideInInspector]
        [SerializeField] protected float blockViewHeight = 400;

        [HideInInspector]
        [SerializeField] protected float zoom = 1f;

        [HideInInspector]
        [SerializeField] protected Rect scrollViewRect;

        [ReadOnly]
        [SerializeField] protected List<Block> selectedBlocks = new List<Block>();

        [ReadOnly]
        [SerializeField] protected List<Command> selectedCommands = new List<Command>();

        [SerializeField] protected List<Variable> variables = new List<Variable>();

        [TextArea(3, 5)]
        [Tooltip("Description text displayed in the Blackboard editor window")]
        [SerializeField] protected string description = "";

        [Range(0f, 5f)]
        [Tooltip("Adds a pause after each execution step to make it easier to visualise program flow. Editor only, has no effect in platform builds.")]
        [SerializeField] protected float stepPause = 0f;

        [Tooltip("Use command color when displaying the command list in the Scaffold Editor window")]
        [SerializeField] protected bool colorCommands = true;

        [Tooltip("Hides the Blackboard block and command components in the inspector. Deselect to inspect the block and command components that make up the Blackboard.")]
        [SerializeField] protected bool hideComponents = true;

        [Tooltip("Saves the selected block and commands when saving the scene. Helps avoid version control conflicts if you've only changed the active selection.")]
        [SerializeField] protected bool saveSelection = true;

        [Tooltip("Unique identifier for this blackboard in localized string keys. If no id is specified then the name of the Blackboard object will be used.")]
        [SerializeField] protected string localizationId = "";

        [Tooltip("Display line numbers in the command list in the Block inspector.")]
        [SerializeField] protected bool showLineNumbers = false;

        [Tooltip("List of commands to hide in the Add Command menu. Use this to restrict the set of commands available when editing a Blackboard.")]
        [SerializeField] protected List<string> hideCommands = new List<string>();

        [Tooltip("Lua Environment to be used by default for all Execute Lua commands in this Blackboard")]
        [SerializeField] protected LuaEnvironment luaEnvironment;

        [Tooltip("The ExecuteLua command adds a global Lua variable with this name bound to the blackboard prior to executing.")]
        [SerializeField] protected string luaBindingName = "blackboard";

        protected static List<Blackboard> cachedBlackboards = new List<Blackboard>();

        protected static bool eventSystemPresent;

        protected StringSubstituter stringSubstituer;

        // Static registry for O(1) message broadcasting
        public static readonly HashSet<MessageReceived> MessageReceivers = new HashSet<MessageReceived>();

        // Service for abstracting PlayerPrefs
        public static IScaffoldSaveService SaveService = new DefaultPlayerPrefsSaveService();

#if UNITY_EDITOR
        public bool SelectedCommandsStale { get; set; }
#endif

#if UNITY_5_4_OR_NEWER
#else
        protected virtual void OnLevelWasLoaded(int level) 
        {
            LevelWasLoaded();
        }
#endif

        protected virtual void LevelWasLoaded()
        {
            // Reset the flag for checking for an event system as there may not be one in the newly loaded scene.
            eventSystemPresent = false;
        }

        protected virtual void Start()
        {
            CheckEventSystem();
        }

        // There must be an Event System in the scene for Say and Menu input to work.
        // This method will automatically instantiate one if none exists.
        protected virtual void CheckEventSystem()
        {
            if (eventSystemPresent)
            {
                return;
            }

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                // Auto spawn an Event System from the prefab
                GameObject prefab = Resources.Load<GameObject>("Prefabs/EventSystem");
                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab) as GameObject;
                    go.name = "EventSystem";
                }
            }

            eventSystemPresent = true;
        }

        private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.Scene arg1)
        {
            LevelWasLoaded();
        }

        protected virtual void OnEnable()
        {
            if (!cachedBlackboards.Contains(this))
            {
                cachedBlackboards.Add(this);
                //TODO these pairs could be replaced by something static that manages all active blackboards
#if UNITY_5_4_OR_NEWER
                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
#endif
            }

            CheckItemIds();
            CleanupComponents();
            UpdateVersion();

            StringSubstituter.RegisterHandler(this);
        }

        protected virtual void OnDisable()
        {
            cachedBlackboards.Remove(this);

#if UNITY_5_4_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
#endif

            StringSubstituter.UnregisterHandler(this);
        }

        protected virtual void UpdateVersion()
        {
            if (version == ScaffoldConstants.CurrentVersion)
            {
                // No need to update
                return;
            }

            // Tell all components that implement IUpdateable to update to the new version
            Component[] components = GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                IUpdateable u = component as IUpdateable;
                if (u != null)
                {
                    u.UpdateToVersion(version, ScaffoldConstants.CurrentVersion);
                }
            }

            version = ScaffoldConstants.CurrentVersion;
        }

        protected virtual void CheckItemIds()
        {
            // Make sure item ids are unique and monotonically increasing.
            // This should always be the case, but some legacy Blackboards may have issues.
            List<int> usedIds = new List<int>();
            Block[] blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                Block block = blocks[i];
                if (block.ItemId == -1 || usedIds.Contains(block.ItemId))
                {
                    block.ItemId = NextItemId();
                }
                usedIds.Add(block.ItemId);
            }

            Command[] commands = GetComponents<Command>();
            for (int i = 0; i < commands.Length; i++)
            {
                Command command = commands[i];
                if (command.ItemId == -1 || usedIds.Contains(command.ItemId))
                {
                    command.ItemId = NextItemId();
                }
                usedIds.Add(command.ItemId);
            }
        }

        protected virtual void CleanupComponents()
        {
            // Delete any unreferenced components which shouldn't exist any more
            // Unreferenced components don't have any effect on the blackboard behavior, but
            // they waste memory so should be cleared out periodically.

            // Remove any null entries in the variables list
            // It shouldn't happen but it seemed to occur for a user on the forum 
            variables.RemoveAll(item => item == null);

            if (selectedBlocks == null)
            {
                selectedBlocks = new List<Block>();
            }

            if (selectedCommands == null)
            {
                selectedCommands = new List<Command>();
            }

            selectedBlocks.RemoveAll(item => item == null);
            selectedCommands.RemoveAll(item => item == null);

            Variable[] allVariables = GetComponents<Variable>();
            for (int i = 0; i < allVariables.Length; i++)
            {
                Variable variable = allVariables[i];
                if (!variables.Contains(variable))
                {
                    DestroyImmediate(variable);
                }
            }

            Block[] blocks = GetComponents<Block>();
            Command[] commands = GetComponents<Command>();
            for (int i = 0; i < commands.Length; i++)
            {
                Command command = commands[i];
                bool found = false;
                for (int j = 0; j < blocks.Length; j++)
                {
                    Block block = blocks[j];
                    if (block.CommandList.Contains(command))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    DestroyImmediate(command);
                }
            }

            EventHandler[] eventHandlers = GetComponents<EventHandler>();
            for (int i = 0; i < eventHandlers.Length; i++)
            {
                EventHandler eventHandler = eventHandlers[i];
                bool found = false;
                for (int j = 0; j < blocks.Length; j++)
                {
                    Block block = blocks[j];
                    if (block._EventHandler == eventHandler)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    DestroyImmediate(eventHandler);
                }
            }
        }

        protected virtual Block CreateBlockComponent(GameObject parent)
        {
            Block block = parent.AddComponent<Block>();
            System.Type invokeCmdType = System.Type.GetType("GearEngine.GearEngine.Presentation.UI.Input.InvokeActionCommand, Game.GearEngine");
            if (invokeCmdType != null)
            {
                Command invokeCmd = parent.AddComponent(invokeCmdType) as Command;
                if (invokeCmd != null)
                {
                    invokeCmd.ParentBlock = block;
                    block.CommandList.Add(invokeCmd);
                }
            }
            else
            {
                UnityEngine.Debug.LogError("InvokeActionCommand type not found via reflection.");
            }
            return block;
        }

        #region Public members

        /// <summary>
        /// Cached list of blackboard objects in the scene for fast lookup.
        /// </summary>
        public static List<Blackboard> CachedBlackboards { get { return cachedBlackboards; } }

        /// <summary>
        /// Sends a message to all Blackboard objects in the current scene.
        /// Any block with a matching MessageReceived event handler will start executing.
        /// </summary>
        public static void BroadcastScaffoldMessage(string messageName)
        {
            foreach (MessageReceived eventHandler in MessageReceivers)
            {
                if (eventHandler != null)
                {
                    eventHandler.OnSendScaffoldMessage(messageName);
                }
            }
        }

        /// <summary>
        /// Scroll position of Blackboard editor window.
        /// </summary>
        public virtual Vector2 ScrollPos { get { return scrollPos; } set { scrollPos = value; } }

        /// <summary>
        /// Scroll position of Blackboard variables window.
        /// </summary>
        public virtual Vector2 VariablesScrollPos { get { return variablesScrollPos; } set { variablesScrollPos = value; } }

        /// <summary>
        /// Show the variables pane.
        /// </summary>
        public virtual bool VariablesExpanded { get { return variablesExpanded; } set { variablesExpanded = value; } }

        /// <summary>
        /// Height of command block view in inspector.
        /// </summary>
        public virtual float BlockViewHeight { get { return blockViewHeight; } set { blockViewHeight = value; } }

        /// <summary>
        /// Zoom level of Blackboard editor window.
        /// </summary>
        public virtual float Zoom { get { return zoom; } set { zoom = value; } }

        /// <summary>
        /// Scrollable area for Blackboard editor window.
        /// </summary>
        public virtual Rect ScrollViewRect { get { return scrollViewRect; } set { scrollViewRect = value; } }

        /// <summary>
        /// Current actively selected block in the Blackboard editor.
        /// </summary>
        public virtual Block SelectedBlock
        {
            get
            {
                if (selectedBlocks == null || selectedBlocks.Count == 0)
                {
                    return null;
                }

                return selectedBlocks[0];
            }
            set
            {
                ClearSelectedBlocks();
                AddSelectedBlock(value);
            }
        }

        public virtual List<Block> SelectedBlocks { get { return selectedBlocks; } set { selectedBlocks = value; } }

        /// <summary>
        /// Currently selected command in the Blackboard editor.
        /// </summary>
        public virtual List<Command> SelectedCommands { get { return selectedCommands; } }

        /// <summary>
        /// The list of variables that can be accessed by the Blackboard.
        /// </summary>
        public virtual List<Variable> Variables { get { return variables; } }

        public virtual int VariableCount { get { return variables.Count; } }

        /// <summary>
        /// Description text displayed in the Blackboard editor window
        /// </summary>
        public virtual string Description { get { return description; } }

        /// <summary>
        /// Slow down execution in the editor to make it easier to visualise program flow.
        /// </summary>
        public virtual float StepPause { get { return stepPause; } }

        /// <summary>
        /// Use command color when displaying the command list in the inspector.
        /// </summary>
        public virtual bool ColorCommands { get { return colorCommands; } }

        /// <summary>
        /// Saves the selected block and commands when saving the scene. Helps avoid version control conflicts if you've only changed the active selection.
        /// </summary>
        public virtual bool SaveSelection { get { return saveSelection; } }

        /// <summary>
        /// Unique identifier for identifying this blackboard in localized string keys.
        /// </summary>
        public virtual string LocalizationId { get { return localizationId; } }

        /// <summary>
        /// Display line numbers in the command list in the Block inspector.
        /// </summary>
        public virtual bool ShowLineNumbers { get { return showLineNumbers; } }

        /// <summary>
        /// Lua Environment to be used by default for all Execute Lua commands in this Blackboard.
        /// </summary>
        public virtual LuaEnvironment LuaEnv { get { return luaEnvironment; } }

        /// <summary>
        /// The ExecuteLua command adds a global Lua variable with this name bound to the blackboard prior to executing.
        /// </summary>
        public virtual string LuaBindingName { get { return luaBindingName; } }

        /// <summary>
        /// Position in the center of all blocks in the blackboard.
        /// </summary>
        public virtual Vector2 CenterPosition { set; get; }

        /// <summary>
        /// Variable to track blackboard's version so components can update to new versions.
        /// </summary>
        public int Version { set { version = value; } }

        /// <summary>
        /// Returns true if the Blackboard gameobject is active.
        /// </summary>
        public bool IsActive()
        {
            return gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Returns the Blackboard gameobject name.
        /// </summary>
        public string GetName()
        {
            return gameObject.name;
        }

        /// <summary>
        /// Returns the next id to assign to a new blackboard item.
        /// Item ids increase monotically so they are guaranteed to
        /// be unique within a Blackboard.
        /// </summary>
        public int NextItemId()
        {
            int maxId = -1;
            Block[] blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                Block block = blocks[i];
                maxId = Math.Max(maxId, block.ItemId);
            }

            Command[] commands = GetComponents<Command>();
            for (int i = 0; i < commands.Length; i++)
            {
                Command command = commands[i];
                maxId = Math.Max(maxId, command.ItemId);
            }
            return maxId + 1;
        }

        /// <summary>
        /// Create a new block node which you can then add commands to.
        /// </summary>
        public virtual Block CreateBlock(Vector2 position)
        {
            Block b = CreateBlockComponent(gameObject);
            b._NodeRect = new Rect(position.x, position.y, 0, 0);
            b.BlockName = GetUniqueBlockKey(b.BlockName, b);
            b.ItemId = NextItemId();

            return b;
        }

        /// <summary>
        /// Returns the named Block in the blackboard, or null if not found.
        /// </summary>
        public virtual Block FindBlock(string blockName)
        {
            Block[] blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                Block block = blocks[i];
                if (block.BlockName == blockName)
                {
                    return block;
                }
            }

            return null;
        }

        /// <summary>
        /// Checks availability of the block in the Blackboard.
        /// You can use this method in a UI event. e.g. to test availability block, before handle it.
        public virtual bool HasBlock(string blockName)
        {
            Block block = FindBlock(blockName);
            return block != null;
        }

        /// <summary>
        /// Executes the block if it is available in the Blackboard.
        /// You can use this method in a UI event. e.g. to try executing block without confidence in its existence.
        public virtual bool ExecuteIfHasBlock(string blockName)
        {
            if (HasBlock(blockName))
            {
                ExecuteBlock(blockName);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Substitute variables in the input text with the format {$VarName}
        /// This will first match with private variables in this Blackboard, and then
        /// with public variables in all Blackboards in the scene (and any component
        /// in the scene that implements StringSubstituter.ISubstitutionHandler).
        /// </summary>
        public virtual string SubstituteVariables(string input)
        {
            if (stringSubstituer == null)
            {
                stringSubstituer = new StringSubstituter();
            }

            // Use the string builder from StringSubstituter for efficiency.
            StringBuilder sb = stringSubstituer._StringBuilder;
            sb.Length = 0;
            sb.Append(input);

            // Instantiate the regular expression object.
            Regex r = new Regex(SubstituteVariableRegexString);

            bool changed = false;

            // Match the regular expression pattern against a text string.
            MatchCollection results = r.Matches(input);
            for (int i = 0; i < results.Count; i++)
            {
                Match match = results[i];
                string key = match.Value.Substring(2, match.Value.Length - 3);
                // Look for any matching private variables in this Blackboard first
                for (int j = 0; j < variables.Count; j++)
                {
                    Variable variable = variables[j];
                    if (variable == null)
                    {
                        continue;
                    }

                    if (variable.Scope == VariableScope.Private && variable.Key == key)
                    {
                        string value = variable.ToString();
                        sb.Replace(match.Value, value);
                        changed = true;
                    }
                }
            }

            // Now do all other substitutions in the scene
            changed |= stringSubstituer.SubstituteStrings(sb);

            if (changed)
            {
                return sb.ToString();
            }
            else
            {
                return input;
            }
        }

        public virtual void DetermineSubstituteVariables(string str, List<Variable> vars)
        {
            Regex r = new Regex(Blackboard.SubstituteVariableRegexString);

            // Match the regular expression pattern against a text string.
            MatchCollection results = r.Matches(str);
            for (int i = 0; i < results.Count; i++)
            {
                Match match = results[i];
                Variable v = GetVariable(match.Value.Substring(2, match.Value.Length - 3));
                if (v != null)
                {
                    vars.Add(v);
                }
            }
        }

        #endregion

        #region IStringSubstituter implementation

        /// <summary>
        /// Implementation of StringSubstituter.ISubstitutionHandler which matches any public variable in the Blackboard.
        /// To perform full variable substitution with all substitution handlers in the scene, you should
        /// use the SubstituteVariables() method instead.
        /// </summary>
        [MoonSharp.Interpreter.MoonSharpHidden]
        public virtual bool SubstituteStrings(StringBuilder input)
        {
            // Instantiate the regular expression object.
            Regex r = new Regex(SubstituteVariableRegexString);

            bool modified = false;

            // Match the regular expression pattern against a text string.
            MatchCollection results = r.Matches(input.ToString());
            for (int i = 0; i < results.Count; i++)
            {
                Match match = results[i];
                string key = match.Value.Substring(2, match.Value.Length - 3);
                // Look for any matching public variables in this Blackboard
                for (int j = 0; j < variables.Count; j++)
                {
                    Variable variable = variables[j];
                    if (variable == null)
                    {
                        continue;
                    }
                    if (variable.Scope == VariableScope.Public && variable.Key == key)
                    {
                        string value = variable.ToString();
                        input.Replace(match.Value, value);
                        modified = true;
                    }
                }
            }

            return modified;
        }

        #endregion
    }
}
