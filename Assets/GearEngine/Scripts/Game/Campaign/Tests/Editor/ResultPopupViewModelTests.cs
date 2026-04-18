using GearEngine.Campaign;
using GearEngine.Campaign.Presentation;
using NUnit.Framework;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class ResultPopupViewModelTests
    {
        [Test]
        public void Continue_WhenGoodResult_AdvancesTrackAndOpensMain()
        {
            var good = new RaceResultModel(raceTime: 0f, lapCount: 1);
            Assert.That(good.IsGoodResult, Is.True);

            var trackService = new FakeTrackService(null, null, null);
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
        }

        [Test]
        public void Continue_WhenPoorResult_DoesNotAdvanceTrack()
        {
            var poor = new RaceResultModel(raceTime: 100f, lapCount: 1);
            Assert.That(poor.IsGoodResult, Is.False);

            var trackService = new FakeTrackService(null, null, null);
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
        }

        [Test]
        public void Upgrade_OpensRoguelikeViewModel()
        {
            var result = new RaceResultModel(raceTime: 0f, lapCount: 1);
            var trackService = new FakeTrackService(null, null, null);
            var wallet = new FakeWalletService();
            var navigation = new RecordingNavigation();

            var vm = new ResultPopupViewModel(result);
            ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
            ViewModelTestInject.InjectPrivateField(vm, "walletService", wallet);
            ViewModelTestInject.InjectNavigation(vm, navigation);

            vm.Upgrade();

            Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<RoguelikeViewModel>());
        }
    }
}
