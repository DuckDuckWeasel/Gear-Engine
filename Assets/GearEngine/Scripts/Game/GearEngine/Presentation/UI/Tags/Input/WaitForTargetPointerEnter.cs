using System.Collections;
using System.Collections.Generic;
using GearEngine.GearEngine.Presentation.UI.Tags;
using Fungus;
using UnityEngine;
using UnityEngine.EventSystems;
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
        private List<EventTrigger> addedTriggers = new List<EventTrigger>();
        private List<EventTrigger.Entry> addedEntries = new List<EventTrigger.Entry>();

        public override void OnEnter()
        {
            if (targetTagSOList == null || targetTagSOList.Count == 0)
            {
                Debug.LogError($"[WaitForTargetPointerEnter] targetTagSOList is null or empty. Method: {nameof(OnEnter)}");
                Continue();
                return;
            }

            isTargetPointered = false;

            // Find all GameObjects that have matching tags
            TagComponent[] allComponents = FindObjectsOfType<TagComponent>();
            bool foundTarget = false;

            foreach (TagComponent comp in allComponents)
            {
                if (comp.ContainsTag(targetTagSOList.ToArray(), matchAll))
                {
                    AttachPointerEnterTrigger(comp.gameObject);
                    foundTarget = true;
                }
            }

            if (!foundTarget)
            {
                Debug.LogWarning($"[WaitForTargetPointerEnter] No GameObjects found with matching tags.");
                Continue();
                return;
            }

            StartCoroutine(WaitForTargetPointerEnterCoroutine());
        }

        private void AttachPointerEnterTrigger(GameObject target)
        {
            // If target is a 3D object (has Collider but no Graphic), ensure Camera has PhysicsRaycaster
            if (target.GetComponent<Collider>() != null && target.GetComponent<UnityEngine.UI.Graphic>() == null)
            {
                if (Camera.main != null && Camera.main.GetComponent<UnityEngine.EventSystems.PhysicsRaycaster>() == null)
                {
                    Camera.main.gameObject.AddComponent<UnityEngine.EventSystems.PhysicsRaycaster>();
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
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { isTargetPointered = true; });
            trigger.triggers.Add(entry);

            if (wasAdded)
            {
                addedTriggers.Add(trigger);
            }
            addedEntries.Add(entry);
        }

        private IEnumerator WaitForTargetPointerEnterCoroutine()
        {
            yield return new WaitUntil(() => isTargetPointered);

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
