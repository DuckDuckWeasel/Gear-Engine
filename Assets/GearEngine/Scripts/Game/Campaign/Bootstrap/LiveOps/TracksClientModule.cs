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
