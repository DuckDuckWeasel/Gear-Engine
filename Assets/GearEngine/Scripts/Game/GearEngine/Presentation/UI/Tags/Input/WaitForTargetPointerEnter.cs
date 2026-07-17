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

namespace GearEngine.GearEngine.Presentation.UI.Actions
{
    [Serializable]
    public class WaitForTargetPointerEnterAction : ActionBase
    {
        public TargetReference target = new TargetReference();

        [Inject] private IInputFilterService _inputService;
        [Inject] private IEventBus _eventBus;

        private bool isTargetPointered = false;

        public override void OnEnter()
        {
            isTargetPointered = false;

            if (_inputService == null || _eventBus == null)
            {
                this.TryInject();

                if (_eventBus == null) _eventBus = new Scaffold.Events.EventController();
                if (_inputService == null) _inputService = new Scaffold.Input.InputFilterService(_eventBus);
            }

            // Provide filtering for UI pointer enter based on target reference
            _inputService.FilterForPointerEnterTarget(target);

            _eventBus.AddListener<ScreenPointerEnterEvent>(OnPointerEnter);

            hostCommand.StartCoroutine(WaitForTargetPointerEnterCoroutine());
        }

        private void OnPointerEnter(ScreenPointerEnterEvent signal)
        {
            if (signal.TopResult == null || signal.TopResult.transform == null)
            {
                return;
            }

            GameObject enteredObj = signal.TopResult.gameObject;

            if (target.IsMatch(enteredObj))
            {
                isTargetPointered = true;
            }
            else
            {
                // Fallback to parents
                var tagComponent = enteredObj.GetComponentInParent<TagComponent>();
                if (tagComponent != null && target.IsMatch(tagComponent.gameObject))
                {
                    isTargetPointered = true;
                }
            }
        }

        private IEnumerator WaitForTargetPointerEnterCoroutine()
        {
            yield return new WaitUntil(() => isTargetPointered);

            Cleanup();
            Continue();
        }

        private void Cleanup()
        {
            if (_eventBus != null)
            {
                _eventBus.RemoveListener<ScreenPointerEnterEvent>(OnPointerEnter);
            }

            if (_inputService != null)
            {
                _inputService.ClearPointerEnterFilters();
            }
        }

        public override string GetSummary()
        {
            if (target == null) return "Error: No Target";
            return target.GetSummary();
        }
    }
}
