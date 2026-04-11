using UnityEngine;
using VContainer;

namespace Game.GearEngine
{
    public class GearBootstrap : MonoBehaviour, IGearSceneElement
    {
        [SerializeField] private GearConfig initialGear;
        [SerializeField] private GameObject emptySlotPrefab;

        private IGridManager grid;
        private GearNodeFactory nodeFactory;
        private GearViewFactory viewFactory;
        private BoardConfigSO boardConfig;
        private bool initialized;

        [Inject]
        public void Construct(IGridManager grid, GearNodeFactory nodeFactory, GearViewFactory viewFactory, BoardConfigSO boardConfig)
        {
            this.grid = grid;
            this.nodeFactory = nodeFactory;
            this.viewFactory = viewFactory;
            this.boardConfig = boardConfig;
        }

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            Debug.Log("[GearBootstrap] Initializing Gear Grid with Factories and SO-driven Abilities...");

            Transform gridRoot = CreateGridRoot();
            PopulateGrid(gridRoot);

            Debug.Log("[GearBootstrap] Grid initialized. Ticking via GridManager.");
        }

        public void Enable() => gameObject.SetActive(true);

        public void Disable() => gameObject.SetActive(false);

        private Transform CreateGridRoot() => transform;

        private void PopulateGrid(Transform gridRoot)
        {
            for (int x = 0; x < boardConfig.GridWidth; x++)
            {
                for (int y = 0; y < boardConfig.GridHeight; y++)
                {
                    var pos = new Vector2Int(x, y);

                    if (emptySlotPrefab != null)
                    {
                        var slotView = Object.Instantiate(emptySlotPrefab, gridRoot);
                        slotView.transform.localPosition = boardConfig.GetWorldPosition(pos, 0.5f);
                        slotView.name = $"EmptySlot_{x}_{y}";
                    }

                    int centerX = boardConfig.GridWidth / 2;
                    int centerY = boardConfig.GridHeight / 2;
                    bool isCoreCell = x == centerX && y == centerY;

                    if (isCoreCell)
                    {
                        SpawnGear(pos, gridRoot);
                    }
                }
            }
        }

        private void SpawnGear(Vector2Int pos, Transform parent)
        {
            GearConfigData runtimeData = ResolveConfig();

            IGridNode node = nodeFactory.CreateNode(pos, runtimeData);

            grid.AddNode(node);
            viewFactory.CreateView(node, runtimeData, parent);
        }

        private GearConfigData ResolveConfig()
        {
            if (initialGear != null)
            {
                return initialGear.CreateRuntimeData();
            }

            return new GearConfigData
            {
                Id = "core_default",
                Category = GearCategory.Core,
                BaseRotationSpeed = 90f,
                TriggerPattern = TriggerPattern.EightWay,
                MaxCharge = 100f,
                ChargeOverTimeAmount = 10f,
                ChargeOnTriggerAmount = 25f
            };
        }
    }
}
