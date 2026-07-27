using System.Collections.Generic;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class DragTargetFinderUiTests
    {
        private GameObject eventSystemObject;
        private GameObject canvasObject;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(canvasObject);
            Object.DestroyImmediate(eventSystemObject);
        }

        [Test]
        public void FindFirstAccepting_SkipsRejectedUiTargetAndReturnsAcceptingTargetBelow()
        {
            CreateUiEnvironment();
            BoardViewComponent accepted = CreateBoardTarget("Accepted");
            GameObject rejected = CreateRejectedTarget("Rejected");
            List<RaycastResult> results = new List<RaycastResult>
            {
                new RaycastResult { gameObject = rejected },
                new RaycastResult { gameObject = accepted.gameObject },
            };

            IDragTarget result = DragTargetFinder.FindFirstAccepting(
                new DragPayload(new GearItemData(), Vector2.zero),
                results);

            Assert.That(result, Is.SameAs(accepted));
        }

        [Test]
        public void Find_WithoutEventSystem_ReturnsNull()
        {
            IDragTarget result = DragTargetFinder.Find(
                new DragPayload("gear", Vector2.zero),
                Vector2.zero);

            Assert.IsNull(result);
        }

        private void CreateUiEnvironment()
        {
            eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
            canvasObject = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        private BoardViewComponent CreateBoardTarget(string targetName)
        {
            GameObject targetObject = new GameObject(targetName, typeof(RectTransform), typeof(Image));
            RectTransform rect = targetObject.GetComponent<RectTransform>();
            rect.SetParent(canvasObject.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return targetObject.AddComponent<BoardViewComponent>();
        }

        private GameObject CreateRejectedTarget(string targetName)
        {
            GameObject targetObject = new GameObject(targetName, typeof(RectTransform), typeof(Image));
            RectTransform rect = targetObject.GetComponent<RectTransform>();
            rect.SetParent(canvasObject.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return targetObject;
        }
    }
}
