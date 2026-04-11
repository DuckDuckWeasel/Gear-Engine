using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.GearEngine.Presentation
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
                IGridNode node = draggedView.TargetNode;
                GearConfigData droppedConfig = node?.ConfigData;
                draggedView.IsBeingDragged = false;
                DestroyGO(draggedView.gameObject);
                draggedView = null;
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
            boardView.NotifyDropped(draggedView.TargetNode, targetPos);
            draggedView.IsBeingDragged = false;
            draggedView = null;
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
