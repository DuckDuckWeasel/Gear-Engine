using System.Collections.Generic;
using GearEngine.Cards.Powerups;
using UnityEngine;

namespace GearEngine.Cards
{
    [CreateAssetMenu(menuName = "Game/Cards/Card Definition", fileName = "CardDefinition")]
    public class CardDefinition : ScriptableObject
    {
        public string Id => id;

        [SerializeField] private string id;

        public IReadOnlyList<CarPowerupModifierSO> ModifierAssets => modifiers;

        [SerializeField] private List<CarPowerupModifierSO> modifiers = new List<CarPowerupModifierSO>();

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
