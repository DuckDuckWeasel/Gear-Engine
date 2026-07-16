using System.Collections;
using System.Collections.Generic;
using GearEngine.GearEngine.Presentation.UI.Tags;
using Fungus;
using UnityEngine;
using Scaffold.Input.Events;
using UnityEngine.EventSystems;
using GearEngine.Core.Architecture.References;
using Command = Fungus.Command;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    [CommandInfo("Input", "Wait For Target Click", "Waits for a click on a object with the specified target tags.")]
    [AddComponentMenu("")]
    public class WaitForTargetClick : Command
    {
        public TargetReference target = new TargetReference();

        // Legacy fields for migration
        [HideInInspector] public List<TagSO> targetTagSOList;
        [HideInInspector] public bool matchAll = false;
        [HideInInspector] [SerializeField] private bool migratedToTargetReference = false;

        protected virtual void OnEnable()
        {
            if (!migratedToTargetReference && targetTagSOList != null && targetTagSOList.Count > 0)
            {
                target.strategy = TargetResolutionStrategy.Tags;
                target.tagFilter.soTags = new List<TagSO>(targetTagSOList);
                target.tagFilter.matchAll = matchAll;
                migratedToTargetReference = true;
                targetTagSOList.Clear();
            }
        }

        private bool isTargetClicked = false;
        private List<EventTrigger> addedTriggers = new List<EventTrigger>();
        private List<EventTrigger.Entry> addedEntries = new List<EventTrigger.Entry>();

        public override void OnEnter()
        {
            isTargetClicked = false;

            bool foundTarget = false;

            if (target.strategy == TargetResolutionStrategy.Tags)
            {
                // Find all GameObjects that have matching tags
                TagComponent[] allComponents = FindObjectsOfType<TagComponent>();

                foreach (TagComponent comp in allComponents)
                {
                    if (target.IsMatch(comp.gameObject))
                    {
                        AttachPointerClickTrigger(comp.gameObject);
                        foundTarget = true;
                    }
                }
            }
            else
            {
                List<GameObject> resolvedTargets = target.ResolveAll();
                if (resolvedTargets != null && resolvedTargets.Count > 0)
                {
                    foreach (var resolvedTarget in resolvedTargets)
                    {
                        if (resolvedTarget != null)
                        {
                            AttachPointerClickTrigger(resolvedTarget);
                            foundTarget = true;
                        }
                    }
                }
            }

            if (!foundTarget)
            {
                Debug.LogWarning($"[WaitForTargetClick] No valid targets found for the specified TargetReference.");
                Continue();
                return;
            }

            StartCoroutine(WaitForTargetClickCoroutine());
        }

        private void AttachPointerClickTrigger(GameObject target)
        {
            // If target is a 3D object (has Collider but no Graphic), ensure Camera has PhysicsRaycaster
            if (target.GetComponent<Collider>() != null && target.GetComponent<UnityEngine.UI.Graphic>() == null)
            {
                if (Camera.main != null && Camera.main.GetComponent<PhysicsRaycaster>() == null)
                {
                    Camera.main.gameObject.AddComponent<PhysicsRaycaster>();
                }
            }

            EventTrigger trigger = target.GetComponent<EventTrigger>();
            bool wasAdded = false;
            if (trigger == null)
            {
                trigger = target.AddComponent<EventTrigger>();
                wasAdded = true;
            }

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { isTargetClicked = true; });
            trigger.triggers.Add(entry);

            if (wasAdded)
            {
                addedTriggers.Add(trigger);
            }
            addedEntries.Add(entry);
        }

        private IEnumerator WaitForTargetClickCoroutine()
        {
            yield return new WaitUntil(() => isTargetClicked);

            CleanupTriggers();

            Continue();
        }

        public void OnDisable()
        {
            CleanupTriggers();
        }

        private void CleanupTriggers()
        {
            // Remove entries we added (from triggers that already existed)
            foreach (EventTrigger.Entry entry in addedEntries)
            {
                entry.callback.RemoveAllListeners();
            }
            addedEntries.Clear();

            // Destroy triggers we created
            foreach (EventTrigger trigger in addedTriggers)
            {
                if (trigger != null)
                {
                    Destroy(trigger);
                }
            }
            addedTriggers.Clear();
        }

        public override string GetSummary()
        {
            if (target == null) return "Error: No Target";
            return target.GetSummary();
        }
    }
}
