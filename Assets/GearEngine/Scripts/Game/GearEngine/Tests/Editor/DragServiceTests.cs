using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation.UI;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class DragServiceTests
    {
        private sealed class RecordingListener : IDragLifecycleListener
        {
            public int StartedCount;
            public int EndedCount;
            public DragPayload LastStartedPayload;

            public void OnDragStarted(DragPayload payload)
            {
                StartedCount++;
                LastStartedPayload = payload;
            }

            public void OnDragEnded()
            {
                EndedCount++;
            }
        }

        [Test]
        public void StartDrag_EndDrag_BroadcastsToRegisteredListeners()
        {
            var service = new DragService();
            var listener = new RecordingListener();
            service.Register(listener);

            var payload = new DragPayload("data", Vector3.one);
            service.StartDrag(payload);

            Assert.IsTrue(service.IsDragging);
            Assert.AreEqual(1, listener.StartedCount);
            Assert.AreEqual("data", listener.LastStartedPayload.Data);

            service.EndDrag();

            Assert.IsFalse(service.IsDragging);
            Assert.AreEqual(1, listener.EndedCount);
        }

        [Test]
        public void StartDrag_WithNullPayload_ThrowsArgumentNullException()
        {
            var service = new DragService();
            Assert.Throws<System.ArgumentNullException>(() => service.StartDrag(default));
        }
    }
}
