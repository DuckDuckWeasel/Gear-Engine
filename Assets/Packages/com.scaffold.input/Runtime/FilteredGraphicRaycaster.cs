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

        protected override void Awake()
        {
            base.Awake();
            if (_inputFilterService == null)
            {
                TryInject();
                if (_inputFilterService == null && InputFilterService.GlobalFallback != null)
                {
                    _inputFilterService = InputFilterService.GlobalFallback;
                }
            }
        }

        protected override void Start()
        {
            base.Start();
            if (_inputFilterService == null)
            {
                TryInject();
                if (_inputFilterService == null && InputFilterService.GlobalFallback != null)
                {
                    _inputFilterService = InputFilterService.GlobalFallback;
                }
            }
        }

        private void TryInject()
        {
            VContainer.Unity.LifetimeScope scope = Object.FindObjectOfType<VContainer.Unity.LifetimeScope>();
            if (scope != null && scope.Container != null)
            {
                try
                {
                    scope.Container.Inject(this);
                }
                catch
                {
                    // Ignore injection exceptions
                }
            }
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            base.Raycast(eventData, resultAppendList);

            if (_inputFilterService == null && InputFilterService.GlobalFallback != null)
            {
                _inputFilterService = InputFilterService.GlobalFallback;
            }

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
