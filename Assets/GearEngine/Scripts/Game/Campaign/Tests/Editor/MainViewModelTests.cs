using System.Reflection;
using GearEngine.Campaign.Presentation;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Simulation;
using NUnit.Framework;
using Scaffold.MVVM;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class MainViewModelTests
    {
        private sealed class RecordingRaceSessionRunner : IRaceSessionRunner
        {
            public LapRaceSession LastSetSession { get; private set; }

            public LapRaceSession ActiveSession => LastSetSession;

            public void SetSession(LapRaceSession session)
            {
                LastSetSession = session;
            }

            public void Tick()
            {
            }
        }

        [Test]
        public void Initialize_CreatesTrackAndStatsChildren()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;

            LapRaceSession session = CampaignTestUtilities.CreateMinimalSession(carDef, trackDef);
            var trackService = new FakeTrackService(trackDef, carDef, session);

            var runner = new RecordingRaceSessionRunner();
            var vm = new MainViewModel();
            Inject(vm, "trackService", trackService);
            Inject(vm, "trackFactory", new TrackSimulationFactory());
            Inject(vm, "raceSessionRunner", runner);
            InjectNavigation(vm, new RecordingNavigation());

            InvokeInitialize(vm);

            Assert.That(runner.LastSetSession, Is.Null);
            Assert.That(vm.Track, Is.Not.Null);
            Assert.That(vm.Stats, Is.Not.Null);
            Assert.That(vm.Track.Session, Is.SameAs(session));
            Assert.That(vm.Stats.TrackName, Is.EqualTo(trackDef.GetDisplayName()));
            Assert.That(vm.Stats.TargetLaps, Is.EqualTo(trackDef.TotalLaps));
            Assert.That(vm.Stats.TargetTime, Is.EqualTo(trackDef.TimeToBeatSeconds));

            var so = new SerializedObject(trackDef);
            so.FindProperty("trackName").stringValue = "Coast Run";
            so.FindProperty("totalLaps").intValue = 4;
            so.FindProperty("timeToBeatSeconds").floatValue = 72.5f;
            so.ApplyModifiedPropertiesWithoutUndo();

            var statsFromTrack = new TrackStatsViewModel(trackService);
            Assert.That(statsFromTrack.TrackName, Is.EqualTo("Coast Run"));
            Assert.That(statsFromTrack.TargetLaps, Is.EqualTo(4));
            Assert.That(statsFromTrack.TargetTime, Is.EqualTo(72.5f));

            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
        }

        private static void InvokeInitialize(ViewModel vm)
        {
            MethodInfo init = vm.GetType().GetMethod(
                "Initialize",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(init, Is.Not.Null);
            init.Invoke(vm, null);
        }

        private static void Inject(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(target, value);
        }

        private static void InjectNavigation(ViewModel vm, RecordingNavigation navigation)
        {
            FieldInfo field = typeof(ViewModel).GetField("navigation", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(vm, navigation);
        }
    }
}
