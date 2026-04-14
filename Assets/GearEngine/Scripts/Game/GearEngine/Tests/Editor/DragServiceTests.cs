using System;
using GearEngine.GearEngine;
using NUnit.Framework;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public class DragServiceTests
    {
        [Test]
        public void StartDrag_SetsDraggingTrue()
        {
            var service = new DragService();
            var data = new GearConfigData { Id = "test_gear" };

            service.StartDrag(data);

            Assert.IsTrue(service.IsDragging);
        }

        [Test]
        public void StartDrag_FiresOnDragStarted()
        {
            var service = new DragService();
            var data = new GearConfigData { Id = "test_gear" };
            object received = null;
            service.OnDragStarted += d => received = d;

            service.StartDrag(data);

            Assert.AreSame(data, received);
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
        public void EndDrag_FiresOnDragEnded()
        {
            var service = new DragService();
            service.StartDrag(new GearConfigData { Id = "x" });

            bool ended = false;
            service.OnDragEnded += () => ended = true;

            service.EndDrag();

            Assert.IsTrue(ended);
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

            bool endedCalled = false;
            service.OnDragEnded += () => endedCalled = true;

            service.StartDrag(data1);
            service.StartDrag(data2);

            Assert.IsTrue(endedCalled, "EndDrag should have been called for the first drag.");
            Assert.IsTrue(service.IsDragging);
            Assert.AreSame(data2, service.GetDragData<GearConfigData>());
        }
    }
}
