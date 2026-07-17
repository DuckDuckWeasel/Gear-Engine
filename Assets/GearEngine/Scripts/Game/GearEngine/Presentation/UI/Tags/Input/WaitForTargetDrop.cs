using System;
using System.Collections;
using System.Collections.Generic;
using GearEngine.GearEngine.Presentation.UI.Tags;
using Scaffold;
using UnityEngine;
using VContainer;
using Scaffold.Input.Contracts;
using Scaffold.Input.Events;
using Scaffold.Events.Contracts;
using UnityEngine.EventSystems;
using GearEngine.Core.Architecture.References;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Extensions;
using GearEngine.GearEngine.Presentation.UI.Input;

namespace GearEngine.GearEngine.Presentation.UI.Actions
{
    [Serializable]
    public class WaitForTargetDropAction : ActionBase
    {
        public TargetReference dragTarget = new TargetReference();
        public TargetReference dropTarget = new TargetReference();

        public bool checkDroppedGameObject = false;

        [Inject] private IInputFilterService _inputService;
        [Inject] private IEventBus _eventBus;

        private bool isTargetDropped = false;

        public override void OnEnter()
        {
            isTargetDropped = false;

            if (_inputService == null || _eventBus == null)
            {
                this.TryInject();

                if (_eventBus == null) _eventBus = new Scaffold.Events.EventController();
                if (_inputService == null) _inputService = new Scaffold.Input.InputFilterService(_eventBus);
            }

            _inputService.FilterForButtonDownTarget(dragTarget);
            _inputService.FilterForDropEnterTarget(checkDroppedGameObject, dropTarget);

            _eventBus.AddListener<ScreenDroppedEvent>(OnDrop);

            hostCommand.StartCoroutine(WaitForTargetDropCoroutine());
        }

        private void OnDrop(ScreenDroppedEvent screenDroppedSignal)
        {
            if (screenDroppedSignal.DropTopResult == null || screenDroppedSignal.DropTopResult.transform == null)
            {
                Debug.Log($"[WaitForTargetDrop] Dropped object is null. Method: {nameof(OnDrop)}");
                return;
            }

            GameObject droppedObj = screenDroppedSignal.DropTopResult.gameObject;

            // First check if the direct object matches
            if (!dropTarget.IsMatch(droppedObj))
            {
                // Fallback to checking parent just in case, similar to previous logic
                TagComponent tagComponent = droppedObj.GetComponentInParent<TagComponent>();
                if (tagComponent == null || !dropTarget.IsMatch(tagComponent.gameObject))
                {
                    Debug.Log($"[WaitForTargetDrop] Dropped target does not match the required target reference. Method: {nameof(OnDrop)}");
                    return;
                }
            }

            isTargetDropped = true;
        }

        private IEnumerator WaitForTargetDropCoroutine()
        {
            Debug.Log($"[WaitForTargetDrop] Waiting for target drop with the required tags and condition.");

            yield return new WaitUntil(() => isTargetDropped);

            Debug.Log($"[WaitForTargetDrop] Target with the required tags and condition has been dropped!");

            if (_inputService != null)
            {
                _inputService.ClearButtonDownFilters();
                _inputService.ClearButtonUpFilters();
            }

            if (_eventBus != null)
            {
                _eventBus.RemoveListener<ScreenDroppedEvent>(OnDrop);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (dragTarget == null || dropTarget == null) return "Error: No Target";
            return $"{dragTarget.GetSummary()} -> {dropTarget.GetSummary()}";
        }
    }
}
