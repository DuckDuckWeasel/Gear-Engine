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

        [Test]
        public async Task FreshPlayer_CanPreviewNextLockedTrackWithoutStartingIt()
        {
            await InitializeTracks(3, 0);

            MainViewModel model = CreateViewModel();

            Assert.That(model.Track.Track, Is.SameAs(tracks[0]));
            Assert.That(model.TrackPosition, Is.EqualTo("1 / 2"));
            Assert.That(model.CanNavigateTracks, Is.True);
            Assert.That(model.IsTrackLocked, Is.False);

            model.NextTrack();

            Assert.That(model.Track.Track, Is.SameAs(tracks[1]));
            Assert.That(model.TrackPosition, Is.EqualTo("TRACK LOCKED"));
            Assert.That(model.IsTrackLocked, Is.True);
            model.ClickedPlay();
            Assert.That(navigation.OpenedControllers, Is.Empty);

            model.NextTrack();
            Assert.That(model.Track.Track, Is.SameAs(tracks[0]));
            Assert.That(model.TrackPosition, Is.EqualTo("1 / 2"));
            Assert.That(model.IsTrackLocked, Is.False);
        }

        [Test]
        public async Task SingleConfiguredTrack_HidesNavigation()
        {
            await InitializeTracks(1, 0);

            MainViewModel model = CreateViewModel();

            Assert.That(model.TrackPosition, Is.EqualTo("1 / 1"));
            Assert.That(model.CanNavigateTracks, Is.False);
            Assert.That(model.IsTrackLocked, Is.False);
        }

        [Test]
        public async Task AvailableTracks_WrapBothDirectionsAndUpdateStats()
        {
            await InitializeTracks(3, 2);
            MainViewModel model = CreateViewModel();

            Assert.That(model.TrackPosition, Is.EqualTo("3 / 3"));
            model.NextTrack();
            Assert.That(model.Track.Track, Is.SameAs(tracks[0]));
            Assert.That(model.TrackPosition, Is.EqualTo("1 / 3"));
            Assert.That(model.Stats.TrackName, Is.EqualTo(tracks[0].GetDisplayName()));
            model.PreviousTrack();
            Assert.That(model.Track.Track, Is.SameAs(tracks[2]));
            Assert.That(service.CurrentTrack, Is.SameAs(tracks[2]), "Browsing must not change the race selection.");
        }

        [Test]
        public async Task PlayEarlierTrack_SelectsItForSetupAndResultSubmission()
        {
            await InitializeTracks(3, 2);
            MainViewModel model = CreateViewModel();
            model.NextTrack();

            model.ClickedPlay();

            Assert.That(service.CurrentTrack, Is.SameAs(tracks[0]));
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<SetupViewModel>());
            liveOps.Response = new RecordRaceResultResponse { NextTrackId = tracks[1].name, NewBestTimeSec = 12f };
            await service.RecordResultAsync(new RaceResultModel(12f, 1, tracks[0]));
            Assert.That(liveOps.Request.TrackId, Is.EqualTo(tracks[0].name));
        }

        [Test]
        public async Task NewlyUnlockedTrack_AppearsAfterHomeRefresh()
        {
            await InitializeTracks(3, 0);
            MainViewModel model = CreateViewModel();
            liveOps.Response = new RecordRaceResultResponse { NextTrackId = tracks[1].name, NewBestTimeSec = 12f };
            await service.RecordResultAsync(new RaceResultModel(12f, 1, tracks[0]));

            model.RefreshTracks();

            Assert.That(model.Track.Track, Is.SameAs(tracks[1]));
            Assert.That(model.TrackPosition, Is.EqualTo("2 / 3"));
            Assert.That(model.CanNavigateTracks, Is.True);
        }

        [Test]
        public void OpenStorage_ShowsCatalogWhenPlayerOwnsNoPerks()
        {
            MainViewModel model = new MainViewModel();
            navigation = new RecordingNavigation();
            ViewModelTestInject.InjectNavigation(model, navigation);

            model.ClickedTalentPerks();

            Assert.That(navigation.OpenedControllers, Has.Count.EqualTo(1));
            ItemsViewModel items = navigation.OpenedControllers[0] as ItemsViewModel;
            Assert.That(items, Is.Not.Null);
            Assert.That(items.Config.ShowUnownedItems, Is.True);
            Object.DestroyImmediate(items.Config);
        }

        [Test]
        public void StoreToolbar_ShowsCatalogWhenPlayerOwnsNoPerks()
        {
            GameObject toolbarObject = new GameObject("ToolbarTest");
            ItemsViewModel items = null;

            try
            {
                ToolbarController toolbar = toolbarObject.AddComponent<ToolbarController>();
                navigation = new RecordingNavigation();
                toolbar.Construct(navigation);
                System.Reflection.MethodInfo openItemsView = typeof(ToolbarController).GetMethod(
                    "OpenItemsView",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                Assert.That(openItemsView, Is.Not.Null);
                openItemsView.Invoke(toolbar, new object[] { ItemScreenType.Perks, true, "Storage", "MAX OUT YOUR GEAR" });

                Assert.That(navigation.OpenedControllers, Has.Count.EqualTo(1));
                items = navigation.OpenedControllers[0] as ItemsViewModel;
                Assert.That(items, Is.Not.Null);
                Assert.That(items.Config.ShowUnownedItems, Is.True);
            }
            finally
            {
                if (items != null)
                {
                    Object.DestroyImmediate(items.Config);
                }

                Object.DestroyImmediate(toolbarObject);
            }
        }

        [Test]
        public void StoreToRace_LeavesMainViewActivationToNavigation()
        {
            GameObject toolbarObject = new GameObject("ToolbarTest");
            GameObject mainObject = new GameObject("InactiveMainViewTest");
            ItemsScreenState config = ScriptableObject.CreateInstance<ItemsScreenState>();

            try
            {
                ToolbarController toolbar = toolbarObject.AddComponent<ToolbarController>();
                mainObject.AddComponent<MainView>();
                mainObject.SetActive(false);
                navigation = new RecordingNavigation
                {
                    CurrentController = new ItemsViewModel(config),
                };
                toolbar.Construct(navigation);

                toolbar.OpenMainView();

                Assert.That(mainObject.activeSelf, Is.False, "Navigation must own MainView activation and binding.");
                Assert.That(navigation.OpenedControllers, Has.Count.EqualTo(1));
                Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<MainViewModel>());
            }
            finally
            {
                Object.DestroyImmediate(config);
                Object.DestroyImmediate(mainObject);
                Object.DestroyImmediate(toolbarObject);
            }
        }

        private async Task InitializeTracks(int count, int currentIndex)
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
            for (int i = 0; i < currentIndex; i++)
            {
                persistence.BestTimeSec[tracks[i].name] = 15f;
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

            public RecordingLiveOps(TrackGameData data)
            {
                this.data = data;
            }

            public RecordRaceResultRequest Request { get; private set; }
            public RecordRaceResultResponse Response { get; set; }

            public T GetModuleData<T>() where T : class, IGameModuleData => data as T;

            public Task<TResponse> CallAsync<TResponse>(
                ModuleRequest<TResponse> request,
                CancellationToken cancellationToken = default)
                where TResponse : ModuleResponse
            {
                Request = request as RecordRaceResultRequest;
                return Task.FromResult(Response as TResponse);
            }
        }
    }
}
