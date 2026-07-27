using GearEngine.GearEngine.Editor;
using GearEngine.GearEngine.Presentation.UI.Tags.Highlight;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    public class TutorialFocusLayoutTests
    {
        [Test]
        public void DirectionOffset_UsesTheSameScreenDistanceAsThePreview()
        {
            Vector2 anchorPosition = new Vector2(200f, 300f);
            Vector2 centerPosition = new Vector2(200f, 200f);

            Vector2 runtimePosition = TutorialFocusService.CalculateIndicatorScreenPosition(
                anchorPosition,
                centerPosition,
                Vector2.zero,
                3f);
            float previewDistance = 3f *
                                    FocusPresetSOEditor.GetPreviewOffsetScale(2f) *
                                    2f;

            Assert.That(runtimePosition, Is.EqualTo(new Vector2(200f, 360f)));
            Assert.That(
                Vector2.Distance(anchorPosition, runtimePosition),
                Is.EqualTo(previewDistance).Within(0.001f));
        }

        [Test]
        public void PositionOffset_IsAppliedDirectlyInScreenSpace()
        {
            Vector2 runtimePosition = TutorialFocusService.CalculateIndicatorScreenPosition(
                new Vector2(100f, 100f),
                new Vector2(100f, 50f),
                new Vector2(2f, -1f),
                0f);

            Assert.That(runtimePosition, Is.EqualTo(new Vector2(140f, 80f)));
        }
    }
}
