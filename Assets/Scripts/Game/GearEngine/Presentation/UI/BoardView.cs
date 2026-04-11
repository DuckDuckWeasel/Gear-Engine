using Game.GearEngine;
using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.GearEngine.Presentation
{
    public class BoardView : ViewComponent<BoardViewModel>
    {
        [SerializeField] private GearView draggedView;
        private Vector2Int originalGridPos;

        private Camera mainCamera;

        protected override void OnBind()
        {
            mainCamera = Camera.main;
            viewModel.SetBoardVisualRoot(transform);
        }

        private void OnDestroy()
        {
            viewModel?.Dispose();
        }

        private void Update()
        {
            if (viewModel == null || mainCamera == null || viewModel.BoardConfig == null ||
                viewModel.EngineService == null || viewModel.GearViewFactory == null)
            {
                return;
            }

            if (viewModel.EngineService.IsRunning)
            {
                return;
            }

            HandleBoardDragInteraction();
        }

        private bool IsPointerDown() => Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        private bool IsPointerHeld() => Input.GetMouseButton(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary));
        private bool IsPointerUp() => Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled));
        private Vector3 GetPointerPosition() => Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;

        private void HandleBoardDragInteraction()
        {
            BoardConfigSO boardConfig = viewModel.BoardConfig;
            GearViewFactory gearViewFactory = viewModel.GearViewFactory;

            Vector3 pointerPos = GetPointerPosition();
            Vector3 mousePos = pointerPos;
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            worldPos.z = -1f;

            if (IsPointerDown())
            {
                float closestDist = boardConfig.MaxDragGrabDistance;
                GearView closestView = null;

                foreach (var view in gearViewFactory.EnumerateGearViews())
                {
                    if (view == null || view.TargetNode == null || !view.TargetNode.IsInteractable)
                    {
                        continue;
                    }

                    Vector2 vPos = new Vector2(view.transform.position.x, view.transform.position.y);
                    Vector2 wPos = new Vector2(worldPos.x, worldPos.y);

                    float dist = Vector2.Distance(vPos, wPos);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestView = view;
                    }
                }

                if (closestView != null)
                {
                    draggedView = closestView;
                    draggedView.IsBeingDragged = true;
                    originalGridPos = closestView.TargetNode.Position;
                    viewModel.OnGearPickedUp(closestView.TargetNode, originalGridPos);
                    Debug.Log($"<color=#33ccff>[BoardView]</color> Picked up gear from {originalGridPos}");
                }
            }

            if (IsPointerHeld() && draggedView != null)
            {
                draggedView.transform.position = worldPos;
            }

            if (IsPointerUp() && draggedView != null)
            {
                bool overUI = false;
                if (EventSystem.current != null)
                {
                    if (Input.touchCount > 0)
                    {
                        overUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                    }
                    else
                    {
                        overUI = EventSystem.current.IsPointerOverGameObject();
                    }
                }

                IGridNode node = draggedView.TargetNode;
                Vector2Int targetDropPos = boardConfig.GetGridPosition(worldPos);
                viewModel.OnGearDropped(node, targetDropPos, overUI);

                draggedView.IsBeingDragged = false;
                draggedView = null;
            }
        }
    }
}
