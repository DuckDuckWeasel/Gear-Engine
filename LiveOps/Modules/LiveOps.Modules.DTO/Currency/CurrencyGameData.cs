using System;
using System.Collections.Generic;
using LiveOps.DTO.GameModule;
using Newtonsoft.Json;

namespace LiveOps.Modules.DTO.Currency
{
    public sealed class CurrencyGameData : IGameModuleData
    {
        public string Key => nameof(CurrencyGameData);

        [JsonProperty("wallets")]
        private List<CurrencyWallet> _wallets = new List<CurrencyWallet>();

        [JsonIgnore]
        public IReadOnlyList<CurrencyWallet> Wallets => _wallets;

        [JsonConstructor]
        private CurrencyGameData()
        {
        }

        /// <summary>
        /// Server-only ctor. Assumes <paramref name="persistence"/> already has an entry
        /// for every configured id (seeded by CurrencyModule.Initialize).
        /// </summary>
        public CurrencyGameData(CurrencyPersistence persistence, CurrencyConfig config)
        {
            if (persistence == null)
            {
                throw new ArgumentNullException(nameof(persistence));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            foreach (CurrencyConfigEntry entry in config.Entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id))
                {
                    continue;
                }

                if (!persistence.TryGet(entry.Id, out long current))
                {
                    continue;
                }

                _wallets.Add(new CurrencyWallet
                {
                    Id = entry.Id,
                    Current = current,
                    Min = entry.Min,
                    Max = entry.Max,
                });
            }
        }

        public CurrencyWallet GetWallet(string id)
        {
            for (int i = 0; i < _wallets.Count; i++)
            {
                if (_wallets[i] != null && _wallets[i].Id == id)
                {
                    return _wallets[i];
                }
            }

            return null;
        }
    }
}
