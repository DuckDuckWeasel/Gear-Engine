using Scaffold;
using Scaffold.Tutorial.Controllers;
using UnityEngine;

namespace Scaffold.Tutorial.ScaffoldIntegration
{
    [RequireComponent(typeof(Flowchart), typeof(TutorialProgressController))]
    public class ScaffoldTutorialAdapter : MonoBehaviour
    {
        private Flowchart flowchart;
        private TutorialProgressController tutorialController;

        private void Awake()
        {
            flowchart = GetComponent<Flowchart>();
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
            if (block.GetFlowchart() == flowchart)
            {
                tutorialController.NotifyStepReached(block.BlockName);
            }
        }
    }
}
