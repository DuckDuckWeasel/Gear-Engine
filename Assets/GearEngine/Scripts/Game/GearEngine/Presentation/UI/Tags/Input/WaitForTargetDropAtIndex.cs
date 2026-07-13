using System;
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
    [Serializable]
    public class DropTargetConfig
    {
        public TagSO tagSO;
        public List<int> allowedNodeIndices = new List<int>();
    }

    [CommandInfo("Input", "Wait For Target Drop At Index", "Waits for dropping an object with the specified target tags node.")]
    [AddComponentMenu("")]
    public class WaitForTargetDropAtIndex : Command
    {
        [Inject] private IInputFilterService _inputService;
        [Inject] private IEventBus _eventBus;

        [Header("Drag-side tags")]
        [SerializeField]
        private List<TagSO> dragTargetTagSOList = new List<TagSO>();

        [Header("Drop-side configurations")]
        [SerializeField]
        private List<DropTargetConfig> dropConfigs = new List<DropTargetConfig>();
        
        [SerializeField]
        private bool matchAll = false;
        [SerializeField]
        private bool checkGameObject = false;

        private bool isTargetDropped;

        public override void OnEnter()
        {
            if (dragTargetTagSOList == null || dragTargetTagSOList.Count == 0 || dropConfigs == null || dropConfigs.Count == 0)
            {
                Debug.LogError($"[WaitForTargetDropAtIndex] Invalid configuration.");
                Finish(false);
                return;
            }

            _inputService.FilterForButtonDownTags(matchAll, dragTargetTagSOList.ToArray());
            _inputService.FilterForDropEnterTags(matchAll, checkGameObject, 
                                               dropConfigs.ConvertAll(dc => dc.tagSO).ToArray());
            _inputService.FilterForPointerEnterTags(matchAll, dragTargetTagSOList.ToArray());

            _eventBus.AddListener<ScreenDroppedEvent>(OnDrop);
            _eventBus.AddListener<ScreenPointerExitEvent>(OnPointerExit);

            isTargetDropped = false;
            StartCoroutine(WaitForDrop());
        }

        private void OnDrop(ScreenDroppedEvent signal)
        {
            GameObject go = signal.DropTopResult;
            if (go == null)
            {
                Finish(false);
                return;
            }

            if (!go.TryGetComponent<TagComponent>(out TagComponent tagComp))
            {
                Finish(false);
                return;
            }

            foreach (DropTargetConfig dropConfig in dropConfigs)
            {
                if (!tagComp.ContainsTag(new[] { dropConfig.tagSO }, matchAll))
                    continue;

                List<TagComponent> allTagged = GetAllTaggedObjects(dropConfig.tagSO);
                int indexToCheck = allTagged.IndexOf(tagComp);
                if (indexToCheck >= 0 && dropConfig.allowedNodeIndices.Contains(indexToCheck))
                {
                    isTargetDropped = true;
                    Finish(true);
                    return;
                }
                else
                {
                    Finish(false);
                    return;
                }
            }

            Finish(false);
        }

        private void OnPointerExit(ScreenPointerExitEvent _)
        {
            Finish(false);
        }

        private IEnumerator WaitForDrop()
        {
            yield return new WaitUntil(() => isTargetDropped);
        }

        private void Finish(bool success)
        {
            if (_eventBus != null)
            {
                _eventBus.RemoveListener<ScreenDroppedEvent>(OnDrop);
                _eventBus.RemoveListener<ScreenPointerExitEvent>(OnPointerExit);
            }

            _inputService.ClearAllFilters();

            if (success)
            {
                Continue();
            }
            else
            {
                StopAllCoroutines();
            }
        }

        private List<TagComponent> GetAllTaggedObjects(TagSO tagSO)
        {
            TagComponent[] allComponents = FindObjectsOfType<TagComponent>();
            List<TagComponent> list = new List<TagComponent>();
            foreach (TagComponent comp in allComponents)
            {
                if (comp.HasTag(tagSO))
                {
                    list.Add(comp);
                }
            }
            // Sort by sibling index as a fallback for deterministic indexing
            list.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            return list;
        }
    }
}
