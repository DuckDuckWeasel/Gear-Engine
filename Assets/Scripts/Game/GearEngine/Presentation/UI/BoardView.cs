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
            if (gridManager.GetNode(targetDropPos) == null)
            {
                // Consume from Inventory VM legally
                bool consumed = inventoryViewModel.ConsumeSpecificGear(context.GearData);
                if (consumed)
                {
                    // Logic Spawn deferred fully to the Configured Engine Layer Factory
                    IGridNode newNode = nodeFactory.CreateNode(targetDropPos, context.GearData);
                    
                    gridManager.AddNode(newNode);

                    // Visual Spawn directly into the Board
                    viewFactory.CreateView(newNode, context.GearData, transform);
                    
                    Debug.Log($"<color=#55ff55>[BoardView]</color> Successfully consumed inventory gear and spawned onto Grid at {targetDropPos}!");
                }
            }
            else
            {
                Debug.LogWarning($"<color=#ff5555>[BoardView]</color> Rejected UI Drop! Slot {targetDropPos} is already occupied.");
            }
        }

        private void Update()
        {
            if (gridManager == null || mainCamera == null) return;
            
            // Cannot modify the board while the simulation is running
            if (gridManager.IsRunning) return;

            HandleBoardDragInteraction();
        }

        private void HandleBoardDragInteraction()
        {
            // Convert mouse screen pos to World Z=0
            Vector3 mousePos = Input.mousePosition;
            if (mainCamera.orthographic)
            {
                mousePos.z = 0f; 
            }
            else
            {
                mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
            }
            
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);

            // 1. Pickup
            if (Input.GetMouseButtonDown(0))
            {
                float closestDist = 1.0f; // Click radius tolerance
                GearView[] allViews = FindObjectsOfType<GearView>();
                
                foreach (var view in allViews)
                {
                    if (view.TargetNode != null && Vector3.Distance(view.transform.position, worldPos) < closestDist)
                    {
                        if (view.TargetNode.IsInteractable)
                        {
                            draggedView = view;
                            draggedView.IsBeingDragged = true;
                            originalGridPos = view.TargetNode.Position;
                            
                            // Unregister from grid mechanically while hovering
                            gridManager.RemoveNode(originalGridPos); 
                            Debug.Log($"<color=#33ccff>[BoardView]</color> Picked up gear from {originalGridPos}");
                            break;
                        }
                    }
                }
            }

            // 2. Drag Hover
            if (Input.GetMouseButton(0) && draggedView != null)
            {
                // Instantly teleport visual to mouse pointer instead of letting it lerp
                draggedView.transform.position = worldPos;
            }

            // 3. Drop
            if (Input.GetMouseButtonUp(0) && draggedView != null)
            {
                // Convert continuous world geometry mapping through boardConfig
                Vector2Int targetDropPos = boardConfig.GetGridPosition(worldPos);

                if (gridManager.GetNode(targetDropPos) == null)
                {
                    // Slot is perfectly empty! Complete the transaction.
                    ((NodeBase)draggedView.TargetNode).Position = targetDropPos;
                    gridManager.AddNode(draggedView.TargetNode);
                    Debug.Log($"<color=#55ff55>[BoardView]</color> Successfully dropped gear into {targetDropPos}");
                }
                else
                {
                    // Bump collision - Bounce back natively!
                    ((NodeBase)draggedView.TargetNode).Position = originalGridPos;
                    gridManager.AddNode(draggedView.TargetNode);
                    Debug.Log($"<color=#ff5555>[BoardView]</color> Slot {targetDropPos} occupied! Snapping back to {originalGridPos}.");
                }

                // Drop the reference. GearView.Update() will naturally catch TargetNode.Position and violently lerp it back!
                draggedView.IsBeingDragged = false;
                draggedView = null; 
            }
        }
    }
}
