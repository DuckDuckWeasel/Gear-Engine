using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Abilities;
using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    [Serializable]
    public class GearConfigData
    {
        public string Id;
        public GearCategory Category = GearCategory.Base;
        public float BaseRotationSpeed;
        public GameObject VisualPrefab;
        public Sprite UIIcon;
        public float UIScaleMultiplier = 115f; // Used to scale native visually into overlay canvases
        public TriggerPattern TriggerPattern = TriggerPattern.FourWay;
        public bool IsInteractable = true;
        public bool IsMovable = true;
        public bool IsReturnable = true;

        // Progression Mechanics
        public float MaxCharge = 100f;
        public float ChargeOverTimeAmount = 10f; // Amount gained per second from CoreGear
        public float ChargeOnTriggerAmount = 25f; // Amount gained when hit by a trigger

        // Snap Feedback Mechanics
        public float SnapSlowdownDuration = 0.5f;
        public float SnapSlowdownMultiplier = 0.15f;
        public float TriggerSpinDegrees = 45f;

        // Delete / Scrap Mechanics (opt-in)
        public bool IsDeletable = false;
        public int DeleteRewardAmount = 0;

        // Abilities configured specifically for this gear
        public List<GearAbilitySO> Abilities = new List<GearAbilitySO>();

        // Runtime copy of the next level config
        [NonSerialized] public GearConfig NextLevelConfig;

        public GearConfigData Clone(GearConfig nextLevelConfig, List<GearAbilitySO> abilities)
        {
            return new GearConfigData
            {
                Id = Id,
                Category = Category,
                BaseRotationSpeed = BaseRotationSpeed,
                VisualPrefab = VisualPrefab,
                UIIcon = UIIcon,
                UIScaleMultiplier = UIScaleMultiplier,
                TriggerPattern = TriggerPattern,
                IsInteractable = IsInteractable,
                IsMovable = IsMovable,
                IsReturnable = IsReturnable,
                MaxCharge = MaxCharge,
                ChargeOverTimeAmount = ChargeOverTimeAmount,
                ChargeOnTriggerAmount = ChargeOnTriggerAmount,
                SnapSlowdownDuration = SnapSlowdownDuration,
                SnapSlowdownMultiplier = SnapSlowdownMultiplier,
                TriggerSpinDegrees = TriggerSpinDegrees,
                IsDeletable = IsDeletable,
                DeleteRewardAmount = DeleteRewardAmount,
                NextLevelConfig = nextLevelConfig,
                Abilities = new List<GearAbilitySO>(abilities ?? new List<GearAbilitySO>())
            };
        }
    }
}
