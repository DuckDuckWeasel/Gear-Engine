using System;
using NUnit.Framework;
using UnityEngine;
using Scaffold.GearEngine.Presentation.World;

namespace Scaffold.GearEngine.Tests.Editor
{
    /// <summary>
    /// EditMode unit tests for FrustumFitMath and FrustumBounds.
    /// All tests are pure-math: no Camera, Renderer, or scene objects required.
    /// </summary>
    [TestFixture]
    public class FrustumFitMathTests
    {
        private const float Tolerance = 0.001f;

        // --- FrustumBounds.FromCamera -----------------------------------------

        [Test]
        public void PerspectiveBounds_KnownFovAspectDepth_ReturnsExpectedDimensions()
        {
            // FOV 60°, aspect 16/9, depth 10
            // height = 2 * tan(30°) * 10 = 2 * 0.57735 * 10 ≈ 11.547
            // width  = 11.547 * (16/9) ≈ 20.528
            float fov    = 60f;
            float aspect = 16f / 9f;
            float depth  = 10f;

            FrustumBounds bounds = FrustumBounds.FromCamera(false, 0f, fov, aspect, depth);

            float expectedHeight = 2f * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * depth;
            float expectedWidth  = expectedHeight * aspect;

            Assert.AreEqual(expectedWidth,  bounds.Width,  Tolerance, "Perspective width mismatch.");
            Assert.AreEqual(expectedHeight, bounds.Height, Tolerance, "Perspective height mismatch.");
        }

        [Test]
        public void OrthographicBounds_DepthHasNoEffect_WidthHeightMatchOrthographicSize()
        {
            float orthoSize = 5f;
            float aspect    = 16f / 9f;

            FrustumBounds boundsDepth1  = FrustumBounds.FromCamera(true, orthoSize, 60f, aspect, 1f);
            FrustumBounds boundsDepth50 = FrustumBounds.FromCamera(true, orthoSize, 60f, aspect, 50f);

            float expectedHeight = orthoSize * 2f;
            float expectedWidth  = expectedHeight * aspect;

            Assert.AreEqual(expectedWidth,  boundsDepth1.Width,   Tolerance, "Ortho width at depth 1.");
            Assert.AreEqual(expectedHeight, boundsDepth1.Height,  Tolerance, "Ortho height at depth 1.");
            Assert.AreEqual(boundsDepth1.Width,  boundsDepth50.Width,  Tolerance, "Ortho width must be depth-independent.");
            Assert.AreEqual(boundsDepth1.Height, boundsDepth50.Height, Tolerance, "Ortho height must be depth-independent.");
        }

        // --- FrustumFitMath.ComputeLocalScale — fill modes -------------------

        /// <summary>
        /// Builds a FrustumBounds with known Width and Height for use in scale tests.
        /// </summary>
        private static FrustumBounds MakeBounds(float w, float h) => new FrustumBounds(w, h);

        [Test]
        public void Stretch_DifferentFillXY_ScalesAxesIndependently()
        {
            // frustum 20 x 10, fill 0.5 x 1.0, mesh 1x1, no parent scale
            var bounds   = MakeBounds(20f, 10f);
            var meshSize = new Vector2(1f, 1f);

            Vector2 result = FrustumFitMath.ComputeLocalScale(
                bounds, 0.5f, 1.0f, FrustumFillMode.Stretch, meshSize, Vector2.one);

            Assert.AreEqual(10f, result.x, Tolerance, "Stretch X should be 20 * 0.5 / 1.");
            Assert.AreEqual(10f, result.y, Tolerance, "Stretch Y should be 10 * 1.0 / 1.");
        }

        [Test]
        public void Fit_WiderMesh_UniformScaleEqualsMinRawScale()
        {
            // frustum 20x10, fill 1x1, mesh 2x1 — rawX=10, rawY=10 → min=10
            var bounds   = MakeBounds(20f, 10f);
            var meshSize = new Vector2(2f, 1f);

            Vector2 result = FrustumFitMath.ComputeLocalScale(
                bounds, 1f, 1f, FrustumFillMode.Fit, meshSize, Vector2.one);

            float rawX = 20f / 2f; // 10
            float rawY = 10f / 1f; // 10
            float expected = Mathf.Min(rawX, rawY); // 10

            Assert.AreEqual(expected, result.x, Tolerance, "Fit X must equal min(rawX, rawY).");
            Assert.AreEqual(expected, result.y, Tolerance, "Fit Y must equal min(rawX, rawY).");
            Assert.AreEqual(result.x, result.y, Tolerance, "Fit must produce uniform scale.");
        }

        [Test]
        public void Fit_TallerFrustum_ScaleConstrainedByNarrowerAxis()
        {
            // frustum 10x20, fill 1x1, mesh 1x1 — rawX=10, rawY=20 → min=10
            var bounds   = MakeBounds(10f, 20f);
            var meshSize = new Vector2(1f, 1f);

            Vector2 result = FrustumFitMath.ComputeLocalScale(
                bounds, 1f, 1f, FrustumFillMode.Fit, meshSize, Vector2.one);

            Assert.AreEqual(10f, result.x, Tolerance);
            Assert.AreEqual(10f, result.y, Tolerance);
        }

