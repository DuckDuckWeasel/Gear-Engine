using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Scaffold
{
    public partial class Blackboard
    {

        /// <summary>
        /// Execute a child block in the Blackboard.
        /// You can use this method in a UI event. e.g. to handle a button click.
        public virtual void ExecuteBlock(string blockName)
        {
            Block block = FindBlock(blockName);

            if (block == null)
            {
                Debug.LogError("Block " + blockName + " does not exist");
                return;
            }

            if (!ExecuteBlock(block))
            {
                Debug.LogWarning("Block " + blockName + " failed to execute");
            }
        }

        /// <summary>
        /// Stops an executing Block in the Blackboard.
        /// </summary>
        public virtual void StopBlock(string blockName)
        {
            Block block = FindBlock(blockName);

            if (block == null)
            {
                Debug.LogError("Block " + blockName + " does not exist");
                return;
            }

            if (block.IsExecuting())
            {
                block.Stop();
            }
        }

        /// <summary>
        /// Execute a child block in the blackboard.
        /// The block must be in an idle state to be executed.
        /// This version provides extra options to control how the block is executed.
        /// Returns true if the Block started execution.
        /// </summary>
        public virtual bool ExecuteBlock(Block block, int commandIndex = 0, Action onComplete = null)
        {
            if (block == null)
            {
                Debug.LogError("Block must not be null");
                return false;
            }

            if (((Block)block).gameObject != gameObject)
            {
                Debug.LogError("Block must belong to the same gameobject as this Blackboard");
                return false;
            }

            // Can't restart a running block, have to wait until it's idle again
            if (block.IsExecuting())
            {
                Debug.LogWarning(block.BlockName + " cannot be called/executed, it is already running.");
                return false;
            }

            // Start executing the Block as a new coroutine
            StartCoroutine(block.Execute(commandIndex, onComplete));

            return true;
        }

        /// <summary>
        /// Stop all executing Blocks in this Blackboard.
        /// </summary>
        public virtual void StopAllBlocks()
        {
            Block[] blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                Block block = blocks[i];
                if (block.IsExecuting())
                {
                    block.Stop();
                    continue;
                }

                block.ResetExecutionFeedback();
            }
        }

        /// <summary>
        /// Stops every executing Block, then restarts the target Block from the requested Command.
        /// The restart is deferred until the target is idle so async stop callbacks cannot cause
        /// the Blackboard to reject it as an attempt to run an already-executing Block.
        /// </summary>
        public virtual void StopAllBlocksAndRestartBlock(
            Block block,
            int commandIndex = 0,
            Action onComplete = null)
        {
            if (block == null)
            {
                Debug.LogError("Block must not be null");
                return;
            }

            StopAllBlocks();
            StartCoroutine(RestartBlockWhenIdle(block, commandIndex, onComplete));
        }

        /// <summary>
        /// Stops the target Block when necessary, then restarts it from the requested Command.
        /// </summary>
        public virtual void RestartBlock(
            Block block,
            int commandIndex = 0,
            Action onComplete = null)
        {
            if (block == null)
            {
                Debug.LogError("Block must not be null");
                return;
            }

            if (block.IsExecuting())
            {
                block.Stop();
            }

            StartCoroutine(RestartBlockWhenIdle(block, commandIndex, onComplete));
        }

        private IEnumerator RestartBlockWhenIdle(
            Block block,
            int commandIndex,
            Action onComplete)
        {
            // Do not rely on a scaled-time delay: it can hang when the game is paused and
            // provides no guarantee that asynchronous stop work has completed.
            yield return null;
            yield return new WaitUntil(() => !block.IsExecuting());

            if (!ExecuteBlock(block, commandIndex, onComplete))
            {
                Debug.LogWarning(
                    $"[Blackboard] Unable to restart '{block.BlockName}' from command index {commandIndex}.");
            }
        }

        /// <summary>
        /// Sends a message to this Blackboard only.
        /// Any block with a matching MessageReceived event handler will start executing.
        /// </summary>
        public virtual void SendScaffoldMessage(string messageName)
        {
            // Instead of GetComponents, filter from the registry
            foreach (MessageReceived eventHandler in MessageReceivers)
            {
                if (eventHandler != null && eventHandler.gameObject == this.gameObject)
                {
                    eventHandler.OnSendScaffoldMessage(messageName);
                }
            }
        }

        /// <summary>
        /// Returns a new variable key that is guaranteed not to clash with any existing variable in the list.
        /// </summary>
        public virtual string GetUniqueVariableKey(string originalKey, Variable ignoreVariable = null)
        {
            int suffix = 0;
            string baseKey = originalKey;

            // Only letters and digits allowed
            char[] arr = baseKey.Where(c => (char.IsLetterOrDigit(c) || c == '_')).ToArray();
            baseKey = new string(arr);

            // No leading digits allowed
            baseKey = baseKey.TrimStart('0', '1', '2', '3', '4', '5', '6', '7', '8', '9');

            // No empty keys allowed
            if (baseKey.Length == 0)
            {
                baseKey = "Var";
            }

            string key = baseKey;
            while (true)
            {
                bool collision = false;
                for (int i = 0; i < variables.Count; i++)
                {
                    Variable variable = variables[i];
                    if (variable == null || variable == ignoreVariable || variable.Key == null)
                    {
                        continue;
                    }
                    if (variable.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        collision = true;
                        suffix++;
                        key = baseKey + suffix;
                    }
                }

                if (!collision)
                {
                    return key;
                }
            }
        }

        /// <summary>
        /// Returns a new Block key that is guaranteed not to clash with any existing Block in the Blackboard.
        /// </summary>
        public virtual string GetUniqueBlockKey(string originalKey, Block ignoreBlock = null)
        {
            int suffix = 0;
            string baseKey = originalKey.Trim();

            // No empty keys allowed
            if (baseKey.Length == 0)
            {
                baseKey = ScaffoldConstants.DefaultBlockName;
            }

            Block[] blocks = GetComponents<Block>();

            string key = baseKey;
            while (true)
            {
                bool collision = false;
                for (int i = 0; i < blocks.Length; i++)
                {
                    Block block = blocks[i];
                    if (block == ignoreBlock || block.BlockName == null)
                    {
                        continue;
                    }
                    if (block.BlockName.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        collision = true;
                        suffix++;
                        key = baseKey + suffix;
                    }
                }

                if (!collision)
                {
                    return key;
                }
            }
        }

        /// <summary>
        /// Set the block objects to be hidden or visible depending on the hideComponents property.
        /// </summary>
        public virtual void UpdateHideFlags()
        {
            if (hideComponents)
            {
                Block[] blocks = GetComponents<Block>();
                for (int i = 0; i < blocks.Length; i++)
                {
                    Block block = blocks[i];
                    block.hideFlags = HideFlags.HideInInspector;
                    if (block.gameObject != gameObject)
                    {
                        block.hideFlags = HideFlags.HideInHierarchy;
                    }
                }

                Command[] commands = GetComponents<Command>();
                for (int i = 0; i < commands.Length; i++)
                {
                    Command command = commands[i];
                    command.hideFlags = HideFlags.HideInInspector;
                }

                EventHandler[] eventHandlers = GetComponents<EventHandler>();
                for (int i = 0; i < eventHandlers.Length; i++)
                {
                    EventHandler eventHandler = eventHandlers[i];
                    eventHandler.hideFlags = HideFlags.HideInInspector;
                }
            }
            else
            {
                MonoBehaviour[] monoBehaviours = GetComponents<MonoBehaviour>();
                for (int i = 0; i < monoBehaviours.Length; i++)
                {
                    MonoBehaviour monoBehaviour = monoBehaviours[i];
                    if (monoBehaviour == null)
                    {
                        continue;
                    }
                    monoBehaviour.hideFlags = HideFlags.None;
                    monoBehaviour.gameObject.hideFlags = HideFlags.None;
                }
            }
        }

        /// <summary>
        /// Clears the list of selected commands.
        /// </summary>
        public virtual void ClearSelectedCommands()
        {
            selectedCommands.Clear();
#if UNITY_EDITOR
            SelectedCommandsStale = true;
#endif
        }

        /// <summary>
        /// Adds a command to the list of selected commands.
        /// </summary>
        public virtual void AddSelectedCommand(Command command)
        {
            if (!selectedCommands.Contains(command))
            {
                selectedCommands.Add(command);
#if UNITY_EDITOR
                SelectedCommandsStale = true;
#endif
            }
        }

        /// <summary>
        /// Clears the list of selected blocks.
        /// </summary>
        public virtual void ClearSelectedBlocks()
        {
            if (selectedBlocks == null)
            {
                selectedBlocks = new List<Block>();
            }

            for (int i = 0; i < selectedBlocks.Count; i++)
            {
                Block item = selectedBlocks[i];

                if (item != null)
                {
                    item.IsSelected = false;
                }
            }
            selectedBlocks.Clear();
        }

        /// <summary>
        /// Adds a block to the list of selected blocks.
        /// </summary>
        public virtual void AddSelectedBlock(Block block)
        {
            if (!selectedBlocks.Contains(block))
            {
                block.IsSelected = true;
                selectedBlocks.Add(block);
            }
        }

        public virtual bool DeselectBlock(Block block)
        {
            if (selectedBlocks.Contains(block))
            {
                DeselectBlockNoCheck(block);
                return true;
            }
            return false;
        }

        public virtual void DeselectBlockNoCheck(Block b)
        {
            b.IsSelected = false;
            selectedBlocks.Remove(b);
        }

        public void UpdateSelectedCache()
        {
            selectedBlocks.Clear();
            Block[] res = gameObject.GetComponents<Block>();
            selectedBlocks = res.Where(x => x.IsSelected).ToList();
        }

        public void ReverseUpdateSelectedCache()
        {
            for (int i = 0; i < selectedBlocks.Count; i++)
            {
                if (selectedBlocks[i] != null)
                {
                    selectedBlocks[i].IsSelected = true;
                }
            }
        }

        /// <summary>
        /// Reset the commands and variables in the Blackboard.
        /// </summary>
        public virtual void Reset(bool resetCommands, bool resetVariables)
        {
            if (resetCommands)
            {
                Command[] commands = GetComponents<Command>();
                for (int i = 0; i < commands.Length; i++)
                {
                    Command command = commands[i];
                    command.OnReset();
                }
            }

            if (resetVariables)
            {
                for (int i = 0; i < variables.Count; i++)
                {
                    Variable variable = variables[i];
                    variable.OnReset();
                }
            }
        }

        /// <summary>
        /// Override this in a Blackboard subclass to filter which commands are shown in the Add Command list.
        /// </summary>
        public virtual bool IsCommandSupported(CommandInfoAttribute commandInfo)
        {
            for (int i = 0; i < hideCommands.Count; i++)
            {
                // Match on category or command name (case insensitive)
                string key = hideCommands[i];
                if (String.Compare(commandInfo.Category, key, StringComparison.OrdinalIgnoreCase) == 0 || String.Compare(commandInfo.CommandName, key, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if there are any executing blocks in this Blackboard.
        /// </summary>
        public virtual bool HasExecutingBlocks()
        {
            Block[] blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                Block block = blocks[i];
                if (block.IsExecuting())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a list of all executing blocks in this Blackboard.
        /// </summary>
        public virtual List<Block> GetExecutingBlocks()
        {
            List<Block> executingBlocks = new List<Block>();
            Block[] blocks = GetComponents<Block>();
            for (int i = 0; i < blocks.Length; i++)
            {
                Block block = blocks[i];
                if (block.IsExecuting())
                {
                    executingBlocks.Add(block);
                }
            }

            return executingBlocks;
        }
    }
}
