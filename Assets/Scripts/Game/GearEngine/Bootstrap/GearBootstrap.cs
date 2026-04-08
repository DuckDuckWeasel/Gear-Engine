using UnityEngine;
using VContainer;

namespace Game.GearEngine
{
    public class GearBootstrap : MonoBehaviour
    {
        [SerializeField] private GearConfig[] gearConfigs;

        private IGridManager grid;
        private GearNodeFactory nodeFactory;
        private GearViewFactory viewFactory;

        [Inject]
        public void Construct(IGridManager grid, GearNodeFactory nodeFactory, GearViewFactory viewFactory)
        {
            this.grid = grid;
            this.nodeFactory = nodeFactory;
            this.viewFactory = viewFactory;
        }

        private void Start()
        {
            Debug.Log("[GearBootstrap] Initializing Gear Grid with Factories and SO-driven Abilities...");

            Transform gridRoot = CreateGridRoot();
            PopulateGrid(gridRoot);

            Debug.Log("[GearBootstrap] Grid initialized. Ticking via GridManager.");
        }

        private Transform CreateGridRoot()
        {
            var root = new GameObject("GearGridVisuals").transform;
            root.SetParent(transform);
            return root;
        }

        private void PopulateGrid(Transform gridRoot)
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    var pos = new Vector2Int(x, y);
                    bool isCore = (x == 1 && y == 1);

                    SpawnGear(pos, isCore, gridRoot);
                }
            }
        }

        private void SpawnGear(Vector2Int pos, bool isCore, Transform parent)
        {
            GearConfigData runtimeData = ResolveConfig(isCore);

            IGridNode node = isCore 
                ? nodeFactory.CreateCoreGear(pos, runtimeData) 
                : nodeFactory.CreateBaseGear(pos, runtimeData);

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
