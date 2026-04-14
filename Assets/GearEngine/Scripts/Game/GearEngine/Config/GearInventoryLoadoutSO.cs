using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    [CreateAssetMenu(menuName = "GearEngine/Inventory Loadout", fileName = "GearInventoryLoadout")]
    public sealed class GearInventoryLoadoutSO : ScriptableObject
    {
        public IReadOnlyList<GearConfig> StartingGears => startingGears;

        [SerializeField] private GearConfig[] startingGears;

        [Header("Limits")]
        [Tooltip("Maximum number of gears the inventory can hold at once.")]
        [Min(1)]
        public int MaxInventorySlots = 10;
    }
}
