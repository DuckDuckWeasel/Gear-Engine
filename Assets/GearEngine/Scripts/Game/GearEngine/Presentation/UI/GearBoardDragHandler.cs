using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation.UI.Tags;
using GearEngine.GearEngine.Visuals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    [RequireComponent(typeof(BoardView))]
    internal sealed class GearBoardDragHandler : MonoBehaviour
    {
        internal GearConfigData DraggedGearData
        {
            get;
            private set;
        }

        [Tooltip("Tag identifying the trash zone drop target.")]
        [SerializeField] private TagSO trashZoneTag;

        private BoardView boardView;
        private Camera mainCamera;
        private GearView draggedView;
        private Vector2Int originalGridPos;

        private void Awake()
        {
            boardView = GetComponent<BoardView>();
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            EnsureMainCamera();
            LogPointerAttempt();
            if (!CanProcessInput())
            {
                return;
            }

            Vector3 worldPos = GetWorldPointerPosition();
            TryHandlePickup(worldPos);
            TryHandleHover(worldPos);
            TryHandleDrop(worldPos);
        }

        private void EnsureMainCamera()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }

        private void LogPointerAttempt()
        {
            if (!IsPointerDown())
            {
                return;
            }

            bool isRunning = boardView != null && boardView.IsRunning();
            Debug.Log($"<color=#ff00ff>[GearBoardDragHandler]</color> CLICK_OR_TOUCH! HasBoardView={boardView != null}, HasCamera={mainCamera != null}, IsRunning={isRunning}, HasDraggedView={draggedView != null}");
        }

        private bool CanProcessInput()
        {
            return boardView != null && mainCamera != null && !boardView.IsRunning();
        }

        private void TryHandlePickup(Vector3 worldPos)
        {
            if (IsPointerDown() && draggedView == null)
            {
                HandlePickup(worldPos);
            }
        }

        private void TryHandleHover(Vector3 worldPos)
        {
            if (IsPointerHeld())
            {
                HandleHover(worldPos);
            }
        }

        private void TryHandleDrop(Vector3 worldPos)
        {
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
                Debug.LogWarning("[GearBoardDragHandler] HandlePickup failed: BoardConfig is null.");
                return;
            }

            GearView closest = FindClosestView(worldPos, boardConfig.MaxDragGrabDistance, out int viewCount);
            if (closest == null)
            {
                Debug.Log($"[GearBoardDragHandler] HandlePickup failed: No movable gear found within {boardConfig.MaxDragGrabDistance} of Pointer at {worldPos}. Evaluated {viewCount} views.");
                return;
            }

            BeginDrag(closest);
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

            GetDropTargetFlags(out bool overTrash, out bool overInventory);
            if (TryHandleSpecialDrop(worldPos, overTrash, overInventory))
            {
                return;
            }

            HandleBoardDrop(worldPos);
        }

        private GearView FindClosestView(Vector3 worldPos, float maxDistance, out int viewCount)
        {
            GearView closest = null;
            viewCount = 0;
            float closestDist = maxDistance;
            foreach (GearView view in boardView.GetViews())
            {
                viewCount++;
                if (!IsPickupCandidate(view))
                {
                    continue;
                }

                closestDist = TrySelectCloserView(worldPos, view, closest, closestDist, out closest);
            }

            return closest;
        }

        private bool IsPickupCandidate(GearView view)
        {
            return HasTargetNode(view) && IsInteractable(view) && IsMovable(view);
        }

        private float TrySelectCloserView(Vector3 worldPos, GearView view, GearView currentClosest, float closestDist, out GearView nextClosest)
        {
            float dist = Vector2.Distance(new Vector2(view.transform.position.x, view.transform.position.y), new Vector2(worldPos.x, worldPos.y));
            Debug.Log($"[GearBoardDragHandler] Evaluating '{view.TargetNode.ConfigData?.Id}' at distance {dist} (Allowed: {closestDist})");
            if (dist < closestDist)
            {
                nextClosest = view;
                return dist;
            }

            nextClosest = currentClosest;
            return closestDist;
        }

        private void BeginDrag(GearView closest)
        {
            Debug.Log($"<color=#00ff00>[GearBoardDragHandler]</color> Successfully Picked up '{closest.TargetNode.ConfigData?.Id}'!");
            draggedView = closest;
            draggedView.enabled = false;
            originalGridPos = closest.TargetNode.Position;
            DraggedGearData = closest.TargetNode.ConfigData;
            boardView.NotifyPickedUp(closest.TargetNode, originalGridPos);
        }

        private void GetDropTargetFlags(out bool overTrash, out bool overInventory)
        {
            overTrash = false;
            overInventory = false;
            if (EventSystem.current == null)
            {
                return;
            }

            PointerEventData ped = new PointerEventData(EventSystem.current) { position = GetPointerPosition() };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);
            EvaluateDropTargets(results, ref overTrash, ref overInventory);
        }

        private void EvaluateDropTargets(System.Collections.Generic.List<RaycastResult> results, ref bool overTrash, ref bool overInventory)
        {
            foreach (RaycastResult result in results)
            {
                bool isTrash = IsTrashResult(result);
                if (isTrash)
                {
                    overTrash = true;
                    return;
                }

                if (IsInventoryResult(result))
                {
                    overInventory = true;
                }
            }
        }

        private bool IsTrashResult(RaycastResult result)
        {
            bool isTrash = result.gameObject.GetComponentInParent<TrashDropZoneView>() != null;
            if (!isTrash && trashZoneTag != null)
            {
                TagComponent tc = result.gameObject.GetComponentInParent<TagComponent>();
                isTrash = tc != null && tc.HasTag(trashZoneTag);
            }

            return isTrash;
        }

        private bool IsInventoryResult(RaycastResult result)
        {
            if (result.gameObject.GetComponentInParent<GearInventoryView>() != null || result.gameObject.name == "ItemsContainer")
            {
                return true;
            }

            UnityEngine.UI.Image image = result.gameObject.GetComponent<UnityEngine.UI.Image>();
            return result.gameObject.layer == LayerMask.NameToLayer("UI") && image != null && image.color.a > 0.05f;
        }

        private bool TryHandleSpecialDrop(Vector3 worldPos, bool overTrash, bool overInventory)
        {
            if (overTrash)
            {
                IGridNode returnNode = draggedView.TargetNode;
                Debug.Log($"<color=#ff9900>[GearBoardDragHandler]</color> Gear '{returnNode?.ConfigData?.Id}' dropped on trash zone. Forwarding to BoardView.");
                ResetDraggedView();
                boardView.NotifyTrashDrop(returnNode);
                return true;
            }

            if (overInventory)
            {
                HandleInventoryDrop(worldPos);
                return true;
            }

            return false;
        }

        private void HandleInventoryDrop(Vector3 worldPos)
        {
            IGridNode node = draggedView.TargetNode;
            GearConfigData droppedConfig = node?.ConfigData;
            if (droppedConfig != null && !droppedConfig.IsReturnable)
            {
                Debug.Log($"<color=#ff9900>[GearBoardDragHandler]</color> Gear '{droppedConfig.Id}' is not returnable. Snapping back.");
                boardView.NotifyDropped(node, originalGridPos);
                ResetDraggedView();
                return;
            }

            Debug.Log($"<color=#ff9900>[GearBoardDragHandler]</color> Gear '{droppedConfig?.Id}' dropped over UI (not trash). Returning to inventory.");
            ResetDraggedView();
            boardView.NotifyBoardGearDroppedOverUI(node, droppedConfig, worldPos);
        }

        private void HandleBoardDrop(Vector3 worldPos)
        {
            BoardConfigSO cfg = boardView.GetBoardConfig();
            if (cfg == null)
            {
                ResetDraggedView();
                return;
            }

            Vector2Int targetPos = cfg.GetGridPosition(worldPos);
            if (IsOutOfBounds(cfg, targetPos))
            {
                boardView.NotifyDropped(draggedView.TargetNode, originalGridPos);
                ResetDraggedView();
                return;
            }

            boardView.NotifyDropped(draggedView.TargetNode, targetPos);
            ResetDraggedView();
        }

        private bool IsOutOfBounds(BoardConfigSO cfg, Vector2Int targetPos)
        {
            return targetPos.x < 0 || targetPos.x >= cfg.GridWidth || targetPos.y < 0 || targetPos.y >= cfg.GridHeight;
        }

        private void ResetDraggedView()
        {
            if (draggedView != null)
            {
                draggedView.enabled = true;
            }

            draggedView = null;
            DraggedGearData = null;
        }

        private bool HasTargetNode(GearView view)
        {
            if (view != null && view.TargetNode != null)
            {
                return true;
            }

            Debug.Log("[GearBoardDragHandler] Skipping view: view or target node is null.");
            return false;
        }

        private bool IsInteractable(GearView view)
        {
            if (view.TargetNode.IsInteractable)
            {
                return true;
            }

            Debug.Log($"[GearBoardDragHandler] Skipping '{view.TargetNode.ConfigData?.Id}': IsInteractable is false.");
            return false;
        }

        private bool IsMovable(GearView view)
        {
            if (view.TargetNode.ConfigData == null || view.TargetNode.ConfigData.IsMovable)
            {
                return true;
            }

            Debug.Log($"[GearBoardDragHandler] Skipping '{view.TargetNode.ConfigData?.Id}': IsMovable is false.");
            return false;
        }

        private bool IsPointerDown()
        {
            return Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        }

        private bool IsPointerHeld()
        {
            return Input.GetMouseButton(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary));
        }

        private bool IsPointerUp()
        {
            return Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled));
        }

        private Vector3 GetWorldPointerPosition()
        {
            Vector3 p = GetPointerPosition();
            p.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 world = mainCamera.ScreenToWorldPoint(p);
            world.z = -1f;
            return world;
        }

        private Vector3 GetPointerPosition()
        {
            return Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
        }
    }
}
