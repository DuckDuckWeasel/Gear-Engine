using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Scaffold.Tutorial.Data;
using Scaffold.Events.Contracts;
using Scaffold.Tutorial.Events.Analytics;
using Scaffold.Tutorial.Events;
using Scaffold.Analytics;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scaffold.Tutorial.Controllers
{
    public class TutorialController : IInitializable, IDisposable
    {
        private readonly TutorialWrapper tutorialWrapper;
        private readonly IObjectResolver resolver;
        private readonly IEventBus eventBus;
        private readonly IAnalyticsService analyticsService;
        
        // Simulating the save data/progress state
        private readonly HashSet<string> completedTutorials = new HashSet<string>();
        private string currentInProgressId;
        
        private TutorialProgressController currentTutorialInstance;

        [Inject]
        public TutorialController(TutorialWrapper tutorialWrapper, IObjectResolver resolver, IEventBus eventBus, IAnalyticsService analyticsService = null)
        {
            this.tutorialWrapper = tutorialWrapper;
            this.resolver = resolver;
            this.eventBus = eventBus;
            this.analyticsService = analyticsService;
        }

        public void Initialize()
        {
            // Initialization logic, checking if a tutorial needs to be resumed
            _ = TutorialCheckAsync();
        }

        public void Dispose()
        {
            if (currentTutorialInstance != null)
            {
                currentTutorialInstance.OnTutorialStarted -= HandleTutorialStarted;
                currentTutorialInstance.OnTutorialCompleted -= HandleTutorialCompleted;
                currentTutorialInstance.OnTutorialStepReached -= HandleTutorialStepReached;
            }
        }

        public async UniTask<bool> TutorialCheckAsync()
        {
            Debug.Log("[TutorialController] Running TutorialCheckAsync");

            if (string.IsNullOrEmpty(currentInProgressId))
            {
                Debug.Log("[TutorialController] No in-progress tutorial found");
                return false;
            }

            TutorialSO tutorialData = tutorialWrapper.GetTutorialReference(currentInProgressId);
            if (tutorialData == null)
            {
                Debug.LogError($"[TutorialController] Tutorial with Id: {currentInProgressId} not found.");
                return false;
            }

            await LoadTutorialAsync(tutorialData.Id);
            return true;
        }

        public void StartTutorial(string id)
        {
            currentInProgressId = id;
            _ = LoadTutorialAsync(id);
        }

        public async UniTask CompleteTutorialAsync(string id, bool skipped, bool loadNext = true)
        {
            eventBus.Raise(new TutorialLoadingEvent(true, "Completing Tutorial..."));
            try
            {
                completedTutorials.Add(id);
                currentInProgressId = null;

                if (currentTutorialInstance != null)
                {
                    currentTutorialInstance.gameObject.SetActive(false);
                }

                TutorialSO completedData = tutorialWrapper.GetTutorialReference(id);
                if (completedData != null && completedData.NextTutorial != null && loadNext)
                {
                    currentInProgressId = completedData.NextTutorial.Id;
                    await LoadTutorialAsync(completedData.NextTutorial.Id);
                }
            }
            finally
            {
                eventBus.Raise(new TutorialLoadingEvent(false));
            }
        }

        private async UniTask LoadTutorialAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("[TutorialController] Id is null or empty");
                return;
            }

            TutorialSO referenceController = tutorialWrapper.GetTutorialReference(id);
            if (referenceController == null)
            {
                Debug.LogError($"[TutorialController] No TutorialData found for id: {id}");
                return;
            }

            if (referenceController.TutorialProgressController == null)
            {
                Debug.LogError($"[TutorialController] TutorialData {id} has no ProgressController prefab assigned.");
                return;
            }

            // In a real project, you'd instantiate this properly or resolve it via VContainer
            GameObject tutorialGameObject = UnityEngine.Object.Instantiate(referenceController.TutorialProgressController.gameObject);
            resolver.InjectGameObject(tutorialGameObject);
            
            tutorialGameObject.SetActive(true);
            
            if (currentTutorialInstance != null)
            {
                currentTutorialInstance.OnTutorialStarted -= HandleTutorialStarted;
                currentTutorialInstance.OnTutorialCompleted -= HandleTutorialCompleted;
                currentTutorialInstance.OnTutorialStepReached -= HandleTutorialStepReached;
            }

            currentTutorialInstance = tutorialGameObject.GetComponent<TutorialProgressController>();
            currentTutorialInstance.OnTutorialStarted += HandleTutorialStarted;
            currentTutorialInstance.OnTutorialCompleted += HandleTutorialCompleted;
            currentTutorialInstance.OnTutorialStepReached += HandleTutorialStepReached;
            
            currentTutorialInstance.Initialize(referenceController);
        }

        private void HandleTutorialStarted(string id)
        {
            Debug.Log($"[TutorialController] Tutorial {id} started.");
            analyticsService?.Record(new Scaffold.Tutorial.Events.Analytics.TutorialStartedEvent(id));
        }

        private void HandleTutorialStepReached(string id, string stepName)
        {
            Debug.Log($"[TutorialController] Tutorial {id} reached step: {stepName}");
            analyticsService?.Record(new Scaffold.Tutorial.Events.Analytics.TutorialStepReachedEvent(id, stepName));
        }

        private void HandleTutorialCompleted(string id, bool skipped)
        {
            Debug.Log($"[TutorialController] Tutorial {id} completed. Skipped: {skipped}");
            analyticsService?.Record(new Scaffold.Tutorial.Events.Analytics.TutorialCompletedEvent(id, skipped));
            _ = CompleteTutorialAsync(id, skipped);
        }
    }
}