        [Test]
        public void Fill_WiderMesh_UniformScaleEqualsMaxRawScale()
        {
            // frustum 20x10, fill 1x1, mesh 2x1 — rawX=10, rawY=10 → max=10
            // Use asymmetric frustum to make max meaningful:
            // frustum 20x10, mesh 1x1 — rawX=20, rawY=10 → max=20
            var bounds   = MakeBounds(20f, 10f);
            var meshSize = new Vector2(1f, 1f);

            Vector2 result = FrustumFitMath.ComputeLocalScale(
                bounds, 1f, 1f, FrustumFillMode.Fill, meshSize, Vector2.one);

            float expected = Mathf.Max(20f, 10f); // 20

            Assert.AreEqual(expected, result.x, Tolerance, "Fill X must equal max(rawX, rawY).");
            Assert.AreEqual(expected, result.y, Tolerance, "Fill Y must equal max(rawX, rawY).");
            Assert.AreEqual(result.x, result.y, Tolerance, "Fill must produce uniform scale.");
        }

        [Test]
        public void FillWidth_ScalesXToTargetWidth_YFollowsProportionally()
        {
            // frustum 20x10, fill 1x1, mesh 2x2 — rawX=10
            var bounds   = MakeBounds(20f, 10f);
            var meshSize = new Vector2(2f, 2f);

            Vector2 result = FrustumFitMath.ComputeLocalScale(
                bounds, 1f, 1f, FrustumFillMode.FillWidth, meshSize, Vector2.one);

            float rawX = 20f / 2f; // 10
            Assert.AreEqual(rawX, result.x, Tolerance, "FillWidth X must match raw width scale.");
            Assert.AreEqual(rawX, result.y, Tolerance, "FillWidth Y must equal X (proportional).");
        }

        [Test]
        public void FillHeight_ScalesYToTargetHeight_XFollowsProportionally()
        {
            // frustum 20x10, fill 1x1, mesh 2x2 — rawY=5
            var bounds   = MakeBounds(20f, 10f);
            var meshSize = new Vector2(2f, 2f);

            Vector2 result = FrustumFitMath.ComputeLocalScale(
                bounds, 1f, 1f, FrustumFillMode.FillHeight, meshSize, Vector2.one);

            float rawY = 10f / 2f; // 5
            Assert.AreEqual(rawY, result.x, Tolerance, "FillHeight X must equal Y (proportional).");
            Assert.AreEqual(rawY, result.y, Tolerance, "FillHeight Y must match raw height scale.");
        }

        // --- Parent lossyScale correction ------------------------------------

        [Test]
        public void ParentLossyScale_HalvesAndThirdsOutput_RelativeToIdentityParent()
        {
            var bounds   = MakeBounds(20f, 10f);
            var meshSize = new Vector2(1f, 1f);

            Vector2 noParent   = FrustumFitMath.ComputeLocalScale(
                bounds, 1f, 1f, FrustumFillMode.Stretch, meshSize, Vector2.one);

            Vector2 withParent = FrustumFitMath.ComputeLocalScale(
                bounds, 1f, 1f, FrustumFillMode.Stretch, meshSize, new Vector2(2f, 3f));

            Assert.AreEqual(noParent.x / 2f, withParent.x, Tolerance, "Parent scale x=2 must halve localScale.x.");
            Assert.AreEqual(noParent.y / 3f, withParent.y, Tolerance, "Parent scale y=3 must reduce localScale.y to one-third.");
        }

        // --- Guard clauses ---------------------------------------------------

        [Test]
        public void ZeroMeshSizeX_ThrowsArgumentException()
        {
            var bounds = MakeBounds(20f, 10f);
            Assert.Throws<ArgumentException>(() =>
                FrustumFitMath.ComputeLocalScale(
                    bounds, 1f, 1f, FrustumFillMode.Stretch,
                    new Vector2(0f, 1f), Vector2.one));
        }

        [Test]
        public void ZeroMeshSizeY_ThrowsArgumentException()
        {
            var bounds = MakeBounds(20f, 10f);
            Assert.Throws<ArgumentException>(() =>
                FrustumFitMath.ComputeLocalScale(
                    bounds, 1f, 1f, FrustumFillMode.Stretch,
                    new Vector2(1f, 0f), Vector2.one));
        }

        [Test]
        public void ZeroParentLossyScaleX_ThrowsArgumentException()
        {
            var bounds = MakeBounds(20f, 10f);
            Assert.Throws<ArgumentException>(() =>
                FrustumFitMath.ComputeLocalScale(
                    bounds, 1f, 1f, FrustumFillMode.Stretch,
                    new Vector2(1f, 1f), new Vector2(0f, 1f)));
        }

        [Test]
        public void ZeroParentLossyScaleY_ThrowsArgumentException()
        {
            var bounds = MakeBounds(20f, 10f);
            Assert.Throws<ArgumentException>(() =>
                FrustumFitMath.ComputeLocalScale(
                    bounds, 1f, 1f, FrustumFillMode.Stretch,
                    new Vector2(1f, 1f), new Vector2(1f, 0f)));
        }
    }
}
