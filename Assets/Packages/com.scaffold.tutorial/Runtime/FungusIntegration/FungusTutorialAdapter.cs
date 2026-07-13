using Fungus;
using Scaffold.Tutorial.Controllers;
using UnityEngine;

namespace Scaffold.Tutorial.FungusIntegration
{
    [RequireComponent(typeof(Flowchart), typeof(TutorialProgressController))]
    public class FungusTutorialAdapter : MonoBehaviour
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
