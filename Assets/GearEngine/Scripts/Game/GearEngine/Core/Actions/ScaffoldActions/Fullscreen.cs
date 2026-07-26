using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Fullscreen mode options.
    /// </summary>
    public enum FullscreenMode
    {
        /// <summary> Toggle the current mode between fullscreen and windowed. </summary>
        Toggle,
        /// <summary> Switch to fullscreen mode. </summary>
        Fullscreen,
        /// <summary> Switch to windowed mode. </summary>
        Windowed
    }

    /// <summary>
    /// Sets the application to fullscreen, windowed or toggles the current state.
    /// </summary>
    [CommandInfo("Camera",
                 "Fullscreen",
                 "Sets the application to fullscreen, windowed or toggles the current state.")]
    [Serializable]
    public class Fullscreen : ActionBase
    {
        [Tooltip("The Fullscreen mode")]
        [SerializeField] protected FullscreenMode fullscreenMode;

        #region Public members

        public override void OnEnter()
        {
            switch (fullscreenMode)
            {
                case FullscreenMode.Toggle:
                    Screen.fullScreen = !Screen.fullScreen;
                    break;
                case FullscreenMode.Fullscreen:
                    Screen.fullScreen = true;
                    break;
                case FullscreenMode.Windowed:
                    Screen.fullScreen = false;
                    break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return fullscreenMode.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(216, 228, 170, 255);
        }

        #endregion
    }
}