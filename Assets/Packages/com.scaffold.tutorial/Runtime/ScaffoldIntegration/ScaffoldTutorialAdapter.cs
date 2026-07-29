using System;
using Scaffold.Tutorial.Controllers;
using Scaffold.VisualScripting;
using Scaffold.VisualScripting.Unity;
using UnityEngine;

namespace Scaffold.Tutorial.ScaffoldIntegration
{
    [RequireComponent(
        typeof(BlackboardBehaviour),
        typeof(TutorialProgressController))]
    public sealed class ScaffoldTutorialAdapter : MonoBehaviour
    {
        private BlackboardBehaviour blackboardBehaviour;
        private TutorialProgressController tutorialController;
        private IDisposable blockStartedSubscription;

        private void Awake()
        {
            blackboardBehaviour = GetComponent<BlackboardBehaviour>();
            tutorialController = GetComponent<TutorialProgressController>();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            blockStartedSubscription?.Dispose();
            blockStartedSubscription = null;
        }

        private void Subscribe()
        {
            if (blockStartedSubscription != null ||
                blackboardBehaviour == null ||
                !blackboardBehaviour.IsRuntimeAvailable)
            {
                return;
            }

            blockStartedSubscription =
                blackboardBehaviour.Runtime.EventBus.Subscribe<
                    BlackboardBlockStartedEvent>(HandleBlockStarted);
        }

        private void HandleBlockStarted(
            BlackboardBlockStartedEvent eventValue)
        {
            if (eventValue.RuntimeInstanceId !=
                blackboardBehaviour.Runtime.RuntimeInstanceId)
            {
                return;
            }

            tutorialController.NotifyStepReached(eventValue.BlockName);
        }
    }
}
