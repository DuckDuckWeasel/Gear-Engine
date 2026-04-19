using System.Reflection;
using GearEngine.GearEngine.Presentation.UI;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class RectContentFitterTests
    {
        private static MethodInfo GetRefitMethod()
        {
            return typeof(RectContentFitter).GetMethod(
                "Refit",
                BindingFlags.Instance | BindingFlags.NonPublic);
        }

        [Test]
        public void Refit_NoChildren_DoesNotThrow()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            var slot = new GameObject("Slot", typeof(RectTransform));
            slot.transform.SetParent(canvasGo.transform, false);
            var rect = slot.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100f, 100f);
            var fitter = slot.AddComponent<RectContentFitter>();

            MethodInfo refit = GetRefitMethod();
            Assert.IsNotNull(refit);
            Assert.DoesNotThrow(() => refit.Invoke(fitter, null));

            Object.DestroyImmediate(canvasGo);
        }

        [Test]
        public void Refit_ScalesChild_WithRenderer_ToNonDefaultScale()
        {
            var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            var slot = new GameObject("Slot", typeof(RectTransform));
            slot.transform.SetParent(canvasGo.transform, false);
            var rect = slot.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100f, 100f);
            var fitter = slot.AddComponent<RectContentFitter>();

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(slot.transform, false);
            cube.transform.localScale = Vector3.one * 2f;

            MethodInfo refit = GetRefitMethod();
            Assert.IsNotNull(refit);
            refit.Invoke(fitter, null);

            Assert.Greater(cube.transform.localScale.x, 0f);
            Assert.Less(cube.transform.localScale.x, 2.01f);

            Object.DestroyImmediate(canvasGo);
        }
    }
}
