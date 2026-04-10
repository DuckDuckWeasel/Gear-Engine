using UnityEngine;
using VContainer;

using Scaffold.Events;
using Game.GearEngine.Events;

namespace Game.GearEngine.Presentation
{
    public class BoardView : MonoBehaviour
    {
        private IGridManager gridManager;
        private Camera mainCamera;
        
        // Factories & Services injected
        private EventController eventController;
        private GearNodeFactory nodeFactory;
        private GearViewFactory viewFactory;
        private GearInventoryViewModel inventoryViewModel;
        private BoardConfigSO boardConfig;
        
        // Drag State
        [SerializeField]
        private GearView draggedView;
        private Vector2Int originalGridPos;

        [Header("Debug")]
        public IGridManager GridState => gridManager;

        [Inject]
        public void Construct(IGridManager gridManager, EventController eventController, GearNodeFactory nodeFactory, GearViewFactory viewFactory, GearInventoryViewModel inventoryViewModel, BoardConfigSO boardConfig)
        {
            this.gridManager = gridManager;
            this.eventController = eventController;
            this.nodeFactory = nodeFactory;
            this.viewFactory = viewFactory;
            this.inventoryViewModel = inventoryViewModel;
            this.boardConfig = boardConfig;
            mainCamera = Camera.main;

            // Subscribe to generic drops incoming from UI Inventory overlapping the Board Tag
            this.eventController.AddListener<GearDroppedFromUIEvent>(HandleGearDroppedFromUI);
        }

        private void OnDestroy()
        {
            if (eventController != null)
            {
                eventController.RemoveListener<GearDroppedFromUIEvent>(HandleGearDroppedFromUI);
            }
        }

        private void HandleGearDroppedFromUI(GearDroppedFromUIEvent context)
        {
            if (gridManager == null || gridManager.IsRunning) return;

            // Snap the generic 3D world position dropped by the generic UI Drag handler into Grid coords
            Vector2Int targetDropPos = boardConfig != null 
                ? boardConfig.GetGridPosition(context.WorldPosition) 
                : new Vector2Int(Mathf.RoundToInt(context.WorldPosition.x / 1.5f), Mathf.RoundToInt(context.WorldPosition.y / 1.5f));


            // Is the slot mathematically empty?
            IGridNode occupant = gridManager.GetNode(targetDropPos);

            if (occupant == null)
            {
                // Consume from Inventory VM legally
                bool consumed = inventoryViewModel.ConsumeSpecificGear(context.GearData);
                if (consumed)
                {
                    // Logic Spawn
                    IGridNode newNode = nodeFactory.CreateNode(targetDropPos, context.GearData);
                    gridManager.AddNode(newNode);
                    viewFactory.CreateView(newNode, context.GearData, transform);
                }
            }
            else
            {
                // UI Drop over an Ocupied slot!
                GearConfigData occupantData = ((NodeBase)occupant).ConfigData;
                
                // Are they mergeable? (Same ID, and occupant HAS a NextLevelConfig)
                if (occupantData.Id == context.GearData.Id && occupantData.NextLevelConfig != null)
                {
                    // Consumes the dragged item from the inventory
                    bool consumed = inventoryViewModel.ConsumeSpecificGear(context.GearData);
                    if (consumed)
                    {
                        // Remove the old logical layer
                        gridManager.RemoveNode(targetDropPos);
                        
                        // Destroy old visual layer
                        GearView[] oldViews = FindObjectsOfType<GearView>();
                        foreach (var view in oldViews)
                        {
                            if (view.TargetNode == occupant)
                            {
                                Destroy(view.gameObject);
                            }
                        }

                        // Spawn the UPGRADED Gear!
                        GearConfigData upgradedData = occupantData.NextLevelConfig.CreateRuntimeData();
                        IGridNode newNode = nodeFactory.CreateNode(targetDropPos, upgradedData);
                        gridManager.AddNode(newNode);
                        viewFactory.CreateView(newNode, upgradedData, transform);
                        
                        Debug.Log($"<color=#ffaa55>[BoardView]</color> MERGED UI {context.GearData.Id} into {upgradedData.Id} at {targetDropPos}!");
                    }
                }
                else
                {
                    // NOT mergeable! The user explicitly requested to CANCEL the dragging from UI without dropping.
                    // Doing nothing here natively causes the UI DragHandler to simply fail and respawn/do nothing.
                    Debug.LogWarning($"<color=#ff5555>[BoardView]</color> UI Drop Cancelled! {context.GearData.Id} dropped on incompatible/occupied {occupantData.Id}.");
                }
            }
        }

