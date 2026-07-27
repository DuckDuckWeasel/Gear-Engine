
﻿using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// The block will execute when the Blackboard game object is enabled.
    /// </summary>
    [EventHandlerInfo("Scene",
                      "Blackboard Enabled",
                      "The block will execute when the Blackboard game object is enabled.")]
    [AddComponentMenu("")]
    public class BlackboardEnabled : EventHandler
    {   
        protected virtual void OnEnable()
        {
            // Blocks use coroutines to schedule command execution, but Unity's coroutines are
            // sometimes unreliable when enabling / disabling objects.
            // To workaround this we execute the block on the next frame.
            Invoke("DoEvent", 0);
        }

        protected virtual void DoEvent()
        {
            ExecuteBlock();
        }
    }
}
