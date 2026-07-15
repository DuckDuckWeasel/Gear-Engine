using System.Collections;
using System.Collections.Generic;
using GearEngine.GearEngine.Presentation.UI.Tags;
using Fungus;
using UnityEngine;
using UnityEngine.EventSystems;
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
        private List<EventTrigger> addedTriggers = new List<EventTrigger>();
        private List<EventTrigger.Entry> addedEntries = new List<EventTrigger.Entry>();

        public override void OnEnter()
        {
            if (targetTagSOList == null || targetTagSOList.Count == 0)
            {
                Debug.LogError($"[WaitForTargetClick] targetTagSOList is null or empty. Method: {nameof(OnEnter)}");
                Continue();
                return;
            }

            isTargetClicked = false;

            // Find all GameObjects that have matching tags
            TagComponent[] allComponents = FindObjectsOfType<TagComponent>();
            bool foundTarget = false;

            foreach (TagComponent comp in allComponents)
            {
                if (comp.ContainsTag(targetTagSOList.ToArray(), matchAll))
                {
                    AttachPointerClickTrigger(comp.gameObject);
                    foundTarget = true;
                }
            }

            if (!foundTarget)
            {
                Debug.LogWarning($"[WaitForTargetClick] No GameObjects found with matching tags.");
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
    }
}
