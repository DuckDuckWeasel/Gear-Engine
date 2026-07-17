using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Calls UnityEngine.Object.DontDestroyOnLoad on the target gameobject.
    /// </summary>
    [CommandInfo("Scripting",
                 "DestroyOnLoad",
                 "Calls UnityEngine.Object.DontDestroyOnLoad on the target gameobject")]
    [Serializable]
    public class DestroyOnLoad : ActionBase
    {
        [SerializeField] protected GameObjectData target;

        public override void OnEnter()
        {
            UnityEngine.Object.DontDestroyOnLoad(target.Value);

            Continue();
        }

        public override string GetSummary()
        {
            return target.Value != null ? target.Value.name : "Error: no target set";
        }

        public override bool HasReference(Variable variable)
        {
            return variable == target.gameObjectRef;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }
    }
}