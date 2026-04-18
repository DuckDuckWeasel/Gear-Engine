using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.Tracks;
using NUnit.Framework;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class CarSimulationCoreTypesCompileTests
    {
        [Test]
        public void SimpleTrackDriverTuning_Is_In_Game_CarSimulation_Assembly()
        {
            Assert.That(typeof(SimpleTrackDriverTuning).Assembly.GetName().Name, Is.EqualTo("Game.CarSimulation"));
        }

        [Test]
        public void SplineWaypointPath_Is_In_Game_CarSimulation_Assembly()
        {
            Assert.That(typeof(SplineWaypointPath).Assembly.GetName().Name, Is.EqualTo("Game.CarSimulation"));
        }

        [Test]
        public void SimpleWaypointDriver_Is_In_Game_CarSimulation_Assembly()
        {
            Assert.That(typeof(SimpleWaypointDriver).Assembly.GetName().Name, Is.EqualTo("Game.CarSimulation"));
        }
    }
}
