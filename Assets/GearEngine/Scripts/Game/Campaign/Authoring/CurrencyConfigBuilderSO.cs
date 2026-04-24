using System.Collections.Generic;
using LiveOps.Modules.DTO.Currency;
using Scaffold.LiveOps.Authoring;
using UnityEngine;

namespace GearEngine.Campaign.Authoring
{
    /// <summary>
    /// Authors <see cref="CurrencyConfig"/> for Remote Config from <see cref="CurrencySO"/> assets (ids + bounds + editor icons).
    /// </summary>
    [CreateAssetMenu(menuName = "LiveOps/Authoring/Currency Config Builder", fileName = "CurrencyConfigBuilder")]
    public sealed class CurrencyConfigBuilderSO : ConfigBuilderSO<CurrencyConfig>
    {
        [SerializeField]
        private List<CurrencySO> currencies = new List<CurrencySO>();

        public override string ConfigKey => nameof(CurrencyConfig);

        public override CurrencyConfig Build()
        {
            var cfg = new CurrencyConfig();

            if (currencies == null)
            {
                return cfg;
            }

            foreach (CurrencySO so in currencies)
            {
                if (so == null || string.IsNullOrEmpty(so.Id))
                {
                    continue;
                }

                cfg.AddEntry(so.ToConfigEntry());
            }

            return cfg;
        }

        public override void Apply(CurrencyConfig pulled)
        {
            if (pulled == null || pulled.Entries.Count == 0 || currencies == null)
            {
                return;
            }

            foreach (CurrencySO so in currencies)
            {
                if (so == null || string.IsNullOrEmpty(so.Id))
                {
                    continue;
                }

                if (pulled.TryGet(so.Id, out CurrencyConfigEntry entry))
                {
                    so.ApplyPulled(entry);
                }
            }
        }
    }
}
