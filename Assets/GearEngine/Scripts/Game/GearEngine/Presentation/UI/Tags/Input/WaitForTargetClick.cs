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
    public class WaitForTargetClickAction : ActionBase
    {
        public TargetReference target = new TargetReference();

        [Inject] private IInputFilterService _inputService;
        [Inject] private IEventBus _eventBus;

        private bool isTargetClicked = false;

        public override void OnEnter()
        {
            isTargetClicked = false;

            if (_inputService == null || _eventBus == null)
            {
                this.TryInject();

                if (_eventBus == null) _eventBus = new Scaffold.Events.EventController();
                if (_inputService == null) _inputService = new Scaffold.Input.InputFilterService(_eventBus);
            }

            // Provide filtering for UI clicks based on target reference
            _inputService.FilterForButtonDownTarget(target);
            _inputService.FilterForButtonUpTarget(target); // Click often requires Down and Up on the same target

            _eventBus.AddListener<ScreenClickedEvent>(OnClicked);

            hostCommand.StartCoroutine(WaitForTargetClickCoroutine());
        }

        private void OnClicked(ScreenClickedEvent signal)
        {
            if (signal.TopResult == null || signal.TopResult.transform == null)
            {
                return;
            }

            GameObject clickedObj = signal.TopResult.gameObject;

            if (target.IsMatch(clickedObj))
            {
                isTargetClicked = true;
            }
            else
            {
                // Fallback to parents
                var tagComponent = clickedObj.GetComponentInParent<TagComponent>();
                if (tagComponent != null && target.IsMatch(tagComponent.gameObject))
                {
                    isTargetClicked = true;
                }
            }
        }

        private IEnumerator WaitForTargetClickCoroutine()
        {
            yield return new WaitUntil(() => isTargetClicked);

            Cleanup();
            Continue();
        }

        private void Cleanup()
        {
            if (_eventBus != null)
            {
                _eventBus.RemoveListener<ScreenClickedEvent>(OnClicked);
            }

            if (_inputService != null)
            {
                _inputService.ClearButtonDownFilters();
                _inputService.ClearButtonUpFilters();
            }
        }

        public override string GetSummary()
        {
            if (target == null) return "Error: No Target";
            return target.GetSummary();
        }
    }
}
