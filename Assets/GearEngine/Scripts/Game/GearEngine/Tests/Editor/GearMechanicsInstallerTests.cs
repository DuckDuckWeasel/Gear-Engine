using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using GearEngine.GearEngine.Services.Inventory;
using NUnit.Framework;
using Scaffold.Events.Container;
using Scaffold.Events.Contracts;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public class GearMechanicsInstallerTests
    {
        [Test]
        public void Install_BuildsContainer_ResolvesCoreServices()
        {
            var board = ScriptableObject.CreateInstance<BoardRulesSO>();
            board.GridWidth = 2;
            board.GridHeight = 2;

            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            var builder = new ContainerBuilder();
            new EventsInstaller().Install(builder);
            new GearMechanicsInstaller(board, null, GearInventoryLoadoutData.Empty(), new GearBoardLoadoutData()).Install(builder);
            using (IObjectResolver container = builder.Build())
            {
                Assert.DoesNotThrow(() => container.Resolve<IEventBus>());
                Assert.DoesNotThrow(() => container.Resolve<IGridManager>());
                Assert.DoesNotThrow(() => container.Resolve<IGearEngineService>());
                Assert.DoesNotThrow(() => container.Resolve<GearNodeFactory>());
                Assert.DoesNotThrow(() => container.Resolve<IDragService>());
                Assert.DoesNotThrow(() => container.Resolve<IInventoryService>());
                Assert.DoesNotThrow(() => container.Resolve<IBoardService>());
                Assert.DoesNotThrow(() => container.Resolve<IGearPresentationTransferService>());
                Assert.DoesNotThrow(() => container.Resolve<GearEngineFeatureToggleSO>());
            }

            UnityEngine.Object.DestroyImmediate(board);
        }

        [Test]
        public void Install_WithFeatureToggle_RegistersProvidedToggle()
        {
            var board = ScriptableObject.CreateInstance<BoardRulesSO>();
            var toggle = ScriptableObject.CreateInstance<GearEngineFeatureToggleSO>();

            var builder = new ContainerBuilder();
            new EventsInstaller().Install(builder);
            new GearMechanicsInstaller(board, toggle, GearInventoryLoadoutData.Empty(), new GearBoardLoadoutData()).Install(builder);
            using (IObjectResolver container = builder.Build())
            {
                var resolved = container.Resolve<GearEngineFeatureToggleSO>();
                Assert.AreSame(toggle, resolved, "Should resolve the explicitly provided toggle.");
            }

            UnityEngine.Object.DestroyImmediate(board);
            UnityEngine.Object.DestroyImmediate(toggle);
        }

        [Test]
        public void Install_BoardService_ExposesRegisteredBoardRules()
        {
            var board = ScriptableObject.CreateInstance<BoardRulesSO>();
            board.GridWidth = 4;
            board.GridHeight = 3;
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            var builder = new ContainerBuilder();
            new EventsInstaller().Install(builder);
            new GearMechanicsInstaller(board, null, GearInventoryLoadoutData.Empty(), new GearBoardLoadoutData()).Install(builder);
            using (IObjectResolver container = builder.Build())
            {
                IBoardService boardService = container.Resolve<IBoardService>();
                Assert.AreSame(board, boardService.BoardRules);
                Assert.AreEqual(4, boardService.BoardRules.GridWidth);
            }

            UnityEngine.Object.DestroyImmediate(board);
        }
    }
}
