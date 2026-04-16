using System.Collections.Generic;
using GearEngine.Cards.Powerups;
using UnityEngine;

namespace GearEngine.Cards
{
    [CreateAssetMenu(menuName = "Game/Cards/Card Definition", fileName = "CardDefinition")]
    public class CardDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private List<CarPowerupModifierSO> modifiers = new List<CarPowerupModifierSO>();

        public string Id => id;

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
