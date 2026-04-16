using System.Collections.Generic;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using TrackViewComponent = GearEngine.CarSimulation.Tracks.Track;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.TestTools;

namespace GearEngine.CarSimulation.Tests
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

        private void RunCarTrackShellBindAsserts(GameObject root)
        {
            CarTrackTestView shell = root.AddComponent<CarTrackTestView>();
            SplineContainer container = CreateTrackChildWithSpline(root, shell);
            var def = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                WriteTwoPointOpenSpline(def);
                var carDef = ScriptableObject.CreateInstance<CarDefinition>();
                try
                {
                    LogAssert.Expect(LogType.Error, "[CarTrackTestView] CarPrefab is missing on CarDefinition.");
                    LapRaceSession session = new TrackSimulationFactory().Create(carDef, def, null);
                    shell.Bind(new TrackListViewModel(new List<LapRaceSession> { session }));
                    AssertOpenTwoKnotSpline(container);
                }
                finally
                {
                    Object.DestroyImmediate(carDef);
                }
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
            var track = trackGo.AddComponent<TrackViewComponent>();
            var shellSo = new SerializedObject(shell);
            shellSo.FindProperty("track").objectReferenceValue = track;
            shellSo.ApplyModifiedPropertiesWithoutUndo();
            return container;
        }
    }
}
