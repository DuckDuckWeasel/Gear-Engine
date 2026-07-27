using GearEngine.Actions.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Tests.Editor
{
    public class TargetClickRelayTests
    {
        private GameObject eventSystemObject;
        private GameObject targetObject;

        [TearDown]
        public void TearDown()
        {
            if (targetObject != null)
            {
                Object.DestroyImmediate(targetObject);
            }

            if (eventSystemObject != null)
            {
                Object.DestroyImmediate(eventSystemObject);
            }
        }

        [Test]
        public void PointerDown_NotifiesRegisteredListener()
        {
            eventSystemObject = new GameObject("EventSystem");
            EventSystem eventSystem = eventSystemObject.AddComponent<EventSystem>();
            targetObject = new GameObject("Target");
            TargetClickRelay relay = targetObject.AddComponent<TargetClickRelay>();
            bool wasClicked = false;
            relay.AddListener(() => wasClicked = true);

            ExecuteEvents.Execute(
                targetObject,
                new PointerEventData(eventSystem),
                ExecuteEvents.pointerDownHandler);

            Assert.That(wasClicked, Is.True);
        }
    }
}
