using System;
using LiveOps.Modules.DTO.Currency;
using Scaffold.LiveOps.Authoring;
using UnityEngine;

namespace GearEngine.Campaign.Authoring
{
    /// <summary>
    /// Serialized rows for currencies shipped in Remote Config (no separate catalog SO required).
    /// </summary>
    [Serializable]
    public sealed class CurrencyEntryDraft
    {
        public string id = "gold";

        public long initial;

        public bool hasMin;

        public long min;

        public bool hasMax;

        public long max;
    }

    [CreateAssetMenu(menuName = "LiveOps/Authoring/Currency Config Builder", fileName = "CurrencyConfigBuilder")]
    public sealed class CurrencyConfigBuilderSO : ConfigBuilderSO<CurrencyConfig>
    {
        [SerializeField]
        private CurrencyEntryDraft[] entries =
        {
            new CurrencyEntryDraft { id = "gold", initial = 0 },
        };

        public override string ConfigKey => nameof(CurrencyConfig);

        public override CurrencyConfig Build()
        {
            var cfg = new CurrencyConfig();

            if (entries == null)
            {
                return cfg;
            }

            foreach (CurrencyEntryDraft draft in entries)
            {
                if (draft == null || string.IsNullOrEmpty(draft.id))
                {
                    continue;
                }

                var row = new CurrencyConfigEntry
                {
                    Id = draft.id,
                    Initial = draft.initial,
                };

                if (draft.hasMin)
                {
                    row.Min = draft.min;
                }

                if (draft.hasMax)
                {
                    row.Max = draft.max;
                }

                cfg.AddEntry(row);
            }

            return cfg;
        }

        public override void Apply(CurrencyConfig pulled)
        {
            if (pulled == null || pulled.Entries.Count == 0)
            {
                return;
            }

            int n = pulled.Entries.Count;
            entries = new CurrencyEntryDraft[n];
            for (int i = 0; i < n; i++)
            {
                CurrencyConfigEntry e = pulled.Entries[i];
                var d = new CurrencyEntryDraft
                {
                    id = e.Id,
                    initial = e.Initial,
                    hasMin = e.Min.HasValue,
                    min = e.Min.GetValueOrDefault(),
                    hasMax = e.Max.HasValue,
                    max = e.Max.GetValueOrDefault(),
                };
                entries[i] = d;
            }
        }
    }
}
