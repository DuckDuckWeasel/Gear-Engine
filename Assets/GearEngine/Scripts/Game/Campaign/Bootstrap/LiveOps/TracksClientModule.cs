using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.Tracks;
using LiveOps.Modules.DTO.ModuleRequests;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using GearEngine.Currency;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class TracksClientModule : GameClientModuleBase<TrackGameData>, ITrackService
    {
        private readonly CurrencyClientModule currencyClient;
        private readonly TrackCatalogSO catalog;
        private readonly TrackProgressModel progress = new TrackProgressModel();

        public TracksClientModule(
            ILiveOpsService liveOps,
            CurrencyClientModule currencyClient,
            TrackCatalogSO catalog)
            : base(liveOps)
        {
            this.currencyClient = currencyClient ?? throw new ArgumentNullException(nameof(currencyClient));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        protected override Task OnInitializedAsync(TrackGameData moduleData)
        {
            if (moduleData == null)
            {
                return Task.CompletedTask;
            }

            TrackGameData data = moduleData;

            List<string> ordered = data.OrderedTrackIds;
            if (catalog.GetTrack(data.CurrentTrackId) == null)
            {
                string resolved = null;
                if (ordered != null)
                {
                    foreach (string id in ordered)
                    {
                        if (!string.IsNullOrEmpty(id) && catalog.GetTrack(id) != null)
                        {
                            resolved = id;
                            break;
                        }
                    }
                }

                if (resolved == null)
                {
                    resolved = catalog.GetFirstResolvableTrackId();
                }

                if (!string.IsNullOrEmpty(resolved))
                {
                    data.CurrentTrackId = resolved;
                }
                else
                {
                    Debug.LogWarning(
                        "[TracksClientModule] No track id resolves in TrackCatalogSO (check Remote Config track list vs catalog TrackIds).");
                }
            }

            if (ordered != null)
            {
                for (int i = 0; i < ordered.Count; i++)
                {
                    string id = ordered[i];
                    if (string.IsNullOrEmpty(id))
                    {
                        continue;
                    }

                    if (catalog.GetTrack(id) == null)
                    {
                        Debug.LogWarning($"[TracksClientModule] Config id '{id}' has no asset in TrackCatalogSO.");
                    }
                }
            }

            progress.CurrentTrackIndex = Math.Max(0, IndexOfCurrentTrack(data));
            return Task.CompletedTask;
        }

        private static int IndexOfCurrentTrack(TrackGameData moduleData)
        {
            if (moduleData?.OrderedTrackIds == null || string.IsNullOrEmpty(moduleData.CurrentTrackId))
            {
                return 0;
            }

            return moduleData.OrderedTrackIds.IndexOf(moduleData.CurrentTrackId);
        }

        public TrackDefinition CurrentTrack => catalog.GetTrack(data?.CurrentTrackId ?? string.Empty);

        public CarDefinition CurrentCar => catalog.GetCarFor(data?.CurrentTrackId ?? string.Empty);

        public TrackProgressModel GetTrackProgress() => progress;

        public IReadOnlyList<TrackEntry> GetOrderedTracks()
        {
            IReadOnlyList<string> ids = data?.OrderedTrackIds;
            return catalog.OrderedEntries(ids ?? Array.Empty<string>());
        }

        public async Task RecordResultAsync(RaceResultModel result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            try
            {
                string trackId = data?.CurrentTrackId ?? string.Empty;
                if (string.IsNullOrEmpty(trackId))
                {
                    return;
                }

                RecordRaceResultResponse resp = await liveOps.CallAsync(new RecordRaceResultRequest(trackId, result.RaceTime));
                if (resp == null || data == null)
                {
                    return;
                }

                result.ServerOutcome = resp;

                if (resp.Responses != null)
                {
                    for (int i = 0; i < resp.Responses.Count; i++)
                    {
                        if (resp.Responses[i] is AddCurrencyResponse add)
                        {
                            currencyClient.ApplyNestedAddCurrency(add);
                        }
                    }
                }

                data.BestTimeSec[trackId] = resp.NewBestTimeSec;
                if (resp.Advanced && !string.IsNullOrEmpty(resp.NextTrackId))
                {
                    data.CurrentTrackId = resp.NextTrackId;
                    progress.CurrentTrackIndex = Math.Max(0, data.OrderedTrackIds.IndexOf(resp.NextTrackId));
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TracksClientModule] RecordResultAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
