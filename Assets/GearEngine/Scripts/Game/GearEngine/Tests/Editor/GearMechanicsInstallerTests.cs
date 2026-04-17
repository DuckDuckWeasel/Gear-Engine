using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
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
            var board = ScriptableObject.CreateInstance<BoardConfigSO>();
            board.GridWidth = 2;
            board.GridHeight = 2;

            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            var builder = new ContainerBuilder();
            new EventsInstaller().Install(builder);
            new GearMechanicsInstaller(board, null).Install(builder);
            using (IObjectResolver container = builder.Build())
            {
                Assert.DoesNotThrow(() => container.Resolve<IEventBus>());
                Assert.DoesNotThrow(() => container.Resolve<IGridManager>());
                Assert.DoesNotThrow(() => container.Resolve<IGearEngineService>());
                Assert.DoesNotThrow(() => container.Resolve<GearNodeFactory>());
                Assert.DoesNotThrow(() => container.Resolve<IDragService>());
                Assert.DoesNotThrow(() => container.Resolve<IGearTrashService>());
                Assert.DoesNotThrow(() => container.Resolve<GearEngineFeatureToggleSO>());
            }

            Object.DestroyImmediate(board);
        }

        [Test]
        public void Install_WithFeatureToggle_RegistersProvidedToggle()
        {
            var board = ScriptableObject.CreateInstance<BoardConfigSO>();
            var toggle = ScriptableObject.CreateInstance<GearEngineFeatureToggleSO>();

            var builder = new ContainerBuilder();
            new EventsInstaller().Install(builder);
            new GearMechanicsInstaller(board, toggle).Install(builder);
            using (IObjectResolver container = builder.Build())
            {
                var resolved = container.Resolve<GearEngineFeatureToggleSO>();
                Assert.AreSame(toggle, resolved, "Should resolve the explicitly provided toggle.");
            }

            Object.DestroyImmediate(board);
            Object.DestroyImmediate(toggle);
        }

        [Test]
        public void Install_DoesNotRegisterGearViewFactory()
        {
            var board = ScriptableObject.CreateInstance<BoardConfigSO>();
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            var builder = new ContainerBuilder();
            new EventsInstaller().Install(builder);
            new GearMechanicsInstaller(board, null).Install(builder);
            using (IObjectResolver container = builder.Build())
            {
                Assert.Throws<VContainerException>(() => container.Resolve<GearViewFactory>());
            }

            Object.DestroyImmediate(board);
        }
    }
}

