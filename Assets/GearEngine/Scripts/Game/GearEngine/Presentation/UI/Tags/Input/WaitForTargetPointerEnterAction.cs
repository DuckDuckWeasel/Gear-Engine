using System;
using System.Collections;
using System.Collections.Generic;
using Scaffold;
using UnityEngine;
using GearEngine.Core.Architecture.References;
using GearEngine.Core.Actions;
using VContainer;
using Scaffold.Input.Contracts;
using Scaffold.Input.Events;
using Scaffold.Events.Contracts;
using GearEngine.GearEngine.Extensions;
using GearEngine.GearEngine.Presentation.UI.Tags;
using GearEngine.GearEngine.Presentation.UI.Input;

namespace GearEngine.Actions.Input
{
    [CommandInfo("Input", "Wait For Target Pointer Enter", "Waits until the pointer enters a specific target.")]
    [Serializable]
    public class WaitForTargetPointerEnterAction : WaitForInputActionBase
    {
        public TargetReference Target = new TargetReference();

        private bool isTargetPointered = false;

        public override void OnEnter()
        {
            isTargetPointered = false;

            InitializeInputService();

            // Provide filtering for UI pointer enter based on target reference
            inputService.FilterForPointerEnterTarget(
                Target,
                TargetResolver);

            eventBus.AddListener<ScreenPointerEnterEvent>(OnPointerEnter);

            RunRoutine(WaitForTargetPointerEnterCoroutine());
        }

        private void OnPointerEnter(ScreenPointerEnterEvent signal)
        {
            if (signal.TopResult == null || signal.TopResult.transform == null)
            {
                return;
            }

            GameObject enteredObject = signal.TopResult.gameObject;
            isTargetPointered = IsTargetMatch(enteredObject);
        }

        private bool IsTargetMatch(GameObject enteredObject)
        {
            if (IsTargetMatch(Target, enteredObject))
            {
                return true;
            }

            TagComponent tagComponent = enteredObject.GetComponentInParent<TagComponent>();
            return tagComponent != null &&
                IsTargetMatch(Target, tagComponent.gameObject);
        }

        private IEnumerator WaitForTargetPointerEnterCoroutine()
        {
            while (!isTargetPointered)
            {
                TickFallbackIfNeeded();
                yield return null;
            }

            Cleanup();
            Continue();
        }

        public override void OnStopExecuting()
        {
            Cleanup();
            base.OnStopExecuting();
        }

        private void Cleanup()
        {
            if (eventBus != null)
            {
                eventBus.RemoveListener<ScreenPointerEnterEvent>(OnPointerEnter);
            }

            if (inputService != null)
            {
                inputService.ClearPointerEnterFilters();
            }
        }

        public override string GetSummary()
        {
            if (Target == null)
            {
                return "Error: No Target";
            }

            return Target.GetSummary();
        }
    }
}
