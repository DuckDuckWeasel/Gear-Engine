using System;
using Game.CarSimulation;
using NUnit.Framework;

namespace Game.CarSimulation.Tests
{
    public sealed class CarTrackBootstrapTests
    {
        [Test]
        public void CarTrackBootstrap_StartDriving_BeforeInitialize_Throws()
        {
            var bootstrap = new CarTrackBootstrap(null, null, null, null);
            Assert.Throws<InvalidOperationException>(() => bootstrap.StartDriving());
        }
    }
}
