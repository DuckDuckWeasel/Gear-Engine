using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.Core.Config.Events;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;
using UnityEngine;
using Scaffold.Analytics;
using Scaffold.Events.Contracts;
using GearEngine.Campaign.Analytics;

namespace GearEngine.Campaign.Services
{
    public sealed class RoguelikeRollService : IRoguelikeRollService
    {
        public RoguelikeRollService(RoguelikeClientModule module, GearCatalogSO catalog, IAnalyticsService analytics, IEventBus eventBus)
        {
            this.module = module ?? throw new ArgumentNullException(nameof(module));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.analytics = analytics;
            this.eventBus = eventBus;
        }

        private readonly RoguelikeClientModule module;
        private readonly GearCatalogSO catalog;
        private readonly IAnalyticsService analytics;
        private readonly IEventBus eventBus;

        public async Task<IReadOnlyList<IItem>> GetCurrentRollAsync(CancellationToken cancellationToken = default)
        {
            eventBus?.Raise(new GlobalLoadingEvent(true));
            try
            {
                IReadOnlyList<string> ids = await module.EnsureCurrentRollAsync(cancellationToken);
                return GenerateFallbackIfNeeded(ids);
            }
            finally
            {
                eventBus?.Raise(new GlobalLoadingEvent(false));
            }
        }

        public async Task ConsumePickAsync(string pickedId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pickedId))
            {
                throw new ArgumentNullException(nameof(pickedId));
            }

            analytics?.Record(new RoguelikeRollEvent());
            
            eventBus?.Raise(new GlobalLoadingEvent(true));
            try
            {
                await module.ClaimAsync(pickedId, cancellationToken);
            }
            finally
            {
                eventBus?.Raise(new GlobalLoadingEvent(false));
            }
        }

        public async Task SkipPickAsync(CancellationToken cancellationToken = default)
        {
            eventBus?.Raise(new GlobalLoadingEvent(true));
            try
            {
                await module.SkipAsync(cancellationToken);
            }
            finally
            {
                eventBus?.Raise(new GlobalLoadingEvent(false));
            }
        }

        public async Task<IReadOnlyList<IItem>> RerollAsync(CancellationToken cancellationToken = default)
        {
            analytics?.Record(new RoguelikeRollEvent());
            
            eventBus?.Raise(new GlobalLoadingEvent(true));
            try
            {
                IReadOnlyList<string> ids = await module.RerollAsync(cancellationToken);
                return GenerateFallbackIfNeeded(ids);
            }
            finally
            {
                eventBus?.Raise(new GlobalLoadingEvent(false));
            }
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
