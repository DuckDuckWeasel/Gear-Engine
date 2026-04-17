using System;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation.UI;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public class DragServiceTests
    {
        private sealed class RecordingTarget : IDragTarget
        {
            public object LastStartedData;
            public int StartedCount;
            public int EndedCount;

            public void OnDragStarted(DragPayload payload)
            {
                StartedCount++;
                LastStartedData = payload.Data;
            }

            public void OnDragEnded()
            {
                EndedCount++;
            }

            public bool CanAccept(DragPayload payload) => false;

            public void OnDrop(DragPayload payload)
            {
            }

            public void OnHoverEnter(DragPayload payload)
            {
            }

            public void OnHoverExit()
            {
            }
        }

        [Test]
        public void StartDrag_SetsDraggingTrue()
        {
            var service = new DragService();
            var data = new GearConfigData { Id = "test_gear" };

            service.StartDrag(data);

            Assert.IsTrue(service.IsDragging);
        }

        [Test]
        public void StartDrag_NotifiesRegisteredTargets()
        {
            var service = new DragService();
            var data = new GearConfigData { Id = "test_gear" };
            var target = new RecordingTarget();
            service.Register(target);

            service.StartDrag(data);

            Assert.AreEqual(1, target.StartedCount);
            Assert.AreSame(data, target.LastStartedData);
        }

        [Test]
        public void EndDrag_SetsDraggingFalse()
        {
            var service = new DragService();
            service.StartDrag(new GearConfigData { Id = "x" });

            service.EndDrag();

            Assert.IsFalse(service.IsDragging);
        }

        [Test]
        public void EndDrag_NotifiesRegisteredTargets()
        {
            var service = new DragService();
            service.StartDrag(new GearConfigData { Id = "x" });
            var target = new RecordingTarget();
            service.Register(target);

            service.EndDrag();

            Assert.AreEqual(1, target.EndedCount);
        }

        [Test]
        public void GetDragData_ReturnsCorrectType()
        {
            var service = new DragService();
            var data = new GearConfigData { Id = "typed_test" };
            service.StartDrag(data);

            GearConfigData result = service.GetDragData<GearConfigData>();

            Assert.AreSame(data, result);
        }

        [Test]
        public void GetDragData_ReturnsNull_WhenTypeMismatch()
        {
            var service = new DragService();
            service.StartDrag("a string, not gear data");

            GearConfigData result = service.GetDragData<GearConfigData>();

            Assert.IsNull(result);
        }

        [Test]
        public void GetDragData_ReturnsNull_WhenNotDragging()
        {
            var service = new DragService();

            GearConfigData result = service.GetDragData<GearConfigData>();

            Assert.IsNull(result);
        }

        [Test]
        public void StartDrag_ThrowsOnNull()
        {
            var service = new DragService();

            Assert.Throws<ArgumentNullException>(() => service.StartDrag(null));
        }

        [Test]
        public void StartDrag_WhileAlreadyDragging_EndsFirstThenStarts()
        {
            var service = new DragService();
            var data1 = new GearConfigData { Id = "first" };
            var data2 = new GearConfigData { Id = "second" };

            var target = new RecordingTarget();
            service.Register(target);

            service.StartDrag(data1);
            service.StartDrag(data2);

            Assert.AreEqual(2, target.StartedCount, "Second StartDrag should notify after implicit EndDrag + new Start.");
            Assert.AreEqual(1, target.EndedCount, "First drag should end before second starts.");
            Assert.IsTrue(service.IsDragging);
            Assert.AreSame(data2, service.GetDragData<GearConfigData>());
        }
    }
}
