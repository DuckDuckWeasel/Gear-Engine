using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Scaffold.Tutorial.Requirements;
using Scaffold.VisualScripting;
using UnityEngine;

namespace Scaffold.Tutorial.ScaffoldIntegration
{
    [Serializable]
    public sealed class WaitTutorialRequirementCommand :
        Scaffold.VisualScripting.ActionBase
    {
        [SerializeField]
        [Tooltip("The requirement to evaluate before continuing.")]
        private TutorialRequirementReference requirement =
            new TutorialRequirementReference();

        [NonSerialized, BlackboardTransient]
        private IDisposable scheduledRoutine;

        protected override void OnExecute()
        {
            if (requirement?.Value == null)
            {
                Succeed();
                return;
            }

            scheduledRoutine = Context.Scheduler.ScheduleRoutine(
                WaitForRequirement());
        }

        protected override void OnInterrupted()
        {
            scheduledRoutine?.Dispose();
            scheduledRoutine = null;
        }

        public string GetSummary()
        {
            return requirement?.Value == null
                ? "Error: No Requirement specified"
                : "Wait for Requirement";
        }

        private IEnumerator WaitForRequirement()
        {
            yield return requirement.Value.WaitUntilMetAsync().ToCoroutine();
            scheduledRoutine = null;
            Succeed();
        }
    }
}
