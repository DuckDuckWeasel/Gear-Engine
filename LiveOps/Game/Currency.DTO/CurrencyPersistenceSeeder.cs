using System;

namespace LiveOps.Modules.DTO.Currency
{
    /// <summary>
    /// Seeds missing currency balances from config and clamps stored values to configured bounds.
    /// Shared by Cloud Code CurrencyModule.Initialize and tests.
    /// </summary>
    public static class CurrencyPersistenceSeeder
    {
        public static bool SeedAndClampInPlace(CurrencyPersistence persistence, CurrencyConfig config)
        {
            if (persistence == null)
            {
                throw new ArgumentNullException(nameof(persistence));
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            bool dirty = false;
            foreach (CurrencyConfigEntry entry in config.Entries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.Id))
                {
                    continue;
                }

                if (!persistence.TryGet(entry.Id, out long v))
                {
                    persistence.Set(entry.Id, entry.Initial);
                    dirty = true;
                    continue;
                }

                long clamped = v;
                if (entry.Min.HasValue && clamped < entry.Min.Value)
                {
                    clamped = entry.Min.Value;
                }

                if (entry.Max.HasValue && clamped > entry.Max.Value)
                {
                    clamped = entry.Max.Value;
                }

                if (clamped != v)
                {
                    persistence.Set(entry.Id, clamped);
                    dirty = true;
                }
            }

            return dirty;
        }
    }
}
