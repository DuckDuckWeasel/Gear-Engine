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
        public TargetReference Target = new TargetReference();

        private readonly List<TargetClickRelay> clickRelays = new List<TargetClickRelay>();
        private bool isTargetClicked;
        [NonSerialized] private IDisposable waitHandle;

        public override void OnEnter()
        {
            isTargetClicked = false;
            if (Target == null)
            {
                Debug.LogError("[WaitForTargetClickAction] Target reference is missing.");
                Fail();
                return;
            }

            InitializeInputService();
            RegisterTargetRelays();

            inputService.FilterForButtonDownTarget(Target, TargetResolver);
            inputService.FilterForButtonUpTarget(Target, TargetResolver);

            eventBus.AddListener<ScreenClickedEvent>(OnClicked);

            waitHandle = RunRoutine(WaitForTargetClickCoroutine());
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
                if (IsTargetMatch(Target, current.gameObject))
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

            waitHandle = null;
            Cleanup();
            Continue();
        }

        public override void OnStopExecuting()
        {
            waitHandle?.Dispose();
            waitHandle = null;
            Cleanup();
            base.OnStopExecuting();
        }

        private void RegisterTargetRelays()
        {
            clickRelays.Clear();
            HashSet<GameObject> resolvedTargets = ResolveTargets();
            foreach (GameObject resolvedTarget in resolvedTargets)
            {
                AttachRelay(resolvedTarget);
            }
        }

        private HashSet<GameObject> ResolveTargets()
        {
            HashSet<GameObject> resolvedTargets = new HashSet<GameObject>();
            if (Target.strategy == TargetResolutionStrategy.Tags)
            {
                AddTaggedTargets(resolvedTargets);
            }
            else
            {
                AddResolvedTargets(resolvedTargets);
            }

            return resolvedTargets;
        }

        private void AddTaggedTargets(HashSet<GameObject> resolvedTargets)
        {
            foreach (TagComponent tagComponent in TagComponent.Instances)
            {
                if (tagComponent != null &&
                    IsTargetMatch(Target, tagComponent.gameObject))
                {
                    resolvedTargets.Add(tagComponent.gameObject);
                }
            }
        }

        private void AddResolvedTargets(HashSet<GameObject> resolvedTargets)
        {
            foreach (GameObject resolvedTarget in ResolveTargets(Target))
            {
                if (resolvedTarget != null)
                {
                    resolvedTargets.Add(resolvedTarget);
                }
            }
        }

        private void AttachRelay(GameObject resolvedTarget)
        {
            TargetClickRelay relay = resolvedTarget.GetComponent<TargetClickRelay>();
            relay = relay != null ? relay : resolvedTarget.AddComponent<TargetClickRelay>();
            relay.AddListener(OnTargetClicked);
            clickRelays.Add(relay);
        }

        private void Cleanup()
        {
            RemoveRelayListeners();
            ClearInputSubscriptions();
        }

        private void RemoveRelayListeners()
        {
            foreach (TargetClickRelay relay in clickRelays)
            {
                if (relay != null)
                {
                    relay.RemoveListener(OnTargetClicked);
                }
            }

            clickRelays.Clear();
        }

        private void ClearInputSubscriptions()
        {
            if (eventBus != null)
            {
                eventBus.RemoveListener<ScreenClickedEvent>(OnClicked);
            }

            if (inputService != null)
            {
                inputService.ClearButtonDownFilters();
                inputService.ClearButtonUpFilters();
            }
        }

        public override string GetSummary()
        {
            if (Target == null)
            {
                return "Error: No Target";
            }

            return Target.GetSummary();
        }
    }
}
