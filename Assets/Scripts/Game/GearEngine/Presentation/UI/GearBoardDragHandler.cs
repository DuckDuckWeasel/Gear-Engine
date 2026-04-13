using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.GearEngine.Presentation
{
    [RequireComponent(typeof(BoardView))]
    internal sealed class GearBoardDragHandler : MonoBehaviour
    {
        [Tooltip("Tag identifying the trash zone drop target.")]
        [SerializeField] private TagSO trashZoneTag;

        private BoardView boardView;
        private Camera mainCamera;
        private GearView draggedView;
        private Vector2Int originalGridPos;

        /// <summary>Config data of the gear currently being dragged (null when idle).</summary>
        internal GearConfigData DraggedGearData { get; private set; }

        private void Awake()
        {
            boardView = GetComponent<BoardView>();
            enabled = false;
        }

        private void Start() => mainCamera = Camera.main;

        private void Update()
        {
            if (boardView == null || mainCamera == null)
            {
                return;
            }

            if (boardView.IsRunning())
            {
                return;
            }

            Vector3 worldPos = GetWorldPointerPosition();

            if (IsPointerDown())
            {
                HandlePickup(worldPos);
            }

            if (IsPointerHeld())
            {
                HandleHover(worldPos);
            }

            if (IsPointerUp())
            {
                HandleDrop(worldPos);
            }
        }

        private void HandlePickup(Vector3 worldPos)
        {
            BoardConfigSO boardConfig = boardView.GetBoardConfig();
            if (boardConfig == null)
            {
                return;
            }

            float closestDist = boardConfig.MaxDragGrabDistance;
            GearView closest = null;

            foreach (GearView view in boardView.GetViews())
            {
                if (view == null || view.TargetNode == null || !view.TargetNode.IsInteractable)
                {
                    continue;
                }

                // Gears marked as not movable cannot be picked up or swapped
                if (view.TargetNode.ConfigData != null && !view.TargetNode.ConfigData.IsMovable)
                {
                    continue;
                }

                float dist = Vector2.Distance(
                    new Vector2(view.transform.position.x, view.transform.position.y),
                    new Vector2(worldPos.x, worldPos.y));

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = view;
                }
            }

            if (closest == null)
            {
                return;
            }

            draggedView = closest;
            draggedView.IsBeingDragged = true;
            originalGridPos = closest.TargetNode.Position;
            DraggedGearData = closest.TargetNode.ConfigData;
            boardView.NotifyPickedUp(closest.TargetNode, originalGridPos);
        }

        private void HandleHover(Vector3 worldPos)
        {
            if (draggedView != null)
            {
                draggedView.transform.position = worldPos;
            }
        }

        private void HandleDrop(Vector3 worldPos)
        {
            if (draggedView == null)
            {
                return;
            }

            bool overUI = IsPointerOverUI();

            if (overUI)
            {
                if (EventSystem.current != null)
                {
                    PointerEventData ped = new PointerEventData(EventSystem.current) { position = GetPointerPosition() };
                    var results = new System.Collections.Generic.List<RaycastResult>();
                    EventSystem.current.RaycastAll(ped, results);
                    
                    foreach (var result in results)
                    {
                        bool isTrash = result.gameObject.GetComponentInParent<TrashDropZoneView>() != null;
                        if (!isTrash && trashZoneTag != null)
                        {
                            var tc = result.gameObject.GetComponentInParent<TagComponent>();
                            isTrash = tc != null && tc.HasTag(trashZoneTag);
                        }

                        if (isTrash)
                        {
                            IGridNode returnNode = draggedView.TargetNode;
                            Debug.Log($"<color=#ff9900>[GearBoardDragHandler]</color> Gear '{returnNode?.ConfigData?.Id}' dropped on trash zone. Forwarding to BoardView.");
                            draggedView.IsBeingDragged = false;
                            draggedView = null;
                            DraggedGearData = null;
                            boardView.NotifyTrashDrop(returnNode);
                            return;
                        }
                    }
                }
                IGridNode node = draggedView.TargetNode;
                GearConfigData droppedConfig = node?.ConfigData;

                // Check if this gear is allowed to return to inventory
                if (droppedConfig != null && !droppedConfig.IsReturnable)
                {
                    Debug.Log($"<color=#ff9900>[GearBoardDragHandler]</color> Gear '{droppedConfig.Id}' is not returnable. Snapping back.");
                    boardView.NotifyDropped(draggedView.TargetNode, originalGridPos);
                    draggedView.IsBeingDragged = false;
                    draggedView = null;
                    DraggedGearData = null;
                    return;
                }

                Debug.Log($"<color=#ff9900>[GearBoardDragHandler]</color> Gear '{droppedConfig?.Id}' dropped over UI (not trash). Returning to inventory.");
                draggedView.IsBeingDragged = false;
                DestroyGO(draggedView.gameObject);
                draggedView = null;
                DraggedGearData = null;
                boardView.NotifyBoardGearDroppedOverUI(node, droppedConfig, worldPos);
                return;
            }

            BoardConfigSO cfg = boardView.GetBoardConfig();
            if (cfg == null)
            {
                draggedView.IsBeingDragged = false;
                draggedView = null;
                return;
            }

            Vector2Int targetPos = cfg.GetGridPosition(worldPos);

            // Reject drops outside valid grid bounds — snap back
            if (targetPos.x < 0 || targetPos.x >= cfg.GridWidth
                || targetPos.y < 0 || targetPos.y >= cfg.GridHeight)
            {
                boardView.NotifyDropped(draggedView.TargetNode, originalGridPos);
                draggedView.IsBeingDragged = false;
                draggedView = null;
                DraggedGearData = null;
                return;
            }

            boardView.NotifyDropped(draggedView.TargetNode, targetPos);
            draggedView.IsBeingDragged = false;
            draggedView = null;
            DraggedGearData = null;
        }

        private bool IsPointerDown()
            => Input.GetMouseButtonDown(0)
            || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        private bool IsPointerHeld()
            => Input.GetMouseButton(0)
            || (Input.touchCount > 0
                && (Input.GetTouch(0).phase == TouchPhase.Moved
                    || Input.GetTouch(0).phase == TouchPhase.Stationary));

        private bool IsPointerUp()
            => Input.GetMouseButtonUp(0)
            || (Input.touchCount > 0
                && (Input.GetTouch(0).phase == TouchPhase.Ended
                    || Input.GetTouch(0).phase == TouchPhase.Canceled));

        private Vector3 GetPointerPosition()
            => Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;

        private Vector3 GetWorldPointerPosition()
        {
            Vector3 p = GetPointerPosition();
            p.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 world = mainCamera.ScreenToWorldPoint(p);
            world.z = -1f;
            return world;
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            return Input.touchCount > 0
                ? EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
                : EventSystem.current.IsPointerOverGameObject();
        }

        private static void DestroyGO(GameObject go)
        {
            if (go == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(go);
                return;
            }
#endif
            UnityEngine.Object.Destroy(go);
        }
    }
}
