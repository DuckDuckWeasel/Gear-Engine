using UnityEngine;
using VContainer;

namespace Game.GearEngine
{
    public class GearBootstrap : MonoBehaviour
    {
        [SerializeField] private GearConfig[] gearConfigs;
        [SerializeField] private GameObject emptySlotPrefab;
        
        [Header("Initial Inventory")]
        [Tooltip("The items that the player will start with in their UI inventory")]
        [SerializeField] private GearConfig[] startingInventoryGears;

        private IGridManager grid;
        private GearNodeFactory nodeFactory;
        private GearViewFactory viewFactory;
        private BoardConfigSO boardConfig;
        private Presentation.GearInventoryViewModel inventoryViewModel;

        [Inject]
        public void Construct(IGridManager grid, GearNodeFactory nodeFactory, GearViewFactory viewFactory, BoardConfigSO boardConfig, Presentation.GearInventoryViewModel inventoryViewModel)
        {
            this.grid = grid;
            this.nodeFactory = nodeFactory;
            this.viewFactory = viewFactory;
            this.boardConfig = boardConfig;
            this.inventoryViewModel = inventoryViewModel;
        }

        private void PopulateStartingInventory()
        {
            if (startingInventoryGears == null || startingInventoryGears.Length == 0) return;

            foreach(var gearConfigSO in startingInventoryGears)
            {
                if (gearConfigSO != null)
                {
                    inventoryViewModel.AddGearToInventory(gearConfigSO.CreateRuntimeData());
                }
            }
            Debug.Log($"<color=#ffff55>[GearBootstrap]</color> Populated Inventory with {startingInventoryGears.Length} Initial Gears!");
        }

        private void Start()
        {
            Debug.Log("[GearBootstrap] Initializing Gear Grid with Factories and SO-driven Abilities...");

            PopulateStartingInventory();

            Transform gridRoot = CreateGridRoot();
            PopulateGrid(gridRoot);

            Debug.Log("[GearBootstrap] Grid initialized. Ticking via GridManager.");
        }

        private Transform CreateGridRoot() => transform;

        private void PopulateGrid(Transform gridRoot)
        {
            for (int x = 0; x < boardConfig.GridWidth; x++)
            {
                for (int y = 0; y < boardConfig.GridHeight; y++)
                {
                    var pos = new Vector2Int(x, y);

                    // 1. Visually instantiate the background slot so players know they can drop here
                    if (emptySlotPrefab != null)
                    {
                        var slotView = Object.Instantiate(emptySlotPrefab, gridRoot);
                        slotView.transform.localPosition = boardConfig.GetWorldPosition(pos, 0.5f);
                        slotView.name = $"EmptySlot_{x}_{y}";
                    }

                    // 2. We only logically spawn a mechanical gear exactly at the center!
                    // The rest of the board is intentionally left mechanically hollow for Drag And Drop!
                    int centerX = boardConfig.GridWidth / 2;
                    int centerY = boardConfig.GridHeight / 2;
                    bool isCore = (x == centerX && y == centerY);
                    
                    if (isCore)
                    {
                        SpawnGear(pos, isCore, gridRoot);
                    }
                }
            }
        }

        private void SpawnGear(Vector2Int pos, bool isCore, Transform parent)
        {
            GearConfigData runtimeData = ResolveConfig(isCore);

            IGridNode node = nodeFactory.CreateNode(pos, runtimeData);

            grid.AddNode(node);
            viewFactory.CreateView(node, runtimeData, parent);
        }

        private GearConfigData ResolveConfig(bool isCore)
        {
            if (gearConfigs == null || gearConfigs.Length == 0)
            {
                return new GearConfigData
                {
                    Id = isCore ? "core_default" : "base_default",
                    Category = isCore ? GearCategory.Core : GearCategory.Base,
                    BaseRotationSpeed = isCore ? 90f : 45f,
                    TriggerPattern = TriggerPattern.EightWay,
                    MaxCharge = 100f,
                    ChargeOverTimeAmount = 10f,
                    ChargeOnTriggerAmount = 25f
                };
            }

            int index = isCore ? 0 : Mathf.Min(1, gearConfigs.Length - 1);
            return gearConfigs[index].CreateRuntimeData();
        }
    }
}
