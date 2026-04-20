using System;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.GameModule;
using GameModuleDTO.Modules.Currency;
using GameModuleDTO.ModuleRequests;
using GearEngine.Campaign;
using GearEngine.Campaign.Presentation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.Currency;
using Newtonsoft.Json;
using NUnit.Framework;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class ResultPopupViewModelTests
    {
        [Test]
        public void Continue_WhenGoodResult_OpensMain()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithScoreBandsForTests(
                new TrackScoreBand(50f, 800),
                new TrackScoreBand(9999f, 100));
            var good = new RaceResultModel(raceTime: 0f, lapCount: 1, track);
            Assert.That(good.IsGoodResult, Is.True);

            var navigation = new RecordingNavigation();

            using (IObjectResolver container = BuildCurrencyContainer(100))
            {
                CurrencyClientModule currency = container.Resolve<CurrencyClientModule>();
                currency.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                var vm = new ResultPopupViewModel(good);
                ViewModelTestInject.InjectPrivateField(vm, "currencyClient", currency);
                ViewModelTestInject.InjectNavigation(vm, navigation);

                vm.Continue();

                Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
                Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<MainViewModel>());
            }

            UnityEngine.Object.DestroyImmediate(track);
        }

        [Test]
        public void Continue_WhenPoorResult_OpensMain()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithScoreBandsForTests(
                new TrackScoreBand(50f, 800),
                new TrackScoreBand(90f, 400),
                new TrackScoreBand(9999f, 100));
            var poor = new RaceResultModel(raceTime: 100f, lapCount: 1, track);
            Assert.That(poor.IsGoodResult, Is.False);

            var navigation = new RecordingNavigation();

            using (IObjectResolver container = BuildCurrencyContainer(0))
            {
                CurrencyClientModule currency = container.Resolve<CurrencyClientModule>();
                currency.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                var vm = new ResultPopupViewModel(poor);
                ViewModelTestInject.InjectPrivateField(vm, "currencyClient", currency);
                ViewModelTestInject.InjectNavigation(vm, navigation);

                vm.Continue();

                Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
                Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<MainViewModel>());
            }

            UnityEngine.Object.DestroyImmediate(track);
        }

        [Test]
        public void Upgrade_OpensRoguelikeViewModel()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithScoreBandsForTests(
                new TrackScoreBand(50f, 800),
                new TrackScoreBand(9999f, 100));
            var result = new RaceResultModel(raceTime: 0f, lapCount: 1, track);
            var navigation = new RecordingNavigation();

            using (IObjectResolver container = BuildCurrencyContainer(0))
            {
                CurrencyClientModule currency = container.Resolve<CurrencyClientModule>();
                currency.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

                var vm = new ResultPopupViewModel(result);
                ViewModelTestInject.InjectPrivateField(vm, "currencyClient", currency);
                ViewModelTestInject.InjectNavigation(vm, navigation);

                vm.Upgrade();

                Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
                Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<RoguelikeViewModel>());
            }

            UnityEngine.Object.DestroyImmediate(track);
        }

        private static CurrencyGameData BuildGameData(long gold)
        {
            var persistence = new CurrencyPersistence();
            persistence.Set("gold", gold);
            CurrencyConfig config = JsonConvert.DeserializeObject<CurrencyConfig>(
                "{\"entries\":[{\"id\":\"gold\",\"initial\":0}]}");
            return new CurrencyGameData(persistence, config);
        }

        private static IObjectResolver BuildCurrencyContainer(long initialGold)
        {
            var fake = new FakeLiveOpsService
            {
                ModuleData = BuildGameData(initialGold),
                CallImpl = (_, _) => new AddCurrencyResponse("gold", 0, 0),
            };

            var builder = new ContainerBuilder();
            builder.RegisterInstance<ILiveOpsService>(fake);
            builder.Register<CurrencyClientModule>(Lifetime.Singleton);
            return builder.Build();
        }

        private sealed class FakeLiveOpsService : ILiveOpsService
        {
            public CurrencyGameData ModuleData { get; set; }

            public Func<object, CancellationToken, ModuleResponse> CallImpl { get; set; }

            public T GetModuleData<T>()
                where T : class, IGameModuleData
            {
                return ModuleData as T;
            }

            public Task<TResponse> CallAsync<TResponse>(ModuleRequest<TResponse> request, CancellationToken cancellationToken = default)
                where TResponse : ModuleResponse
            {
                ModuleResponse result = CallImpl((object)request, cancellationToken);
                return Task.FromResult((TResponse)result);
            }
        }
    }

    public sealed class RaceResultModelTests
    {
        [Test]
        public void WhenTrackHasScoreBands_ScoreAndGoldMatchBandReward()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithScoreBandsForTests(
                new TrackScoreBand(30f, 900),
                new TrackScoreBand(9999f, 100));

            try
            {
                var result = new RaceResultModel(raceTime: 20f, lapCount: 3, track);

                Assert.That(result.Score, Is.EqualTo(900));
                Assert.That(result.Gold.Amount, Is.EqualTo(900));
                Assert.That(result.IsGoodResult, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(track);
            }
        }

        [Test]
        public void WhenTrackHasNoBands_UsesLegacyScoreAndScaledGold()
        {
            TrackDefinition track = ScriptableObject.CreateInstance<TrackDefinition>();

            try
            {
                var result = new RaceResultModel(raceTime: 10f, lapCount: 1, track);

                Assert.That(result.Score, Is.EqualTo(900));
                Assert.That(result.Gold.Amount, Is.EqualTo(4500));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(track);
            }
        }
    }
}
