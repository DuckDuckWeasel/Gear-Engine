using System.Linq;
using Game.CarSimulation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Splines;

namespace Game.CarSimulation.Tests
{
    public sealed class TrackInitializationTests
    {
        [Test]
        public void Initialize_CopiesKnotsFromTrackDefinitionOntoSplineContainer()
        {
            var go = new GameObject("TrackTest");
            try
            {
                var container = go.AddComponent<SplineContainer>();
                var track = go.AddComponent<Track>();
                var so = new UnityEditor.SerializedObject(track);
                so.FindProperty("splineContainer").objectReferenceValue = container;
                so.ApplyModifiedPropertiesWithoutUndo();

                var def = ScriptableObject.CreateInstance<TrackDefinition>();
                try
                {
                    var source = def.Spline;
                    source.Knots = new[]
                    {
                        new BezierKnot(new Vector3(0, 0, 0)),
                        new BezierKnot(new Vector3(10, 0, 0)),
                    };
                    source.Closed = false;

                    track.Initialize(def);

                    var target = container.Spline;
                    Assert.That(target.Count, Is.EqualTo(2));
                    Assert.That(target.Closed, Is.False);
                    var knots = target.Knots.ToArray();
                    Assert.That(knots[0].Position.x, Is.EqualTo(0).Within(0.001f));
                    Assert.That(knots[1].Position.x, Is.EqualTo(10).Within(0.001f));
                }
                finally
                {
                    Object.DestroyImmediate(def);
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Initialize_ThrowsWhenTrackDefinitionIsNull()
        {
            var go = new GameObject("TrackTestNullDef");
            try
            {
                go.AddComponent<SplineContainer>();
                var track = go.AddComponent<Track>();

                Assert.Throws<System.ArgumentNullException>(() => track.Initialize(null));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Initialize_UpdatesParentSplineExtrudeContainerSpline()
        {
            var root = new GameObject("TrackVisualRoot");
            var trackGo = new GameObject("Track");
            trackGo.transform.SetParent(root.transform, false);

            try
            {
                var rootContainer = root.AddComponent<SplineContainer>();
                var splineExtrude = root.AddComponent<SplineExtrude>();
                splineExtrude.Container = rootContainer;

                var trackContainer = trackGo.AddComponent<SplineContainer>();
                var track = trackGo.AddComponent<Track>();
                var trackSo = new UnityEditor.SerializedObject(track);
                trackSo.FindProperty("splineContainer").objectReferenceValue = trackContainer;
                trackSo.ApplyModifiedPropertiesWithoutUndo();

                var def = ScriptableObject.CreateInstance<TrackDefinition>();
                try
                {
                    def.Spline.Knots = new[]
                    {
                        new BezierKnot(new Vector3(-5f, 0f, -5f)),
                        new BezierKnot(new Vector3(-5f, 0f, 5f)),
                        new BezierKnot(new Vector3(5f, 0f, 5f)),
                        new BezierKnot(new Vector3(5f, 0f, -5f)),
                    };
                    def.Spline.Closed = true;

                    track.Initialize(def);

                    Assert.That(splineExtrude.Container, Is.SameAs(rootContainer));
                    Assert.That(rootContainer.Spline.Count, Is.EqualTo(4));
                    Assert.That(rootContainer.Spline.Closed, Is.True);
                }
                finally
                {
                    Object.DestroyImmediate(def);
                }
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
