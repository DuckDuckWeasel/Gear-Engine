using System;
using NUnit.Framework;
using UnityEngine;

namespace Game.GearEngine.Tests
{
    [TestFixture]
    public class GearEngineServiceTests
    {
        private sealed class StubGrid : IGridManager
        {
            public bool IsRunning { get; set; }
            public float GlobalSpeedModifier { get; set; }
            public bool PlayCalled;
            public bool StopCalled;

            public void AddNode(IGridNode node)
            {
            }

            public void RemoveNode(Vector2Int pos)
            {
            }

            public IGridNode ExtractNode(Vector2Int pos) => null;

            public IGridNode GetNode(Vector2Int pos) => null;

            public void Play() => PlayCalled = true;

            public void Stop() => StopCalled = true;
        }

        private sealed class StubSceneElement : IGearSceneElement
        {
            public void Initialize()
            {
            }

            public void Enable()
            {
            }

            public void Disable()
            {
            }
        }

        [Test]
        public void Play_DelegatesToGridManager()
        {
            var grid = new StubGrid();
            var scene = new StubSceneElement();
            var service = new GearEngineService(grid, scene);

            service.Play();

            Assert.IsTrue(grid.PlayCalled);
        }

        [Test]
        public void Stop_DelegatesToGridManager()
        {
            var grid = new StubGrid();
            var scene = new StubSceneElement();
            var service = new GearEngineService(grid, scene);

            service.Stop();

            Assert.IsTrue(grid.StopCalled);
        }

        [Test]
        public void IsRunning_ReflectsGridManagerState()
        {
            var grid = new StubGrid { IsRunning = true };
            var scene = new StubSceneElement();
            var service = new GearEngineService(grid, scene);

            Assert.IsTrue(service.IsRunning);
        }

        [Test]
        public void Constructor_ThrowsWhenGridManagerNull()
        {
            var scene = new StubSceneElement();

            Assert.Throws<ArgumentNullException>(() => new GearEngineService(null, scene));
        }

        [Test]
        public void Constructor_ThrowsWhenSceneElementNull()
        {
            var grid = new StubGrid();

            Assert.Throws<ArgumentNullException>(() => new GearEngineService(grid, null));
        }
    }
}
