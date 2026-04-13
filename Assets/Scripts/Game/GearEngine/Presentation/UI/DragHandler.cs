using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public GameObject GhostPrefab { get => ghostPrefab; set => ghostPrefab = value; }

        [SerializeField]
        private GameObject ghostPrefab;

        public bool IsInteractable { get; set; } = true;
        public float GhostScaleMultiplier { get; set; } = 115f;

        [Tooltip("The conceptual tags that THIS drag handler is allowed to drop onto.")]
        [SerializeField]
        private System.Collections.Generic.List<TagSO> acceptedTargetTags;

        public Action<Vector3> OnValidDropWorldPos;

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
            if (!IsInteractable || mainCanvas == null)
            {
                return;
            }

            CreateDragGhost();
            ConfigureGhostForDrag();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsInteractable || currentGhost == null)
            {
                return;
            }

            if (mainCanvas != null)
            {
                TryMoveGhostToCanvasLocalPoint(eventData);
            }
            else
            {
                currentGhost.transform.position = Input.mousePosition;
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

            TryCompleteWorldDrop();
        }

        private void CreateDragGhost()
        {
            if (ghostPrefab != null)
            {
                currentGhost = Instantiate(ghostPrefab, mainCanvas.transform);
                if (currentGhost.GetComponent<RectTransform>() == null)
                {
                    currentGhost.transform.localScale = new Vector3(GhostScaleMultiplier, GhostScaleMultiplier, GhostScaleMultiplier);
                }

                return;
            }

            currentGhost = Instantiate(gameObject, mainCanvas.transform);
            StripInventoryBehaviorsFromGhost(currentGhost);
        }

        private void ConfigureGhostForDrag()
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

        private void StripInventoryBehaviorsFromGhost(GameObject ghost)
        {
            Component slotView = ghost.GetComponent("GearInventorySlotView");
            if (slotView != null)
            {
                DestroyImmediate(slotView);
            }

            DragHandler childDrag = ghost.GetComponent<DragHandler>();
            if (childDrag != null)
            {
                DestroyImmediate(childDrag);
            }
        }

        private void TryMoveGhostToCanvasLocalPoint(PointerEventData eventData)
        {
            Camera eventCam = mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)mainCanvas.transform, eventData.position, eventCam, out Vector2 localPoint))
            {
                currentGhost.transform.localPosition = localPoint;
            }
        }

        private void TryCompleteWorldDrop()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                LogDropMissed();
                return;
            }

            TryNotifyValidTagDrop(hit);
        }

        private void LogDropMissed()
        {
            Debug.Log($"<color=#ff5555>[DragHandler]</color> Drop missed! Raycast hit no colliders.");
        }

        private void TryNotifyValidTagDrop(RaycastHit hit)
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
