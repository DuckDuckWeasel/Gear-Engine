using GearEngine.Campaign;
using GearEngine.Campaign.Presentation;
using GearEngine.CarSimulation.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class ResultPopupViewModelTests
    {
        [Test]
        public void Continue_WhenGoodResult_AdvancesTrackAndOpensMain()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithScoreBandsForTests(
                new TrackScoreBand(50f, 800),
                new TrackScoreBand(9999f, 100));
            var good = new RaceResultModel(raceTime: 0f, lapCount: 1, track);
            Assert.That(good.IsGoodResult, Is.True);

            var trackService = new FakeTrackService(null, null);
            var wallet = new FakeWalletService();
            var navigation = new RecordingNavigation();

            var vm = new ResultPopupViewModel(good);
            ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
            ViewModelTestInject.InjectPrivateField(vm, "walletService", wallet);
            ViewModelTestInject.InjectNavigation(vm, navigation);

            vm.Continue();

            Assert.That(trackService.AdvanceCallCount, Is.EqualTo(1));
            Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<MainViewModel>());

            Object.DestroyImmediate(track);
        }

        [Test]
        public void Continue_WhenPoorResult_DoesNotAdvanceTrack()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithScoreBandsForTests(
                new TrackScoreBand(50f, 800),
                new TrackScoreBand(90f, 400),
                new TrackScoreBand(9999f, 100));
            var poor = new RaceResultModel(raceTime: 100f, lapCount: 1, track);
            Assert.That(poor.IsGoodResult, Is.False);

            var trackService = new FakeTrackService(null, null);
            var wallet = new FakeWalletService();
            var navigation = new RecordingNavigation();

            var vm = new ResultPopupViewModel(poor);
            ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
            ViewModelTestInject.InjectPrivateField(vm, "walletService", wallet);
            ViewModelTestInject.InjectNavigation(vm, navigation);

            vm.Continue();

            Assert.That(trackService.AdvanceCallCount, Is.EqualTo(0));
            Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<MainViewModel>());

            Object.DestroyImmediate(track);
        }

        [Test]
        public void Upgrade_OpensRoguelikeViewModel()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithScoreBandsForTests(
                new TrackScoreBand(50f, 800),
                new TrackScoreBand(9999f, 100));
            var result = new RaceResultModel(raceTime: 0f, lapCount: 1, track);
            var trackService = new FakeTrackService(null, null);
            var wallet = new FakeWalletService();
            var navigation = new RecordingNavigation();

            var vm = new ResultPopupViewModel(result);
            ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
            ViewModelTestInject.InjectPrivateField(vm, "walletService", wallet);
            ViewModelTestInject.InjectNavigation(vm, navigation);

            vm.Upgrade();

            Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<RoguelikeViewModel>());

            Object.DestroyImmediate(track);
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
                Object.DestroyImmediate(track);
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
                Object.DestroyImmediate(track);
            }
        }
    }
}
