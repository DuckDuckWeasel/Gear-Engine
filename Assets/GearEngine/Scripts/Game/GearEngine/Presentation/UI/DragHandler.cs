using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDragSource
    {
        public GameObject GhostPrefab { get => ghostPrefab; set => ghostPrefab = value; }

        [Tooltip("Visual prefab or sprite to show as a ghost while dragging.")]
        [SerializeField] private GameObject ghostPrefab;

        public bool IsInteractable { get; set; } = true;
        public float GhostScaleMultiplier { get; set; } = 115f;

        public Action OnDragBegin;
        public Action OnDragEnd;

        /// <summary>Builds a <see cref="DragPayload"/> for the current drag at the given world hit position.</summary>
        public Func<Vector3, DragPayload> BuildPayload;

        /// <summary>Invoked when a target accepts the drop (after <see cref="IDragTarget.OnDrop"/>).</summary>
        public Action<IDragTarget> OnDragAccepted;

        private GameObject currentGhost;
        private Canvas mainCanvas;

        private void Start()
        {
            mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogWarning($"<color=#ffaa33>[DragHandler]</color> No Canvas found in scene. Ghost visuals may fail.");
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsInteractable)
            {
                return;
            }

            OnDragBegin?.Invoke();
            TryCreateGhost();
        }

        private void TryCreateGhost()
        {
            if (mainCanvas == null)
            {
                return;
            }

            if (ghostPrefab != null)
            {
                currentGhost = Instantiate(ghostPrefab, mainCanvas.transform);
                ApplyGhostScaleIfNeeded();
            }
            else
            {
                CloneSelfAsGhost();
            }

            ConfigureGhostRaycast();
        }

        private void ApplyGhostScaleIfNeeded()
        {
            if (currentGhost.GetComponent<RectTransform>() == null)
            {
                currentGhost.transform.localScale = new Vector3(GhostScaleMultiplier, GhostScaleMultiplier, GhostScaleMultiplier);
            }
        }

        private void CloneSelfAsGhost()
        {
            currentGhost = Instantiate(gameObject, mainCanvas.transform);
            Component slotView = currentGhost.GetComponent("GearInventorySlotView");
            if (slotView != null)
            {
                DestroyImmediate(slotView);
            }

            DragHandler childDrag = currentGhost.GetComponent<DragHandler>();
            if (childDrag != null)
            {
                DestroyImmediate(childDrag);
            }
        }

        private void ConfigureGhostRaycast()
        {
            currentGhost.transform.SetAsLastSibling();
            CanvasGroup canvasGroup = currentGhost.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = currentGhost.AddComponent<CanvasGroup>();
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.6f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsInteractable || currentGhost == null)
            {
                return;
            }

            UpdateGhostDragPosition(eventData);
        }

        private void UpdateGhostDragPosition(PointerEventData eventData)
        {
            if (mainCanvas != null && CanvasPositionUtility.ScreenToCanvasLocal(mainCanvas, eventData.position, out Vector2 localPoint))
            {
                currentGhost.transform.localPosition = localPoint;
            }
            else
            {
                currentGhost.transform.position = Input.mousePosition;
            }
        }

        public void ForceGhostCleanup()
        {
            if (currentGhost != null)
            {
                Destroy(currentGhost);
                currentGhost = null;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsInteractable)
            {
                return;
            }

            if (currentGhost != null)
            {
                Destroy(currentGhost);
                currentGhost = null;
            }

            try
            {
                TryProcessDrop(eventData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DragHandler] TryProcessDrop failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                OnDragEnd?.Invoke();
            }
        }

        private void TryProcessDrop(PointerEventData eventData)
        {
            Camera cam = Camera.main;
            if (cam == null || BuildPayload == null)
            {
                return;
            }

            Vector2 screenPos = eventData.position;
            Ray ray = cam.ScreenPointToRay(screenPos);
            Vector3 worldPos = Physics.Raycast(ray, out RaycastHit hit) ? hit.point : Vector3.zero;
            DragPayload payload = BuildPayload(worldPos);

            IDragTarget target = DragTargetFinder.Find(payload, screenPos, cam);
            if (target != null)
            {
                target.OnDrop(payload);
            }
            else
            {
                Debug.Log($"<color=#ff5555>[DragHandler]</color> Drop missed — no accepting IDragTarget under pointer.");
            }
        }

        public void OnDropAccepted(IDragTarget by)
        {
            OnDragAccepted?.Invoke(by);
        }

        public void OnDropRejected()
        {
        }
    }
}
