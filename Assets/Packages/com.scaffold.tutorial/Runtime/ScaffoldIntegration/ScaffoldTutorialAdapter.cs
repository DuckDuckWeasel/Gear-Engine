using Scaffold;
using Scaffold.Tutorial.Controllers;
using UnityEngine;

namespace Scaffold.Tutorial.ScaffoldIntegration
{
    [RequireComponent(typeof(Blackboard), typeof(TutorialProgressController))]
    public class ScaffoldTutorialAdapter : MonoBehaviour
    {
        private Blackboard blackboard;
        private TutorialProgressController tutorialController;

        private void Awake()
        {
            blackboard = GetComponent<Blackboard>();
            tutorialController = GetComponent<TutorialProgressController>();
        }

        private void OnEnable()
        {
            BlockSignals.OnBlockStart += HandleBlockStart;
        }

        private void OnDisable()
        {
            BlockSignals.OnBlockStart -= HandleBlockStart;
        }

        private void HandleBlockStart(Block block)
        {
            if (block.GetBlackboard() == blackboard)
            {
                tutorialController.NotifyStepReached(block.BlockName);
            }
        }
    }
}
