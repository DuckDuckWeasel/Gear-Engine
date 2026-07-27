using GearEngine.GearEngine.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class BoardScreenPositionUtilityTests
    {
        private GameObject canvasHost;
        private GameObject boardRootHost;

        [TearDown]
        public void TearDown()
        {
            if (canvasHost != null)
            {
                Object.DestroyImmediate(canvasHost);
                canvasHost = null;
            }

            if (boardRootHost != null)
            {
                Object.DestroyImmediate(boardRootHost);
                boardRootHost = null;
            }
        }

        [Test]
        public void TryGetLocalPoint_OverlayCanvas_ReturnsBoardOriginAtRectCenter()
        {
            Canvas canvas = CreateOverlayCanvas();
            RectTransform boardRoot = CreateBoardRoot(canvas.transform);

            bool projected = BoardScreenPositionUtility.TryGetLocalPoint(
                boardRoot,
                canvas,
                new Vector2(500f, 500f),
                out Vector2 localPoint);

            Assert.IsTrue(projected);
            Assert.That(localPoint.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(localPoint.y, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void TryGetLocalPoint_WithMissingCanvas_ReturnsFalse()
        {
            GameObject board = new GameObject("Board", typeof(RectTransform));
            boardRootHost = board;

            bool projected = BoardScreenPositionUtility.TryGetLocalPoint(
                board.GetComponent<RectTransform>(),
                null,
                Vector2.zero,
                out _);

            Assert.IsFalse(projected);
        }

        private Canvas CreateOverlayCanvas()
        {
            canvasHost = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
            RectTransform canvasRect = canvasHost.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1000f, 1000f);
            Canvas canvas = canvasHost.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            return canvas;
        }

        private RectTransform CreateBoardRoot(Transform parent)
        {
            boardRootHost = new GameObject("BoardRoot", typeof(RectTransform));
            RectTransform boardRect = boardRootHost.GetComponent<RectTransform>();
            boardRect.SetParent(parent, false);
            boardRect.sizeDelta = new Vector2(400f, 300f);
            boardRect.position = new Vector3(500f, 500f, 0f);
            return boardRect;
        }
    }
}
