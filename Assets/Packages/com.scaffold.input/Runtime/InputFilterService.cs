using System;
using System.Collections.Generic;
using System.Linq;
using Scaffold.Events.Contracts;
using Scaffold.Input.Contracts;
using Scaffold.Input.Events;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using VContainer;
using VContainer.Unity;

namespace Scaffold.Input
{
    public class InputFilterService : ITickable, IInputFilterService
    {
        private static IEventBus s_globalEventBus;

        public static IInputFilterService GlobalFallback { get; private set; }

        private readonly IEventBus eventBus;

        private readonly List<Func<GameObject, bool>> pointerEnterFilters = new List<Func<GameObject, bool>>();
        private readonly List<Func<GameObject, bool>> buttonDownFilters = new List<Func<GameObject, bool>>();
        private readonly List<Func<GameObject, bool>> buttonUpFilters = new List<Func<GameObject, bool>>();

        private readonly List<GameObject> pointerEnterHitList = new List<GameObject>();
        private readonly List<GameObject> pointerExitHitList = new List<GameObject>();
        private readonly List<GameObject> buttonDownHitList = new List<GameObject>();
        private readonly List<GameObject> buttonUpHitList = new List<GameObject>();
        private readonly List<GameObject> previousHoveredObjects = new List<GameObject>();

        private bool ticked = false;
        private bool inputBlocked = false;
        private bool checkDroppedGameObject = false;

        [Inject]
        public InputFilterService(IEventBus eventBus)
        {
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            GlobalFallback = this;
            s_globalEventBus = eventBus;
        }

        public static bool TryGetGlobalContext(
            out IInputFilterService inputService,
            out IEventBus eventBus)
        {
            inputService = GlobalFallback;
            eventBus = s_globalEventBus;
            return inputService != null && eventBus != null;
        }

        public void Tick()
        {
            PointerEnterHandler();
            DropHandler();
            ClickHandler();
            PointerExitHandler();
        }

        private void PointerEnterHandler()
        {
            if (pointerEnterHitList.Count <= 0)
            {
                return;
            }

            eventBus.Raise(new ScreenPointerEnterEvent(pointerEnterHitList, pointerEnterHitList.FirstOrDefault()));
            pointerEnterHitList.Clear();
        }

        private void PointerExitHandler()
        {
            if (pointerExitHitList.Count <= 0)
            {
                return;
            }

            eventBus.Raise(new ScreenPointerExitEvent(pointerExitHitList, pointerExitHitList.FirstOrDefault()));
            pointerExitHitList.Clear();
        }

        private void DropHandler()
        {
            if (!ticked)
            {
                return;
            }

            if (buttonDownHitList.Count > 0 && buttonUpHitList.Count > 0)
            {
                // Without the custom TagComponent logic, we just send all down/up matches 
                // and let the external listeners figure out if it was a valid drag/drop.
                // The exact Data2073 drop validation is delegated to the specific filters now.
                eventBus.Raise(new ScreenDroppedEvent(buttonUpHitList, buttonUpHitList.FirstOrDefault(), buttonDownHitList, buttonDownHitList.FirstOrDefault()));
            }

            buttonDownHitList.Clear();
        }

        private void ClickHandler()
        {
            if (!ticked)
            {
                return;
            }

            ticked = false;

            if (buttonUpHitList.Count > 0)
            {
                eventBus.Raise(new ScreenClickedEvent(buttonUpHitList, buttonUpHitList.FirstOrDefault()));
            }

            buttonUpHitList.Clear();
        }

        public void ToggleInput(bool toggle)
        {
            inputBlocked = !toggle;
        }

        public void FilterRaycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (inputBlocked)
            {
                resultAppendList.Clear();
                return;
            }

            // Combine UI hits with 3D physics hits (if any)
            IEnumerable<GameObject> physicsHits = GetPhysicsHits();

            if (pointerEnterFilters.Count > 0)
            {
                pointerEnterHitList.AddRange(physicsHits);
                pointerEnterHitList.AddRange(resultAppendList.Select(x => x.gameObject));

                pointerEnterHitList.RemoveAll(hit => !FilterRaycastResult(hit, pointerEnterFilters));
                resultAppendList.RemoveAll(result => !pointerEnterHitList.Contains(result.gameObject));
            }

