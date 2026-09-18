using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign.Presentation;
using GearEngine.Campaign.Services;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services.Inventory;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class RoguelikeViewModelTests
    {
        [Test]
        public void LoadRoll_PopulatesPerkOptions()
        {
            GearItem g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            GearItem g2 = CampaignTestUtilities.CreateGearConfigWithData("g2");
            try
            {
                FakeRoguelikeRollService roll = new FakeRoguelikeRollService { ToReturn = new[] { g1.CreateRuntimeData(), g2.CreateRuntimeData() } };
                BoardRulesSO boardConfig = ScriptableObject.CreateInstance<BoardRulesSO>();
                boardConfig.GridWidth = 5;
                boardConfig.GridHeight = 5;

                using (GearMechanicsTestContext gear = new GearMechanicsTestContext(boardConfig))
                {
                    using Scaffold.Ads.RewardedAdManager ads = new Scaffold.Ads.RewardedAdManager();
                    using RoguelikeViewModel vm = new RoguelikeViewModel();
                    ViewModelTestInject.InjectPrivateField(vm, "adManager", ads);
                    ViewModelTestInject.InjectPrivateField(vm, "eventBus", gear.EventBus);
                    ViewModelTestInject.InjectPrivateField(vm, "rollService", roll);
                    ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                    ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                    ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                    ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                    ViewModelTestInject.InjectPrivateField(vm, "inventoryService", gear.InventoryService);
                    ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                    ViewModelTestInject.InjectNavigation(vm, new RecordingNavigation());

                    ViewModelTestInject.InvokeInitialize(vm);

                    Assert.That(vm.PerkOptions.Count, Is.EqualTo(2));
                    Assert.That(vm.PerkOptionsRevision, Is.GreaterThan(0));
                }

                Object.DestroyImmediate(boardConfig);
            }
            finally
            {
                CampaignTestUtilities.DestroyGearConfig(g1);
                CampaignTestUtilities.DestroyGearConfig(g2);
            }
        }

        [Test]
        public void PickPerk_AddsItem_ConsumesRoll_OpensMain()
        {
            GearItem g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            try
            {
                FakeRoguelikeRollService roll = new FakeRoguelikeRollService { ToReturn = new[] { g1.CreateRuntimeData() } };
                RecordingNavigation navigation = new RecordingNavigation();
                BoardRulesSO boardConfig = ScriptableObject.CreateInstance<BoardRulesSO>();
                boardConfig.GridWidth = 5;
                boardConfig.GridHeight = 5;

                using (GearMechanicsTestContext gear = new GearMechanicsTestContext(boardConfig))
                {
                    using Scaffold.Ads.RewardedAdManager ads = new Scaffold.Ads.RewardedAdManager();
                    using RoguelikeViewModel vm = new RoguelikeViewModel();
                    ViewModelTestInject.InjectPrivateField(vm, "adManager", ads);
                    ViewModelTestInject.InjectPrivateField(vm, "eventBus", gear.EventBus);
                    ViewModelTestInject.InjectPrivateField(vm, "rollService", roll);
                    ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                    ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                    ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                    ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                    ViewModelTestInject.InjectPrivateField(vm, "inventoryService", gear.InventoryService);
                    ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                    ViewModelTestInject.InjectNavigation(vm, navigation);

                    ViewModelTestInject.InvokeInitialize(vm);

                    System.Reflection.MethodInfo confirmMethod = typeof(RoguelikeViewModel).GetMethod("ConfirmPickAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Task<bool> task = (Task<bool>)confirmMethod.Invoke(vm, new object[] { vm.PerkOptions[0].Item.Id });
                    task.GetAwaiter().GetResult();

                    Assert.That(gear.InventoryService.Owned.Count, Is.EqualTo(1));
                    Assert.That(roll.Consumed, Has.Count.EqualTo(1));
                    Assert.That(roll.Consumed[0], Is.EqualTo(g1.Id));
                    Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
                    Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<MainViewModel>());
                }

                Object.DestroyImmediate(boardConfig);
            }
            finally
            {
                CampaignTestUtilities.DestroyGearConfig(g1);
            }
        }

        [Test]
        public void PickPerk_WhenRemoteClaimFails_StillOpensReceivedReward()
        {
            GearItem gearConfig = CampaignTestUtilities.CreateGearConfigWithData("offline_reward");
            RaceResultModel raceResult = new RaceResultModel(50f, 3, null);
            try
            {
                FakeRoguelikeRollService roll = new FakeRoguelikeRollService
                {
                    ToReturn = new[] { gearConfig.CreateRuntimeData() },
                    ConsumeException = new InvalidOperationException("Remote claim unavailable"),
                };
                RecordingNavigation navigation = new RecordingNavigation();
                BoardRulesSO boardConfig = ScriptableObject.CreateInstance<BoardRulesSO>();
                boardConfig.GridWidth = 5;
                boardConfig.GridHeight = 5;

                using (GearMechanicsTestContext gear = new GearMechanicsTestContext(boardConfig))
                {
                    using Scaffold.Ads.RewardedAdManager ads = new Scaffold.Ads.RewardedAdManager();
                    using RoguelikeViewModel vm = new RoguelikeViewModel(raceResult);
                    ViewModelTestInject.InjectPrivateField(vm, "adManager", ads);
                    ViewModelTestInject.InjectPrivateField(vm, "eventBus", gear.EventBus);
                    ViewModelTestInject.InjectPrivateField(vm, "rollService", roll);
                    ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                    ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                    ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                    ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                    ViewModelTestInject.InjectPrivateField(vm, "inventoryService", gear.InventoryService);
                    ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                    ViewModelTestInject.InjectNavigation(vm, navigation);

                    ViewModelTestInject.InvokeInitialize(vm);

                    System.Reflection.MethodInfo confirmMethod = typeof(RoguelikeViewModel).GetMethod(
                        "ConfirmPickAsync",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    LogAssert.Expect(LogType.Error, new Regex("Remote pick consumption failed after granting offline_reward"));
                    Task<bool> task = (Task<bool>)confirmMethod.Invoke(vm, new object[] { vm.PerkOptions[0].Item.Id });

                    Assert.That(task.GetAwaiter().GetResult(), Is.True);
                    Assert.That(gear.InventoryService.Owned.Count, Is.EqualTo(1));
                    Assert.That(navigation.OpenedControllers, Has.Count.EqualTo(1));
                    Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<ReceivedRewardsViewModel>());
                }

                Object.DestroyImmediate(boardConfig);
            }
            finally
            {
                CampaignTestUtilities.DestroyGearConfig(gearConfig);
            }
        }

        private sealed class FakeRoguelikeRollService : IRoguelikeRollService
        {
            public IReadOnlyList<IItem> ToReturn = Array.Empty<IItem>();

            public List<string> Consumed { get; } = new List<string>();

            public Exception ConsumeException { get; set; }

            public Task<IReadOnlyList<IItem>> GetCurrentRollAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(ToReturn);
            }

            public Task ConsumePickAsync(string pickedId, CancellationToken cancellationToken = default)
            {
                if (ConsumeException != null)
                {
                    return Task.FromException(ConsumeException);
                }

                Consumed.Add(pickedId);
                return Task.CompletedTask;
            }

            public Task SkipPickAsync(CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task<IReadOnlyList<IItem>> RerollAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(ToReturn);
            }
        }
    }
}
