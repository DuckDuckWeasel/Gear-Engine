using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.GameModule;
using GameModuleDTO.ModuleRequests;
using GameModuleDTO.Modules.Tracks;
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
            var persistence = new TrackPersistence { CurrentTrackId = "a" };

            var data = new TrackGameData(persistence, config);

            Assert.That(data.OrderedTrackIds.Count, Is.EqualTo(2));
            Assert.That(data.OrderedTrackIds[0], Is.EqualTo("a"));
            Assert.That(data.CurrentTrackId, Is.EqualTo("a"));
        }

        [Test]
        public async Task TracksClientModule_InitializeAsync_WhenCurrentTrackIdEmpty_RepairsToFirstOrderedTrackInCatalog()
        {
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.name = "track_alpha";
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();

            var catalog = ScriptableObject.CreateInstance<TrackCatalogSO>();
            catalog.SetRuntimeEntries(new[] { CreateTrackEntry(trackDef, carDef) });

            var persistence = new TrackPersistence { CurrentTrackId = string.Empty };
            TrackConfig config = JsonConvert.DeserializeObject<TrackConfig>(
                "{\"entries\":[{\"id\":\"track_alpha\",\"baseReward\":0,\"bands\":[]}]}");
            var gameData = new TrackGameData(persistence, config);

            var liveOps = new StubLiveOps(gameData);
            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILiveOpsService>(liveOps);
            builder.Register<CurrencyClientModule>(Lifetime.Singleton);
            using IObjectResolver resolver = builder.Build();

            CurrencyClientModule currency = resolver.Resolve<CurrencyClientModule>();
            var module = new TracksClientModule(resolver, liveOps, currency, catalog);
            await module.InitializeAsync(CancellationToken.None);

            Assert.That(module.CurrentTrack, Is.SameAs(trackDef));
            Assert.That(gameData.CurrentTrackId, Is.EqualTo("track_alpha"));

            Object.DestroyImmediate(trackDef);
            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public async Task TracksClientModule_InitializeAsync_WhenRemoteTrackListEmpty_RepairsToFirstCatalogTrack()
        {
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.name = "local_only";
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();

            var catalog = ScriptableObject.CreateInstance<TrackCatalogSO>();
            catalog.SetRuntimeEntries(new[] { CreateTrackEntry(trackDef, carDef) });

            var persistence = new TrackPersistence { CurrentTrackId = string.Empty };
            TrackConfig config = JsonConvert.DeserializeObject<TrackConfig>(
                "{\"entries\":[]}");
            var gameData = new TrackGameData(persistence, config);

            var liveOps = new StubLiveOps(gameData);
            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILiveOpsService>(liveOps);
            builder.Register<CurrencyClientModule>(Lifetime.Singleton);
            using IObjectResolver resolver = builder.Build();

            CurrencyClientModule currency = resolver.Resolve<CurrencyClientModule>();
            var module = new TracksClientModule(resolver, liveOps, currency, catalog);
            await module.InitializeAsync(CancellationToken.None);

            Assert.That(module.CurrentTrack, Is.SameAs(trackDef));
            Assert.That(gameData.CurrentTrackId, Is.EqualTo("local_only"));

            Object.DestroyImmediate(trackDef);
            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(catalog);
        }

        [Test]
        public void TrackCatalogSO_GetFirstResolvableTrackId_ReturnsFirstValidEntryId()
        {
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.name = "only_track";
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var catalog = ScriptableObject.CreateInstance<TrackCatalogSO>();
            catalog.SetRuntimeEntries(new[] { CreateTrackEntry(trackDef, carDef) });

            Assert.That(catalog.GetFirstResolvableTrackId(), Is.EqualTo("only_track"));

            Object.DestroyImmediate(trackDef);
            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(catalog);
        }

        private static TrackEntry CreateTrackEntry(TrackDefinition track, CarDefinition car)
        {
            var entry = new TrackEntry();
            FieldInfo trackField = typeof(TrackEntry).GetField("track", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo carField = typeof(TrackEntry).GetField("car", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(trackField, Is.Not.Null);
            Assert.That(carField, Is.Not.Null);
            trackField.SetValue(entry, track);
            carField.SetValue(entry, car);
            return entry;
        }

        private sealed class StubLiveOps : ILiveOpsService
        {
            private readonly IGameModuleData slice;

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
                return Task.FromResult<TResponse>(null);
            }
        }
    }
}
