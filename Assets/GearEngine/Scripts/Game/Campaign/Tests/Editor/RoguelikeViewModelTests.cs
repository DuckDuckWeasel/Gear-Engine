using System.Collections.Generic;
using GearEngine.Campaign.Presentation;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class RoguelikeViewModelTests
    {
        [Test]
        public void Initialize_BuildsCardOptionsFromTrackService()
        {
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            GearConfig g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            GearConfig g2 = CampaignTestUtilities.CreateGearConfigWithData("g2");
            try
            {
                var carDef = ScriptableObject.CreateInstance<CarDefinition>();
                var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
                trackDef.Spline.Closed = false;

                LapRaceSession session = CampaignTestUtilities.CreateMinimalSession(carDef, trackDef);
                var trackService = new FakeTrackService(trackDef, carDef, session, new[] { g1, g2 });

                var boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
                boardConfig.GridWidth = 5;
                boardConfig.GridHeight = 5;

                using (var gear = new GearMechanicsTestContext(boardConfig))
                {
                    var vm = new RoguelikeViewModel();
                    ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
                    ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                    ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                    ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                    ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                    ViewModelTestInject.InjectPrivateField(vm, "inventoryService", gear.InventoryService);
                    ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                    ViewModelTestInject.InjectNavigation(vm, new RecordingNavigation());

                    ViewModelTestInject.InvokeInitialize(vm);

                    Assert.That(vm.CardOptions.Count, Is.EqualTo(2));
                }

                Object.DestroyImmediate(boardConfig);
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
            finally
            {
                CampaignTestUtilities.DestroyGearConfig(g1);
                CampaignTestUtilities.DestroyGearConfig(g2);
            }
        }

        [Test]
        public void SelectCard_SetsCanConfirm()
        {
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            GearConfig g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            try
            {
                var carDef = ScriptableObject.CreateInstance<CarDefinition>();
                var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
                trackDef.Spline.Closed = false;

                LapRaceSession session = CampaignTestUtilities.CreateMinimalSession(carDef, trackDef);
                var trackService = new FakeTrackService(trackDef, carDef, session, new List<GearConfig> { g1 });

                var boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
                boardConfig.GridWidth = 5;
                boardConfig.GridHeight = 5;

                using (var gear = new GearMechanicsTestContext(boardConfig))
                {
                    var vm = new RoguelikeViewModel();
                    ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
                    ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                    ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                    ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                    ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                    ViewModelTestInject.InjectPrivateField(vm, "inventoryService", gear.InventoryService);
                    ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                    ViewModelTestInject.InjectNavigation(vm, new RecordingNavigation());

                    ViewModelTestInject.InvokeInitialize(vm);
                    Assert.That(vm.CanConfirm, Is.False);

                    vm.SelectCard(vm.CardOptions[0]);

                    Assert.That(vm.CanConfirm, Is.True);
                }

                Object.DestroyImmediate(boardConfig);
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
            finally
            {
                CampaignTestUtilities.DestroyGearConfig(g1);
            }
        }

        [Test]
        public void Confirm_AddsItemAndOpensMain()
        {
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            GearConfig g1 = CampaignTestUtilities.CreateGearConfigWithData("g1");
            try
            {
                var carDef = ScriptableObject.CreateInstance<CarDefinition>();
                var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
                trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
                trackDef.Spline.Closed = false;

                LapRaceSession session = CampaignTestUtilities.CreateMinimalSession(carDef, trackDef);
                var trackService = new FakeTrackService(trackDef, carDef, session, new List<GearConfig> { g1 });

                var boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
                boardConfig.GridWidth = 5;
                boardConfig.GridHeight = 5;

                var inventory = new RecordingInventoryService();
                var navigation = new RecordingNavigation();

                using (var gear = new GearMechanicsTestContext(boardConfig))
                {
                    var vm = new RoguelikeViewModel();
                    ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
                    ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                    ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                    ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                    ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                    ViewModelTestInject.InjectPrivateField(vm, "inventoryService", inventory);
                    ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                    ViewModelTestInject.InjectNavigation(vm, navigation);

                    ViewModelTestInject.InvokeInitialize(vm);
                    vm.SelectCard(vm.CardOptions[0]);
                    vm.Confirm();

                    Assert.That(inventory.AddedItems.Count, Is.EqualTo(1));
                    Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
                    Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<MainViewModel>());
                }

                Object.DestroyImmediate(boardConfig);
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
            finally
            {
                CampaignTestUtilities.DestroyGearConfig(g1);
            }
        }
    }
}
