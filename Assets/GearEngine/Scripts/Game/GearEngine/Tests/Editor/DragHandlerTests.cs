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
        public void OnBeginDrag_InvokesOnDragBeginWithSamePointerEventData()
        {
            _host = new GameObject("DragHost", typeof(DragHandler));
            var drag = _host.GetComponent<DragHandler>();
            PointerEventData received = null;
            drag.OnDragBegin += e => received = e;
            var ped = new PointerEventData(EventSystem.current) { position = new Vector2(5f, 7f) };
            drag.OnBeginDrag(ped);
            Assert.AreSame(ped, received);
        }

        [Test]
        public void OnDrag_InvokesOnDragMovedWithSamePointerEventData()
        {
            _host = new GameObject("DragHost", typeof(DragHandler));
            var drag = _host.GetComponent<DragHandler>();
            drag.IsInteractable = true;
            PointerEventData received = null;
            drag.OnDragMoved += e => received = e;
            var ped = new PointerEventData(EventSystem.current) { position = new Vector2(1f, 2f) };
            drag.OnDrag(ped);
            Assert.AreSame(ped, received);
        }
    }
}
