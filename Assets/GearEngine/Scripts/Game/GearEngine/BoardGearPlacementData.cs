using System;
using UnityEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;

namespace GearEngine.GearEngine
{
    [Serializable]
    public sealed class BoardGearPlacementData
    {
        public BoardGearPlacementData()
        {
        }

        /// <summary>Authoring / test layout without inventory (Owner null).</summary>
        public BoardGearPlacementData(Vector2Int position, GearItem gearConfig)
        {
            this.position = position;
            gearConfigField = gearConfig;
        }

        /// <summary>Campaign loadout: ties placement to a live <see cref="OwnedGear"/>.</summary>
        public BoardGearPlacementData(Vector2Int position, OwnedGear owner)
        {
            this.position = position;
            Owner = owner;
            gearConfigField = owner?.Config;
        }

        public Vector2Int Position => position;

        [SerializeField] private Vector2Int position;

        /// <summary>Non-null when loaded from LiveOps inventory.</summary>
        [NonSerialized] public OwnedGear Owner;

        [SerializeField] private GearItem gearConfigField;

        public GearItem GearItem => Owner?.Config ?? gearConfigField;
    }
}
