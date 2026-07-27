using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Destroys a specified game object in the scene.
    /// </summary>
    [CommandInfo("Scripting",
                 "Destroy",
                 "Destroys a specified game object in the scene.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class Destroy : ActionBase
    {
        [Tooltip("Reference to game object to destroy")]
        [SerializeField] protected GameObjectData targetGameObject;

        [Tooltip("Optional delay given to destroy")]
        [SerializeField]
        protected FloatData destroyInXSeconds = new FloatData(0);

        #region Public members

        public override void OnEnter()
        {
            if (targetGameObject.Value != null)
            {
                if (destroyInXSeconds.Value != 0)
                {
                    global::UnityEngine.Object.Destroy(targetGameObject, destroyInXSeconds.Value);
                }
                else
                {
                    global::UnityEngine.Object.Destroy(targetGameObject.Value);
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (targetGameObject.Value == null)
            {
                return "Error: No game object selected";
            }

            return targetGameObject.Value.name + (destroyInXSeconds.Value == 0 ? "" : " in " + destroyInXSeconds.Value.ToString());
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            if (targetGameObject.gameObjectRef == variable || destroyInXSeconds.floatRef == variable)
            {
                return true;
            }

            return false;
        }

        #endregion

    }
}