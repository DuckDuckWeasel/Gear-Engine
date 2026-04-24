using LiveOps.Modules.DTO.Currency;
using UnityEngine;

namespace GearEngine.Campaign.Authoring
{
    [CreateAssetMenu(menuName = "LiveOps/Currency Definition", fileName = "Currency")]
    public sealed class CurrencySO : ScriptableObject
    {
        [SerializeField]
        private string id = "gold";

        [SerializeField]
        private long initial;

        [SerializeField]
        private bool hasMin;

        [SerializeField]
        private long min;

        [SerializeField]
        private bool hasMax;

        [SerializeField]
        private long max;

        [Header("Presentation (client only)")]
        [SerializeField]
        private string displayName;

        [SerializeField]
        private Sprite icon;

        [SerializeField]
        private Color displayColor = Color.white;

        public string Id => id;

        public string DisplayName => displayName;

        public Sprite Icon => icon;

        public Color DisplayColor => displayColor;

        public CurrencyConfigEntry ToConfigEntry()
        {
            var row = new CurrencyConfigEntry
            {
                Id = id,
                Initial = initial,
            };

            if (hasMin)
            {
                row.Min = min;
            }

            if (hasMax)
            {
                row.Max = max;
            }

            return row;
        }

        public void ApplyPulled(CurrencyConfigEntry entry)
        {
            if (entry == null || entry.Id != id)
            {
                return;
            }

            initial = entry.Initial;
            hasMin = entry.Min.HasValue;
            min = entry.Min.GetValueOrDefault();
            hasMax = entry.Max.HasValue;
            max = entry.Max.GetValueOrDefault();
        }
    }
}