        private void Update()
        {
            if (gridManager == null || mainCamera == null) return;
            
            // Cannot modify the board while the simulation is running
            if (gridManager.IsRunning) return;

            HandleBoardDragInteraction();
        }

        // --- Input Abstractions for Mobile ---
        private bool IsPointerDown() => Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        private bool IsPointerHeld() => Input.GetMouseButton(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary));
        private bool IsPointerUp() => Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled));
        private Vector3 GetPointerPosition() => Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;

        private void HandleBoardDragInteraction()
        {
            Vector3 pointerPos = GetPointerPosition();
            Vector3 mousePos = pointerPos;
            
            // For both Orthographic and Perspective, we need the Z depth from the camera to correctly place the object on the Board (Z=0).
            mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
            
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            // Lock dragged object securely to board visual plane
            worldPos.z = -1f;

            // 1. Pickup
            if (IsPointerDown())
            {
                float closestDist = boardConfig.MaxDragGrabDistance; // Geometric targeting threshold mapped to configuration
                GearView[] allViews = FindObjectsOfType<GearView>();
                GearView closestView = null;
                
                foreach (var view in allViews)
                {
                    if (view.TargetNode == null || !view.TargetNode.IsInteractable) continue;

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
                    
                    gridManager.ExtractNode(originalGridPos); 
                    Debug.Log($"<color=#33ccff>[BoardView]</color> Picked up gear from {originalGridPos}");
                }
            }

            // 2. Drag Hover
            if (IsPointerHeld() && draggedView != null)
            {
                draggedView.transform.position = worldPos;
            }

            // 3. Drop
            if (IsPointerUp() && draggedView != null)
            {
                // Validate if dropping on UI Space (Inventory return mechanic)
                bool overUI = false;
                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    if (Input.touchCount > 0)
                        overUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                    else
                        overUI = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
                }

                if (overUI)
                {
                    // Return logic back to inventory safely
                    GearConfigData draggedData = ((NodeBase)draggedView.TargetNode).ConfigData;
                    inventoryViewModel.AddGearToInventory(draggedData);
                    
                    draggedView.TargetNode.Dispose(); // Clear mechanical triggers since it left the simulation space
                    Destroy(draggedView.gameObject);
                    draggedView = null;
                    return;
                }

                // Grid mapping conversion
                Vector2Int targetDropPos = boardConfig.GetGridPosition(worldPos);

                // Validation: Prevent dropping outside of physical Grid bounds
                bool isValidDrop = targetDropPos.x >= 0 && targetDropPos.x < boardConfig.GridWidth &&
                                   targetDropPos.y >= 0 && targetDropPos.y < boardConfig.GridHeight;

                if (!isValidDrop)
                {
                    // Invalid dropping zone! Snap instantly back to origin
                    ((NodeBase)draggedView.TargetNode).Position = originalGridPos;
                    gridManager.AddNode(draggedView.TargetNode);
                    draggedView.RecalculateRotationOffset();
                    Debug.LogWarning($"<color=#ff5555>[BoardView]</color> Missed valid node! Dropped at {targetDropPos} out of bounds. Snapped back.");
                }
                else
                {
                    IGridNode occupant = gridManager.GetNode(targetDropPos);

                    if (occupant == null)
                    {
                        // Spot is clean
                        ((NodeBase)draggedView.TargetNode).Position = targetDropPos;
                        gridManager.AddNode(draggedView.TargetNode);
                        draggedView.RecalculateRotationOffset();
                        Debug.Log($"<color=#55ff55>[BoardView]</color> Successfully dropped gear into {targetDropPos}");
                    }
                    else
                    {
                        // Board to Board strict 1-for-1 Swap!
                        IGridNode occupantNode = gridManager.ExtractNode(targetDropPos);

                        ((NodeBase)draggedView.TargetNode).Position = targetDropPos;
                        gridManager.AddNode(draggedView.TargetNode);
                        draggedView.RecalculateRotationOffset();

                        ((NodeBase)occupantNode).Position = originalGridPos;
                        gridManager.AddNode(occupantNode);

                        foreach (GearView view in FindObjectsOfType<GearView>())
                        {
                            if (view.TargetNode == occupantNode)
                            {
                                view.RecalculateRotationOffset();
                                break;
                            }
                        }
                        
                        Debug.Log($"<color=#ffff33>[BoardView]</color> Swapped positions! {targetDropPos} <-> {originalGridPos}");
                    }
                } // <--- Added missing brace for the outer else (isValidDrop)

                draggedView.IsBeingDragged = false;
                draggedView = null; 
            }
        }
    }
}
