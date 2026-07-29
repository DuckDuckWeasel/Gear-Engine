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

namespace GearEngine.Actions.Input
{
    [CommandInfo("Input", "Wait For Target Drop", "Waits until a drag target is dropped onto a drop target.")]
    [Serializable]
    public class WaitForTargetDropAction : WaitForInputActionBase
    {
        public TargetReference DragTarget = new TargetReference();
        public TargetReference DropTarget = new TargetReference();

        public bool CheckDroppedGameObject = false;

        private bool isTargetDropped = false;

        public override void OnEnter()
        {
            isTargetDropped = false;

            InitializeInputService();

            inputService.FilterForButtonDownTarget(
                DragTarget,
                TargetResolver);
            inputService.FilterForDropEnterTarget(
                CheckDroppedGameObject,
                DropTarget,
                TargetResolver);

            eventBus.AddListener<ScreenDroppedEvent>(OnDrop);

            RunRoutine(WaitForTargetDropCoroutine());
        }

        private void OnDrop(ScreenDroppedEvent screenDroppedSignal)
        {
            if (screenDroppedSignal.DropTopResult == null || screenDroppedSignal.DropTopResult.transform == null)
            {
                Debug.Log($"[WaitForTargetDrop] Dropped object is null. Method: {nameof(OnDrop)}");
                return;
            }

            GameObject droppedObj = screenDroppedSignal.DropTopResult.gameObject;

            if (!IsValidDropTarget(droppedObj))
            {
                Debug.Log($"[WaitForTargetDrop] Dropped target does not match the required target reference. Method: {nameof(OnDrop)}");
                return;
            }

            isTargetDropped = true;
        }

        private bool IsValidDropTarget(GameObject droppedObject)
        {
            if (IsTargetMatch(DropTarget, droppedObject))
            {
                return true;
            }

            TagComponent tagComponent = droppedObject.GetComponentInParent<TagComponent>();
            return tagComponent != null &&
                IsTargetMatch(DropTarget, tagComponent.gameObject);
        }

        private IEnumerator WaitForTargetDropCoroutine()
        {
            while (!isTargetDropped)
            {
                TickFallbackIfNeeded();
                yield return null;
            }

            Debug.Log($"[WaitForTargetDrop] Target with the required tags and condition has been dropped!");

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
            if (inputService != null)
            {
                inputService.ClearButtonDownFilters();
                inputService.ClearButtonUpFilters();
            }

            eventBus?.RemoveListener<ScreenDroppedEvent>(OnDrop);
        }

        public override string GetSummary()
        {
            if (DragTarget == null || DropTarget == null)
            {
                return "Error: No Target";
            }

            return $"{DragTarget.GetSummary()} -> {DropTarget.GetSummary()}";
        }
    }
}
