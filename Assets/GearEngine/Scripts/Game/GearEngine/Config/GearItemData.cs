using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Inventory;
using GearEngine.GearEngine.Visuals;

namespace GearEngine.GearEngine.Config
{
    [Serializable]
    public class GearItemData : IItem
    {
        [SerializeField] private string id;
        public string Id { get => id; set => id = value; }
        
        [SerializeField] private ItemRarity rarity = ItemRarity.Common;
        public ItemRarity Rarity { get => rarity; set => rarity = value; }
        
        [SerializeField] private RarityConfigSO rarityConfig;
        public RarityConfigSO RarityConfig { get => rarityConfig; set => rarityConfig = value; }

        [SerializeField] private string displayName;
        public string DisplayName { get => displayName; set => displayName = value; }

        public string Name => !string.IsNullOrEmpty(displayName) ? displayName : (SourceGearConfig != null ? SourceGearConfig.name : Id);

        [SerializeField] [TextArea] private string description;
        public string Description
        {
            get
            {
                if (Abilities == null || Abilities.Count == 0) return description;
                
                string values = "";
                foreach (var ability in Abilities)
                {
                    if (ability is IDescribable describable)
                    {
                        string val = describable.GetRichTextDescription();
                        if (!string.IsNullOrEmpty(val))
                        {
                            if (values.Length > 0) values += "\n";
                            values += val;
                        }
                    }
                }
                
                if (values.Length > 0)
                {
                    return string.IsNullOrEmpty(description) ? values : $"{description}\n\n{values}";
                }
                return description;
            }
            set => description = value;
        }

        
        public GearCategory Category = GearCategory.Base;
        public float BaseRotationSpeed;
        public GearView ViewPrefab;
        public Sprite UIIcon;
        public Sprite Icon => UIIcon;
        [Tooltip("Relative size modifier for this specific gear (1.0 is default), applied to the GearVisual child of ViewPrefab.")]
        public float RelativeScaleMultiplier = 1.0f;
        [Tooltip("Initial visual and trigger phase in degrees. Used by the Core Gear to keep contact timing aligned with its sprite.")]
        public float InitialRotationOffset;
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
        public Ami.BroAudio.SoundID HitSound;
        public Ami.BroAudio.SoundID ChargeCompleteSound;

        // Delete / Scrap Mechanics (opt-in)
        public bool IsDeletable = false;
        public int DeleteRewardAmount = 0;

        // Abilities — populated at runtime by GearItem.CreateRuntimeData(), not edited here.
        [HideInInspector] public List<GearAbilitySO> Abilities = new List<GearAbilitySO>();

        // Runtime copy of the next level config
        [NonSerialized] public GearItem NextLevelConfig;

        /// <summary>ScriptableObject this runtime row was created from (set by <see cref="GearItem.CreateRuntimeData"/>).</summary>
        [NonSerialized] private GearItem sourceGearConfig;

        /// <summary>LiveOps inventory instance this runtime row represents (tray / owned board gear).</summary>
        [NonSerialized] public OwnedGear Owner;

        public GearItem SourceGearConfig
        {
            get => sourceGearConfig;
            set => sourceGearConfig = value;
        }

        public GearItemData Clone(GearItem nextLevelConfig, List<GearAbilitySO> abilities)
        {
            return new GearItemData
            {
                Id = Id,
                DisplayName = DisplayName,
                Description = description,
                Rarity = Rarity,
                RarityConfig = RarityConfig,
                Category = Category,
                BaseRotationSpeed = BaseRotationSpeed,
                ViewPrefab = ViewPrefab,
                UIIcon = UIIcon,
                RelativeScaleMultiplier = RelativeScaleMultiplier,
                InitialRotationOffset = InitialRotationOffset,
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
                HitSound = HitSound,
                ChargeCompleteSound = ChargeCompleteSound,
                IsDeletable = IsDeletable,
                DeleteRewardAmount = DeleteRewardAmount,
                NextLevelConfig = nextLevelConfig,
                Abilities = new List<GearAbilitySO>(abilities ?? new List<GearAbilitySO>()),
                SourceGearConfig = SourceGearConfig,
                Owner = Owner
            };
        }
    }
}
