using System;
using System.Collections.Generic;
using System.Linq;
using Scaffold.Events.Contracts;
using Scaffold.Input.Contracts;
using Scaffold.Input.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using VContainer.Unity;

namespace Scaffold.Input
{
    public class InputFilterService : ITickable, IInputFilterService
    {
        private readonly IEventBus _eventBus;

        private readonly List<Func<GameObject, bool>> _pointerEnterFilters = new List<Func<GameObject, bool>>();
        private readonly List<Func<GameObject, bool>> _buttonDownFilters = new List<Func<GameObject, bool>>();
        private readonly List<Func<GameObject, bool>> _buttonUpFilters = new List<Func<GameObject, bool>>();

        private readonly List<GameObject> _pointerEnterHitList = new List<GameObject>();
        private readonly List<GameObject> _pointerExitHitList = new List<GameObject>();
        private readonly List<GameObject> _buttonDownHitList = new List<GameObject>();
        private readonly List<GameObject> _buttonUpHitList = new List<GameObject>();
        private readonly List<GameObject> _previousHoveredObjects = new List<GameObject>();

        private bool _ticked = false;
        private bool _inputBlocked = false;
        private bool _checkDroppedGameObject = false;

        [Inject]
        public InputFilterService(IEventBus eventBus)
        {
            _eventBus = eventBus;
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
            if (_pointerEnterHitList.Count <= 0) return;

            _eventBus.Raise(new ScreenPointerEnterEvent(_pointerEnterHitList, _pointerEnterHitList.FirstOrDefault()));
            _pointerEnterHitList.Clear();
        }

        private void PointerExitHandler()
        {
            if (_pointerExitHitList.Count <= 0) return;

            _eventBus.Raise(new ScreenPointerExitEvent(_pointerExitHitList, _pointerExitHitList.FirstOrDefault()));
            _pointerExitHitList.Clear();
        }

        private void DropHandler()
        {
            if (!_ticked) return;

            if (_buttonDownHitList.Count > 0 && _buttonUpHitList.Count > 0)
            {
                // Without the custom TagComponent logic, we just send all down/up matches 
                // and let the external listeners figure out if it was a valid drag/drop.
                // The exact Data2073 drop validation is delegated to the specific filters now.
                _eventBus.Raise(new ScreenDroppedEvent(_buttonUpHitList, _buttonUpHitList.FirstOrDefault(), _buttonDownHitList, _buttonDownHitList.FirstOrDefault()));
            }

            _buttonDownHitList.Clear();
        }

        private void ClickHandler()
        {
            if (!_ticked) return;

            _ticked = false;

            if (_buttonUpHitList.Count > 0)
            {
                _eventBus.Raise(new ScreenClickedEvent(_buttonUpHitList, _buttonUpHitList.FirstOrDefault()));
            }
            
            _buttonUpHitList.Clear();
        }

        public void ToggleInput(bool toggle)
        {
            _inputBlocked = !toggle;
        }

        public void FilterRaycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (_inputBlocked)
            {
                resultAppendList.Clear();
                return;
            }

            // Combine UI hits with 3D physics hits (if any)
            IEnumerable<GameObject> physicsHits = GetPhysicsHits();

            if (_pointerEnterFilters.Count > 0)
            {
                _pointerEnterHitList.AddRange(physicsHits);
                _pointerEnterHitList.AddRange(resultAppendList.Select(x => x.gameObject));

                _pointerEnterHitList.RemoveAll(hit => !FilterRaycastResult(hit, _pointerEnterFilters));
                resultAppendList.RemoveAll(result => !_pointerEnterHitList.Contains(result.gameObject));
            }

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _buttonDownHitList.AddRange(resultAppendList.Select(x => x.gameObject));
                _buttonDownHitList.AddRange(physicsHits);

                if (_buttonDownFilters.Count > 0)
                {
                    _buttonDownHitList.RemoveAll(hit => !FilterRaycastResult(hit, _buttonDownFilters));
                    resultAppendList.RemoveAll(result => !_buttonDownHitList.Contains(result.gameObject));
                }
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                _buttonUpHitList.AddRange(resultAppendList.Select(x => x.gameObject));
                _buttonUpHitList.AddRange(physicsHits);

                if (_buttonUpFilters.Count > 0)
                {
                    _buttonUpHitList.RemoveAll(hit => !FilterRaycastResult(hit, _buttonUpFilters));
                    resultAppendList.RemoveAll(result => !_buttonUpHitList.Contains(result.gameObject));
                }

                _ticked = true;
            }
        }

        public void IndividualFilterRaycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (_pointerEnterFilters.Count > 0)
            {
                _pointerExitHitList.AddRange(_previousHoveredObjects.Except(_pointerEnterHitList));
                _previousHoveredObjects.Clear();
                _previousHoveredObjects.AddRange(_pointerEnterHitList);
            }
        }

        public void SetPointerEnterRaycastFilter(Func<GameObject, bool> predicate, bool removeOtherFilters = false)
        {
            SetRaycastFilter(predicate, _pointerEnterFilters, removeOtherFilters);
        }

        public void SetButtonUpRaycastFilter(Func<GameObject, bool> predicate, bool removeOtherFilters = false)
        {
            SetRaycastFilter(predicate, _buttonUpFilters, removeOtherFilters);
        }

        public void SetDropRaycastFilter(Func<GameObject, bool> predicate, bool checkDroppedGameObject, bool removeOtherFilters = false)
        {
            _checkDroppedGameObject = checkDroppedGameObject;
            SetRaycastFilter(predicate, _buttonUpFilters, removeOtherFilters);
        }

        public void SetButtonDownRaycastFilter(Func<GameObject, bool> predicate, bool removeOtherFilters = false)
        {
            SetRaycastFilter(predicate, _buttonDownFilters, removeOtherFilters);
        }

        public void ClearPointerEnterFilters()
        {
            _pointerEnterFilters.Clear();
        }

        public void ClearButtonUpFilters()
        {
            _buttonUpFilters.Clear();
        }

        public void ClearButtonDownFilters()
        {
            _buttonDownFilters.Clear();
        }

        public void ClearAllFilters()
        {
            _pointerEnterFilters.Clear();
            _buttonUpFilters.Clear();
            _buttonDownFilters.Clear();
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
    }
}
