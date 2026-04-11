using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GearEngine
{
    [Serializable]
    public sealed class GearEngineStartData
    {
        [SerializeField] private BoardLayoutData boardLayout;
        [SerializeField] private List<GearConfig> inventoryGears = new List<GearConfig>();

        public BoardLayoutData BoardLayout => boardLayout;

        public IReadOnlyList<GearConfig> InventoryGears => inventoryGears;
    }

    [Serializable]
    public sealed class BoardLayoutData
    {
        [SerializeField] private List<BoardGearPlacementData> placements = new List<BoardGearPlacementData>();

        public IReadOnlyList<BoardGearPlacementData> Placements => placements;

        public BoardLayoutData()
        {
        }

        public BoardLayoutData(IEnumerable<BoardGearPlacementData> items)
        {
            if (items != null)
            {
                placements = new List<BoardGearPlacementData>(items);
            }
        }
    }

    [Serializable]
    public sealed class BoardGearPlacementData
    {
        [SerializeField] private Vector2Int position;
        [SerializeField] private GearConfig gearConfig;

        public Vector2Int Position => position;

        public GearConfig GearConfig => gearConfig;

        public BoardGearPlacementData()
        {
        }

        public BoardGearPlacementData(Vector2Int position, GearConfig gearConfig)
        {
            this.position = position;
            this.gearConfig = gearConfig;
        }
    }
}
