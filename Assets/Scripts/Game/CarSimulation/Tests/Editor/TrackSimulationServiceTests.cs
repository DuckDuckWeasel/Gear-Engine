using System.Text.RegularExpressions;
using Game.CarSimulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.CarSimulation.Tests
{
    public sealed class TrackSimulationServiceTests
    {
        [Test]
        public void CreateSimulation_BuildsViewModelWithCar_AndIsRunningStartsFalse()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                var service = new TrackSimulationService();
                service.CreateSimulation(carDef, trackDef);

                Assert.That(service.TrackViewModel, Is.Not.Null);
                Assert.That(service.TrackViewModel.Car, Is.Not.Null);
                Assert.That(service.TrackViewModel.Car.Instance, Is.Not.Null);
                Assert.That(service.TrackViewModel.IsRunning, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void ToggleSimulation_ThrowsBeforeCreateSimulation()
        {
            var service = new TrackSimulationService();
            LogAssert.Expect(LogType.Error, new Regex(@"\[TrackSimulationService\] ToggleSimulation failed:.*"));
            Assert.Throws<System.InvalidOperationException>(() => service.ToggleSimulation(true));
        }

        [Test]
        public void ToggleSimulation_PausesAndResumes_IsRunning()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                var service = new TrackSimulationService();
                service.CreateSimulation(carDef, trackDef);
                service.ToggleSimulation(true);
                Assert.That(service.TrackViewModel.IsRunning, Is.True);
                service.ToggleSimulation(false);
                Assert.That(service.TrackViewModel.IsRunning, Is.False);
                service.ToggleSimulation(true);
                Assert.That(service.TrackViewModel.IsRunning, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void CompleteSimulation_ThrowsWhenNotRunningOrPaused()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                var service = new TrackSimulationService();
                service.CreateSimulation(carDef, trackDef);
                LogAssert.Expect(LogType.Error, new Regex(@"\[TrackSimulationService\] CompleteSimulation failed:.*"));
                Assert.Throws<System.InvalidOperationException>(() => service.CompleteSimulation());
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void CompleteSimulation_FromRunning_ThenToggleThrows()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                var service = new TrackSimulationService();
                service.CreateSimulation(carDef, trackDef);
                service.ToggleSimulation(true);
                service.CompleteSimulation();
                Assert.That(service.TrackViewModel.IsRunning, Is.False);
                LogAssert.Expect(LogType.Error, new Regex(@"\[TrackSimulationService\] ToggleSimulation failed:.*"));
                Assert.Throws<System.InvalidOperationException>(() => service.ToggleSimulation(true));
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void CreateSimulation_ThrowsWhenCalledTwice()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                var service = new TrackSimulationService();
                service.CreateSimulation(carDef, trackDef);
                LogAssert.Expect(LogType.Error, new Regex(@"\[TrackSimulationService\] CreateSimulation failed:.*"));
                Assert.Throws<System.InvalidOperationException>(() => service.CreateSimulation(carDef, trackDef));
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }
    }
}
