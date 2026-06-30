using System;
using UnityEngine;
using GearEngine.GearEngine.Services.Inventory;
using GearEngine.Perks.Powerups;
using GearEngine.GearEngine.Config;
using System.Collections.Generic;

namespace GearEngine.Perks.Config
{
    [Serializable]
    public class PerkItemData : IItem
    {
        [SerializeField] private string id;
        public string Id { get => id; set => id = value; }
        
        [SerializeField] private string perkName;
        public string Name { get => string.IsNullOrEmpty(perkName) ? id : perkName; set => perkName = value; }

        [SerializeField] private ItemRarity rarity = ItemRarity.Common;
        public ItemRarity Rarity { get => rarity; set => rarity = value; }

        [SerializeField] private RarityConfigSO rarityConfig;
        public RarityConfigSO RarityConfig => rarityConfig;

        [SerializeField] [TextArea] private string description;
        public string Description 
        { 
            get 
            {
                if (modifiers == null || modifiers.Count == 0) return description;
                
                string values = "";
                for (int i = 0; i < modifiers.Count; i++)
                {
                    var m = modifiers[i];
                    if (m == null) continue;
                    string val = m.GetFormattedValue();
                    if (!string.IsNullOrEmpty(val))
                    {
                        if (values.Length > 0) values += ", ";
                        values += val;
                    }
                }
                
                if (values.Length > 0)
                {
                    return string.IsNullOrEmpty(description) ? values : $"{description} {values}";
                }
                return description;
            }
            set => description = value; 
        }

        public Sprite UIIcon;
        public Sprite Icon => UIIcon;

        [SerializeField] private List<CarPowerupModifierSO> modifiers = new List<CarPowerupModifierSO>();
        public IReadOnlyList<CarPowerupModifierSO> ModifierAssets => modifiers;

        public void CollectModifiers(List<ICarPowerupModifier> destination)
        {
            if (destination == null)
            {
                return;
            }

            for (var i = 0; i < modifiers.Count; i++)
            {
                CarPowerupModifierSO m = modifiers[i];
                if (m != null)
                {
                    destination.Add(m);
                }
            }
        }
    }
}
