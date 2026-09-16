using System;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.ModuleRequests;
using GearEngine.Campaign;
using GearEngine.Campaign.Presentation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.Currency;
using GearEngine.GearEngine.Config;
using Newtonsoft.Json;
using NUnit.Framework;
using Scaffold.LiveOps;
using UnityEngine;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class ResultPopupViewModelTests
    {
        [Test]
        public void Victory_DoesNotInventRacePlacement()
        {
            ResultPopupViewModel vm = new ResultPopupViewModel(new RaceResultModel(30f, 3, null, 1200));
            Assert.That(vm.VictoryTitle, Is.EqualTo("RESULTS"));
        }

        [Test]
        public void Continue_OpensSeparateRewardScreen()
        {
            RecordingNavigation navigation = new RecordingNavigation();
            ResultPopupViewModel vm = new ResultPopupViewModel(new RaceResultModel(30f, 3, null, 1200));
            ViewModelTestInject.InjectNavigation(vm, navigation);
            vm.Continue();
            Assert.That(navigation.OpenedControllers, Has.Count.EqualTo(1));
            Assert.That(navigation.OpenedControllers[0].GetType().Name, Is.EqualTo("ReceivedRewardsViewModel"));
        }

        [Test]
        public void Continue_WhenGoodResult_AdvancesRewardAndProgressBeforeMain()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithTiersForTests(
                new TrackTierConfig(50f, 1000, 800),
                new TrackTierConfig(9999f, 0, 100));
            RaceResultModel good = new RaceResultModel(raceTime: 0f, lapCount: 1, track);
            Assert.That(good.IsGoodResult, Is.True);

            RecordingNavigation navigation = new RecordingNavigation();

            CurrencyClientModule currency = BuildCurrencyClient(100);
            currency.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

            ResultPopupViewModel vm = new ResultPopupViewModel(good);
            ViewModelTestInject.InjectPrivateField(vm, "currencyClient", currency);
            ViewModelTestInject.InjectNavigation(vm, navigation);

            vm.Continue();
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<ReceivedRewardsViewModel>());
            ReceivedRewardsViewModel rewards = (ReceivedRewardsViewModel)navigation.OpenedControllers[0];
            ViewModelTestInject.InjectNavigation(rewards, navigation);

            rewards.Continue();
            RaceProgressViewModel progress = navigation.OpenedControllers[1] as RaceProgressViewModel;
            Assert.That(progress, Is.Not.Null);
            ViewModelTestInject.InjectNavigation(progress, navigation);
            progress.Continue();
            progress.Continue();
            Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(3));
            Assert.That(navigation.OpenedControllers[2], Is.InstanceOf<MainViewModel>());

            UnityEngine.Object.DestroyImmediate(track);
        }

        [Test]
        public void Continue_WhenPoorResult_AdvancesRewardAndProgressBeforeMain()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithTiersForTests(
                new TrackTierConfig(50f, 1000, 800),
                new TrackTierConfig(90f, 500, 400),
                new TrackTierConfig(9999f, 0, 100));
            RaceResultModel poor = new RaceResultModel(raceTime: 100f, lapCount: 1, track);
            Assert.That(poor.IsGoodResult, Is.False);

            RecordingNavigation navigation = new RecordingNavigation();

            CurrencyClientModule currency = BuildCurrencyClient(0);
            currency.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

            ResultPopupViewModel vm = new ResultPopupViewModel(poor);
            ViewModelTestInject.InjectPrivateField(vm, "currencyClient", currency);
            ViewModelTestInject.InjectNavigation(vm, navigation);

            vm.Continue();
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<ReceivedRewardsViewModel>());
            ReceivedRewardsViewModel rewards = (ReceivedRewardsViewModel)navigation.OpenedControllers[0];
            ViewModelTestInject.InjectNavigation(rewards, navigation);
            rewards.Continue();
            RaceProgressViewModel progress = navigation.OpenedControllers[1] as RaceProgressViewModel;
            Assert.That(progress, Is.Not.Null);
            ViewModelTestInject.InjectNavigation(progress, navigation);
            progress.Continue();
            Assert.That(navigation.OpenedControllers[2], Is.InstanceOf<MainViewModel>());

            UnityEngine.Object.DestroyImmediate(track);
        }

        [Test]
        public void Upgrade_OpensRoguelikeViewModel()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithTiersForTests(
                new TrackTierConfig(50f, 1000, 800),
                new TrackTierConfig(9999f, 0, 100));
            RaceResultModel result = new RaceResultModel(raceTime: 0f, lapCount: 1, track);
            RecordingNavigation navigation = new RecordingNavigation();

            CurrencyClientModule currency = BuildCurrencyClient(0);
            currency.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

            ResultPopupViewModel vm = new ResultPopupViewModel(result);
            ViewModelTestInject.InjectPrivateField(vm, "currencyClient", currency);
            ViewModelTestInject.InjectNavigation(vm, navigation);

            vm.Upgrade();

            Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<RoguelikeViewModel>());

            UnityEngine.Object.DestroyImmediate(track);
        }

        [Test]
        public async Task Continue_WhenPersistenceIsPending_WaitsBeforeAdvancing()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithTiersForTests(
                new TrackTierConfig(50f, 1000, 800),
                new TrackTierConfig(9999f, 0, 100));
            RaceResultModel result = new RaceResultModel(raceTime: 0f, lapCount: 1, track);
            InvokePersistenceMethod(result, "BeginPersistence");
            ResultPopupViewModel vm = new ResultPopupViewModel(result);
            RecordingNavigation navigation = new RecordingNavigation();
            ViewModelTestInject.InjectNavigation(vm, navigation);

            try
            {
                vm.Continue();
                await Task.Yield();

                Assert.That(navigation.OpenedControllers, Is.Empty);

                InvokePersistenceMethod(result, "CompletePersistence");
                await Task.Yield();

                Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<ReceivedRewardsViewModel>());
            }
            finally
            {
                InvokePersistenceMethod(result, "CompletePersistence");
                UnityEngine.Object.DestroyImmediate(track);
            }
        }

        [Test]
        public void RoguelikePostRaceDestination_OpensRewardStage()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithTiersForTests(
                new TrackTierConfig(50f, 1000, 800),
                new TrackTierConfig(9999f, 0, 100));
            RaceResultModel result = new RaceResultModel(raceTime: 0f, lapCount: 1, track);
            GearItemData reward = new GearItemData { Id = "echo", DisplayName = "Echo Gear" };
            RecordingNavigation navigation = new RecordingNavigation();
            RoguelikeViewModel vm = new RoguelikeViewModel(result);
            ViewModelTestInject.InjectNavigation(vm, navigation);

            try
            {
                System.Reflection.MethodInfo method = typeof(RoguelikeViewModel).GetMethod(
                    "OpenPostRaceDestination",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                Assert.That(method, Is.Not.Null);
                method.Invoke(vm, new object[] { reward });

                Assert.That(navigation.OpenedControllers, Has.Count.EqualTo(1));
                ReceivedRewardsViewModel rewardViewModel = navigation.OpenedControllers[0] as ReceivedRewardsViewModel;
                Assert.That(rewardViewModel, Is.Not.Null);
                Assert.That(rewardViewModel.RewardCount, Is.EqualTo(2));
                Assert.That(rewardViewModel.RewardCountText, Is.EqualTo("REWARD 1/2"));
                ViewModelTestInject.InjectNavigation(rewardViewModel, navigation);
                rewardViewModel.Continue();
                Assert.That(rewardViewModel.RewardName, Is.EqualTo("Echo Gear"));
                Assert.That(rewardViewModel.RewardCountText, Is.EqualTo("REWARD 2/2"));
                Assert.That(rewardViewModel.ReceivedReward, Is.SameAs(reward));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(track);
            }
        }

        private static void InvokePersistenceMethod(RaceResultModel result, string methodName)
        {
            System.Reflection.MethodInfo method = typeof(RaceResultModel).GetMethod(
                methodName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, $"Race result persistence method {methodName} was not found.");
            method.Invoke(result, null);
        }

        private static CurrencyGameData BuildGameData(long gold)
        {
            CurrencyPersistence persistence = new CurrencyPersistence();
            persistence.Set("gold", gold);
            CurrencyConfig config = JsonConvert.DeserializeObject<CurrencyConfig>(
                "{\"entries\":[{\"id\":\"gold\",\"initial\":0}]}");
            return new CurrencyGameData(persistence, config);
        }

        private static CurrencyClientModule BuildCurrencyClient(long initialGold)
        {
            FakeLiveOpsService fake = new FakeLiveOpsService
            {
                ModuleData = BuildGameData(initialGold),
                CallImpl = (_, _) => new AddCurrencyResponse("gold", 0, 0),
            };
            return new CurrencyClientModule(fake);
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

}
