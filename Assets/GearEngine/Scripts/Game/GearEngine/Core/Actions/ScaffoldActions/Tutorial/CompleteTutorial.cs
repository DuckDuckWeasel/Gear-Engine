using System;
using GearEngine.Core.Actions;
using Scaffold.Tutorial.Controllers;
using UnityEngine;

namespace Scaffold
{
    [CommandInfo(
        "Tutorial",
        "Complete Tutorial",
        "Completes the TutorialProgressController attached to this Blackboard.")]
    [AddComponentMenu("")]
    [Serializable]
    public sealed class CompleteTutorial : ActionBase
    {
        public override void OnEnter()
        {
            TutorialProgressController progressController =
                blackboard != null
                    ? blackboard.GetComponent<TutorialProgressController>()
                    : null;

            if (progressController == null)
            {
                Debug.LogError(
                    "[CompleteTutorial] The Blackboard has no " +
                    "TutorialProgressController.");
                Fail();
                return;
            }

            progressController.CompleteProgress();
            Continue();
        }

        public override Color GetButtonColor()
        {
            return new Color32(255, 204, 153, 255);
        }
    }
}
