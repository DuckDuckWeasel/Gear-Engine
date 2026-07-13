using System;
using Scaffold.Tutorial.Variables;

namespace Scaffold.Tutorial.Requirements
{
    [Serializable]
    public class TutorialRequirementReference : TutorialVariableReference<ITutorialRequirement, TutorialRequirementSO>
    {
        protected override ITutorialRequirement GetReferenceValue(TutorialRequirementSO reference)
        {
            return reference != null ? reference.Data : default;
        }
    }
}
