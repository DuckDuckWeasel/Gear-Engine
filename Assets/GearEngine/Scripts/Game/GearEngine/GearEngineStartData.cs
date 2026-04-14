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
    }
}
