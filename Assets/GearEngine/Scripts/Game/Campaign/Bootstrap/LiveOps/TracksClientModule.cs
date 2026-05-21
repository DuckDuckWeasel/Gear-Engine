using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.DTO.Tracks;
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
        public TracksClientModule(ILiveOpsService liveOps, CurrencyClientModule currencyClient, TrackAssetIndex index) : base(liveOps)
        {
            this.currencyClient = currencyClient ?? throw new ArgumentNullException(nameof(currencyClient));
            this.index = index ?? throw new ArgumentNullException(nameof(index));
        }

        public TrackDefinition CurrentTrack => index.GetTrack(data?.CurrentTrackId ?? string.Empty);

        public CarDefinition CurrentCar => index.DefaultCar;

        private readonly CurrencyClientModule currencyClient;
        private readonly TrackAssetIndex index;
        private readonly TrackProgressModel progress = new TrackProgressModel();

        public TrackProgressModel GetTrackProgress()
        {
            return progress;
        }

        public IReadOnlyList<TrackEntry> GetOrderedTracks()
        {
            IReadOnlyList<string> ids = data?.OrderedTrackIds;
            return index.OrderedEntries(ids ?? Array.Empty<string>());
        }

        public async Task RecordResultAsync(RaceResultModel result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            try
            {
                await ApplyRecordedRaceOutcomeAsync(result);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TracksClientModule] RecordResultAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        protected override Task OnInitializedAsync(TrackGameData moduleData)
        {
            RepairCurrentTrackIdIfNotInCatalog(moduleData);
            WarnWhenOrderedIdsMissingFromCatalog(moduleData.OrderedTrackIds);
            progress.CurrentTrackIndex = Math.Max(0, GetProgressIndexForTrack(moduleData));
            return Task.CompletedTask;
        }

        private void RepairCurrentTrackIdIfNotInCatalog(TrackGameData trackData)
        {
            if (index.GetTrack(trackData.CurrentTrackId) != null)
            {
                return;
            }

            string orderedMatch = ResolveFirstOrderedTrackIdInCatalog(trackData.OrderedTrackIds);
            string resolved = orderedMatch ?? index.GetFirstResolvableTrackId();
            if (!string.IsNullOrEmpty(resolved))
            {
                trackData.CurrentTrackId = resolved;
                return;
            }

            LogNoTrackResolvesInIndex();
        }

        private void LogNoTrackResolvesInIndex()
        {
            Debug.LogWarning(
                "[TracksClientModule] No track id resolves in the track index (check Remote Config track list vs TrackDefinition.name ids).");
        }

        private string ResolveFirstOrderedTrackIdInCatalog(List<string> ordered)
        {
            if (ordered == null)
            {
                return null;
            }

            foreach (string id in ordered)
            {
                if (!string.IsNullOrEmpty(id) && index.GetTrack(id) != null)
                {
                    return id;
                }
            }

            return null;
        }

        private void WarnWhenOrderedIdsMissingFromCatalog(List<string> ordered)
        {
            if (ordered == null)
            {
                return;
            }

            for (int i = 0; i < ordered.Count; i++)
            {
                WarnSingleOrderedIdIfMissing(ordered[i]);
            }
        }

        private void WarnSingleOrderedIdIfMissing(string id)
        {
            if (string.IsNullOrEmpty(id) || index.GetTrack(id) != null)
            {
                return;
            }

            Debug.LogWarning($"[TracksClientModule] Config id '{id}' has no matching TrackDefinition in the index.");
        }

        private int GetProgressIndexForTrack(TrackGameData moduleData)
        {
            if (moduleData?.OrderedTrackIds == null || string.IsNullOrEmpty(moduleData.CurrentTrackId))
            {
                return 0;
            }

            return moduleData.OrderedTrackIds.IndexOf(moduleData.CurrentTrackId);
        }

        private async Task ApplyRecordedRaceOutcomeAsync(RaceResultModel result)
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
            ApplyCurrencySideEffectsFromResponse(resp);
            data.BestTimeSec[trackId] = resp.NewBestTimeSec;
            ApplyAdvanceToNextTrackIfNeeded(resp);
        }

        private void ApplyCurrencySideEffectsFromResponse(RecordRaceResultResponse resp)
        {
            if (resp.Responses == null)
            {
                return;
            }

            for (int i = 0; i < resp.Responses.Count; i++)
            {
                if (resp.Responses[i] is AddCurrencyResponse add)
                {
                    currencyClient.ApplyNestedAddCurrency(add);
                }
            }
        }

        private void ApplyAdvanceToNextTrackIfNeeded(RecordRaceResultResponse resp)
        {
            if (string.IsNullOrEmpty(resp.NextTrackId))
            {
                return;
            }

            data.CurrentTrackId = resp.NextTrackId;
            progress.CurrentTrackIndex = Math.Max(0, data.OrderedTrackIds.IndexOf(resp.NextTrackId));
        }
    }
}
