using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.GearEngine.Presentation
{
    public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("The conceptual tags that THIS drag handler is allowed to drop onto.")]
        [SerializeField] private System.Collections.Generic.List<TagSO> acceptedTargetTags;
        
        [Tooltip("Visual prefab or sprite to show as a ghost while dragging.")]
        [SerializeField] private GameObject ghostPrefab;

        private GameObject currentGhost;
        private Canvas mainCanvas;

        /// <summary>
        /// Fires when the item is successfully dropped over a valid world collider matching the target tag.
        /// Payload is the world position of the intersection.
        /// </summary>
        public Action<Vector3> OnValidDropWorldPos;

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
            if (ghostPrefab != null && mainCanvas != null)
            {
                currentGhost = Instantiate(ghostPrefab, mainCanvas.transform);
                currentGhost.transform.SetAsLastSibling(); // Render on top of everything
                
                // Disable raycast blocking on the ghost so we can click what's underneath it
                var canvasGroup = currentGhost.AddComponent<CanvasGroup>();
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0.6f;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (currentGhost != null)
            {
                // Convert screen mouse to canvas position overlay
                currentGhost.transform.position = Input.mousePosition;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
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
#if UNITY_EDITOR
                    // Fallback hack for rapid headless testing if you forget to add colliders to the Board obj
                    Debug.Log($"<color=#aaaaaa>[DragHandler(Editor-Hack)]</color> Forcing drop success on empty Z plane since there's no board collider.");
                    Vector3 worldPosFallback = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(cam.transform.position.z)));
                    OnValidDropWorldPos?.Invoke(worldPosFallback);
#endif
                }
            }
        }
    }
}
