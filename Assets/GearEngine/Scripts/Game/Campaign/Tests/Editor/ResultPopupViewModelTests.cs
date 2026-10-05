using System.Threading.Tasks;
using GearEngine.Campaign.Presentation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine.Config;
using NUnit.Framework;
using Scaffold.MVVM;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class ResultPopupViewModelTests
    {
        private static void InvokePersistence(RaceResultModel result, string name)
        {
            typeof(RaceResultModel).GetMethod(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(result, null);
        }

        [Test]
        public void Continue_OpensRewardsOnlyOnce()
        {
            RecordingNavigation navigation = new RecordingNavigation();
            ResultPopupViewModel vm = new ResultPopupViewModel(new RaceResultModel(80f, 3, null));
            ViewModelTestInject.InjectNavigation(vm, navigation);
            vm.Continue(); vm.Continue();
            Assert.That(navigation.OpenedControllers, Has.Count.EqualTo(1));
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<ReceivedRewardsViewModel>());
        }

        [Test]
        public async Task Continue_WaitsForPersistence()
        {
            RaceResultModel result = new RaceResultModel(80f, 3, null);
            InvokePersistence(result, "BeginPersistence");
            RecordingNavigation navigation = new RecordingNavigation();
            ResultPopupViewModel vm = new ResultPopupViewModel(result);
            ViewModelTestInject.InjectNavigation(vm, navigation);
            try
            {
                vm.Continue();
                Assert.That(navigation.OpenedControllers, Is.Empty);
                InvokePersistence(result, "CompletePersistence");
                for (int i = 0; i < 30 && navigation.OpenedControllers.Count == 0; i++)
                {
                    await Task.Delay(10);
                }

                Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<ReceivedRewardsViewModel>());
            }
            finally { InvokePersistence(result, "CompletePersistence"); }
        }

        [Test]
        public void ResultStars_FillHorizontallyFromFinalBankedScore()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithTiersForTests(
                new TrackTierConfig(1f, 600, 100),
                new TrackTierConfig(1f, 1800, 100),
                new TrackTierConfig(1f, 3000, 100));
            GameObject instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GearEngine/Prefabs/Campaign/Campaign_ResultPopupView.prefab"));
            try
            {
                ResultPopupView view = instance.GetComponent<ResultPopupView>();
                ResultPopupViewModel vm = new ResultPopupViewModel(new RaceResultModel(80f, 3, track, 900));
                vm.Bind(new RecordingNavigation());
                instance.GetComponent<ViewElement>().Bind(vm);

                SerializedProperty stars = new SerializedObject(view).FindProperty("stars");
                float[] expected = { 1f, .25f, 0f };
                for (int i = 0; i < expected.Length; i++)
                {
                    Image star = (Image)stars.GetArrayElementAtIndex(i).objectReferenceValue;
                    Image fill = star.transform.Find("ProgressFill").GetComponent<Image>();
                    Assert.That(fill.type, Is.EqualTo(Image.Type.Filled));
                    Assert.That(fill.fillMethod, Is.EqualTo(Image.FillMethod.Horizontal));
                    Assert.That(fill.fillOrigin, Is.EqualTo((int)Image.OriginHorizontal.Left));
                    Assert.That(fill.fillAmount, Is.EqualTo(expected[i]).Within(.001f));
                }
            }
            finally
            {
                Object.DestroyImmediate(instance);
                Object.DestroyImmediate(track);
            }
        }

        [TestCase(50f, 0, true)]
        [TestCase(80f, 1000, true)]
        [TestCase(50f, 1000, true)]
        [TestCase(80f, 0, false)]
        public void Rewards_OnlyOffersGearForFirstPlaceOrAStar(float time, int score, bool gear)
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithTiersForTests(new TrackTierConfig(1f, 1000, 100));
            try
            {
                RecordingNavigation navigation = new RecordingNavigation();
                ReceivedRewardsViewModel vm = new ReceivedRewardsViewModel(new RaceResultModel(time, 3, track, score));
                ViewModelTestInject.InjectNavigation(vm, navigation);
                Assert.That(vm.RewardCount, Is.EqualTo(gear ? 2 : 1));
                Assert.That(vm.ContinueLabel, Is.EqualTo("CONTINUE"));
                vm.Continue();
                if (gear)
                {
                    Assert.That(vm.NeedsGearSelection, Is.True);
                    Assert.That(vm.ContinueLabel, Is.EqualTo("UPGRADE"));
                    Assert.That(navigation.OpenedControllers, Is.Empty);
                    vm.Continue(); vm.Continue();
                    Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<RoguelikeViewModel>());
                }
                else
                {
                    Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<MainViewModel>());
                }

                Assert.That(navigation.OpenedControllers, Has.Count.EqualTo(1));
            }
            finally { Object.DestroyImmediate(track); }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GearReturn_DoesNotRepeatGoldOrOfferAnotherPick(bool picked)
        {
            RaceResultModel result = new RaceResultModel(50f, 3, null);
            GearItemData gear = picked ? new GearItemData { Id = "echo", DisplayName = "Echo Gear" } : null;
            RecordingNavigation navigation = new RecordingNavigation();
            RoguelikeViewModel vm = new RoguelikeViewModel(result);
            ViewModelTestInject.InjectNavigation(vm, navigation);
            typeof(RoguelikeViewModel).GetMethod("OpenPostRaceDestination",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(vm, new object[] { gear });
            if (picked)
            {
                ReceivedRewardsViewModel rewards = (ReceivedRewardsViewModel)navigation.OpenedControllers[0];
                Assert.That(rewards.RewardName, Is.EqualTo("Echo Gear"));
                Assert.That(rewards.RewardCountText, Is.EqualTo("REWARD 2/2"));
                Assert.That(rewards.NeedsGearSelection, Is.False);
                ViewModelTestInject.InjectNavigation(rewards, navigation);
                rewards.Continue();
                Assert.That(navigation.OpenedControllers[1], Is.InstanceOf<MainViewModel>());
            }
            else
            {
                Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<MainViewModel>());
            }
        }

        [Test]
        public void GearReward_UsesConfiguredCompositeIconScale()
        {
            GearItemData gear = new GearItemData
            {
                Id = "quantum_link",
                DisplayName = "Quantum Link",
                UIIconScaleMultiplier = 0.8f
            };
            ReceivedRewardsViewModel vm = new ReceivedRewardsViewModel(
                new RaceResultModel(50f, 3, null),
                gear);

            Assert.That(vm.RewardIconScale, Is.EqualTo(0.8f).Within(0.001f));
        }

        [Test]
        public void FinalReward_ReturnsHomeOnce()
        {
            RecordingNavigation navigation = new RecordingNavigation();
            ReceivedRewardsViewModel vm = new ReceivedRewardsViewModel(new RaceResultModel(80f, 3, null));
            ViewModelTestInject.InjectNavigation(vm, navigation);
            vm.Continue(); vm.Continue();
            Assert.That(navigation.OpenedControllers, Has.Count.EqualTo(1));
            Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<MainViewModel>());
        }
    }
}
