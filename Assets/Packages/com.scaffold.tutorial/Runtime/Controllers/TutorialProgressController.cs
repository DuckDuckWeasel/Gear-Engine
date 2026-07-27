using System;
using UnityEngine;

namespace Scaffold.Tutorial.Controllers
{
    public class TutorialProgressController : MonoBehaviour
    {
        private string id;

        public event Action<string> OnTutorialStarted;
        public event Action<string, string> OnTutorialStepReached;
        public event Action<string, bool> OnTutorialCompleted;

        public void NotifyStepReached(string stepName)
        {
            OnTutorialStepReached?.Invoke(this.id, stepName);
        }

        public void Initialize(Data.TutorialSO data)
        {
            this.id = data.Id;
            StartProgress(this.id);
        }

        public virtual void StartProgress(string id)
        {
            OnTutorialStarted?.Invoke(id);
        }

        public virtual void CompleteProgress(bool skipped = false)
        {
            OnTutorialCompleted?.Invoke(this.id, skipped);
        }
    }
}
