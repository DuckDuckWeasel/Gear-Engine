using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.Campaign.Presentation;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using GearEngine.Currency;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.DTO.Tracks;
using Newtonsoft.Json;
using NUnit.Framework;
using Scaffold.LiveOps;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class MainTrackNavigationTests
    {
        private readonly List<TrackDefinition> tracks = new List<TrackDefinition>();
        private CarDefinition car;
        private TracksClientModule service;
        private TrackGameData data;
        private RecordingLiveOps liveOps;
        private RecordingNavigation navigation;

        [TearDown]
        public void TearDown()
        {
            foreach (TrackDefinition track in tracks)
            {
                Object.DestroyImmediate(track);
            }

            tracks.Clear();
            if (car != null)
            {
                Object.DestroyImmediate(car);
            }
        }

        [TestCase("")]
        [TestCase("Track2")]
        public async Task FreshPlayer_AlwaysStartsWithFirstConfiguredTrackUnlocked(string savedTrackId)
        {
            await InitializeTracks(3, 0);
            data.CurrentTrackId = savedTrackId;
            data.OrderedTrackIds.Remove(tracks[2].name);
            data.BestTimeSec.Clear();
            service = new TracksClientModule(liveOps, new CurrencyClientModule(liveOps), new TrackAssetIndex(tracks, car));
            await service.InitializeAsync(CancellationToken.None);

            MainViewModel model = CreateViewModel();
            Assert.That(service.IsTrackUnlocked(tracks[0].name), Is.True);
            Assert.That(service.IsTrackUnlocked(tracks[1].name), Is.False);
            Assert.That(service.CurrentTrack, Is.SameAs(tracks[0]));
            Assert.That(model.Track.Track, Is.SameAs(tracks[0]));
            Assert.That(model.IsTrackLocked, Is.False);
            model.ClickedPlay();
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<SetupViewModel>());
        }

        [Test]
        public async Task AllUnlocked_WrapsBothDirectionsAndUpdatesCounterAndStats()
        {
            await InitializeTracks(3, 2);
            MainViewModel model = CreateViewModel();
            Assert.That(model.TrackPosition, Is.EqualTo("3 of 3"));
            model.NextTrack();
            Assert.That(model.Track.Track, Is.SameAs(tracks[0]));
            Assert.That(model.TrackPosition, Is.EqualTo("1 of 3"));
            Assert.That(model.Stats.TrackName, Is.EqualTo(tracks[0].GetDisplayName()));
            model.PreviousTrack();
            Assert.That(model.Track.Track, Is.SameAs(tracks[2]));
            Assert.That(service.CurrentTrack, Is.SameAs(tracks[2]), "Browsing must not change the race selection.");
        }

        [Test]
        public async Task SingleUnlockedTrackWithoutLockedPreview_HidesNavigation()
        {
            await InitializeTracks(1, 0);
            MainViewModel model = CreateViewModel();
            Assert.That(model.CanNavigateTracks, Is.False);
            model.NextTrack();
            model.PreviousTrack();
            Assert.That(model.TrackPosition, Is.EqualTo("1 of 1"));
        }

        [Test]
        public async Task NextLockedTrack_IsPreviewOnlyAndCanReturnToUnlockedTrack()
        {
            await InitializeTracks(3, 0);
            MainViewModel model = CreateViewModel();
            model.NextTrack();
            Assert.That(model.Track.Track, Is.SameAs(tracks[1]));
            Assert.That(model.TrackPosition, Is.EqualTo("2 of 2"));
            Assert.That(model.IsTrackLocked, Is.True);
            model.ClickedPlay();
            Assert.That(navigation.OpenedControllers, Is.Empty);
            Assert.That(service.TrySelectTrack(tracks[1].name), Is.False);
            model.NextTrack();
            Assert.That(model.Track.Track, Is.SameAs(tracks[0]));
            Assert.That(model.IsTrackLocked, Is.False);
        }

        [Test]
        public async Task EmptyCatalog_HidesNavigationAndPreventsPlay()
        {
            await InitializeTracks(1, 0);
            data.OrderedTrackIds.Clear();
            MainViewModel model = CreateViewModel();
            Assert.That(model.CanNavigateTracks, Is.False);
            Assert.That(model.IsTrackLocked, Is.True);
            Assert.That(model.Track, Is.Null);
            Assert.That(model.TrackPosition, Is.EqualTo("0 of 0"));
            model.NextTrack();
            model.PreviousTrack();
            model.ClickedPlay();
            Assert.That(navigation.OpenedControllers, Is.Empty);
        }

        [Test]
        public async Task NoResolvableUnlockedTrack_ShowsOnlyLockedPreview()
        {
            await InitializeTracks(2, 0);
            MainViewModel model = new MainViewModel();
            ViewModelTestInject.InjectPrivateField(model, "trackService", new LockedTrackService(tracks[1]));
            model.Bind(navigation = new RecordingNavigation());
            Assert.That(model.CanNavigateTracks, Is.False);
            Assert.That(model.IsTrackLocked, Is.True);
            Assert.That(model.Track.Track, Is.SameAs(tracks[1]));
            Assert.That(model.TrackPosition, Is.EqualTo("1 of 1"));
        }

        [Test]
        public async Task PlayEarlierTrack_SubmitsSelectedIdAndKeepsUnlocksAfterReload()
        {
            await InitializeTracks(3, 2);
            MainViewModel model = CreateViewModel();
            model.NextTrack();
            model.ClickedPlay();
            Assert.That(service.CurrentTrack, Is.SameAs(tracks[0]));
            Assert.That(data.CurrentTrackId, Is.EqualTo(tracks[2].name));
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<SetupViewModel>());
            liveOps.Response = new RecordRaceResultResponse { NextTrackId = tracks[1].name, NewBestTimeSec = 12f };
            await service.RecordResultAsync(new RaceResultModel(12f, 1, tracks[0]));
            Assert.That(liveOps.Request.TrackId, Is.EqualTo(tracks[0].name));
            Assert.That(service.IsTrackUnlocked(tracks[2].name), Is.True);
            TracksClientModule reloaded = new TracksClientModule(liveOps, new CurrencyClientModule(liveOps), new TrackAssetIndex(tracks, car));
            await reloaded.InitializeAsync(CancellationToken.None);
            Assert.That(reloaded.IsTrackUnlocked(tracks[2].name), Is.True);
        }

        [Test]
        public async Task CompletedLastTrack_RestoresAllUnlocksAfterCampaignWraps()
        {
            await InitializeTracks(3, 0, 2);
            Assert.That(service.IsTrackUnlocked(tracks[2].name), Is.True);
            MainViewModel model = CreateViewModel();
            model.PreviousTrack();
            Assert.That(model.Track.Track, Is.SameAs(tracks[2]));
            Assert.That(model.IsTrackLocked, Is.False);
        }

        [Test]
        public async Task RaceUnlock_RefreshesCarouselAndSelectsNewTrack()
        {
            await InitializeTracks(3, 0);
            MainViewModel model = CreateViewModel();
            liveOps.Response = new RecordRaceResultResponse { NextTrackId = tracks[1].name, NewBestTimeSec = 12f };
            await service.RecordResultAsync(new RaceResultModel(12f, 1, tracks[0]));
            model.RefreshTracks();
            Assert.That(model.Track.Track, Is.SameAs(tracks[1]));
            Assert.That(model.IsTrackLocked, Is.False);
            Assert.That(model.TrackPosition, Is.EqualTo("2 of 3"));
            model.PreviousTrack();
            Assert.That(model.IsTrackLocked, Is.False);
        }

        private async Task InitializeTracks(int count, int currentIndex, int playedIndex = -1)
        {
            car = ScriptableObject.CreateInstance<CarDefinition>();
            List<object> entries = new List<object>();
            for (int i = 0; i < count; i++)
            {
                TrackDefinition track = ScriptableObject.CreateInstance<TrackDefinition>();
                track.name = $"Track{i}";
                tracks.Add(track);
                entries.Add(new { id = track.name, baseReward = 0, bands = Array.Empty<object>() });
            }

            TrackConfig config = JsonConvert.DeserializeObject<TrackConfig>(JsonConvert.SerializeObject(new { entries }));
            TrackPersistence persistence = new TrackPersistence { CurrentTrackId = tracks[currentIndex].name };
            if (currentIndex > 0)
            {
                persistence.BestTimeSec[tracks[currentIndex - 1].name] = 15f;
            }

            if (playedIndex >= 0)
            {
                persistence.BestTimeSec[tracks[playedIndex].name] = 15f;
            }

            data = new TrackGameData(persistence, config);
            liveOps = new RecordingLiveOps(data);
            service = new TracksClientModule(liveOps, new CurrencyClientModule(liveOps), new TrackAssetIndex(tracks, car));
            await service.InitializeAsync(CancellationToken.None);
        }

        private MainViewModel CreateViewModel()
        {
            MainViewModel model = new MainViewModel();
            navigation = new RecordingNavigation();
            ViewModelTestInject.InjectPrivateField(model, "trackService", service);
            model.Bind(navigation);
            return model;
        }

        private sealed class RecordingLiveOps : ILiveOpsService
        {
            private readonly TrackGameData data;
            public RecordRaceResultRequest Request { get; private set; }
            public RecordRaceResultResponse Response { get; set; }

            public RecordingLiveOps(TrackGameData data) { this.data = data; }
            public T GetModuleData<T>() where T : class, IGameModuleData => data as T;
            public Task<TResponse> CallAsync<TResponse>(ModuleRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : ModuleResponse
            {
                Request = request as RecordRaceResultRequest;
                return Task.FromResult(Response as TResponse);
            }
        }

        private sealed class LockedTrackService : ITrackService
        {
            private readonly TrackDefinition track;
            public LockedTrackService(TrackDefinition track) { this.track = track; }
            public TrackDefinition CurrentTrack => null;
            public CarDefinition CurrentCar => null;
            public TrackProgressModel GetTrackProgress() => new TrackProgressModel();
            public IReadOnlyList<TrackEntry> GetOrderedTracks() => new[] { new TrackEntry(track) };
            public bool IsTrackUnlocked(string trackId) => false;
            public bool TrySelectTrack(string trackId) => false;
            public Task RecordResultAsync(RaceResultModel result) => Task.CompletedTask;
        }
    }
}
