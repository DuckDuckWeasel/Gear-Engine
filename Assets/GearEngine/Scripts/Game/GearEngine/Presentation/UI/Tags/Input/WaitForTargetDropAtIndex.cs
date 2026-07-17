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
using GearEngine.Core.Architecture.References;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Extensions;
using GearEngine.GearEngine.Presentation.UI.Input;

namespace GearEngine.GearEngine.Presentation.UI.Actions
{
    [Serializable]
    public class DropTargetConfig
    {
        public TargetReference target = new TargetReference();
        public List<int> allowedNodeIndices = new List<int>();
    }

    [Serializable]
    public class WaitForTargetDropAtIndexAction : ActionBase
    {
        [Inject] private IInputFilterService _inputService;
        [Inject] private IEventBus _eventBus;

        [Header("Drag-side Target")]
        public TargetReference dragTarget = new TargetReference();

        [Header("Drop-side configurations")]
        [SerializeField]
        private List<DropTargetConfig> dropConfigs = new List<DropTargetConfig>();
        
        [SerializeField]
        private bool checkGameObject = false;

        private bool isTargetDropped;
        public override void OnEnter()
        {
            
            if (dropConfigs == null || dropConfigs.Count == 0)
            {
                Debug.LogError($"[WaitForTargetDropAtIndex] Invalid configuration.");
                Finish(false);
                return;
            }

            if (_inputService == null || _eventBus == null)
            {
                this.TryInject();

                if (_eventBus == null) _eventBus = new Scaffold.Events.EventController();
                if (_inputService == null) _inputService = new Scaffold.Input.InputFilterService(_eventBus);
            }

            // Provide filtering for UI highlights/raycasts based on target references
            _inputService.FilterForButtonDownTarget(dragTarget);
            _inputService.FilterForPointerEnterTarget(dragTarget);

            List<TargetReference> dropTargetRefs = new List<TargetReference>();
            foreach (var cfg in dropConfigs)
            {
                dropTargetRefs.Add(cfg.target);
            }
            _inputService.FilterForDropEnterTargets(checkGameObject, dropTargetRefs);

            _eventBus.AddListener<ScreenDroppedEvent>(OnDrop);
            _eventBus.AddListener<ScreenPointerExitEvent>(OnPointerExit);

            isTargetDropped = false;
            hostCommand.StartCoroutine(WaitForDrop());
        }

        private void OnDrop(ScreenDroppedEvent signal)
        {
            GameObject go = signal.DropTopResult;
            if (go == null)
            {
                Finish(false);
                return;
            }

            foreach (DropTargetConfig dropConfig in dropConfigs)
            {
                if (!dropConfig.target.IsMatch(go))
                {
                    // Fallback to parents
                    TagComponent parentTagComp = go.GetComponentInParent<TagComponent>();
                    if (parentTagComp == null || !dropConfig.target.IsMatch(parentTagComp.gameObject))
                    {
                        continue;
                    }
                }

                List<GameObject> allMatching = GetAllMatchingObjects(dropConfig.target);
                int indexToCheck = allMatching.IndexOf(go);
                
                // Also check if index was from the parent component
                if (indexToCheck < 0)
                {
                    TagComponent parentTagComp = go.GetComponentInParent<TagComponent>();
                    if (parentTagComp != null)
                        indexToCheck = allMatching.IndexOf(parentTagComp.gameObject);
                }

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

            if (_inputService != null)
            {
                _inputService.ClearAllFilters();
            }

            if (success)
            {
                Continue();
            }
            else
            {
                hostCommand.StopAllCoroutines();
            }
        }

        private List<GameObject> GetAllMatchingObjects(TargetReference targetRef)
        {
            TagComponent[] allComponents = UnityEngine.Object.FindObjectsOfType<TagComponent>();
            List<GameObject> list = new List<GameObject>();

            if (targetRef.strategy == TargetResolutionStrategy.Tags)
            {
                foreach (TagComponent comp in allComponents)
                {
                    if (targetRef.IsMatch(comp.gameObject))
                    {
                        list.Add(comp.gameObject);
                    }
                }
            }
            else
            {
                List<GameObject> resolvedTargets = targetRef.ResolveAll();
                if (resolvedTargets != null)
                {
                    list.AddRange(resolvedTargets);
                }
            }

            // Sort by sibling index as a fallback for deterministic indexing
            list.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            return list;
        }

        public override string GetSummary()
        {
            if (dragTarget == null) return "Error: No Drag Target";
            int count = dropConfigs != null ? dropConfigs.Count : 0;
            return $"{dragTarget.GetSummary()} -> [{count} Slots]";
        }
    }
}
