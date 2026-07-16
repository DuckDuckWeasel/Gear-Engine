using System.Collections;
using System.Collections.Generic;
using GearEngine.GearEngine.Presentation.UI.Tags;
using Fungus;
using UnityEngine;
using VContainer;
using Scaffold.Input.Contracts;
using Scaffold.Input.Events;
using Scaffold.Events.Contracts;
using UnityEngine.EventSystems;
using GearEngine.Core.Architecture.References;
using Command = Fungus.Command;
using GearEngine.GearEngine.Extensions;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    [CommandInfo("Input", "Wait For Target Drop", "Waits for dropping an object with the specified target tags.")]
    [AddComponentMenu("")]
    public class WaitForTargetDrop : Command
    {
        public TargetReference dragTarget = new TargetReference();
        public TargetReference dropTarget = new TargetReference();

        public bool checkDroppedGameObject = false;

        // Legacy fields for migration
        [HideInInspector] public List<TagSO> dragTargetTagSOList = new();
        [HideInInspector] public List<TagSO> dropTargetTagSOList = new();
        [HideInInspector] public bool matchAll = false;
        [HideInInspector] [SerializeField] private bool migratedToTargetReference = false;

        protected virtual void OnEnable()
        {
            if (!migratedToTargetReference)
            {
                if (dragTargetTagSOList != null && dragTargetTagSOList.Count > 0)
                {
                    dragTarget.strategy = TargetResolutionStrategy.Tags;
                    dragTarget.tagFilter.soTags = new List<TagSO>(dragTargetTagSOList);
                    dragTarget.tagFilter.matchAll = matchAll;
                    dragTargetTagSOList.Clear();
                }
                
                if (dropTargetTagSOList != null && dropTargetTagSOList.Count > 0)
                {
                    dropTarget.strategy = TargetResolutionStrategy.Tags;
                    dropTarget.tagFilter.soTags = new List<TagSO>(dropTargetTagSOList);
                    dropTarget.tagFilter.matchAll = matchAll;
                    dropTargetTagSOList.Clear();
                }

                migratedToTargetReference = true;
            }
        }

        private bool isTargetDropped = false;
        
        [Inject] private IInputFilterService _inputService;
        [Inject] private IEventBus _eventBus;

        public override void OnEnter()
        {
            if (_inputService == null || _eventBus == null)
            {
                this.TryInject();

                if (_eventBus == null) _eventBus = new Scaffold.Events.EventController();
                if (_inputService == null) _inputService = new Scaffold.Input.InputFilterService(_eventBus);
            }

            _inputService.FilterForButtonDownTarget(dragTarget);
            _inputService.FilterForDropEnterTarget(checkDroppedGameObject, dropTarget);

            isTargetDropped = false;

            _eventBus.AddListener<ScreenDroppedEvent>(OnDrop);

            StartCoroutine(WaitForTargetDropCoroutine());
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

            _inputService.ClearButtonDownFilters();
            _inputService.ClearButtonUpFilters();

            _eventBus.RemoveListener<ScreenDroppedEvent>(OnDrop);

            Continue();
        }

        public override string GetSummary()
        {
            if (dragTarget == null || dropTarget == null) return "Error: No Target";
            return $"{dragTarget.GetSummary()} -> {dropTarget.GetSummary()}";
        }
    }
}
