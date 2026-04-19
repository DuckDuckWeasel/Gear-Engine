using System;
using System.Collections.Generic;
using UnityEngine;
using GearEngine.GearEngine.Services.Inventory;
using GearEngine.GearEngine.Visuals;

namespace GearEngine.GearEngine.Config
{
    [Serializable]
    public class GearConfigData : IItem
    {
        [SerializeField] private string id;
        public string Id { get => id; set => id = value; }
        public GearCategory Category = GearCategory.Base;
        public float BaseRotationSpeed;
        public GearView ViewPrefab;
        public Sprite UIIcon;
        [Tooltip("Relative size modifier for this specific gear (1.0 is default), applied to the GearVisual child of ViewPrefab.")]
        public float RelativeScaleMultiplier = 1.0f;
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

        /// <summary>ScriptableObject this runtime row was created from (set by <see cref="GearConfig.CreateRuntimeData"/>).</summary>
        [NonSerialized] private GearConfig sourceGearConfig;

        public GearConfig SourceGearConfig
        {
            get => sourceGearConfig;
            set => sourceGearConfig = value;
        }

        public GearConfigData Clone(GearConfig nextLevelConfig, List<GearAbilitySO> abilities)
        {
            return new GearConfigData
            {
                Id = Id,
                Category = Category,
                BaseRotationSpeed = BaseRotationSpeed,
                ViewPrefab = ViewPrefab,
                UIIcon = UIIcon,
                RelativeScaleMultiplier = RelativeScaleMultiplier,
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
                Abilities = new List<GearAbilitySO>(abilities ?? new List<GearAbilitySO>()),
                SourceGearConfig = SourceGearConfig
            };
        }
    }
}
