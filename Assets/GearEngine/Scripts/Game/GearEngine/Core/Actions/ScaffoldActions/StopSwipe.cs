using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Deactivates swipe panning mode.
    /// </summary>
    [CommandInfo("Camera",
                 "Stop Swipe",
                 "Deactivates swipe panning mode.")]
    [Serializable]
    public class StopSwipe : ActionBase
    {
        [SerializeField] private CameraManager cameraManager;

        #region Public members

        public override void OnEnter()
        {
            if (cameraManager == null)
            {
                Debug.LogError(
                    "[StopSwipe] A CameraManager reference is required.");
                Fail();
                return;
            }

            cameraManager.StopSwipePan();

            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(216, 228, 170, 255);
        }

        #endregion
    }
}
