
using UnityEngine;
using System.Collections.Generic;

namespace Scaffold
{
    [EventHandlerInfo("Scene",
                      "Save Point Loaded",
                      "Execute this block when a saved point is loaded. Use the 'new_game' key to handle game start.")]
    public class SavePointLoaded : EventHandler
    {
        [Tooltip("Block will execute if the Save Key of the loaded save point matches this save key.")]
        [SerializeField] protected List<string> savePointKeys = new List<string>();

        public static readonly HashSet<SavePointLoaded> Instances = new HashSet<SavePointLoaded>();

        protected virtual void OnEnable()
        {
            Instances.Add(this);
        }

        protected virtual void OnDisable()
        {
            Instances.Remove(this);
        }

        protected void OnSavePointLoaded(string _savePointKey)
        {
            for (int i = 0; i < savePointKeys.Count; i++)
            {
                string key = savePointKeys[i];
                if (string.Compare(key, _savePointKey, true) == 0)
                {
                    ExecuteBlock();
                    break;
                }
            }
        }

        #region Public methods

        public static void NotifyEventHandlers(string _savePointKey)
        {
            // Fire any matching SavePointLoaded event handler with matching save key.
            foreach (SavePointLoaded eventHandler in Instances)
            {
                if (eventHandler != null)
                {
                    eventHandler.OnSavePointLoaded(_savePointKey);
                }
            }
        }

        #endregion
    }
}