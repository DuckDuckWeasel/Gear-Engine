using System.Collections;
using System.Collections.Generic;
using GearEngine.GearEngine.Presentation.UI.Tags;
using Fungus;
using UnityEngine;
using VContainer;
using Scaffold.Input.Contracts;
using Scaffold.Input.Events;
using Scaffold.Events.Contracts;
using Command = Fungus.Command;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    [CommandInfo("Input", "Wait For Target Drop", "Waits for dropping an object with the specified target tags.")]
    [AddComponentMenu("")]
    public class WaitForTargetDrop : Command
    {
        public List<TagSO> dragTargetTagSOList = new();
        public List<TagSO> dropTargetTagSOList = new();
        public bool matchAll = false;
        public bool checkDroppedGameObject = false;

        private bool isTargetDropped = false;
        
        [Inject] private IInputFilterService _inputService;
        [Inject] private IEventBus _eventBus;

        public override void OnEnter()
        {
            if (dragTargetTagSOList == null || dropTargetTagSOList == null)
            {
                Debug.LogError($"[WaitForTargetDrop] dragTargetTagSOList or dropTargetTagSOList is null. Method: {nameof(OnEnter)}");
                Continue();
                return;
            }

            _inputService.FilterForButtonDownTags(matchAll, dragTargetTagSOList.ToArray());
            _inputService.FilterForDropEnterTags(matchAll, checkDroppedGameObject, dropTargetTagSOList.ToArray());

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

            TagComponent tagComponent = screenDroppedSignal.DropTopResult.transform.GetComponent<TagComponent>();

            if (tagComponent == null || !tagComponent.ContainsTag(dropTargetTagSOList.ToArray(), matchAll))
            {
                Debug.Log($"[WaitForTargetDrop] Dropped target does not match the required tags or condition. Method: {nameof(OnDrop)}");
                return;
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
    }
}
