using System;
using GearEngine.GearEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Presentation.UI
{
    public sealed class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public bool IsInteractable { get; set; } = true;

        public Func<PointerEventData, DragPayload> BuildPayload;
        public Action<IDragTarget> OnDropAccepted;
        public Action OnDropRejected;

        [Tooltip("Optional. Prefab/GO to clone as the drag preview. If null, clones this GameObject.")]
        [SerializeField]
        private GameObject previewSource;

        [Tooltip("If true, the source GameObject is hidden while the drag is in flight.")]
        [SerializeField]
        private bool hideSourceWhileDragging;

        private IDragService dragService;
        private RectTransform dragOverlay;
        private GameObject preview;
        private Graphic[] hiddenGraphics = Array.Empty<Graphic>();

        public void SetHideSourceWhileDragging(bool hide)
        {
            hideSourceWhileDragging = hide;
        }

        public void SetPreviewSource(GameObject source)
        {
            previewSource = source;
        }

        public void Configure(IDragService service, RectTransform overlay)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (overlay == null)
            {
                throw new ArgumentNullException(nameof(overlay));
            }

            dragService = service;
            dragOverlay = overlay;
        }

        public void OnBeginDrag(PointerEventData e)
        {
            if (!IsInteractable || BuildPayload == null || dragService == null || dragOverlay == null)
            {
                return;
            }

            DragPayload payload = BuildPayload(e);
            GameObject source = previewSource != null ? previewSource : gameObject;
            preview = DragPreview.Spawn(source, dragOverlay);
            DragPreview.MoveTo(preview, e);
            if (hideSourceWhileDragging)
            {
                HideSourceGraphics();
            }

            dragService.StartDrag(payload);
        }

        public void OnDrag(PointerEventData e)
        {
            if (preview != null)
            {
                DragPreview.MoveTo(preview, e);
            }
        }

        public void OnEndDrag(PointerEventData e)
        {
            try
            {
                ProcessDrop(e);
            }
            finally
            {
                TeardownPreviewAndDragService();
            }
        }

        private void ProcessDrop(PointerEventData e)
        {
            DragPayload payload = BuildPayload != null ? BuildPayload(e) : default;
            IDragTarget target = DragTargetFinder.Find(payload, e.position);
            bool consumed = target != null && target.OnDrop(payload);
            if (consumed)
            {
                if (hideSourceWhileDragging && preview != null)
                {
                    transform.position = preview.transform.position;
                }

                OnDropAccepted?.Invoke(target);
            }
            else
            {
                OnDropRejected?.Invoke();
            }
        }

        private void TeardownPreviewAndDragService()
        {
            if (preview != null)
            {
                Destroy(preview);
            }

            preview = null;
            if (hideSourceWhileDragging)
            {
                RestoreSourceGraphics();
            }

            dragService?.EndDrag();
        }

        private void HideSourceGraphics()
        {
            hiddenGraphics = GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < hiddenGraphics.Length; i++)
            {
                Graphic graphic = hiddenGraphics[i];
                if (graphic != null)
                {
                    graphic.enabled = false;
                }
            }
        }

        private void RestoreSourceGraphics()
        {
            for (int i = 0; i < hiddenGraphics.Length; i++)
            {
                Graphic graphic = hiddenGraphics[i];
                if (graphic != null)
                {
                    graphic.enabled = true;
                }
            }

            hiddenGraphics = Array.Empty<Graphic>();
        }
    }
}
