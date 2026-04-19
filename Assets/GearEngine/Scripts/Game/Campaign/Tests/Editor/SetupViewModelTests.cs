using GearEngine.Campaign.Presentation;
using GearEngine.Campaign.Services;
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

            var boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
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

            var boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
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

            var boardConfig = ScriptableObject.CreateInstance<BoardConfigSO>();
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
    }
}
