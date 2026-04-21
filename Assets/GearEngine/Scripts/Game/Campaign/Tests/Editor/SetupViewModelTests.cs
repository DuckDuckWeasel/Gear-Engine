using GearEngine.Campaign.Presentation;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class SetupViewModelTests
    {
        [Test]
        public void Initialize_CreatesBoardInventoryTrashAndTrack()
        {
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;

            var trackService = new FakeTrackService(trackDef, carDef);

            var boardConfig = ScriptableObject.CreateInstance<BoardRulesSO>();
            boardConfig.GridWidth = 5;
            boardConfig.GridHeight = 5;

            using (var gear = new GearMechanicsTestContext(boardConfig))
            {
                var vm = new SetupViewModel();
                ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
                ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                ViewModelTestInject.InjectPrivateField(vm, "eventBus", gear.EventBus);
                ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                ViewModelTestInject.InjectPrivateField(vm, "inventoryService", gear.InventoryService);
                ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                ViewModelTestInject.InjectPrivateField(vm, "loadoutService", new LocalGearLoadoutService());
                ViewModelTestInject.InjectNavigation(vm, new RecordingNavigation());

                ViewModelTestInject.InvokeInitialize(vm);

                Assert.That(vm.Board, Is.Not.Null);
                Assert.That(vm.Inventory, Is.Not.Null);
                Assert.That(vm.TrashZone, Is.Not.Null);
                Assert.That(vm.Track, Is.Not.Null);
            }

            Object.DestroyImmediate(boardConfig);
            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
        }

        [Test]
        public void GoToRace_OpensActiveRaceViewModel()
        {
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;

            var trackService = new FakeTrackService(trackDef, carDef);

            var boardConfig = ScriptableObject.CreateInstance<BoardRulesSO>();
            boardConfig.GridWidth = 5;
            boardConfig.GridHeight = 5;

            using (var gear = new GearMechanicsTestContext(boardConfig))
            {
                var navigation = new RecordingNavigation();
                var vm = new SetupViewModel();
                ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
                ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                ViewModelTestInject.InjectPrivateField(vm, "eventBus", gear.EventBus);
                ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                ViewModelTestInject.InjectPrivateField(vm, "inventoryService", gear.InventoryService);
                ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                ViewModelTestInject.InjectPrivateField(vm, "loadoutService", new LocalGearLoadoutService());
                ViewModelTestInject.InjectNavigation(vm, navigation);

                ViewModelTestInject.InvokeInitialize(vm);
                vm.GoToRace();

                Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(1));
                Assert.That(navigation.OpenedControllers[0], Is.InstanceOf<ActiveRaceViewModel>());
            }

            Object.DestroyImmediate(boardConfig);
            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
        }

        [Test]
        public void ReturnToMainMenu_CallsNavigationReturn()
        {
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;

            var trackService = new FakeTrackService(trackDef, carDef);

            var boardConfig = ScriptableObject.CreateInstance<BoardRulesSO>();
            boardConfig.GridWidth = 5;
            boardConfig.GridHeight = 5;

            using (var gear = new GearMechanicsTestContext(boardConfig))
            {
                var navigation = new RecordingNavigation();
                var vm = new SetupViewModel();
                ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
                ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                ViewModelTestInject.InjectPrivateField(vm, "eventBus", gear.EventBus);
                ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                ViewModelTestInject.InjectPrivateField(vm, "inventoryService", gear.InventoryService);
                ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                ViewModelTestInject.InjectPrivateField(vm, "loadoutService", new LocalGearLoadoutService());
                ViewModelTestInject.InjectNavigation(vm, navigation);

                ViewModelTestInject.InvokeInitialize(vm);
                vm.ReturnClicked();

                Assert.That(navigation.ReturnCallCount, Is.EqualTo(1));
                Assert.That(navigation.OpenedControllers.Count, Is.EqualTo(0));
            }

            Object.DestroyImmediate(boardConfig);
            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
        }

        [Test]
        public void Initialize_WithSavedLoadout_ReplacesExistingBoardPlacements()
        {
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;

            var trackService = new FakeTrackService(trackDef, carDef);

            var boardConfig = ScriptableObject.CreateInstance<BoardRulesSO>();
            boardConfig.GridWidth = 5;
            boardConfig.GridHeight = 5;

            GearConfig gearConfig = CampaignTestUtilities.CreateGearConfigWithData("loadout-test-gear");

            using (var gear = new GearMechanicsTestContext(boardConfig))
            {
                var inventory = (RecordingInventoryService)gear.InventoryService;
                OwnedGear owner = inventory.Add(gearConfig);

                var staleLayout = new BoardLayoutData(new[]
                {
                    new BoardGearPlacementData(new Vector2Int(0, 0), owner)
                });
                gear.BoardService.LoadLayout(staleLayout);
                Assert.That(gear.BoardService.GetNode(new Vector2Int(0, 0)), Is.Not.Null);

                var persistedLayout = new BoardLayoutData(new[]
                {
                    new BoardGearPlacementData(new Vector2Int(2, 2), owner)
                });
                var loadoutService = new StubGearLoadoutService(persistedLayout);

                var vm = new SetupViewModel();
                ViewModelTestInject.InjectPrivateField(vm, "trackService", trackService);
                ViewModelTestInject.InjectPrivateField(vm, "engineService", gear.Engine);
                ViewModelTestInject.InjectPrivateField(vm, "boardService", gear.BoardService);
                ViewModelTestInject.InjectPrivateField(vm, "eventBus", gear.EventBus);
                ViewModelTestInject.InjectPrivateField(vm, "featureToggle", gear.FeatureToggle);
                ViewModelTestInject.InjectPrivateField(vm, "dragService", gear.DragService);
                ViewModelTestInject.InjectPrivateField(vm, "inventoryService", gear.InventoryService);
                ViewModelTestInject.InjectPrivateField(vm, "presentationTransferService", gear.PresentationTransfer);
                ViewModelTestInject.InjectPrivateField(vm, "loadoutService", loadoutService);
                ViewModelTestInject.InjectNavigation(vm, new RecordingNavigation());

                ViewModelTestInject.InvokeInitialize(vm);

                Assert.That(gear.BoardService.GetNode(new Vector2Int(0, 0)), Is.Null);
                Assert.That(gear.BoardService.GetNode(new Vector2Int(2, 2)), Is.Not.Null);
                Assert.That(vm.Board, Is.Not.Null);
            }

            CampaignTestUtilities.DestroyGearConfig(gearConfig);
            Object.DestroyImmediate(boardConfig);
            Object.DestroyImmediate(carDef);
            Object.DestroyImmediate(trackDef);
        }
    }

    internal sealed class StubGearLoadoutService : IGearLoadoutService
    {
        private readonly BoardLayoutData saved;

        public StubGearLoadoutService(BoardLayoutData saved)
        {
            this.saved = saved;
        }

        public bool HasSavedLoadout => saved != null && saved.Placements.Count > 0;

        public BoardLayoutData GetBoardLayout() => saved;
    }
}
