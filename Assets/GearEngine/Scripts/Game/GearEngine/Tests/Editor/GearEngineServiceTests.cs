using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
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

            public IEnumerable<IGridNode> GetAllNodes() => Array.Empty<IGridNode>();

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

        [Test]
        public void Play_DelegatesToGridManager()
        {
            var grid = new StubGrid();
            var service = new GearEngineService(grid);

            service.Play();

            Assert.IsTrue(grid.PlayCalled);
        }

        [Test]
        public void Stop_DelegatesToGridManager()
        {
            var grid = new StubGrid();
            var service = new GearEngineService(grid);

            service.Stop();

            Assert.IsTrue(grid.StopCalled);
        }

        [Test]
        public void IsRunning_ReflectsGridManagerState()
        {
            var grid = new StubGrid { IsRunning = true };
            var service = new GearEngineService(grid);

            Assert.IsTrue(service.IsRunning);
        }

        [Test]
        public void Constructor_ThrowsWhenGridManagerNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GearEngineService(null));
        }
    }
}
