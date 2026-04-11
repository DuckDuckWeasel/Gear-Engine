using Game.CarSimulation;
using Game.GearEngine;
using Game.Race;
using IRaceDriver = Game.CarSimulation.RaceFlowContracts.IRaceDriver;
using NUnit.Framework;

namespace Game.Race.Tests
{
    public sealed class RaceViewModelTests
    {
        private sealed class MockGridManager : IGridManager
        {
            public int PlayCallCount { get; private set; }

            public float GlobalSpeedModifier { get; set; }
            public bool IsRunning => true;

            public void AddNode(IGridNode node) { }
            public void RemoveNode(UnityEngine.Vector2Int pos) { }
            public IGridNode ExtractNode(UnityEngine.Vector2Int pos) => null;
            public IGridNode GetNode(UnityEngine.Vector2Int pos) => null;

            public void Play()
            {
                PlayCallCount++;
            }

            public void Stop() { }
        }

        private sealed class MockRaceDriver : IRaceDriver
        {
            public int StartDrivingCallCount { get; private set; }

            public void StartDriving()
            {
                StartDrivingCallCount++;
            }
        }

        [Test]
        public void RaceViewModel_StartRace_DisablesButtonAndStartsBoth()
        {
            var mockGrid = new MockGridManager();
            var mockDriver = new MockRaceDriver();
            var vm = new RaceViewModel();
            vm.Construct(mockGrid, mockDriver);

            Assert.That(vm.CanRace, Is.True);
            vm.StartRace();
            Assert.That(vm.CanRace, Is.False);
            Assert.That(mockGrid.PlayCallCount, Is.EqualTo(1));
            Assert.That(mockDriver.StartDrivingCallCount, Is.EqualTo(1));
        }

        [Test]
        public void RaceViewModel_StartRace_CalledTwice_OnlyFiresOnce()
        {
            var mockGrid = new MockGridManager();
            var mockDriver = new MockRaceDriver();
            var vm = new RaceViewModel();
            vm.Construct(mockGrid, mockDriver);

            vm.StartRace();
            vm.StartRace();

            Assert.That(mockGrid.PlayCallCount, Is.EqualTo(1));
            Assert.That(mockDriver.StartDrivingCallCount, Is.EqualTo(1));
        }
    }
}
