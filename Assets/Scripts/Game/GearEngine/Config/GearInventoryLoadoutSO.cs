using System.Collections.Generic;
using UnityEngine;

namespace Game.GearEngine
{
    [CreateAssetMenu(menuName = "GearEngine/Inventory Loadout", fileName = "GearInventoryLoadout")]
    public sealed class GearInventoryLoadoutSO : ScriptableObject
    {
        [SerializeField] private GearConfig[] startingGears;

        public IReadOnlyList<GearConfig> StartingGears => startingGears;
    }
}
