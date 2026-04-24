using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using NUnit.Framework;
using UnityEngine;
using VContainer;

namespace GearEngine.GearEngine.Tests.Editor
{
    public class GearMechanicsInstallerTests
    {
        [Test]
        public void Install_RegistersBoardService_AndInventoryService()
        {
            var board = ScriptableObject.CreateInstance<BoardRulesSO>();
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IInventoryService>(new EmptyInventoryService());
            builder.RegisterInstance<IBoardSlotCapacityProvider>(new UnlimitedBoardSlotCapacityProvider());
            builder.RegisterInstance(board);
            builder.RegisterInstance(ScriptableObject.CreateInstance<GearEngineFeatureToggleSO>());
            new GearMechanicsInstaller().Install(builder);

            using (IObjectResolver container = builder.Build())
            {
                Assert.DoesNotThrow(() => container.Resolve<IInventoryService>());
                Assert.DoesNotThrow(() => container.Resolve<IBoardService>());
                Assert.DoesNotThrow(() => container.Resolve<IGearPresentationTransferService>());
            }

            Object.DestroyImmediate(board);
        }

        [Test]
        public void Install_WithFeatureToggle_ResolvesToggle()
        {
            var board = ScriptableObject.CreateInstance<BoardRulesSO>();
            var toggle = ScriptableObject.CreateInstance<GearEngineFeatureToggleSO>();
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IInventoryService>(new EmptyInventoryService());
            builder.RegisterInstance<IBoardSlotCapacityProvider>(new UnlimitedBoardSlotCapacityProvider());
            builder.RegisterInstance(board);
            builder.RegisterInstance(toggle);
            new GearMechanicsInstaller().Install(builder);

            using (IObjectResolver container = builder.Build())
            {
                Assert.DoesNotThrow(() => container.Resolve<IBoardService>());
                Assert.That(container.Resolve<GearEngineFeatureToggleSO>(), Is.SameAs(toggle));
            }

            Object.DestroyImmediate(board);
            Object.DestroyImmediate(toggle);
        }

        [Test]
        public void Install_RegistersFeatureToggleFromContainer()
        {
            var board = ScriptableObject.CreateInstance<BoardRulesSO>();
            var toggle = ScriptableObject.CreateInstance<GearEngineFeatureToggleSO>();
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IInventoryService>(new EmptyInventoryService());
            builder.RegisterInstance<IBoardSlotCapacityProvider>(new UnlimitedBoardSlotCapacityProvider());
            builder.RegisterInstance(board);
            builder.RegisterInstance(toggle);
            new GearMechanicsInstaller().Install(builder);

            using (IObjectResolver container = builder.Build())
            {
                GearEngineFeatureToggleSO resolved = container.Resolve<GearEngineFeatureToggleSO>();
                Assert.IsNotNull(resolved);
                Assert.That(resolved, Is.SameAs(toggle));
            }

            Object.DestroyImmediate(board);
            Object.DestroyImmediate(toggle);
        }
    }
}
