using System.Collections;
using GearEngine.GearEngine.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Tests.Runtime
{
    [TestFixture]
    public sealed class ScreenSpaceDragPlayModeTests
    {
        private sealed class RecordingDragService : IDragService
        {
            public bool IsDragging { get; private set; }

            public int StartedCount { get; private set; }

            public int EndedCount { get; private set; }

            public void Register(IDragLifecycleListener listener)
            {
            }

            public void Unregister(IDragLifecycleListener listener)
            {
            }

            public void StartDrag(DragPayload payload)
            {
                IsDragging = true;
                StartedCount++;
            }

            public void EndDrag()
            {
                IsDragging = false;
                EndedCount++;
            }
        }

        private GameObject eventSystemObject;
        private GameObject canvasObject;
        private RectTransform dragOverlay;
        private Draggable source;
        private RecordingDragService dragService;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            eventSystemObject = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
            canvasObject = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            dragOverlay = CreateFullScreenRect("DragOverlay", canvasObject.transform);
            dragOverlay.SetAsLastSibling();

            RectTransform sourceRect = CreateFullScreenRect("DragSource", canvasObject.transform);
            sourceRect.gameObject.AddComponent<Image>();
            source = sourceRect.gameObject.AddComponent<Draggable>();
            dragService = new RecordingDragService();
            source.Configure(dragService, dragOverlay);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(canvasObject);
            Object.Destroy(eventSystemObject);
            yield return null;
        }

        [UnityTest]
        [TestCase("Inventory", "Board", ExpectedResult = null)]
        [TestCase("Board", "Board", ExpectedResult = null)]
        [TestCase("Board", "Inventory", ExpectedResult = null)]
        [TestCase("Board", "Trash", ExpectedResult = null)]
        public IEnumerator DragBetweenUiZones_DeliversScreenPosition(
            string sourceName,
            string targetName)
        {
            Vector2 pointerPosition = ScreenCenter();
            ScreenSpaceDragTargetStub target = CreateTarget(targetName, acceptsPayload: true);
            object payloadData = new object();
            source.gameObject.name = sourceName;
            source.BuildPayload = eventData => new DragPayload(payloadData, eventData.position);
            bool accepted = false;
            source.OnDropAccepted = _ => accepted = true;
            Canvas.ForceUpdateCanvases();
            yield return null;

            PointerEventData pointer = CreatePointer(pointerPosition);
            source.OnBeginDrag(pointer);
            source.OnEndDrag(pointer);
            yield return null;

            Assert.IsTrue(accepted);
            Assert.That(target.LastPayload.Data, Is.SameAs(payloadData));
            Assert.That(target.LastPayload.ScreenPosition, Is.EqualTo(pointerPosition));
            Assert.That(dragService.StartedCount, Is.EqualTo(1));
            Assert.That(dragService.EndedCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator InvalidDrop_IsRejectedAndDragIsCancelled()
        {
            _ = CreateTarget("Invalid", acceptsPayload: false);
            source.BuildPayload = eventData => new DragPayload("gear", eventData.position);
            bool rejected = false;
            source.OnDropRejected = () => rejected = true;

            PointerEventData pointer = CreatePointer(ScreenCenter());
            source.OnBeginDrag(pointer);
            source.OnEndDrag(pointer);
            yield return null;

            Assert.IsTrue(rejected);
            Assert.IsFalse(dragService.IsDragging);
            Assert.That(dragService.EndedCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator DropWithoutTarget_CancelsDragAndRestoresLifecycle()
        {
            source.BuildPayload = eventData => new DragPayload("gear", eventData.position);
            bool rejected = false;
            source.OnDropRejected = () => rejected = true;

            PointerEventData pointer = CreatePointer(ScreenCenter());
            source.OnBeginDrag(pointer);
            source.OnEndDrag(pointer);
            yield return null;

            Assert.IsTrue(rejected);
            Assert.IsFalse(dragService.IsDragging);
            Assert.That(dragService.StartedCount, Is.EqualTo(1));
            Assert.That(dragService.EndedCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator ReadOnlySource_DoesNotStartDrag()
        {
            source.IsInteractable = false;
            source.BuildPayload = eventData => new DragPayload("gear", eventData.position);

            source.OnBeginDrag(CreatePointer(ScreenCenter()));
            yield return null;

            Assert.IsFalse(dragService.IsDragging);
            Assert.That(dragService.StartedCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator ExplicitPreviewSource_RendersGearAtSourceSize()
        {
            RectTransform gearVisual = CreateFixedRect(
                "GearVisual",
                source.transform,
                new Vector2(96f, 80f));
            gearVisual.gameObject.AddComponent<Image>();
            source.SetPreviewSource(gearVisual.gameObject);
            source.BuildPayload =
                eventData => new DragPayload("gear", eventData.position);

            source.OnBeginDrag(CreatePointer(ScreenCenter()));
            yield return null;

            Transform preview = dragOverlay.Find("GearVisual_DragPreview");
            Assert.IsNotNull(preview);
            RectTransform previewRect = (RectTransform)preview;
            Assert.That(previewRect.rect.width, Is.EqualTo(96f).Within(0.01f));
            Assert.That(previewRect.rect.height, Is.EqualTo(80f).Within(0.01f));
            Assert.IsTrue(preview.GetComponent<Image>().enabled);

            source.OnEndDrag(CreatePointer(ScreenCenter()));
            yield return null;
        }

        private ScreenSpaceDragTargetStub CreateTarget(string targetName, bool acceptsPayload)
        {
            RectTransform targetRect = CreateFullScreenRect(targetName, canvasObject.transform);
            targetRect.gameObject.AddComponent<Image>();
            ScreenSpaceDragTargetStub target =
                targetRect.gameObject.AddComponent<ScreenSpaceDragTargetStub>();
            target.AcceptsPayload = acceptsPayload;
            targetRect.SetAsLastSibling();
            dragOverlay.SetAsLastSibling();
            return target;
        }

        private PointerEventData CreatePointer(Vector2 position)
        {
            return new PointerEventData(EventSystem.current)
            {
                position = position,
            };
        }

        private static RectTransform CreateFullScreenRect(string objectName, Transform parent)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static RectTransform CreateFixedRect(
            string objectName,
            Transform parent,
            Vector2 size)
        {
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            return rect;
        }

        private static Vector2 ScreenCenter()
        {
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }
    }
}
