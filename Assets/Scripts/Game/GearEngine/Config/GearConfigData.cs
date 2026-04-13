using System;
using System.Collections.Generic;
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
            GearConfigData copy = CloneWithoutReferences();
            copy.NextLevelConfig = nextLevelConfig;
            copy.Abilities = new List<GearAbilitySO>(abilities ?? new List<GearAbilitySO>());
            return copy;
        }

        private GearConfigData CloneWithoutReferences()
        {
            GearConfigData copy = new GearConfigData();
            CopyScalarFieldsTo(copy);
            return copy;
        }

        private void CopyScalarFieldsTo(GearConfigData target)
        {
            target.Id = Id;
            target.Category = Category;
            target.BaseRotationSpeed = BaseRotationSpeed;
            target.VisualPrefab = VisualPrefab;
            target.UIIcon = UIIcon;
            target.UIScaleMultiplier = UIScaleMultiplier;
            target.TriggerPattern = TriggerPattern;
            target.IsInteractable = IsInteractable;
            target.MaxCharge = MaxCharge;
            target.ChargeOverTimeAmount = ChargeOverTimeAmount;
            target.ChargeOnTriggerAmount = ChargeOnTriggerAmount;
            target.SnapSlowdownDuration = SnapSlowdownDuration;
            target.SnapSlowdownMultiplier = SnapSlowdownMultiplier;
            target.TriggerSpinDegrees = TriggerSpinDegrees;
        }
    }
}