            if (IsPrimaryButtonPressedThisFrame())
            {
                buttonDownHitList.AddRange(resultAppendList.Select(x => x.gameObject));
                buttonDownHitList.AddRange(physicsHits);

                if (buttonDownFilters.Count > 0)
                {
                    buttonDownHitList.RemoveAll(hit => !FilterRaycastResult(hit, buttonDownFilters));
                    resultAppendList.RemoveAll(result => !buttonDownHitList.Contains(result.gameObject));
                }
            }

            if (IsPrimaryButtonReleasedThisFrame())
            {
                buttonUpHitList.AddRange(resultAppendList.Select(x => x.gameObject));
                buttonUpHitList.AddRange(physicsHits);

                if (buttonUpFilters.Count > 0)
                {
                    buttonUpHitList.RemoveAll(hit => !FilterRaycastResult(hit, buttonUpFilters));
                    resultAppendList.RemoveAll(result => !buttonUpHitList.Contains(result.gameObject));
                }

                ticked = true;
            }
        }

        public void IndividualFilterRaycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (pointerEnterFilters.Count > 0)
            {
                pointerExitHitList.AddRange(previousHoveredObjects.Except(pointerEnterHitList));
                previousHoveredObjects.Clear();
                previousHoveredObjects.AddRange(pointerEnterHitList);
            }
        }

        public void SetPointerEnterRaycastFilter(Func<GameObject, bool> predicate, bool removeOtherFilters = false)
        {
            SetRaycastFilter(predicate, pointerEnterFilters, removeOtherFilters);
        }

        public void SetButtonUpRaycastFilter(Func<GameObject, bool> predicate, bool removeOtherFilters = false)
        {
            SetRaycastFilter(predicate, buttonUpFilters, removeOtherFilters);
        }

        public void SetDropRaycastFilter(Func<GameObject, bool> predicate, bool checkDroppedGameObject, bool removeOtherFilters = false)
        {
            this.checkDroppedGameObject = checkDroppedGameObject;
            SetRaycastFilter(predicate, buttonUpFilters, removeOtherFilters);
        }

        public void SetButtonDownRaycastFilter(Func<GameObject, bool> predicate, bool removeOtherFilters = false)
        {
            SetRaycastFilter(predicate, buttonDownFilters, removeOtherFilters);
        }

        public void ClearPointerEnterFilters()
        {
            pointerEnterFilters.Clear();
        }

        public void ClearButtonUpFilters()
        {
            buttonUpFilters.Clear();
        }

        public void ClearButtonDownFilters()
        {
            buttonDownFilters.Clear();
        }

        public void ClearAllFilters()
        {
            pointerEnterFilters.Clear();
            buttonUpFilters.Clear();
            buttonDownFilters.Clear();
        }

        private void SetRaycastFilter(Func<GameObject, bool> predicate, List<Func<GameObject, bool>> filters, bool removeOtherFilters = false)
        {
            if (removeOtherFilters)
            {
                filters.Clear();
            }
            filters.Add(predicate);
        }

        private bool FilterRaycastResult(GameObject hit, List<Func<GameObject, bool>> filters)
        {
            foreach (Func<GameObject, bool> filter in filters)
            {
                if (filter.Invoke(hit))
                {
                    return true;
                }
            }
            return false;
        }

        private IEnumerable<GameObject> GetPhysicsHits()
        {
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
                return Physics.RaycastAll(ray).Select(hit => hit.collider.gameObject);
            }
            return Enumerable.Empty<GameObject>();
        }

        private static bool IsPrimaryButtonPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if ((Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
                (Pen.current != null && Pen.current.tip.wasPressedThisFrame) ||
                (Touchscreen.current != null &&
                 Touchscreen.current.primaryTouch.press.wasPressedThisFrame))
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }

        private static bool IsPrimaryButtonReleasedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if ((Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame) ||
                (Pen.current != null && Pen.current.tip.wasReleasedThisFrame) ||
                (Touchscreen.current != null &&
                 Touchscreen.current.primaryTouch.press.wasReleasedThisFrame))
            {
                return true;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return UnityEngine.Input.GetMouseButtonUp(0);
#else
            return false;
#endif
        }
    }
}
