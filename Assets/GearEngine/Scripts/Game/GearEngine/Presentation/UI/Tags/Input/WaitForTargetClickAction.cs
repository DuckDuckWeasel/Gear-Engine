using System;
using System.Collections;
using System.Collections.Generic;
using GearEngine.Core.Actions;
using GearEngine.Core.Architecture.References;
using GearEngine.GearEngine.Presentation.UI.Tags;
using GearEngine.GearEngine.Presentation.UI.Input;
using Scaffold;
using Scaffold.Input.Events;
using UnityEngine;

namespace GearEngine.Actions.Input
{
    [CommandInfo("Input", "Wait For Target Click", "Waits until a specific target is clicked.")]
    [Serializable]
    public class WaitForTargetClickAction : WaitForInputActionBase
    {
        public TargetReference target = new TargetReference();

        private readonly List<TargetClickRelay> clickRelays = new List<TargetClickRelay>();
        private bool isTargetClicked;
        private Coroutine waitCoroutine;

        public override void OnEnter()
        {
            isTargetClicked = false;
            if (target == null)
            {
                Debug.LogError("[WaitForTargetClickAction] Target reference is missing.");
                Fail();
                return;
            }

            InitializeInputService();
            RegisterTargetRelays();

            _inputService.FilterForButtonDownTarget(target);
            _inputService.FilterForButtonUpTarget(target);

            _eventBus.AddListener<ScreenClickedEvent>(OnClicked);

            waitCoroutine = hostCommand.StartCoroutine(WaitForTargetClickCoroutine());
        }

        private void OnClicked(ScreenClickedEvent signal)
        {
            if (signal.TopResult == null || signal.TopResult.transform == null)
            {
                return;
            }

            GameObject clickedObj = signal.TopResult.gameObject;

            Transform current = clickedObj.transform;
            while (current != null)
            {
                if (target.IsMatch(current.gameObject))
                {
                    isTargetClicked = true;
                    return;
                }

                current = current.parent;
            }
        }

        private void OnTargetClicked()
        {
            isTargetClicked = true;
        }

        private IEnumerator WaitForTargetClickCoroutine()
        {
            while (!isTargetClicked)
            {
                TickFallbackIfNeeded();
                yield return null;
            }

            waitCoroutine = null;
            Cleanup();
            Continue();
        }

        private void RegisterTargetRelays()
        {
            clickRelays.Clear();
            HashSet<GameObject> resolvedTargets = new HashSet<GameObject>();

            if (target.strategy == TargetResolutionStrategy.Tags)
            {
                foreach (TagComponent tagComponent in TagComponent.Instances)
                {
                    if (tagComponent != null && target.IsMatch(tagComponent.gameObject))
                    {
                        resolvedTargets.Add(tagComponent.gameObject);
                    }
                }
            }
            else
            {
                foreach (GameObject resolvedTarget in target.ResolveAll())
                {
                    if (resolvedTarget != null)
                    {
                        resolvedTargets.Add(resolvedTarget);
                    }
                }
            }

            foreach (GameObject resolvedTarget in resolvedTargets)
            {
                TargetClickRelay relay = resolvedTarget.GetComponent<TargetClickRelay>();
                if (relay == null)
                {
                    relay = resolvedTarget.AddComponent<TargetClickRelay>();
                }

                relay.AddListener(OnTargetClicked);
                clickRelays.Add(relay);
            }
        }

        private void Cleanup()
        {
            foreach (TargetClickRelay relay in clickRelays)
            {
                if (relay != null)
                {
                    relay.RemoveListener(OnTargetClicked);
                }
            }

            clickRelays.Clear();

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

        public override void OnStopExecuting()
        {
            if (hostCommand != null && waitCoroutine != null)
            {
                hostCommand.StopCoroutine(waitCoroutine);
                waitCoroutine = null;
            }

            Cleanup();
        }

        public override string GetSummary()
        {
            if (target == null)
            {
                return "Error: No Target";
            }

            return target.GetSummary();
        }
    }
}
