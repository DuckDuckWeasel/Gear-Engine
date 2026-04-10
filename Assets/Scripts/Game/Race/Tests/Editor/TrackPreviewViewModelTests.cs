using System.Reflection;
using Game.CarSimulation;
using Game.Race;
using Game.Race.Navigation;
using NUnit.Framework;
using Scaffold.Navigation;
using UnityEngine;

namespace Game.Race.Tests
{
    public sealed class TrackPreviewViewModelTests
    {
        [Test]
        public void TrackPreviewViewModel_TrackName_MatchesDefinition()
        {
            var def = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                typeof(TrackDefinition)
                    .GetField("trackName", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(def, "Test Oval");

                var raceCfg = ScriptableObject.CreateInstance<ViewConfig>();
                try
                {
                    var vm = new TrackPreviewViewModel();
                    vm.Construct(null, def, new RaceViewConfigRef(raceCfg));

                    Assert.That(vm.TrackName, Is.EqualTo("Test Oval"));
                }
                finally
                {
                    Object.DestroyImmediate(raceCfg);
                }
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }
    }
}
