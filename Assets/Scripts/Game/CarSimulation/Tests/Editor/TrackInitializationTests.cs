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
        public void Bind_CopiesKnotsFromTrackDefinitionOntoSplineContainer()
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

                    var viewModel = new TrackViewModel(def, car: null);
                    track.Bind(viewModel);

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
        public void TrackViewModel_ThrowsWhenTrackDefinitionIsNull()
        {
            Assert.Throws<System.ArgumentNullException>(() => new TrackViewModel(null, car: null));
        }

        [Test]
        public void Bind_UpdatesSplineExtrudeOnPathChild()
        {
            var go = new GameObject("TrackSelfContained");
            try
            {
                var container = go.AddComponent<SplineContainer>();
                var pathGo = new GameObject("Path");
                pathGo.transform.SetParent(go.transform, false);
                var extrude = pathGo.AddComponent<SplineExtrude>();
                extrude.Container = container;

                var track = go.AddComponent<Track>();
                var trackSo = new UnityEditor.SerializedObject(track);
                trackSo.FindProperty("splineContainer").objectReferenceValue = container;
                trackSo.FindProperty("splineExtrude").objectReferenceValue = extrude;
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

                    var viewModel = new TrackViewModel(def, car: null);
                    track.Bind(viewModel);

                    Assert.That(extrude.Container, Is.SameAs(container));
                    Assert.That(container.Spline.Count, Is.EqualTo(4));
                    Assert.That(container.Spline.Closed, Is.True);
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
        public void CarPrefab_HasCarViewAndSplineDriverOnRoot()
        {
            const string path = "Assets/Game/CarSimulation/Prefabs/Car.prefab";
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null);
            var carView = prefab.GetComponent<CarView>();
            Assert.That(carView, Is.Not.Null);
            var driver = prefab.GetComponent<CarSplineDriver>();
            Assert.That(driver, Is.Not.Null);
        }
    }
}
