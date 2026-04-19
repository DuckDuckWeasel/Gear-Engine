using System.Collections.Generic;
using DG.Tweening;
using GearEngine.FrustumFit;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class FrustumFitAnchorOpenTransitionTests
    {
        [Test]
        public void Play_SingleAnchorNull_ReturnsNull()
        {
            Assert.IsNull(FrustumFitAnchorOpenTransition.Play((FrustumFitAnchor)null, 0.35f));
        }

        [Test]
        public void Play_NullList_ReturnsNull()
        {
            Assert.IsNull(FrustumFitAnchorOpenTransition.Play((IReadOnlyList<FrustumFitAnchor>)null, 0.35f));
        }

        [Test]
        public void Play_EmptyList_ReturnsNull()
        {
            Assert.IsNull(FrustumFitAnchorOpenTransition.Play(new List<FrustumFitAnchor>(), 0.35f));
        }

        [Test]
        public void Play_AllNullAnchorsInList_ReturnsNull()
        {
            IReadOnlyList<FrustumFitAnchor> list = new FrustumFitAnchor[] { null, null };
            Assert.IsNull(FrustumFitAnchorOpenTransition.Play(list, 0.35f));
        }

        [Test]
        public void Play_ParamsWithNoArgs_ReturnsNull()
        {
            Assert.IsNull(FrustumFitAnchorOpenTransition.Play(0.35f, Ease.InOutQuad));
        }

        [Test]
        public void PlayAfterCanvasLayout_NullHost_InvokesOnCompleteOnce()
        {
            int hits = 0;
            FrustumFitAnchorOpenTransition.PlayAfterCanvasLayout(null, new List<FrustumFitAnchor>(), 0.35f, () => hits++);
            Assert.AreEqual(1, hits);
        }

        [Test]
        public void PlayAfterCanvasLayout_EmptyAnchorListWithHost_InvokesOnCompleteOnce()
        {
            var go = new GameObject("FrustumFitTestHost");
            var host = go.AddComponent<FrustumFitAnchorOpenTransitionRunner>();
            int hits = 0;
            FrustumFitAnchorOpenTransition.PlayAfterCanvasLayout(host, new List<FrustumFitAnchor>(), 0.35f, () => hits++);
            Assert.AreEqual(1, hits);
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
