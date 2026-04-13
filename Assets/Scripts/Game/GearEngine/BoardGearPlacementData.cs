using System;
using UnityEngine;
using GearEngine.GearEngine.Config;

namespace GearEngine.GearEngine
{
    [Serializable]
    public sealed class BoardGearPlacementData
    {
        public BoardGearPlacementData()
        {
        }

        public BoardGearPlacementData(Vector2Int position, GearConfig gearConfig)
        {
            this.position = position;
            this.gearConfig = gearConfig;
        }

        public Vector2Int Position => position;

        [SerializeField] private Vector2Int position;

        public GearConfig GearConfig => gearConfig;

        [SerializeField] private GearConfig gearConfig;
    }
}
