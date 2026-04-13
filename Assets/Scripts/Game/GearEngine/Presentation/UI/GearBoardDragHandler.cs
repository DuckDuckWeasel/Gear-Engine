using UnityEngine;
using UnityEngine.EventSystems;

namespace GearEngine.GearEngine.Presentation.UI
{
    [RequireComponent(typeof(BoardView))]
    internal sealed class GearBoardDragHandler : MonoBehaviour
    {
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
            if (CanProcessDragFrame())
            {
                ProcessDragInteractions(GetWorldPointerPosition());
            }
        }

        private void ProcessDragInteractions(Vector3 worldPos)
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

        private bool CanProcessDragFrame()
        {
            if (boardView == null || mainCamera == null)
            {
                return false;
            }

            if (boardView.IsRunning())
            {
                return false;
            }

            return true;
        }

        private void HandlePickup(Vector3 worldPos)
        {
            BoardConfigSO boardConfig = boardView.GetBoardConfig();
            if (boardConfig == null)
            {
                return;
            }

            GearView closest = FindClosestInteractableGear(worldPos, boardConfig.MaxDragGrabDistance);
            if (closest == null)
            {
                return;
            }

            draggedView = closest;
            draggedView.IsBeingDragged = true;
            originalGridPos = closest.TargetNode.Position;
            boardView.NotifyPickedUp(closest.TargetNode, originalGridPos);
        }

        private GearView FindClosestInteractableGear(Vector3 worldPos, float maxDist)
        {
            Vector2 p = new Vector2(worldPos.x, worldPos.y);
            GearView closest = null;

            foreach (GearView view in boardView.GetViews())
            {
                TrySelectCloserGear(view, p, ref maxDist, ref closest);
            }

            return closest;
        }

        private void TrySelectCloserGear(GearView view, Vector2 pointer, ref float bestDist, ref GearView best)
        {
            if (view?.TargetNode == null || !view.TargetNode.IsInteractable)
            {
                return;
            }

            float dist = Vector2.Distance(new Vector2(view.transform.position.x, view.transform.position.y), pointer);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = view;
            }
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
                CompleteDropOverUi(worldPos);
                return;
            }

            CompleteDropOnBoard(worldPos);
        }

        private void CompleteDropOverUi(Vector3 worldPos)
        {
            IGridNode node = draggedView.TargetNode;
            GearConfigData droppedConfig = node?.ConfigData;
            draggedView.IsBeingDragged = false;
            DestroyGO(draggedView.gameObject);
            draggedView = null;
            boardView.NotifyBoardGearDroppedOverUI(node, droppedConfig, worldPos);
        }

        private void CompleteDropOnBoard(Vector3 worldPos)
        {
            BoardConfigSO cfg = boardView.GetBoardConfig();
            if (cfg == null)
            {
                draggedView.IsBeingDragged = false;
                draggedView = null;
                return;
            }

            Vector2Int targetPos = cfg.GetGridPosition(worldPos);
            boardView.NotifyDropped(draggedView.TargetNode, targetPos);
            draggedView.IsBeingDragged = false;
            draggedView = null;
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

        private void DestroyGO(GameObject go)
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
