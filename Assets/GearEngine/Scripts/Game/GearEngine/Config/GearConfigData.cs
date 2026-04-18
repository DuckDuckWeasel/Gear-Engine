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
        [Tooltip("Relative size modifier for this specific gear (1.0 is default). Multiplication depends on the BoardConfig's GlobalGearScale.")]
        public float RelativeScaleMultiplier = 1.0f;
        public TriggerPattern TriggerPattern = TriggerPattern.FourWay;
        public bool IsInteractable = true;
        public bool IsMovable = true;
        public bool IsReturnable = true;

        public float MaxCharge = 100f;
        public float ChargeOverTimeAmount = 10f;
        public float ChargeOnTriggerAmount = 25f;

        public float SnapSlowdownDuration = 0.5f;
        public float SnapSlowdownMultiplier = 0.15f;
        public float TriggerSpinDegrees = 45f;

        public bool IsDeletable = false;
        public int DeleteRewardAmount = 0;

        public List<GearAbilitySO> Abilities = new List<GearAbilitySO>();

        [NonSerialized] public GearConfig NextLevelConfig;

        public GearConfigData Clone(GearConfig nextLevelConfig, List<GearAbilitySO> abilities)
        {
            GearConfigData clone = CreateBaseClone();
            ApplyChargeSettings(clone);
            ApplyDeletionSettings(clone, nextLevelConfig, abilities);
            return clone;
        }

        private GearConfigData CreateBaseClone()
        {
            return new GearConfigData
            {
                Id = Id,
                Category = Category,
                BaseRotationSpeed = BaseRotationSpeed,
                VisualPrefab = VisualPrefab,
                UIIcon = UIIcon,
                RelativeScaleMultiplier = RelativeScaleMultiplier,
                TriggerPattern = TriggerPattern,
                IsInteractable = IsInteractable,
                IsMovable = IsMovable,
                IsReturnable = IsReturnable
            };
        }

        private void ApplyChargeSettings(GearConfigData clone)
        {
            clone.MaxCharge = MaxCharge;
            clone.ChargeOverTimeAmount = ChargeOverTimeAmount;
            clone.ChargeOnTriggerAmount = ChargeOnTriggerAmount;
            clone.SnapSlowdownDuration = SnapSlowdownDuration;
            clone.SnapSlowdownMultiplier = SnapSlowdownMultiplier;
            clone.TriggerSpinDegrees = TriggerSpinDegrees;
        }

        private void ApplyDeletionSettings(GearConfigData clone, GearConfig nextLevelConfig, List<GearAbilitySO> abilities)
        {
            clone.IsDeletable = IsDeletable;
            clone.DeleteRewardAmount = DeleteRewardAmount;
            clone.NextLevelConfig = nextLevelConfig;
            clone.Abilities = abilities == null ? new List<GearAbilitySO>() : new List<GearAbilitySO>(abilities);
        }
    }
}
