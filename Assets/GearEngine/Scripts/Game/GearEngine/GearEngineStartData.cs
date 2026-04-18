using System;
using UnityEngine;

namespace GearEngine.GearEngine
{
    [Serializable]
    public sealed class GearEngineStartData
    {
        [SerializeField] private GearInventoryLoadoutData inventoryLoadout = new GearInventoryLoadoutData();

        [SerializeField] private GearBoardLoadoutData boardLoadout = new GearBoardLoadoutData();

        public GearInventoryLoadoutData InventoryLoadout => inventoryLoadout;

        public GearBoardLoadoutData BoardLoadout => boardLoadout;

        public GearInventoryLoadoutData GetInventoryLoadoutData() => inventoryLoadout;

        public GearBoardLoadoutData GetBoardLoadoutData() => boardLoadout;
    }
}
