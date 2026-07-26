using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

namespace Scaffold
{
    /// <summary>
    /// Shuffle the order of the items in a Scaffold Menu
    /// </summary>
    [CommandInfo("Narrative",
                 "Menu Shuffle",
        "Shuffle the order of the items in a Scaffold Menu")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class MenuShuffle : ActionBase
    {
        public enum Mode
        {
            Every,
            Once
        }
        [SerializeField]
        [Tooltip("Determines if the order is shuffled everytime this command is it (Every) or if it is consistent when returned to but random (Once)")]
        protected Mode shuffleMode = Mode.Once;

        [Tooltip("The Seed")]
        private int seed = -1;

        public override void OnEnter()
        {
            MenuDialog menuDialog = MenuDialog.GetMenuDialog();

            //if we shuffle every time or we haven't shuffled yet
            if (shuffleMode == Mode.Every || seed == -1)
            {
                seed = UnityEngine.Random.Range(0, 1000000);
            }

            if (menuDialog != null)
            {
                menuDialog.Shuffle(new System.Random(seed));
            }

            Continue();
        }

        public override string GetSummary()
        {
            return shuffleMode.ToString();
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255);
        }
    }
}