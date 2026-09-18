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

        public TrackDefinition CurrentTrack => index.GetTrack(selectedTrackId ?? data?.CurrentTrackId ?? string.Empty);

        public CarDefinition CurrentCar => index.DefaultCar;

        private readonly CurrencyClientModule currencyClient;
        private readonly TrackAssetIndex index;
        private readonly TrackProgressModel progress = new TrackProgressModel();
        private readonly HashSet<string> unlockedTrackIds = new HashSet<string>(StringComparer.Ordinal);
        private string selectedTrackId;

        public TrackProgressModel GetTrackProgress()
        {
            return progress;
        }

        public IReadOnlyList<TrackEntry> GetOrderedTracks()
        {
            IReadOnlyList<string> ids = data?.OrderedTrackIds;
            return index.OrderedEntries(ids ?? Array.Empty<string>());
        }

        public bool IsTrackUnlocked(string trackId)
        {
            return !string.IsNullOrEmpty(trackId)
                && unlockedTrackIds.Contains(trackId)
                && index.GetTrack(trackId) != null;
        }

        public bool TrySelectTrack(string trackId)
        {
            if (!IsTrackUnlocked(trackId))
            {
                return false;
            }

            selectedTrackId = trackId;
            return true;
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
            if (moduleData.BestTimeSec != null)
            {
                foreach (KeyValuePair<string, float> best in moduleData.BestTimeSec)
                {
                    progress.RecordBestTime(best.Key, best.Value);
                }
            }
            foreach (TrackDefinition track in index.All)
            {
                if (track != null)
                {
                    progress.RecordEarnedStars(track.name, PlayerPrefs.GetInt(StarStorageKey(track.name), 0));
                }
            }
            RepairCurrentTrackIdIfNotInCatalog(moduleData);
            WarnWhenOrderedIdsMissingFromCatalog(moduleData.OrderedTrackIds);
            RestoreUnlockedTracks(moduleData);
            selectedTrackId = null;
            progress.CurrentTrackIndex = Math.Max(0, GetProgressIndexForTrack(moduleData));
            return Task.CompletedTask;
        }

        private void RestoreUnlockedTracks(TrackGameData moduleData)
        {
            unlockedTrackIds.Clear();
            List<string> ordered = moduleData.OrderedTrackIds;
            if (ordered == null || ordered.Count == 0)
            {
                return;
            }

            int lastUnlocked = FindLastUnlockedIndex(moduleData);
            for (int i = 0; i <= lastUnlocked && i < ordered.Count; i++)
            {
                unlockedTrackIds.Add(ordered[i]);
            }
        }

        private static int FindLastUnlockedIndex(TrackGameData moduleData)
        {
            List<string> ordered = moduleData.OrderedTrackIds;
            int lastUnlocked = ordered.IndexOf(moduleData.CurrentTrackId);
            for (int i = 0; i < ordered.Count; i++)
            {
                if (moduleData.BestTimeSec != null && moduleData.BestTimeSec.ContainsKey(ordered[i]))
                {
                    lastUnlocked = Math.Max(lastUnlocked, i + 1);
                }
            }

            return lastUnlocked;
        }

        private void RepairCurrentTrackIdIfNotInCatalog(TrackGameData trackData)
        {
            List<string> ordered = trackData.OrderedTrackIds;
            bool isConfigured = ordered == null || ordered.Count == 0 || ordered.Contains(trackData.CurrentTrackId);
            if (isConfigured && index.GetTrack(trackData.CurrentTrackId) != null)
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
            string trackId = selectedTrackId ?? data?.CurrentTrackId ?? string.Empty;
            if (string.IsNullOrEmpty(trackId))
            {
                return;
            }

            RecordLocalStars(trackId, result.HighestAchievedTier);

            RecordRaceResultResponse resp = await liveOps.CallAsync(new RecordRaceResultRequest(trackId, result.RaceTime));
            if (resp == null || data == null)
            {
                return;
            }

            result.ServerOutcome = resp;
            ApplyCurrencySideEffectsFromResponse(resp);
            data.BestTimeSec[trackId] = resp.NewBestTimeSec;
            progress.RecordBestTime(trackId, resp.NewBestTimeSec);
            ApplyAdvanceToNextTrackIfNeeded(resp);
        }

        private void RecordLocalStars(string trackId, int earnedStars)
        {
            progress.RecordEarnedStars(trackId, earnedStars);
            PlayerPrefs.SetInt(StarStorageKey(trackId), progress.GetEarnedStars(trackId));
            PlayerPrefs.Save();
        }

        private static string StarStorageKey(string trackId) => $"GearEngine.TrackStars.V1.{trackId}";

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
            unlockedTrackIds.Add(resp.NextTrackId);
            selectedTrackId = null;
            progress.CurrentTrackIndex = Math.Max(0, data.OrderedTrackIds.IndexOf(resp.NextTrackId));
        }
    }
}
