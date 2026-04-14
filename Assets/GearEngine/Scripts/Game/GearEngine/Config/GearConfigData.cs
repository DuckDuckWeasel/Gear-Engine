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
        public float UIScaleMultiplier = 115f;
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
            var copy = new GearConfigData();
            BuildCoreAndUiCopy(source: this, dest: copy);
            BuildProgressionCopy(source: this, dest: copy);
            copy.NextLevelConfig = nextLevelConfig;
            copy.Abilities = new List<GearAbilitySO>(abilities ?? new List<GearAbilitySO>());
            return copy;
        }

        private static void BuildCoreAndUiCopy(GearConfigData source, GearConfigData dest)
        {
            dest.Id = source.Id;
            dest.Category = source.Category;
            dest.BaseRotationSpeed = source.BaseRotationSpeed;
            dest.VisualPrefab = source.VisualPrefab;
            dest.UIIcon = source.UIIcon;
            dest.UIScaleMultiplier = source.UIScaleMultiplier;
            dest.TriggerPattern = source.TriggerPattern;
            dest.IsInteractable = source.IsInteractable;
            dest.IsMovable = source.IsMovable;
            dest.IsReturnable = source.IsReturnable;
        }

        private static void BuildProgressionCopy(GearConfigData source, GearConfigData dest)
        {
            dest.MaxCharge = source.MaxCharge;
            dest.ChargeOverTimeAmount = source.ChargeOverTimeAmount;
            dest.ChargeOnTriggerAmount = source.ChargeOnTriggerAmount;
            dest.SnapSlowdownDuration = source.SnapSlowdownDuration;
            dest.SnapSlowdownMultiplier = source.SnapSlowdownMultiplier;
            dest.TriggerSpinDegrees = source.TriggerSpinDegrees;
            dest.IsDeletable = source.IsDeletable;
            dest.DeleteRewardAmount = source.DeleteRewardAmount;
        }
    }
}
