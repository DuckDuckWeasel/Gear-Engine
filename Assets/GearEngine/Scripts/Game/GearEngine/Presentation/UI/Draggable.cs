using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    public sealed class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private static bool warnedMissingDragServiceRegistry;

        public bool IsInteractable { get; set; } = true;

        /// <summary>
        /// When non-null, the drag preview is parented here instead of <see cref="Component.transform"/>'s parent
        /// (e.g. board space root for grid gears so the preview does not follow the cell slot).
        /// </summary>
        public Transform PreviewParent { get; set; }

        public Func<PointerEventData, DragPayload> BuildPayload;
        public Action<IDragTarget> OnDropAccepted;
        public Action OnDropRejected;

        [Tooltip("Optional. Prefab/GO to clone as the drag preview. If null, clones this GameObject.")]
        [SerializeField]
        private GameObject previewSource;

        [Tooltip("If true, the source GameObject is hidden while the drag is in flight.")]
        [SerializeField]
        private bool hideSourceWhileDragging;

        public void SetHideSourceWhileDragging(bool hide)
        {
            hideSourceWhileDragging = hide;
        }

        private GameObject preview;
        private bool wasActive;

        public void OnBeginDrag(PointerEventData e)
        {
            if (!IsInteractable || BuildPayload == null)
            {
                return;
            }

            DragPayload payload = BuildPayload(e);
            GameObject source = previewSource != null ? previewSource : gameObject;
            Transform parent = ResolvePreviewParent();
            preview = DragPreview.Spawn(source, parent);
            DragPreview.MoveTo(preview, e);
            if (hideSourceWhileDragging)
            {
                wasActive = gameObject.activeSelf;
                gameObject.SetActive(false);
            }

            if (DragServiceRegistry.Instance == null)
            {
                if (!warnedMissingDragServiceRegistry)
                {
                    warnedMissingDragServiceRegistry = true;
                    Debug.LogWarning(
                        "[Draggable] DragServiceRegistry.Instance is null. Call DragServiceRegistry.Register(IDragService) " +
                        "from the scene root view during OnBind so drag lifecycle listeners (e.g. trash zone) run.");
                }
            }
            else
            {
                DragServiceRegistry.Instance.StartDrag(payload);
            }
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
            Camera cam = Camera.main;
            IDragTarget target = cam != null ? DragTargetFinder.Find(payload, e.position, cam) : null;
            bool consumed = target != null && target.OnDrop(payload);
            if (consumed)
            {
                OnDropAccepted?.Invoke(target);
            }
            else
            {
                OnDropRejected?.Invoke();
            }
        }

        private Transform ResolvePreviewParent()
        {
            if (PreviewParent != null)
            {
                return PreviewParent;
            }

            // Default: the root canvas, so the preview floats above any layout groups
            // along the source's ancestor chain (otherwise the preview becomes a phantom
            // laid-out child and its position is overwritten every layout pass).
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                return canvas.rootCanvas.transform;
            }

            return transform.parent;
        }

        private void TeardownPreviewAndDragService()
        {
            if (preview != null)
            {
                Destroy(preview);
            }

            preview = null;
            if (hideSourceWhileDragging && gameObject != null)
            {
                gameObject.SetActive(wasActive);
            }

            DragServiceRegistry.Instance?.EndDrag();
        }
    }
}
