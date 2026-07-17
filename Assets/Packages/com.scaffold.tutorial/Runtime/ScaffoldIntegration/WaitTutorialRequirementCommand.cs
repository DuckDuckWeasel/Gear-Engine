using System.Collections;
using Scaffold;
using UnityEngine;

using Scaffold.Tutorial.Requirements;
using Cysharp.Threading.Tasks;

namespace Scaffold.Tutorial.ScaffoldIntegration
{
    [CommandInfo("Tutorial", "Wait Tutorial Requirement", "Waits until the specified tutorial requirement is met.")]
    public class WaitTutorialRequirementCommand : Command
    {
        [SerializeField]
        [Tooltip("The requirement to evaluate before continuing.")]
        private TutorialRequirementReference requirement = new TutorialRequirementReference();


        public override void OnEnter()
        {
            if (requirement == null || requirement.Value == null)
            {
                Continue();
                return;
            }

            StartCoroutine(WaitForRequirement());
        }
        
        private IEnumerator WaitForRequirement()
        {
            yield return requirement.Value.WaitUntilMetAsync().ToCoroutine();
            Continue();
        }

        public override string GetSummary()
        {
            if (requirement == null || requirement.Value == null) return "Error: No Requirement specified";
            return "Wait for Requirement";
        }
    }
}
