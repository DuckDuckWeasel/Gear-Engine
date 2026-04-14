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
        internal GearConfigData DraggedGearData { get; private set; }

        [Tooltip("Tag identifying the trash zone drop target.")]
        [SerializeField] private TagSO trashZoneTag;

        private BoardView boardView;
        private Camera mainCamera;
        private GearView draggedView;
        private Vector2Int originalGridPos;

        private void Awake()
        {
            boardView = GetComponent<BoardView>();
            enabled = false;
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!CanProcessPointer())
            {
                return;
            }

            Vector3 worldPos = GetWorldPointerPosition();
            ProcessPointerPhases(worldPos);
        }

        private void ProcessPointerPhases(Vector3 worldPos)
        {
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

        private bool CanProcessPointer()
        {
            if (boardView == null || mainCamera == null)
            {
                return false;
            }

            return !boardView.IsRunning();
        }

        private void HandlePickup(Vector3 worldPos)
        {
            BoardConfigSO boardConfig = boardView.GetBoardConfig();
            if (boardConfig == null)
            {
                return;
            }

            GearView closest = FindClosestDraggableGear(worldPos, boardConfig.MaxDragGrabDistance);
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

        private GearView FindClosestDraggableGear(Vector3 worldPos, float maxDist)
        {
            float closestDist = maxDist;
            GearView closest = null;

            foreach (GearView view in boardView.GetViews())
            {
                UpdateClosestIfNearer(ref closestDist, ref closest, view, worldPos);
            }

            return closest;
        }

        private void UpdateClosestIfNearer(ref float closestDist, ref GearView closest, GearView view, Vector3 worldPos)
        {
            if (!IsDraggableBoardGear(view))
            {
                return;
            }

            float dist = Vector2.Distance(new Vector2(view.transform.position.x, view.transform.position.y), new Vector2(worldPos.x, worldPos.y));
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = view;
            }
        }

        private bool IsDraggableBoardGear(GearView view)
        {
            if (view == null || view.TargetNode == null || !view.TargetNode.IsInteractable)
            {
                return false;
            }

            return view.TargetNode.ConfigData == null || view.TargetNode.ConfigData.IsMovable;
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

            if (IsPointerOverUI())
            {
                HandleDropOverUi(worldPos);
                return;
            }

            HandleDropOnGrid(worldPos);
        }

        private void HandleDropOverUi(Vector3 worldPos)
        {
            if (TryTrashDropFromUi())
            {
                return;
            }

            IGridNode node = draggedView.TargetNode;
            GearConfigData droppedConfig = node?.ConfigData;

            if (droppedConfig != null && !droppedConfig.IsReturnable)
            {
                CancelDragSnapBack();
                Debug.Log($"<color=#ff9900>[GearBoardDragHandler]</color> Gear '{droppedConfig.Id}' is not returnable. Snapping back.");
                return;
            }

            CompleteInventoryReturn(node, droppedConfig, worldPos);
        }

        private void CompleteInventoryReturn(IGridNode node, GearConfigData droppedConfig, Vector3 worldPos)
        {
            Debug.Log($"<color=#ff9900>[GearBoardDragHandler]</color> Gear '{droppedConfig?.Id}' dropped over UI (not trash). Returning to inventory.");
            GearView view = draggedView;
            view.IsBeingDragged = false;
            GameObject go = view.gameObject;
            draggedView = null;
            DraggedGearData = null;
            DestroyGameObjectIfPlaying(go);
            boardView.NotifyBoardGearDroppedOverUI(node, droppedConfig, worldPos);
        }

        private bool TryTrashDropFromUi()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            foreach (RaycastResult result in BuildUiRaycastResults())
            {
                if (!IsTrashRaycastHit(result))
                {
                    continue;
                }

                NotifyTrashDropFromUi();
                return true;
            }

            return false;
        }

        private System.Collections.Generic.List<RaycastResult> BuildUiRaycastResults()
        {
            PointerEventData ped = new PointerEventData(EventSystem.current) { position = GetPointerPosition() };
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, results);
            return results;
        }

        private void NotifyTrashDropFromUi()
        {
            IGridNode returnNode = draggedView.TargetNode;
            Debug.Log($"<color=#ff9900>[GearBoardDragHandler]</color> Gear '{returnNode?.ConfigData?.Id}' dropped on trash zone. Forwarding to BoardView.");
            ClearDragState();
            boardView.NotifyTrashDrop(returnNode);
        }

        private bool IsTrashRaycastHit(RaycastResult result)
        {
            bool isTrash = result.gameObject.GetComponentInParent<TrashDropZoneView>() != null;
            if (!isTrash && trashZoneTag != null)
            {
                var tc = result.gameObject.GetComponentInParent<TagComponent>();
                isTrash = tc != null && tc.HasTag(trashZoneTag);
            }

            return isTrash;
        }

        private void HandleDropOnGrid(Vector3 worldPos)
        {
            BoardConfigSO cfg = boardView.GetBoardConfig();
            if (cfg == null)
            {
                ClearDragState();
                return;
            }

            Vector2Int targetPos = cfg.GetGridPosition(worldPos);

            if (targetPos.x < 0 || targetPos.x >= cfg.GridWidth || targetPos.y < 0 || targetPos.y >= cfg.GridHeight)
            {
                boardView.NotifyDropped(draggedView.TargetNode, originalGridPos);
                ClearDragState();
                return;
            }

            boardView.NotifyDropped(draggedView.TargetNode, targetPos);
            ClearDragState();
        }

        private void CancelDragSnapBack()
        {
            boardView.NotifyDropped(draggedView.TargetNode, originalGridPos);
            ClearDragState();
        }

        private void ClearDragState()
        {
            if (draggedView != null)
            {
                draggedView.IsBeingDragged = false;
            }

            draggedView = null;
            DraggedGearData = null;
        }

        private void DestroyGameObjectIfPlaying(GameObject go)
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

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            return Input.touchCount > 0 ? EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) : EventSystem.current.IsPointerOverGameObject();
        }
    }
}
