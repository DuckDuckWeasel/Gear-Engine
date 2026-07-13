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
    [CommandInfo("Input", "Wait For Target Pointer Enter", "Waits for pointer entering an object with the specified target tags.")]
    [AddComponentMenu("")]
    public class WaitForTargetPointerEnter : Command
    {
        public List<TagSO> targetTagSOList;
        public bool matchAll = false;

        private bool isTargetPointered = false;

        [Inject] private IInputFilterService _inputService;
        [Inject] private IEventBus _eventBus;

        public override void OnEnter()
        {
            if (targetTagSOList == null)
            {
                Debug.LogError($"[WaitForTargetPointerEnter] targetTagSOList is null. Method: {nameof(OnEnter)}");
                Continue();
                return;
            }

            _inputService.FilterForPointerEnterTags(matchAll, targetTagSOList.ToArray());

            isTargetPointered = false;

            _eventBus.AddListener<ScreenPointerEnterEvent>(OnPointerEnter);
            _eventBus.AddListener<ScreenPointerExitEvent>(OnPointerExit);

            StartCoroutine(WaitForTargetPointerEnterCoroutine());
        }

        public void OnPointerEnter(ScreenPointerEnterEvent screenPointerEnterSignal)
        {
            if (screenPointerEnterSignal.TopResult == null || screenPointerEnterSignal.TopResult.transform == null)
            {
                return;
            }

            TagComponent tagComponent = screenPointerEnterSignal.TopResult.transform.GetComponent<TagComponent>();

            if (tagComponent == null || !tagComponent.ContainsTag(targetTagSOList.ToArray(), matchAll))
            {
                return;
            }

            isTargetPointered = true;
        }

        private void OnPointerExit(ScreenPointerExitEvent screenPointerExitSignal)
        {
            isTargetPointered = false;
        }

        private IEnumerator WaitForTargetPointerEnterCoroutine()
        {
            yield return new WaitUntil(() => isTargetPointered);

            _inputService.ClearPointerEnterFilters();
            UnsubscribeAll();

            Continue();
        }

        public void OnDisable()
        {
            UnsubscribeAll();
        }

        private void UnsubscribeAll()
        {
            if (_eventBus != null)
            {
                _eventBus.RemoveListener<ScreenPointerEnterEvent>(OnPointerEnter);
                _eventBus.RemoveListener<ScreenPointerExitEvent>(OnPointerExit);
            }
        }
    }
}
