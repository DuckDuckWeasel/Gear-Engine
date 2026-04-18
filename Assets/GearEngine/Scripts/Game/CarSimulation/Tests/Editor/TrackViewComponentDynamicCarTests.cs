using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Tracks;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class TrackViewComponentDynamicCarTests
    {
        [Test]
        public void Bind_WithCarPrefabButSpawnDisabled_LeavesNoCarView()
        {
            var trackGo = new GameObject("TrackHarnessNoSpawn");
            try
            {
                trackGo.AddComponent<SplineContainer>();
                Track track = trackGo.AddComponent<Track>();
                CarDefinition carDef = ScriptableObject.CreateInstance<CarDefinition>();
                TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                GameObject carTemplate = new GameObject("CarTemplateNoSpawn");
                try
                {
                    SeedOpenSpline(trackDef);
                    carTemplate.AddComponent<CarView>();
                    SerializedObject carSo = new SerializedObject(carDef);
                    carSo.FindProperty("carPrefab").objectReferenceValue = carTemplate;
                    carSo.ApplyModifiedPropertiesWithoutUndo();

                    LapRaceSession session = new TrackSimulationFactory().Create(carDef, trackDef, null);
                    var trackVm = new TrackViewModel(session);
                    track.Bind(trackVm);

                    Assert.That(trackGo.GetComponentsInChildren<CarView>(true).Length, Is.EqualTo(0));
                    track.ReleaseViewBinding();
                }
                finally
                {
                    Object.DestroyImmediate(carTemplate);
                    Object.DestroyImmediate(carDef);
                    Object.DestroyImmediate(trackDef);
                }
            }
            finally
            {
                Object.DestroyImmediate(trackGo);
            }
        }

        [Test]
        public void Bind_SpawnWhenSessionRuns_SpawnsAfterToggleTrue()
        {
            var trackGo = new GameObject("TrackHarnessDeferred");
            try
            {
                trackGo.AddComponent<SplineContainer>();
                Track track = trackGo.AddComponent<Track>();
                CarDefinition carDef = ScriptableObject.CreateInstance<CarDefinition>();
                TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                GameObject carTemplate = new GameObject("CarTemplateDeferred");
                try
                {
                    SeedOpenSpline(trackDef);
                    carTemplate.AddComponent<CarView>();
                    SerializedObject carSo = new SerializedObject(carDef);
                    carSo.FindProperty("carPrefab").objectReferenceValue = carTemplate;
                    carSo.ApplyModifiedPropertiesWithoutUndo();

                    LapRaceSession session = new TrackSimulationFactory().Create(carDef, trackDef, null);
                    var trackVm = new TrackViewModel(session, spawnCarOnBindIfNoChild: false, spawnCarWhenSessionStartsRunning: true);
                    track.Bind(trackVm);

                    Assert.That(trackGo.GetComponentsInChildren<CarView>(true).Length, Is.EqualTo(0));
                    trackVm.Toggle(true);
                    Assert.That(trackGo.GetComponentsInChildren<CarView>(true).Length, Is.EqualTo(1));
                    track.ReleaseViewBinding();
                }
                finally
                {
                    Object.DestroyImmediate(carTemplate);
                    Object.DestroyImmediate(carDef);
                    Object.DestroyImmediate(trackDef);
                }
            }
            finally
            {
                Object.DestroyImmediate(trackGo);
            }
        }

        [Test]
        public void Bind_WithCarPrefabAndNoChildCarView_SpawnsCarUnderTrack()
        {
            var trackGo = new GameObject("TrackHarness");
            try
            {
                trackGo.AddComponent<SplineContainer>();
                Track track = trackGo.AddComponent<Track>();
                CarDefinition carDef = ScriptableObject.CreateInstance<CarDefinition>();
                TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                GameObject carTemplate = new GameObject("CarTemplate");
                try
                {
                    SeedOpenSpline(trackDef);
                    carTemplate.AddComponent<CarView>();
                    SerializedObject carSo = new SerializedObject(carDef);
                    carSo.FindProperty("carPrefab").objectReferenceValue = carTemplate;
                    carSo.ApplyModifiedPropertiesWithoutUndo();

                    LapRaceSession session = new TrackSimulationFactory().Create(carDef, trackDef, null);
                    var trackVm = new TrackViewModel(session, spawnCarOnBindIfNoChild: true);
                    track.Bind(trackVm);

                    CarView[] carViews = trackGo.GetComponentsInChildren<CarView>(true);
                    Assert.That(carViews.Length, Is.EqualTo(1));
                    Assert.That(carViews[0].transform.parent, Is.EqualTo(trackGo.transform));

                    track.ReleaseViewBinding();

                    carViews = trackGo.GetComponentsInChildren<CarView>(true);
                    Assert.That(carViews.Length, Is.EqualTo(0));
                }
                finally
                {
                    Object.DestroyImmediate(carTemplate);
                    Object.DestroyImmediate(carDef);
                    Object.DestroyImmediate(trackDef);
                }
            }
            finally
            {
                Object.DestroyImmediate(trackGo);
            }
        }

        private static void SeedOpenSpline(TrackDefinition trackDef)
        {
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
        }
    }
}
