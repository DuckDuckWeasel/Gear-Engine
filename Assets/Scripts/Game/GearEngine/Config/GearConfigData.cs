using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.GearEngine.Config
{
    [Serializable]
    public class GearConfigData
    {
        public string Id;
        public GearCategory Category = GearCategory.Base;
        public float BaseRotationSpeed;
        public GameObject VisualPrefab;
        public Sprite UIIcon;
        public float UIScaleMultiplier = 115f;
        public TriggerPattern TriggerPattern = TriggerPattern.FourWay;
        public bool IsInteractable = true;

        public float MaxCharge = 100f;
        public float ChargeOverTimeAmount = 10f;
        public float ChargeOnTriggerAmount = 25f;

        public float SnapSlowdownDuration = 0.5f;
        public float SnapSlowdownMultiplier = 0.15f;
        public float TriggerSpinDegrees = 45f;

        public List<GearAbilitySO> Abilities = new List<GearAbilitySO>();

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
                MaxCharge = MaxCharge,
                ChargeOverTimeAmount = ChargeOverTimeAmount,
                ChargeOnTriggerAmount = ChargeOnTriggerAmount,
                SnapSlowdownDuration = SnapSlowdownDuration,
                SnapSlowdownMultiplier = SnapSlowdownMultiplier,
                TriggerSpinDegrees = TriggerSpinDegrees,
                NextLevelConfig = nextLevelConfig,
                Abilities = new List<GearAbilitySO>(abilities ?? new List<GearAbilitySO>()),
            };
        }
    }
}
