using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    public sealed class RoguelikeRollService : IRoguelikeRollService
    {
        public RoguelikeRollService(RoguelikeClientModule module, GearCatalogSO catalog)
        {
            this.module = module ?? throw new ArgumentNullException(nameof(module));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        private readonly RoguelikeClientModule module;
        private readonly GearCatalogSO catalog;

        public async Task<IReadOnlyList<IItem>> GetCurrentRollAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<string> ids = await module.EnsureCurrentRollAsync(cancellationToken);
            return GenerateFallbackIfNeeded(ids);
        }

        public Task ConsumePickAsync(string pickedId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pickedId))
            {
                throw new ArgumentNullException(nameof(pickedId));
            }

            return module.ClaimAsync(pickedId, cancellationToken);
        }

        public Task SkipPickAsync(CancellationToken cancellationToken = default)
        {
            return module.SkipAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<IItem>> RerollAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<string> ids = await module.RerollAsync(cancellationToken);
            return GenerateFallbackIfNeeded(ids);
        }

        private IReadOnlyList<IItem> GenerateFallbackIfNeeded(IReadOnlyList<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                // Fallback: mock a random roll locally.
                List<GearItem> validConfigs = new List<GearItem>();
                foreach (GearItem gear in catalog.All)
                {
                    if (gear != null && !string.IsNullOrEmpty(gear.Id))
                    {
                        validConfigs.Add(gear);
                    }
                }

                if (validConfigs.Count > 0)
                {
                    List<IItem> fallbackRoll = new List<IItem>(3);
                    System.Random rng = new System.Random();
                    for (int i = 0; i < 3; i++)
                    {
                        fallbackRoll.Add(validConfigs[rng.Next(validConfigs.Count)].CreateRuntimeData());
                    }
                    return fallbackRoll;
                }
            }

            return MapIdsToConfigs(ids);
        }

        private List<IItem> MapIdsToConfigs(IReadOnlyList<string> ids)
        {
            List<IItem> result = new List<IItem>(ids.Count);
            for (int i = 0; i < ids.Count; i++)
            {
                TryAddConfig(ids[i], result);
            }

            return result;
        }

        private void TryAddConfig(string id, List<IItem> result)
        {
            GearItem g = catalog.Get(id);
            if (g == null)
            {
                Debug.LogError($"[RoguelikeRollService] Roll referenced unknown gearId '{id}'.");
                return;
            }

            result.Add(g.CreateRuntimeData());
        }
    }
}
