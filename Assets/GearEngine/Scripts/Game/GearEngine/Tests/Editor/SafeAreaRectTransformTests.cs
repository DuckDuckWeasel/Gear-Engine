using GearEngine.GearEngine.Presentation.UI;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class SafeAreaRectTransformTests
    {
        [TestCase(1080, 1920)]
        [TestCase(1080, 2400)]
        [TestCase(1080, 1680)]
        public void CalculateAnchors_FullPortraitResolution_FillsParent(int width, int height)
        {
            SafeAreaRectTransform.ToAnchors(
                new Rect(0f, 0f, width, height),
                new Vector2Int(width, height),
                out Vector2 anchorMin,
                out Vector2 anchorMax);

            Assert.That(anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(anchorMax, Is.EqualTo(Vector2.one));
        }

        [Test]
        public void CalculateAnchors_WithTopAndBottomInsets_NormalizesSafeArea()
        {
            SafeAreaRectTransform.ToAnchors(
                new Rect(0f, 80f, 1080f, 1760f),
                new Vector2Int(1080, 1920),
                out Vector2 anchorMin,
                out Vector2 anchorMax);

            Assert.That(anchorMin.x, Is.EqualTo(0f));
            Assert.That(anchorMin.y, Is.EqualTo(80f / 1920f).Within(0.0001f));
            Assert.That(anchorMax.x, Is.EqualTo(1f));
            Assert.That(anchorMax.y, Is.EqualTo(1840f / 1920f).Within(0.0001f));
        }
    }
}
