using System;
using System.Collections.Generic;
using UnityEngine;
using GearEngine.GearEngine.Config;

namespace GearEngine.GearEngine
{
    [Serializable]
    public sealed class GearEngineStartData
    {
        public BoardLayoutData BoardLayout => boardLayout;

        [SerializeField] private BoardLayoutData boardLayout;

        public IReadOnlyList<GearConfig> InventoryGears => inventoryGears;

        [SerializeField] private List<GearConfig> inventoryGears = new List<GearConfig>();

        public int MaxInventorySlots => maxInventorySlots;

        [SerializeField] private int maxInventorySlots = 5;

        /// <summary>Adapter for inventory service construction and new loadout type.</summary>
        public GearInventoryLoadoutData GetInventoryLoadoutData()
        {
            return GearInventoryLoadoutData.FromGearConfigs(maxInventorySlots, inventoryGears);
        }

        /// <summary>Adapter for board startup.</summary>
        public GearBoardLoadoutData GetBoardLoadoutData()
        {
            return new GearBoardLoadoutData { BoardLayout = boardLayout };
        }
    }
}
