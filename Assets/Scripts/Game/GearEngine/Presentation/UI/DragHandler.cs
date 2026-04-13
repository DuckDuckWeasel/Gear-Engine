using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("The conceptual tags that THIS drag handler is allowed to drop onto.")]
        [SerializeField] private System.Collections.Generic.List<TagSO> acceptedTargetTags;
        
        [Tooltip("Visual prefab or sprite to show as a ghost while dragging.")]
        [SerializeField] private GameObject ghostPrefab;

        public GameObject GhostPrefab { get => ghostPrefab; set => ghostPrefab = value; }

        private GameObject currentGhost;
        private Canvas mainCanvas;

        public bool IsInteractable { get; set; } = true;
        public float GhostScaleMultiplier { get; set; } = 115f;

        /// <summary>
        /// Fires when the item is successfully dropped over a valid world collider matching the target tag.
        /// Payload is the world position of the intersection.
        /// </summary>
        public Action<Vector3> OnValidDropWorldPos;

        public void AddAcceptedTag(TagSO tag)
        {
            if (tag == null) return;
            if (acceptedTargetTags == null) acceptedTargetTags = new System.Collections.Generic.List<TagSO>();
            if (!acceptedTargetTags.Contains(tag)) acceptedTargetTags.Add(tag);
        }

        private void Start()
        {
            // Find the active Canvas in scene to put the ghost on top
            mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas == null)
            {
                Debug.LogWarning($"<color=#ffaa33>[DragHandler]</color> No Canvas found in scene. Ghost visuals may fail.");
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!IsInteractable) return;

            if (mainCanvas != null)
            {
                if (ghostPrefab != null)
                {
                    currentGhost = Instantiate(ghostPrefab, mainCanvas.transform);
                    
                    // If it's a native GameObject (like a SpriteRenderer prefab), it needs UI scale and sorting layers over Canvas background
                    if (currentGhost.GetComponent<RectTransform>() == null)
                    {
                        currentGhost.transform.localScale = new Vector3(GhostScaleMultiplier, GhostScaleMultiplier, GhostScaleMultiplier); 
                    }
                }
                else
                {
                    currentGhost = Instantiate(gameObject, mainCanvas.transform);
                    
                    var slotView = currentGhost.GetComponent("GearInventorySlotView");
                    if (slotView != null) DestroyImmediate(slotView);

                    var childDrag = currentGhost.GetComponent<DragHandler>();
                    if (childDrag != null) DestroyImmediate(childDrag);
                    
                    // Note: This relies on the original object having roughly the right size/visuals.
                }

                currentGhost.transform.SetAsLastSibling(); // Render on top of everything
                
                // Disable raycast blocking on the ghost so we can click what's underneath it
                var canvasGroup = currentGhost.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = currentGhost.AddComponent<CanvasGroup>();
                
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0.6f;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!IsInteractable) return;

            if (currentGhost != null)
            {
                if (mainCanvas != null)
                {
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        (RectTransform)mainCanvas.transform,
                        eventData.position,
                        mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera,
                        out Vector2 localPoint))
                    {
                        currentGhost.transform.localPosition = localPoint;
                    }
                }
                else
                {
                    currentGhost.transform.position = Input.mousePosition;
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!IsInteractable) return;

            if (currentGhost != null)
            {
                Destroy(currentGhost);
            }

            // Raycast into the 3D/2D world to find the TargetTag
            Camera cam = Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                
                // We use standard 3D Physics (Raycast) since Test Scene uses 3D camera & colliders
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    TagComponent targetTags = hit.collider.GetComponent<TagComponent>();

                    if (targetTags != null && targetTags.HasAnyTag(acceptedTargetTags))
                    {
                        Debug.Log($"<color=#55ff55>[DragHandler]</color> Successfully dropped on valid TagComponent.");
                        OnValidDropWorldPos?.Invoke(hit.point);
                        return; // Success
                    }
                    else
                    {
                        Debug.LogWarning($"<color=#ff5555>[DragHandler]</color> Dropped on invalid object. It either lacks a TagComponent or does not match Accepted Tags.");
                    }
                }
                else
                {
                    Debug.Log($"<color=#ff5555>[DragHandler]</color> Drop missed! Raycast hit no colliders.");
                }
            }
        }
    }
}
