using GearEngine.FrustumFit;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    /// <summary>
    /// EditMode tests for <see cref="FrustumFitAnchor"/> placement guards (no exceptions on degenerate mesh axes).
    /// </summary>
    [TestFixture]
    public sealed class FrustumFitAnchorTests
    {
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
                _root = null;
            }
        }

        [Test]
        public void TryComputePlacement_ZeroExtentOnFittedAxes_ReturnsFalseWithoutThrowing()
        {
            _root = new GameObject("FrustumFitTestRoot");

            GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(_root.transform);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject uiGo = new GameObject("UI", typeof(RectTransform));
            uiGo.transform.SetParent(canvasGo.transform, false);
            RectTransform uiRt = uiGo.GetComponent<RectTransform>();
            uiRt.sizeDelta = new Vector2(100f, 100f);

            GameObject camGo = new GameObject("Camera");
            camGo.transform.SetParent(_root.transform);
            Camera cam = camGo.AddComponent<Camera>();

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.SetParent(_root.transform);
            Renderer renderer = plane.GetComponent<Renderer>();

            FrustumFitAnchorPlacement placement;
            bool ok = FrustumFitPlacementFactory.TryCreate(uiRt, canvas, cam, 10f, plane.transform, renderer, FrustumFillMode.Fit, FrustumFitAxes.XY, FrustumFitAnchorRotationMode.PreserveTarget, Vector3.one, out placement);

            Assert.IsFalse(ok, "Unity's plane mesh has no Y extent; XY fit axes must fail gracefully.");
        }

        [Test]
        public void TryComputePlacement_PlaneWithXZAxes_ReturnsTrue()
        {
            _root = new GameObject("FrustumFitTestRoot");

            GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(_root.transform);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject uiGo = new GameObject("UI", typeof(RectTransform));
            uiGo.transform.SetParent(canvasGo.transform, false);
            RectTransform uiRt = uiGo.GetComponent<RectTransform>();
            uiRt.sizeDelta = new Vector2(100f, 100f);

            GameObject camGo = new GameObject("Camera");
            camGo.transform.SetParent(_root.transform);
            Camera cam = camGo.AddComponent<Camera>();

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.SetParent(_root.transform);
            Renderer renderer = plane.GetComponent<Renderer>();

            FrustumFitAnchorPlacement placement;
            bool ok = FrustumFitPlacementFactory.TryCreate(uiRt, canvas, cam, 10f, plane.transform, renderer, FrustumFillMode.Fit, FrustumFitAxes.XZ, FrustumFitAnchorRotationMode.PreserveTarget, Vector3.one, out placement);

            Assert.IsTrue(ok);
            Assert.That(placement.LocalScale.x, Is.GreaterThan(0f));
            Assert.That(placement.LocalScale.z, Is.GreaterThan(0f));
        }

        [Test]
        public void ResolveTargetRenderer_ChildMesh_ReturnsChildRenderer()
        {
            _root = new GameObject("FrustumFitTestRoot");
            GameObject parent = new GameObject("TrackRoot");
            parent.transform.SetParent(_root.transform);
            GameObject meshGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meshGo.transform.SetParent(parent.transform);

            Renderer r = FrustumFitTargetRenderer.FromTargetTransform(parent.transform);

            Assert.IsNotNull(r);
            Assert.AreSame(meshGo.GetComponent<Renderer>(), r);
        }
    }
}
