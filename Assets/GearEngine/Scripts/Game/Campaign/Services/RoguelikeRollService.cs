using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.GearEngine.Config;
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

        public async Task<IReadOnlyList<GearConfig>> GetCurrentRollAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<string> ids = await module.EnsureCurrentRollAsync(cancellationToken);
            return MapIdsToConfigs(ids);
        }

        public Task ConsumePickAsync(GearConfig picked, CancellationToken cancellationToken = default)
        {
            if (picked == null)
            {
                throw new ArgumentNullException(nameof(picked));
            }

            return module.ClaimAsync(picked.Id, cancellationToken);
        }

        private List<GearConfig> MapIdsToConfigs(IReadOnlyList<string> ids)
        {
            List<GearConfig> result = new List<GearConfig>(ids.Count);
            for (int i = 0; i < ids.Count; i++)
            {
                TryAddConfig(ids[i], result);
            }

            return result;
        }

        private void TryAddConfig(string id, List<GearConfig> result)
        {
            GearConfig g = catalog.Get(id);
            if (g == null)
            {
                Debug.LogError($"[RoguelikeRollService] Roll referenced unknown gearId '{id}'.");
                return;
            }

            result.Add(g);
        }
    }
}
