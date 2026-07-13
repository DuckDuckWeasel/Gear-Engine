using System.Collections.Generic;
using Scaffold.Input.Contracts;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace Scaffold.Input
{
    public class FilteredGraphicRaycaster : GraphicRaycaster
    {
        private IInputFilterService _inputFilterService;
        
        [SerializeField] private bool useIndividualFilterRaycast = false;

        [Inject]
        public void Construct(IInputFilterService inputFilterService)
        {
            _inputFilterService = inputFilterService;
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            base.Raycast(eventData, resultAppendList);

            if (_inputFilterService != null)
            {
                _inputFilterService.FilterRaycast(eventData, resultAppendList);

                if (useIndividualFilterRaycast)
                {
                    _inputFilterService.IndividualFilterRaycast(eventData, resultAppendList);
                }
            }
        }
    }
}
