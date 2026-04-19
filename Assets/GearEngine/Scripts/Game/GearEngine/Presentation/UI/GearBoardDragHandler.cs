using System;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Visuals;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    [RequireComponent(typeof(BoardViewComponent))]
    internal sealed class GearBoardDragHandler : MonoBehaviour, IDragSource
    {
        private BoardViewComponent boardView;
        private Camera mainCamera;
        private GearView draggedView;
        private Vector2Int originalGridPos;
        private DragGhostController ghostController;

        private IGridNode pendingNode;
        private GearConfigData pendingConfig;
        private Vector3 pendingWorldPos;

        /// <summary>Config data of the gear currently being dragged (null when idle).</summary>
        internal GearConfigData DraggedGearData { get; private set; }

        private void Awake()
        {
            boardView = GetComponent<BoardViewComponent>();
        }

        private void Start()
        {
            mainCamera = Camera.main;
            ghostController = boardView != null ? new DragGhostController(boardView.GetBoardSpaceRoot()) : null;
        }

        private void Update()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            if (boardView == null || mainCamera == null)
            {
                return;
            }

            if (boardView.IsRunning())
            {
                return;
            }

            if (!TryGetWorldPointerPosition(out Vector3 worldPos))
            {
                return;
            }

            if (IsPointerDown() && draggedView == null)
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
                Debug.LogWarning("[GearBoardDragHandler] HandlePickup failed: BoardConfig is null.");
                return;
            }

            float closestDist = boardConfig.MaxDragGrabDistance;
            GearView closest = null;
            int viewCount = 0;

            foreach (GearView view in boardView.GetViews())
            {
                viewCount++;
                if (view == null || view.TargetNode == null)
                {
                    Debug.Log("[GearBoardDragHandler] Skipping view: view or target node is null.");
                    continue;
                }

                if (!view.TargetNode.IsInteractable)
                {
                    continue;
                }

                if (view.TargetNode.ConfigData != null && !view.TargetNode.ConfigData.IsMovable)
                {
                    Debug.Log($"[GearBoardDragHandler] Skipping '{view.TargetNode.ConfigData?.Id}': IsMovable is false.");
                    continue;
                }

                float dist = Vector3.Distance(view.transform.position, worldPos);

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = view;
                }
            }

            if (closest == null)
            {
                Debug.Log($"[GearBoardDragHandler] HandlePickup failed: No movable gear found within {boardConfig.MaxDragGrabDistance} of Pointer at {worldPos}. Evaluated {viewCount} views.");
                return;
            }

            Debug.Log($"<color=#00ff00>[GearBoardDragHandler]</color> Successfully Picked up '{closest.TargetNode.ConfigData?.Id}'!");

            draggedView = closest;
            draggedView.enabled = false;
            draggedView.gameObject.SetActive(false);
            originalGridPos = closest.TargetNode.Position;
            DraggedGearData = closest.TargetNode.ConfigData;

            ghostController?.CreateGhost(closest.TargetNode.ConfigData);
            ghostController?.MoveGhostTo(worldPos);

            boardView.NotifyPickedUp(closest.TargetNode, originalGridPos);
        }

        private void HandleHover(Vector3 worldPos)
        {
            if (draggedView != null)
            {
                ghostController?.MoveGhostTo(worldPos);
            }
        }

        private void HandleDrop(Vector3 worldPos)
        {
            if (draggedView == null)
            {
                return;
            }

            pendingNode = draggedView.TargetNode;
            pendingConfig = pendingNode?.ConfigData;
            pendingWorldPos = worldPos;

            var payload = new DragPayload(pendingNode, worldPos, this);
            Vector2 screenPos = GetPointerPosition();
            IDragTarget target = DragTargetFinder.Find(payload, screenPos, mainCamera);

            GearView released = draggedView;
            draggedView = null;
            DraggedGearData = null;

            ghostController?.DestroyGhost();

            try
            {
                if (target != null)
                {
                    target.OnDrop(payload);
                }
                else
                {
                    BoardConfigSO cfg = boardView.GetBoardConfig();
                    Transform boardRoot = boardView.GetBoardSpaceRoot();
                    Vector2Int targetPos = cfg != null
                        ? cfg.GetGridPosition(boardRoot.InverseTransformPoint(worldPos))
                        : originalGridPos;
                    boardView.NotifyDropped(pendingNode, targetPos);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearBoardDragHandler] HandleDrop failed: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                if (released != null)
                {
                    released.gameObject.SetActive(true);
                    released.enabled = true;
                }
            }
        }

        public void OnDropAccepted(IDragTarget by)
        {
            boardView.NotifyBoardGearReturnAccepted(pendingNode, pendingConfig, pendingWorldPos);
        }

        public void OnDropRejected()
        {
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

        private bool TryGetWorldPointerPosition(out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;
            Transform boardRoot = boardView != null ? boardView.GetBoardSpaceRoot() : null;
            return BoardPointerProjectionUtility.TryProjectScreenPointToPlane(mainCamera, GetPointerPosition(), boardRoot, out worldPosition);
        }
    }
}
