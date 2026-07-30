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
        public TutorialProgressController ProgressController
        {
            get => progressController;
            set => progressController = value;
        }

        [SerializeField] private TutorialProgressController progressController;

        public override void OnEnter()
        {
            if (progressController == null)
            {
                Debug.LogError(
                    "[CompleteTutorial] No TutorialProgressController " +
                    "reference was configured.");
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
