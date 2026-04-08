using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GearEngine
{
    public enum TriggerPattern
    {
        FourWay = 4,   // Cardinal directions (every 90 deg)
        EightWay = 8   // Cardial + Diagonals (every 45 deg)
    }

    [Serializable]
    public class GearConfigData
    {
        public string Id;
        public float BaseRotationSpeed;
        public GameObject VisualPrefab;
        public TriggerPattern TriggerPattern = TriggerPattern.FourWay;
        public bool IsInteractable = true;
        
        // Progression Mechanics
        public float MaxCharge = 100f;
        public float ChargeOverTimeAmount = 10f; // Amount gained per second from CoreGear
        public float ChargeOnTriggerAmount = 25f; // Amount gained when hit by a trigger

        // Abilities configured specifically for this gear
        public List<GearAbilitySO> Abilities = new List<GearAbilitySO>();

        // Runtime copy of the next level config
        [NonSerialized] public GearConfig NextLevelConfig;

        public GearConfigData Clone(GearConfig nextLevelConfig, List<GearAbilitySO> abilities)
        {
            return new GearConfigData
            {
                Id = Id,
                BaseRotationSpeed = BaseRotationSpeed,
                VisualPrefab = VisualPrefab,
                TriggerPattern = TriggerPattern,
                IsInteractable = IsInteractable,
                MaxCharge = MaxCharge,
                ChargeOverTimeAmount = ChargeOverTimeAmount,
                ChargeOnTriggerAmount = ChargeOnTriggerAmount,
                NextLevelConfig = nextLevelConfig,
                Abilities = new List<GearAbilitySO>(abilities ?? new List<GearAbilitySO>())
            };
        }
    }
}
