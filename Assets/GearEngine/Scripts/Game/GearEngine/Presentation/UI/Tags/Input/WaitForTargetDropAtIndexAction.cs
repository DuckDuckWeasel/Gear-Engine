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

namespace GearEngine.Actions.Input
{
    [CommandInfo("Input", "Wait For Target Drop At Index", "Waits until a drag target is dropped onto a specific slot index.")]
    [Serializable]
    public class WaitForTargetDropAtIndexAction : WaitForInputActionBase
    {

        [Header("Drag-side Target")]
        public TargetReference DragTarget = new TargetReference();

        [Header("Drop-side configurations")]
        [SerializeField]
        private List<DropTargetConfig> dropConfigs = new List<DropTargetConfig>();

        [SerializeField]
        private bool checkGameObject = false;

        private bool isTargetDropped;
        [NonSerialized] private IDisposable waitHandle;

        public override void OnEnter()
        {
            if (!ValidateConfiguration())
            {
                return;
            }

            InitializeInputService();
            ConfigureFilters();
            Subscribe();
            isTargetDropped = false;
            waitHandle = RunRoutine(WaitForDrop());
        }

        private bool ValidateConfiguration()
        {
            if (dropConfigs != null && dropConfigs.Count > 0)
            {
                return true;
            }

            Debug.LogError("[WaitForTargetDropAtIndex] Invalid configuration.");
            Finish(false);
            return false;
        }

        private void ConfigureFilters()
        {
            _inputService.FilterForButtonDownTarget(DragTarget);
            _inputService.FilterForPointerEnterTarget(DragTarget);
            List<TargetReference> dropTargets = new List<TargetReference>();
            foreach (DropTargetConfig config in dropConfigs)
            {
                dropTargets.Add(config.Target);
            }
            _inputService.FilterForDropEnterTargets(checkGameObject, dropTargets);
        }

        private void Subscribe()
        {
            _eventBus.AddListener<ScreenDroppedEvent>(OnDrop);
            _eventBus.AddListener<ScreenPointerExitEvent>(OnPointerExit);
        }

        private void OnDrop(ScreenDroppedEvent signal)
        {
            GameObject droppedObject = signal.DropTopResult;
            if (droppedObject == null)
            {
                Finish(false);
                return;
            }

            DropTargetConfig dropConfig = FindMatchingConfig(droppedObject);
            if (dropConfig == null)
            {
                Finish(false);
                return;
            }

            CompleteDrop(dropConfig, droppedObject);
        }

        private void CompleteDrop(DropTargetConfig dropConfig, GameObject droppedObject)
        {
            List<GameObject> allMatching = GetAllMatchingObjects(dropConfig.Target);
            int index = ResolveDroppedIndex(droppedObject, allMatching);
            bool success = index >= 0 && dropConfig.AllowedNodeIndices.Contains(index);
            isTargetDropped = success;
            Finish(success);
        }

        private DropTargetConfig FindMatchingConfig(GameObject droppedObject)
        {
            foreach (DropTargetConfig dropConfig in dropConfigs)
            {
                if (IsTargetMatch(dropConfig.Target, droppedObject))
                {
                    return dropConfig;
                }
            }

            return null;
        }

        private bool IsTargetMatch(TargetReference targetReference, GameObject droppedObject)
        {
            if (targetReference.IsMatch(droppedObject))
            {
                return true;
            }

            TagComponent parentTag = droppedObject.GetComponentInParent<TagComponent>();
            return parentTag != null && targetReference.IsMatch(parentTag.gameObject);
        }

        private int ResolveDroppedIndex(GameObject droppedObject, List<GameObject> allMatching)
        {
            int index = allMatching.IndexOf(droppedObject);
            if (index >= 0)
            {
                return index;
            }

            TagComponent parentTag = droppedObject.GetComponentInParent<TagComponent>();
            return parentTag == null ? -1 : allMatching.IndexOf(parentTag.gameObject);
        }

        private void OnPointerExit(ScreenPointerExitEvent _)
        {
            Finish(false);
        }

        private IEnumerator WaitForDrop()
        {
            while (!isTargetDropped)
            {
                TickFallbackIfNeeded();
                yield return null;
            }
        }

        private void Finish(bool success)
        {
            Cleanup();
            waitHandle?.Dispose();
            waitHandle = null;

            if (success)
            {
                Continue();
            }
            else
            {
                Fail();
            }
        }

        public override void OnStopExecuting()
        {
            waitHandle?.Dispose();
            waitHandle = null;
            Cleanup();
            base.OnStopExecuting();
        }

        private void Cleanup()
        {
            if (_eventBus != null)
            {
                _eventBus.RemoveListener<ScreenDroppedEvent>(OnDrop);
                _eventBus.RemoveListener<ScreenPointerExitEvent>(OnPointerExit);
            }

            _inputService?.ClearAllFilters();
        }

        private List<GameObject> GetAllMatchingObjects(TargetReference targetRef)
        {
            List<GameObject> list = new List<GameObject>();
            if (targetRef.strategy == TargetResolutionStrategy.Tags)
            {
                AddTaggedObjects(targetRef, list);
            }
            else
            {
                AddResolvedObjects(targetRef, list);
            }

            list.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            return list;
        }

        private void AddTaggedObjects(TargetReference targetRef, List<GameObject> list)
        {
            TagComponent[] allComponents = UnityEngine.Object.FindObjectsOfType<TagComponent>();
            foreach (TagComponent component in allComponents)
            {
                if (targetRef.IsMatch(component.gameObject))
                {
                    list.Add(component.gameObject);
                }
            }
        }

        private void AddResolvedObjects(TargetReference targetRef, List<GameObject> list)
        {
            List<GameObject> resolvedTargets = targetRef.ResolveAll();
            if (resolvedTargets != null)
            {
                list.AddRange(resolvedTargets);
            }
        }

        public override string GetSummary()
        {
            if (DragTarget == null)
            {
                return "Error: No Drag Target";
            }

            int count = dropConfigs != null ? dropConfigs.Count : 0;
            return $"{DragTarget.GetSummary()} -> [{count} Slots]";
        }
    }
}
