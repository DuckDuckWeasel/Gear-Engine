using GearEngine.FrustumFit;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    /// <summary>
    /// EditMode tests for the FrustumFit bounds-strategy pipeline and placement factory.
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

        // ── DirectRendererBoundsStrategy ────────────────────────────────────────

        [Test]
        public void DirectRenderer_RendererOnSelf_ReturnsEffectiveMeshSize()
        {
            _root = new GameObject("Root");
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(_root.transform);

            bool ok = DirectRendererBoundsStrategy.TryGetEffectiveMeshSize(cube.transform, out Vector3 meshSize);

            Assert.IsTrue(ok);
            Assert.That(meshSize.x, Is.GreaterThan(0f));
            Assert.That(meshSize.y, Is.GreaterThan(0f));
        }

        [Test]
        public void DirectRenderer_RendererOnChild_ReturnsEffectiveMeshSize()
        {
            _root = new GameObject("Root");
            GameObject parent = new GameObject("Parent");
            parent.transform.SetParent(_root.transform);
            GameObject childCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childCube.transform.SetParent(parent.transform);

            bool ok = DirectRendererBoundsStrategy.TryGetEffectiveMeshSize(parent.transform, out Vector3 meshSize);

            Assert.IsTrue(ok, "DirectRenderer must find the first child renderer when none exists on self.");
            Assert.That(meshSize.x, Is.GreaterThan(0f));
        }

        [Test]
        public void DirectRenderer_NoRendererAnywhere_ReturnsFalse()
        {
            _root = new GameObject("Root");
            GameObject empty = new GameObject("Empty");
            empty.transform.SetParent(_root.transform);
            new GameObject("AlsoEmpty").transform.SetParent(empty.transform);

            bool ok = DirectRendererBoundsStrategy.TryGetEffectiveMeshSize(empty.transform, out _);

            Assert.IsFalse(ok, "DirectRenderer must return false when the subtree has no Renderer.");
        }

        /// <summary>
        /// Regression: the old code used GetComponentInParent which walked up the hierarchy,
        /// picking up an ancestor renderer and producing a completely wrong scale.
        /// DirectRenderer must not search parents.
        /// </summary>
        [Test]
        public void DirectRenderer_ParentHasRenderer_ReturnsFalse()
        {
            _root = new GameObject("Root");
            GameObject parentWithRenderer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            parentWithRenderer.name = "ParentCube";
            parentWithRenderer.transform.SetParent(_root.transform);
            // A child with NO children of its own — DirectRenderer searching up would wrongly
            // return the parent's renderer.
            GameObject childEmpty = new GameObject("ChildEmpty");
            childEmpty.transform.SetParent(parentWithRenderer.transform);

            bool ok = DirectRendererBoundsStrategy.TryGetEffectiveMeshSize(childEmpty.transform, out _);

            Assert.IsFalse(ok, "DirectRenderer must not walk up to parent renderers.");
        }

        // ── CombineChildBoundsStrategy ───────────────────────────────────────────

        [Test]
        public void CombineChildBounds_SingleChildRenderer_ReturnsEffectiveMeshSize()
        {
            _root = new GameObject("Root");
            GameObject parent = new GameObject("Parent");
            parent.transform.SetParent(_root.transform);
            GameObject childCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childCube.transform.SetParent(parent.transform);

            bool ok = CombineChildBoundsStrategy.TryGetEffectiveMeshSize(parent.transform, out Vector3 meshSize);

            Assert.IsTrue(ok);
            Assert.That(meshSize.x, Is.GreaterThan(0f));
            Assert.That(meshSize.y, Is.GreaterThan(0f));
        }

        [Test]
        public void CombineChildBounds_NoRenderers_ReturnsFalse()
        {
            _root = new GameObject("Root");
            GameObject empty = new GameObject("Empty");
            empty.transform.SetParent(_root.transform);

            bool ok = CombineChildBoundsStrategy.TryGetEffectiveMeshSize(empty.transform, out _);

            Assert.IsFalse(ok);
        }

        [Test]
        public void CombineChildBounds_MultipleChildren_EncapsulatesAllBounds()
        {
            _root = new GameObject("Root");
            GameObject parent = new GameObject("Parent");
            parent.transform.SetParent(_root.transform);

            GameObject a = GameObject.CreatePrimitive(PrimitiveType.Cube);
            a.transform.SetParent(parent.transform);
            a.transform.localPosition = new Vector3(-3f, 0f, 0f);

            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.transform.SetParent(parent.transform);
            b.transform.localPosition = new Vector3(3f, 0f, 0f);

            CombineChildBoundsStrategy.TryGetEffectiveMeshSize(parent.transform, out Vector3 meshSize);
            DirectRendererBoundsStrategy.TryGetEffectiveMeshSize(a.transform, out Vector3 singleMeshSize);

            Assert.That(meshSize.x, Is.GreaterThan(singleMeshSize.x),
                "Combined bounds of two laterally separated cubes must be wider than a single cube.");
        }

        // ── FrustumFitPlacementFactory ───────────────────────────────────────────

        [Test]
        public void Factory_ZeroExtentOnFittedAxes_ReturnsFalse()
        {
            _root = new GameObject("Root");
            SetupCanvas(_root, out _, out RectTransform uiRt);
            Camera cam = CreateCamera(_root);

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.SetParent(_root.transform);

            // Plane has no Y extent; XY fit must fail.
            DirectRendererBoundsStrategy.TryGetEffectiveMeshSize(plane.transform, out Vector3 meshSize);
            bool ok = FrustumFitPlacementFactory.TryCreate(
                uiRt, uiRt.GetComponentInParent<Canvas>(), cam, 10f,
                plane.transform, meshSize,
                FrustumFillMode.Fit, FrustumFitAxes.XY,
                FrustumFitAnchorRotationMode.PreserveTarget, Vector3.one,
                out _);

            Assert.IsFalse(ok, "A plane mesh has no Y extent; XY fit axes must fail gracefully.");
        }

        [Test]
        public void Factory_PlaneWithXZAxes_ReturnsValidPlacement()
        {
            _root = new GameObject("Root");
            SetupCanvas(_root, out _, out RectTransform uiRt);
            Camera cam = CreateCamera(_root);

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.SetParent(_root.transform);

            DirectRendererBoundsStrategy.TryGetEffectiveMeshSize(plane.transform, out Vector3 meshSize);
            bool ok = FrustumFitPlacementFactory.TryCreate(
                uiRt, uiRt.GetComponentInParent<Canvas>(), cam, 10f,
                plane.transform, meshSize,
                FrustumFillMode.Fit, FrustumFitAxes.XZ,
                FrustumFitAnchorRotationMode.PreserveTarget, Vector3.one,
                out FrustumFitAnchorPlacement placement);

            Assert.IsTrue(ok);
            Assert.That(placement.LocalScale.x, Is.GreaterThan(0f));
            Assert.That(placement.LocalScale.z, Is.GreaterThan(0f));
        }

        /// <summary>
        /// Regression: the old code used renderer.localBounds.size without accounting for
        /// intermediate child scale, causing the target to be scaled incorrectly.
        /// With world-space bounds the effective mesh size already captures all child scales,
        /// so a 2× child renderer must yield half the localScale on the target.
        /// </summary>
        [Test]
        public void CombineChildBounds_ChildRendererWithNonUnitScale_ProducesCorrectLocalScale()
        {
            _root = new GameObject("Root");
            SetupCanvas(_root, out Canvas canvas, out RectTransform uiRt);
            Camera cam = CreateCamera(_root);

            // Reference: cube directly on target, localScale 1.
            GameObject targetDirect = new GameObject("TargetDirect");
            targetDirect.transform.SetParent(_root.transform);
            GameObject directMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            directMesh.transform.SetParent(targetDirect.transform);
            directMesh.transform.localScale = Vector3.one;

            CombineChildBoundsStrategy.TryGetEffectiveMeshSize(targetDirect.transform, out Vector3 meshSizeDirect);
            FrustumFitPlacementFactory.TryCreate(
                uiRt, canvas, cam, 10f, targetDirect.transform, meshSizeDirect,
                FrustumFillMode.Stretch, FrustumFitAxes.XY,
                FrustumFitAnchorRotationMode.PreserveTarget, Vector3.one,
                out FrustumFitAnchorPlacement placementDirect);

            // Case: cube child has 2× local scale — effective mesh is 2× larger.
            GameObject targetChild = new GameObject("TargetChild");
            targetChild.transform.SetParent(_root.transform);
            GameObject childMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            childMesh.transform.SetParent(targetChild.transform);
            childMesh.transform.localScale = new Vector3(2f, 2f, 2f);

            CombineChildBoundsStrategy.TryGetEffectiveMeshSize(targetChild.transform, out Vector3 meshSizeChild);
            FrustumFitPlacementFactory.TryCreate(
                uiRt, canvas, cam, 10f, targetChild.transform, meshSizeChild,
                FrustumFillMode.Stretch, FrustumFitAxes.XY,
                FrustumFitAnchorRotationMode.PreserveTarget, Vector3.one,
                out FrustumFitAnchorPlacement placementChild);

            Assert.That(placementChild.LocalScale.x, Is.EqualTo(placementDirect.LocalScale.x * 0.5f).Within(0.001f),
                "2× child scale must yield half the target localScale on X.");
            Assert.That(placementChild.LocalScale.y, Is.EqualTo(placementDirect.LocalScale.y * 0.5f).Within(0.001f),
                "2× child scale must yield half the target localScale on Y.");
        }

        // ── DirectColliderBoundsStrategy ─────────────────────────────────────────

        [Test]
        public void DirectCollider_ColliderOnSelf_ReturnsEffectiveMeshSize()
        {
            _root = new GameObject("Root");
            GameObject go = new GameObject("WithCollider");
            go.transform.SetParent(_root.transform);
            BoxCollider col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(5.5f, 4f, 0.5f);

            bool ok = DirectColliderBoundsStrategy.TryGetEffectiveMeshSize(go.transform, out Vector3 meshSize);

            Assert.IsTrue(ok);
            Assert.That(meshSize.x, Is.GreaterThan(0f));
            Assert.That(meshSize.y, Is.GreaterThan(0f));
        }

        [Test]
        public void DirectCollider_ColliderOnChild_ReturnsEffectiveMeshSize()
        {
            _root = new GameObject("Root");
            GameObject parent = new GameObject("Parent");
            parent.transform.SetParent(_root.transform);
            GameObject child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            child.AddComponent<BoxCollider>().size = Vector3.one;

            bool ok = DirectColliderBoundsStrategy.TryGetEffectiveMeshSize(parent.transform, out Vector3 meshSize);

            Assert.IsTrue(ok, "DirectCollider must find the first child collider when none exists on self.");
            Assert.That(meshSize.x, Is.GreaterThan(0f));
        }

        [Test]
        public void DirectCollider_NoColliderAnywhere_ReturnsFalse()
        {
            _root = new GameObject("Root");
            GameObject empty = new GameObject("Empty");
            empty.transform.SetParent(_root.transform);

            bool ok = DirectColliderBoundsStrategy.TryGetEffectiveMeshSize(empty.transform, out _);

            Assert.IsFalse(ok);
        }

        [Test]
        public void DirectCollider_ParentHasCollider_ReturnsFalse()
        {
            _root = new GameObject("Root");
            GameObject parentWithCollider = new GameObject("ParentWithCollider");
            parentWithCollider.transform.SetParent(_root.transform);
            parentWithCollider.AddComponent<BoxCollider>().size = Vector3.one;
            GameObject childEmpty = new GameObject("ChildEmpty");
            childEmpty.transform.SetParent(parentWithCollider.transform);

            bool ok = DirectColliderBoundsStrategy.TryGetEffectiveMeshSize(childEmpty.transform, out _);

            Assert.IsFalse(ok, "DirectCollider must not walk up to parent colliders.");
        }

        // ── CombineChildCollidersStrategy ────────────────────────────────────────

        [Test]
        public void CombineChildColliders_SingleChildCollider_ReturnsEffectiveMeshSize()
        {
            _root = new GameObject("Root");
            GameObject parent = new GameObject("Parent");
            parent.transform.SetParent(_root.transform);
            GameObject child = new GameObject("Child");
            child.transform.SetParent(parent.transform);
            child.AddComponent<BoxCollider>().size = new Vector3(5.5f, 4f, 0.5f);

            bool ok = CombineChildCollidersStrategy.TryGetEffectiveMeshSize(parent.transform, out Vector3 meshSize);

            Assert.IsTrue(ok);
            Assert.That(meshSize.x, Is.GreaterThan(0f));
            Assert.That(meshSize.y, Is.GreaterThan(0f));
        }

        [Test]
        public void CombineChildColliders_NoColliders_ReturnsFalse()
        {
            _root = new GameObject("Root");
            GameObject empty = new GameObject("Empty");
            empty.transform.SetParent(_root.transform);

            bool ok = CombineChildCollidersStrategy.TryGetEffectiveMeshSize(empty.transform, out _);

            Assert.IsFalse(ok);
        }

        [Test]
        public void CombineChildColliders_MultipleChildren_EncapsulatesAllBounds()
        {
            _root = new GameObject("Root");
            GameObject parent = new GameObject("Parent");
            parent.transform.SetParent(_root.transform);

            GameObject a = new GameObject("A");
            a.transform.SetParent(parent.transform);
            a.transform.localPosition = new Vector3(-3f, 0f, 0f);
            a.AddComponent<BoxCollider>().size = Vector3.one;

            GameObject b = new GameObject("B");
            b.transform.SetParent(parent.transform);
            b.transform.localPosition = new Vector3(3f, 0f, 0f);
            b.AddComponent<BoxCollider>().size = Vector3.one;

            CombineChildCollidersStrategy.TryGetEffectiveMeshSize(parent.transform, out Vector3 combinedSize);
            DirectColliderBoundsStrategy.TryGetEffectiveMeshSize(a.transform, out Vector3 singleSize);

            Assert.That(combinedSize.x, Is.GreaterThan(singleSize.x),
                "Combined collider bounds of two laterally separated boxes must be wider than one.");
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static void SetupCanvas(GameObject root, out Canvas canvas, out RectTransform uiRt)
        {
            GameObject canvasGo = new GameObject("Canvas", typeof(RectTransform));
            canvasGo.transform.SetParent(root.transform);
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            GameObject uiGo = new GameObject("UI", typeof(RectTransform));
            uiGo.transform.SetParent(canvasGo.transform, false);
            uiRt = uiGo.GetComponent<RectTransform>();
            uiRt.sizeDelta = new Vector2(200f, 200f);
        }

        private static Camera CreateCamera(GameObject root)
        {
            GameObject camGo = new GameObject("Camera");
            camGo.transform.SetParent(root.transform);
            return camGo.AddComponent<Camera>();
        }
    }
}
