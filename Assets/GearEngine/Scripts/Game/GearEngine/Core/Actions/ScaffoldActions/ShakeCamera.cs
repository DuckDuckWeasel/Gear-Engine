using System;
using GearEngine.Core.Actions;

using UnityEngine;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Applies a camera shake effect to the main camera.
    /// </summary>
    [CommandInfo("Camera",
                 "Shake Camera",
                 "Applies a camera shake effect to the main camera.")]
    [Serializable]
    public class ShakeCamera : ActionBase
    {
        [Tooltip("Time for camera shake effect to complete")]
        [SerializeField] protected float duration = 0.5f;

        [Tooltip("Magnitude of shake effect in x & y axes")]
        [SerializeField] protected Vector2 amount = new Vector2(1, 1);

        [Tooltip("Wait until the shake effect has finished before executing next command")]
        [SerializeField] protected bool waitUntilFinished;

        #region Public members

        public override void OnEnter()
        {
            Vector3 v = new Vector3();
            v = amount;

            Hashtable tweenParams = new Hashtable();
            tweenParams.Add("amount", v);
            tweenParams.Add("time", duration);
            iTween.ShakePosition(Camera.main.gameObject.gameObject, tweenParams);

            if (waitUntilFinished)
            {
                Invoke(nameof(CompleteShake), duration);
            }
            else
            {
                Continue();
            }
        }

        private void CompleteShake()
        {
            Continue();
        }

        public override string GetSummary()
        {
            return "For " + duration + " seconds.";
        }

        public override Color GetButtonColor()
        {
            return new Color32(216, 228, 170, 255);
        }

        #endregion
    }
}
