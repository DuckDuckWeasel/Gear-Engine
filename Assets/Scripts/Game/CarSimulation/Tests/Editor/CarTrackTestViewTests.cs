using Scaffold.CarSimulation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace Scaffold.CarSimulation.Tests
{
    public sealed class CarTrackTestViewTests
    {
        [Test]
        public void CarTrackTestView_OnBind_DelegatesToTrackViewComponent()
        {
            var root = new GameObject("CarTrackTestRoot");
            try
            {
                RunCarTrackShellBindAsserts(root);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CarTrackTestView_OnBind_WithCar_StartsSplinePlayback()
        {
            var root = new GameObject("CarTrackTestRootWithCar");
            try
            {
                CarTrackTestView shell = root.AddComponent<CarTrackTestView>();
                var trackGo = new GameObject("Track");
                trackGo.transform.SetParent(root.transform, false);
                var container = trackGo.AddComponent<SplineContainer>();
                var track = trackGo.AddComponent<Track>();
                var trackSo = new SerializedObject(track);
                trackSo.FindProperty("splineContainer").objectReferenceValue = container;
                trackSo.ApplyModifiedPropertiesWithoutUndo();
                var shellSo = new SerializedObject(shell);
                shellSo.FindProperty("track").objectReferenceValue = track;
                shellSo.ApplyModifiedPropertiesWithoutUndo();

                CarDefinition carDef = AssetDatabase.LoadAssetAtPath<CarDefinition>("Assets/Data/Track/CarDefinition.asset");
                Assert.That(carDef, Is.Not.Null);

                var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                try
                {
                    WriteTwoPointOpenSpline(trackDef);
                    TrackSimulation sim = new TrackSimulationFactory().Create(carDef, trackDef);
                    shell.Bind(new TrackViewModel(sim));

                    SplineAnimate[] splineAnimates = root.GetComponentsInChildren<SplineAnimate>(true);
                    Assert.That(splineAnimates.Length, Is.EqualTo(1));
                    Assert.That(splineAnimates[0].IsPlaying, Is.True);
                }
                finally
                {
                    Object.DestroyImmediate(trackDef);
                }
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private void RunCarTrackShellBindAsserts(GameObject root)
        {
            CarTrackTestView shell = root.AddComponent<CarTrackTestView>();
            SplineContainer container = CreateTrackChildWithSpline(root, shell);
            var def = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                WriteTwoPointOpenSpline(def);
                shell.Bind(new TrackViewModel(new TrackSimulation(def, car: null)));
                AssertOpenTwoKnotSpline(container);
            }
            finally
            {
                Object.DestroyImmediate(def);
            }
        }

        private void AssertOpenTwoKnotSpline(SplineContainer container)
        {
            Assert.That(container.Spline.Count, Is.EqualTo(2));
            Assert.That(container.Spline.Closed, Is.False);
        }

        private void WriteTwoPointOpenSpline(TrackDefinition def)
        {
            def.Spline.Knots = new[]
            {
                new BezierKnot(new Vector3(0f, 0f, 0f)),
                new BezierKnot(new Vector3(5f, 0f, 0f)),
            };
            def.Spline.Closed = false;
        }

        private SplineContainer CreateTrackChildWithSpline(GameObject root, CarTrackTestView shell)
        {
            var trackGo = new GameObject("Track");
            trackGo.transform.SetParent(root.transform, false);
            var container = trackGo.AddComponent<SplineContainer>();
            var track = trackGo.AddComponent<Track>();
            var shellSo = new SerializedObject(shell);
            shellSo.FindProperty("track").objectReferenceValue = track;
            shellSo.ApplyModifiedPropertiesWithoutUndo();
            return container;
        }
    }
}
