using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.DTO.Tracks;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.Campaign.Services;
using GearEngine.Currency;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine.Config;
using Newtonsoft.Json;
using NUnit.Framework;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class TrackGameDataTests
    {
        [Test]
        public void Ctor_CopiesOrderedTrackIdsFromConfig()
        {
            TrackConfig config = JsonConvert.DeserializeObject<TrackConfig>(
                "{\"entries\":[{\"id\":\"a\",\"baseReward\":0,\"bands\":[]},{\"id\":\"b\",\"baseReward\":0,\"bands\":[]}]}");
            TrackPersistence persistence = new TrackPersistence { CurrentTrackId = "a" };

            TrackGameData data = new TrackGameData(persistence, config);

            Assert.That(data.OrderedTrackIds.Count, Is.EqualTo(2));
            Assert.That(data.OrderedTrackIds[0], Is.EqualTo("a"));
            Assert.That(data.CurrentTrackId, Is.EqualTo("a"));
        }

        [Test]
        public async Task TracksClientModule_InitializeAsync_WhenCurrentTrackIdEmpty_RepairsToFirstOrderedTrackInCatalog()
        {
            TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.name = "track_alpha";
            CarDefinition carDef = ScriptableObject.CreateInstance<CarDefinition>();

            TrackAssetIndex index = new TrackAssetIndex(
                new List<TrackDefinition> { trackDef },
                carDef);

            TrackPersistence persistence = new TrackPersistence { CurrentTrackId = string.Empty };
            TrackConfig config = JsonConvert.DeserializeObject<TrackConfig>(
                "{\"entries\":[{\"id\":\"track_alpha\",\"baseReward\":0,\"bands\":[]}]}");
            TrackGameData gameData = new TrackGameData(persistence, config);

            StubLiveOps liveOps = new StubLiveOps(gameData);
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance<ILiveOpsService>(liveOps);
            builder.RegisterInstance<Scaffold.Analytics.IAnalyticsService>(new RecordingAnalytics());
            builder.RegisterInstance<Scaffold.Events.Contracts.IEventBus>(new Scaffold.Events.EventController());
            builder.Register<CurrencyClientModule>(Lifetime.Singleton);
            using IObjectResolver resolver = builder.Build();

            CurrencyClientModule currency = resolver.Resolve<CurrencyClientModule>();
            TracksClientModule module = new TracksClientModule(liveOps, currency, index);
            await module.InitializeAsync(CancellationToken.None);

            Assert.That(module.CurrentTrack, Is.SameAs(trackDef));
            Assert.That(module.CurrentCar, Is.SameAs(carDef));
            Assert.That(gameData.CurrentTrackId, Is.EqualTo("track_alpha"));

            Object.DestroyImmediate(trackDef);
            Object.DestroyImmediate(carDef);
        }

        [Test]
        public async Task TracksClientModule_InitializeAsync_WhenRemoteTrackListEmpty_RepairsToFirstCatalogTrack()
        {
            TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.name = "local_only";
            CarDefinition carDef = ScriptableObject.CreateInstance<CarDefinition>();

            TrackAssetIndex index = new TrackAssetIndex(
                new List<TrackDefinition> { trackDef },
                carDef);

            TrackPersistence persistence = new TrackPersistence { CurrentTrackId = string.Empty };
            TrackConfig config = JsonConvert.DeserializeObject<TrackConfig>(
                "{\"entries\":[]}");
            TrackGameData gameData = new TrackGameData(persistence, config);

            StubLiveOps liveOps = new StubLiveOps(gameData);
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance<ILiveOpsService>(liveOps);
            builder.RegisterInstance<Scaffold.Analytics.IAnalyticsService>(new RecordingAnalytics());
            builder.RegisterInstance<Scaffold.Events.Contracts.IEventBus>(new Scaffold.Events.EventController());
            builder.Register<CurrencyClientModule>(Lifetime.Singleton);
            using IObjectResolver resolver = builder.Build();

            CurrencyClientModule currency = resolver.Resolve<CurrencyClientModule>();
            TracksClientModule module = new TracksClientModule(liveOps, currency, index);
            await module.InitializeAsync(CancellationToken.None);

            Assert.That(module.CurrentTrack, Is.SameAs(trackDef));
            Assert.That(module.CurrentCar, Is.SameAs(carDef));
            Assert.That(gameData.CurrentTrackId, Is.EqualTo("local_only"));

            Object.DestroyImmediate(trackDef);
            Object.DestroyImmediate(carDef);
        }

        [Test]
        public void TrackAssetIndex_GetFirstResolvableTrackId_ReturnsFirstValidEntryId()
        {
            TrackDefinition trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.name = "only_track";
            CarDefinition carDef = ScriptableObject.CreateInstance<CarDefinition>();
            TrackAssetIndex index = new TrackAssetIndex(
                new List<TrackDefinition> { trackDef },
                carDef);

            Assert.That(index.GetFirstResolvableTrackId(), Is.EqualTo("only_track"));

            Object.DestroyImmediate(trackDef);
            Object.DestroyImmediate(carDef);
        }

        [Test]
        public async Task TracksClientModule_RetainsBestScoreStarsAcrossReloadAndSlowerResults()
        {
            const string trackId = "HomeStarsPersistenceRegression";
            const string key = "GearEngine.TrackStars.V1." + trackId;
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithTiersForTests(
                new TrackTierConfig(1f, 1000, 10), new TrackTierConfig(1f, 2000, 20), new TrackTierConfig(1f, 3000, 30));
            track.name = trackId;
            CarDefinition car = ScriptableObject.CreateInstance<CarDefinition>();
            try
            {
                PlayerPrefs.DeleteKey(key);
                TrackConfig config = JsonConvert.DeserializeObject<TrackConfig>(
                    "{\"entries\":[{\"id\":\"" + trackId + "\",\"baseReward\":0,\"bands\":[]}]}");
                TrackGameData data = new TrackGameData(new TrackPersistence { CurrentTrackId = trackId }, config);
                StubLiveOps liveOps = new StubLiveOps(data);
                ContainerBuilder builder = new ContainerBuilder();
                builder.RegisterInstance<ILiveOpsService>(liveOps);
                builder.RegisterInstance<Scaffold.Analytics.IAnalyticsService>(new RecordingAnalytics());
                builder.RegisterInstance<Scaffold.Events.Contracts.IEventBus>(new Scaffold.Events.EventController());
                builder.Register<CurrencyClientModule>(Lifetime.Singleton);
                using IObjectResolver resolver = builder.Build();
                TrackAssetIndex index = new TrackAssetIndex(new[] { track }, car);
                TracksClientModule module = new TracksClientModule(liveOps, resolver.Resolve<CurrencyClientModule>(), index);
                await module.InitializeAsync(CancellationToken.None);
                RaceResultModel result = new RaceResultModel(30f, 1, track, 3000);
                await module.RecordResultAsync(result);
                Assert.That(module.GetTrackProgress().GetEarnedStars(trackId), Is.Zero, "An absent server response must not save stars.");
                liveOps.RaceResponse = new RecordRaceResultResponse { NewBestTimeSec = 30f };
                await module.RecordResultAsync(result);
                await module.RecordResultAsync(new RaceResultModel(40f, 1, track, 1000));
                TracksClientModule reloaded = new TracksClientModule(liveOps, resolver.Resolve<CurrencyClientModule>(), index);
                await reloaded.InitializeAsync(CancellationToken.None);
                Assert.That(reloaded.GetTrackProgress().GetEarnedStars(trackId), Is.EqualTo(3));
                Assert.That(reloaded.GetTrackProgress().GetBestTimeSeconds(trackId), Is.EqualTo(30f));
            }
            finally
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
                Object.DestroyImmediate(track);
                Object.DestroyImmediate(car);
            }
        }

        private sealed class RecordingAnalytics : Scaffold.Analytics.IAnalyticsService
        {
            public readonly List<Scaffold.Analytics.AnalyticsEvent> Events = new List<Scaffold.Analytics.AnalyticsEvent>();

            public void Record<T>(T evt) where T : Scaffold.Analytics.AnalyticsEvent
            {
                Events.Add(evt);
            }
        }

        private sealed class StubLiveOps : ILiveOpsService
        {
            private readonly IGameModuleData slice;
            public RecordRaceResultResponse RaceResponse { get; set; }

            public StubLiveOps(IGameModuleData slice)
            {
                this.slice = slice;
            }

            public T GetModuleData<T>() where T : class, IGameModuleData
            {
                return slice as T;
            }

            public Task<TResponse> CallAsync<TResponse>(ModuleRequest<TResponse> request, CancellationToken cancellationToken = default)
                where TResponse : ModuleResponse
            {
                return Task.FromResult(RaceResponse as TResponse);
            }
        }
    }
}
