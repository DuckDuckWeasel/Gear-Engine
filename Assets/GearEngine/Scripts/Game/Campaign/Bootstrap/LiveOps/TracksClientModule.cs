using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameModuleDTO.Modules.Tracks;
using GameModuleDTO.ModuleRequests;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine.Config;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class TracksClientModule : GameClientModuleBase<TrackGameData>, ITrackService
    {
        private readonly ILiveOpsService liveOpsService;
        private readonly TrackCatalogSO catalog;
        private readonly TrackProgressModel progress = new TrackProgressModel();

        public TracksClientModule(IObjectResolver resolver, ILiveOpsService liveOps, TrackCatalogSO catalog)
            : base(resolver)
        {
            liveOpsService = liveOps ?? throw new ArgumentNullException(nameof(liveOps));
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        protected override Task OnInitializedAsync(TrackGameData moduleData)
        {
            progress.CurrentTrackIndex = Math.Max(0, IndexOfCurrentTrack(moduleData));
            TryRepairCurrentTrackId(moduleData);
            return Task.CompletedTask;
        }

        private void TryRepairCurrentTrackId(TrackGameData moduleData)
        {
            if (moduleData == null)
            {
                return;
            }

            string id = moduleData.CurrentTrackId ?? string.Empty;
            if (!string.IsNullOrEmpty(id) && catalog.GetTrack(id) != null)
            {
                return;
            }

            string repaired = PickFirstResolvableOrderedTrackId(moduleData) ?? catalog.GetFirstResolvableTrackId();
            if (string.IsNullOrEmpty(repaired))
            {
                Debug.LogError(
                    "[TracksClientModule] No track id could be resolved from LiveOps data or the track catalog; assign tracks on CampaignTrackCatalog / Remote Config.");
                return;
            }

            moduleData.CurrentTrackId = repaired;
            List<string> orderedIds = moduleData.OrderedTrackIds;
            if (orderedIds != null)
            {
                int idx = orderedIds.IndexOf(repaired);
                if (idx >= 0)
                {
                    progress.CurrentTrackIndex = idx;
                }
            }
        }

        private string PickFirstResolvableOrderedTrackId(TrackGameData moduleData)
        {
            List<string> ordered = moduleData.OrderedTrackIds;
            if (ordered == null || ordered.Count == 0)
            {
                return null;
            }

            int start = Math.Clamp(progress.CurrentTrackIndex, 0, ordered.Count - 1);
            for (int i = 0; i < ordered.Count; i++)
            {
                string candidate = ordered[(start + i) % ordered.Count];
                if (string.IsNullOrEmpty(candidate))
                {
                    continue;
                }

                if (catalog.GetTrack(candidate) != null)
                {
                    return candidate;
                }
            }

            return null;
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

        public IReadOnlyList<GearConfig> GetRoguelikeCardOptions() => catalog.GetRoguelikeCardOptions();

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

                RecordRaceResultResponse resp = await liveOpsService.CallAsync(new RecordRaceResultRequest(trackId, result.Score));
                if (resp == null || data == null)
                {
                    return;
                }

                data.BestScores[trackId] = resp.NewBestScore;
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
