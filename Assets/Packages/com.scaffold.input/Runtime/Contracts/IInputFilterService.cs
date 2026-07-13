using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Scaffold.Input.Contracts
{
    public interface IInputFilterService
    {
        void ToggleInput(bool toggle);
        void FilterRaycast(PointerEventData eventData, List<RaycastResult> resultAppendList);
        void IndividualFilterRaycast(PointerEventData eventData, List<RaycastResult> resultAppendList);
        
        void SetPointerEnterRaycastFilter(Func<GameObject, bool> predicate, bool removeOtherFilters = false);
        void SetButtonUpRaycastFilter(Func<GameObject, bool> predicate, bool removeOtherFilters = false);
        void SetButtonDownRaycastFilter(Func<GameObject, bool> predicate, bool removeOtherFilters = false);
        void SetDropRaycastFilter(Func<GameObject, bool> predicate, bool checkDroppedGameObject, bool removeOtherFilters = false);
        
        void ClearPointerEnterFilters();
        void ClearButtonUpFilters();
        void ClearButtonDownFilters();
        void ClearAllFilters();
    }
}
