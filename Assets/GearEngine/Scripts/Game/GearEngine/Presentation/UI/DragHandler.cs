using System;
using GearEngine.GearEngine.Presentation.UI.Tags;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public GameObject GhostPrefab { get => ghostPrefab; set => ghostPrefab = value; }

        [Tooltip("Visual prefab or sprite to show as a ghost while dragging.")]
        [SerializeField] private GameObject ghostPrefab;

        public bool IsInteractable { get; set; } = true;
        public float GhostScaleMultiplier { get; set; } = 115f;

        [Tooltip("The conceptual tags that THIS drag handler is allowed to drop onto.")]
        [SerializeField] private System.Collections.Generic.List<TagSO> acceptedTargetTags;

        public Action<Vector3> OnValidDropWorldPos;
        public Action OnDragBegin;
        public Action OnDragEnd;

        private GameObject currentGhost;
        private Canvas mainCanvas;

        public void AddAcceptedTag(TagSO tag)
        {
            if (tag == null)
            {
                return;
            }

            if (acceptedTargetTags == null)
            {
                acceptedTargetTags = new System.Collections.Generic.List<TagSO>();
            }

            if (!acceptedTargetTags.Contains(tag))
            {
                acceptedTargetTags.Add(tag);
            }
        }

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
            var slotView = currentGhost.GetComponent("GearInventorySlotView");
            if (slotView != null)
            {
                DestroyImmediate(slotView);
            }

            var childDrag = currentGhost.GetComponent<DragHandler>();
            if (childDrag != null)
            {
                DestroyImmediate(childDrag);
            }
        }

        private void ConfigureGhostRaycast()
        {
            currentGhost.transform.SetAsLastSibling();
            var canvasGroup = currentGhost.GetComponent<CanvasGroup>();
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
            }

            OnDragEnd?.Invoke();
            TryProcessWorldDrop();
        }

        private void TryProcessWorldDrop()
        {
            Camera cam = Camera.main;
            if (cam == null || IsDropBlockedByUi())
            {
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log($"<color=#ff5555>[DragHandler]</color> Drop missed! Raycast hit no colliders.");
                return;
            }

            TryInvokeValidTagDrop(hit);
        }

        private bool IsDropBlockedByUi()
        {
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            Debug.Log($"<color=#aaaaaa>[DragHandler]</color> Drop landed on UI element (not a 3D collider). Ignoring 3D drop.");
            return true;
        }

        private void TryInvokeValidTagDrop(RaycastHit hit)
        {
            TagComponent targetTags = hit.collider.GetComponent<TagComponent>();

            if (targetTags != null && targetTags.HasAnyTag(acceptedTargetTags))
            {
                Debug.Log($"<color=#55ff55>[DragHandler]</color> Successfully dropped on valid TagComponent.");
                OnValidDropWorldPos?.Invoke(hit.point);
                return;
            }

            Debug.LogWarning($"<color=#ff5555>[DragHandler]</color> Dropped on invalid object. It either lacks a TagComponent or does not match Accepted Tags.");
        }
    }
}
