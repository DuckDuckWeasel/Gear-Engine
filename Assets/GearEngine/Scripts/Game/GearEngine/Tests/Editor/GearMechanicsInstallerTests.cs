using NUnit.Framework;
using Scaffold.Events.Container;
using Scaffold.Events.Contracts;
using UnityEngine;
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

            var builder = new ContainerBuilder();
            new EventsInstaller().Install(builder);
            new GearMechanicsInstaller(board).Install(builder);
            using (IObjectResolver container = builder.Build())
            {
                Assert.DoesNotThrow(() => container.Resolve<IEventBus>());
                Assert.DoesNotThrow(() => container.Resolve<IGridManager>());
                Assert.DoesNotThrow(() => container.Resolve<IGearEngineService>());
                Assert.DoesNotThrow(() => container.Resolve<GearNodeFactory>());
                Assert.DoesNotThrow(() => container.Resolve<IDragService>());
            }
        }

        [Test]
        public void Install_DoesNotRegisterGearViewFactory()
        {
            var board = ScriptableObject.CreateInstance<BoardConfigSO>();
            var builder = new ContainerBuilder();
            new EventsInstaller().Install(builder);
            new GearMechanicsInstaller(board).Install(builder);
            using (IObjectResolver container = builder.Build())
            {
                Assert.Throws<VContainerException>(() => container.Resolve<GearViewFactory>());
            }
        }
    }
}
