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
    [CommandInfo("Input", "Wait For Target Click", "Waits for a click on a object with the specified target tags.")]
    [AddComponentMenu("")]
    public class WaitForTargetClick : Command
    {
        public List<TagSO> targetTagSOList;
        public bool matchAll = false;

        private bool isTargetClicked = false;
        
        [Inject] private IInputFilterService _inputService;
        [Inject] private IEventBus _eventBus;

        public override void OnEnter()
        {
            if (targetTagSOList == null)
            {
                Debug.LogError($"[WaitForTargetClick] targetTagSOList is null, cannot filter input. Method: {nameof(OnEnter)}");
                Continue();
                return;
            }

            _inputService.FilterForButtonUpTags(matchAll, targetTagSOList.ToArray());

            isTargetClicked = false;

            _eventBus.AddListener<ScreenClickedEvent>(OnClick);

            StartCoroutine(WaitForTargetClickCoroutine());
        }

        private void OnClick(ScreenClickedEvent screenClickedSignal)
        {
            if (screenClickedSignal.TopResult == null || screenClickedSignal.TopResult.transform.gameObject == null)
            {
                Debug.Log($"[WaitForTargetClick] Clicked object is null. Method: {nameof(OnClick)}");
                return;
            }

            TagComponent tagComponent = screenClickedSignal.TopResult.transform.GetComponent<TagComponent>();

            if (tagComponent == null || !tagComponent.ContainsTag(targetTagSOList.ToArray(), matchAll))
            {
                Debug.Log($"[WaitForTargetClick] Clicked target does not match the required tags or condition. Method: {nameof(OnClick)}");
                return;
            }

            isTargetClicked = true;
        }

        private IEnumerator WaitForTargetClickCoroutine()
        {
            Debug.Log($"[WaitForTargetClick] Waiting for target click with the required tags and condition.");

            yield return new WaitUntil(() => isTargetClicked);

            Debug.Log($"[WaitForTargetClick] Target with the required tags and condition has been clicked!");

            _inputService.ClearButtonUpFilters();

            _eventBus.RemoveListener<ScreenClickedEvent>(OnClick);

            Continue();
        }
    }
}
