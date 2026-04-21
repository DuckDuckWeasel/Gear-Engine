using System;
using System.Collections.Generic;
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
        public void LoadRoll_PopulatesCardOptions()
        {
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            GearConfig g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            GearConfig g2 = CampaignTestUtilities.CreateGearConfigWithData("g2");
            try
            {
                var roll = new FakeRoguelikeRollService { ToReturn = new[] { g1, g2 } };
                var boardConfig = ScriptableObject.CreateInstance<BoardRulesSO>();
                boardConfig.GridWidth = 5;
                boardConfig.GridHeight = 5;

                using (var gear = new GearMechanicsTestContext(boardConfig))
                {
                    var vm = new RoguelikeViewModel();
                    ViewModelTestInject.InjectPrivateField(vm, "rollService", roll);
                    ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                    ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                    ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                    ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                    ViewModelTestInject.InjectPrivateField(vm, "inventoryService", gear.InventoryService);
                    ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                    ViewModelTestInject.InjectNavigation(vm, new RecordingNavigation());

                    ViewModelTestInject.InvokeInitialize(vm);

                    Assert.That(vm.CardOptions.Count, Is.EqualTo(2));
                    Assert.That(vm.CardOptionsRevision, Is.GreaterThan(0));
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
        public void SelectCard_CanConfirmFalseWhenInventoryFull_RecomputesWhenSpaceFreed()
        {
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            GearConfig g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            try
            {
                var roll = new FakeRoguelikeRollService { ToReturn = new[] { g1 } };
                var boardConfig = ScriptableObject.CreateInstance<BoardRulesSO>();
                boardConfig.GridWidth = 5;
                boardConfig.GridHeight = 5;

                var fillerConfigs = new List<GearConfig>();
                try
                {
                    using (var gear = new GearMechanicsTestContext(boardConfig))
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            GearConfig filler = CampaignTestUtilities.CreateGearConfigWithData($"fill{i}");
                            fillerConfigs.Add(filler);
                            Assert.That(gear.InventoryService.TryAdd(filler.CreateRuntimeData()), Is.True);
                        }

                        var vm = new RoguelikeViewModel();
                        ViewModelTestInject.InjectPrivateField(vm, "rollService", roll);
                        ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                        ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                        ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                        ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                        ViewModelTestInject.InjectPrivateField(vm, "inventoryService", gear.InventoryService);
                        ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                        ViewModelTestInject.InjectNavigation(vm, new RecordingNavigation());

                        ViewModelTestInject.InvokeInitialize(vm);

                        vm.SelectCard(vm.CardOptions[0]);
                        Assert.That(vm.CanConfirm, Is.False);

                        IItem toRemove = gear.InventoryService.GetInventory().Items[0];
                        Assert.That(gear.InventoryService.TryConsume(toRemove), Is.True);
                        Assert.That(vm.CanConfirm, Is.True);
                    }

                    Object.DestroyImmediate(boardConfig);
                }
                finally
                {
                    foreach (GearConfig c in fillerConfigs)
                    {
                        CampaignTestUtilities.DestroyGearConfig(c);
                    }
                }
            }
            finally
            {
                CampaignTestUtilities.DestroyGearConfig(g1);
            }
        }

        [Test]
        public void Confirm_AddsItem_ConsumesRoll_OpensMain()
        {
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            GearConfig g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            try
            {
                var roll = new FakeRoguelikeRollService { ToReturn = new[] { g1 } };
                var navigation = new RecordingNavigation();
                var boardConfig = ScriptableObject.CreateInstance<BoardRulesSO>();
                boardConfig.GridWidth = 5;
                boardConfig.GridHeight = 5;

                using (var gear = new GearMechanicsTestContext(boardConfig))
                {
                    var vm = new RoguelikeViewModel();
                    ViewModelTestInject.InjectPrivateField(vm, "rollService", roll);
                    ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                    ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                    ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                    ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                    ViewModelTestInject.InjectPrivateField(vm, "inventoryService", gear.InventoryService);
                    ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                    ViewModelTestInject.InjectNavigation(vm, navigation);

                    ViewModelTestInject.InvokeInitialize(vm);

                    vm.SelectCard(vm.CardOptions[0]);
                    Assert.That(vm.CanConfirm, Is.True);

                    vm.Confirm();

                    Assert.That(gear.InventoryService.GetInventory().Items.Count, Is.EqualTo(1));
                    Assert.That(roll.Consumed, Has.Count.EqualTo(1));
                    Assert.That(roll.Consumed[0], Is.SameAs(g1));
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

        private sealed class FakeRoguelikeRollService : IRoguelikeRollService
        {
            public IReadOnlyList<GearConfig> ToReturn = Array.Empty<GearConfig>();

            public List<GearConfig> Consumed { get; } = new List<GearConfig>();

            public Task<IReadOnlyList<GearConfig>> GetCurrentRollAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult(ToReturn);
            }

            public Task ConsumePickAsync(GearConfig picked, CancellationToken cancellationToken = default)
            {
                Consumed.Add(picked);
                return Task.CompletedTask;
            }
        }
    }
}
