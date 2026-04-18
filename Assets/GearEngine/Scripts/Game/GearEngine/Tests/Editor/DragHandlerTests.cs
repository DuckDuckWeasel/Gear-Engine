using GearEngine.GearEngine.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class DragHandlerTests
    {
        private GameObject _host;

        [TearDown]
        public void TearDown()
        {
            if (_host != null)
            {
                Object.DestroyImmediate(_host);
                _host = null;
            }
        }

        [Test]
        public void GhostUniformScaleResolver_InvokedOnBeginDragAndOnDrag()
        {
            _host = new GameObject("DragHost", typeof(DragHandler));
            new GameObject("Canvas", typeof(Canvas)).transform.SetParent(_host.transform);
            var ghostTemplate = new GameObject("GhostTemplate");

            var drag = _host.GetComponent<DragHandler>();
            drag.GhostPrefab = ghostTemplate;
            int calls = 0;
            drag.GhostUniformScaleResolver = _ => { calls++; return 2f; };

            var ped = new PointerEventData(EventSystem.current) { position = Vector2.zero };
            drag.OnBeginDrag(ped);
            Assert.GreaterOrEqual(calls, 1);
            int afterBegin = calls;
            drag.OnDrag(ped);
            Assert.Greater(calls, afterBegin);
        }
    }
}
