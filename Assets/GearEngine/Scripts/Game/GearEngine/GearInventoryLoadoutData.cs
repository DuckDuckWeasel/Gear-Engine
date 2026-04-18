using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Config;
using UnityEngine;

namespace GearEngine.GearEngine
{
    /// <summary>
    /// Serializable inventory startup seed: capacity and authored gear list.
    /// </summary>
    [Serializable]
    public sealed class GearInventoryLoadoutData
    {
        [SerializeField] private int maxSlots = 5;

        [SerializeField] private List<GearConfig> startingItems = new List<GearConfig>();

        public int MaxSlots
        {
            get => maxSlots;
            set => maxSlots = value;
        }

        public IReadOnlyList<GearConfig> StartingItems => startingItems;

        public static GearInventoryLoadoutData FromGearConfigs(int maxSlots, IReadOnlyList<GearConfig> gears)
        {
            var data = new GearInventoryLoadoutData
            {
                maxSlots = maxSlots
            };
            if (gears != null)
            {
                foreach (GearConfig g in gears)
                {
                    if (g != null)
                    {
                        data.startingItems.Add(g);
                    }
                }
            }

            return data;
        }

        public static GearInventoryLoadoutData Empty(int maxSlots = 5)
        {
            return new GearInventoryLoadoutData { maxSlots = maxSlots };
        }
    }
}
