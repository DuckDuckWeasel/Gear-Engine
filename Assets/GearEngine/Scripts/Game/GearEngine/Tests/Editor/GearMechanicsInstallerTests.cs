using GearEngine.GearEngine;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace GearEngine.GearEngine.Tests.Editor
{
    public class GearMechanicsInstallerTests
    {
        [Test]
        public void Install_RegistersBoardService_AndInventoryService()
        {
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            var board = ScriptableObject.CreateInstance<BoardRulesSO>();
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IInventoryService>(new EmptyInventoryService());
            new GearMechanicsInstaller(board, null, new GearBoardLoadoutData()).Install(builder);

            using (IObjectResolver container = builder.Build())
            {
                Assert.DoesNotThrow(() => container.Resolve<IInventoryService>());
                Assert.DoesNotThrow(() => container.Resolve<IBoardService>());
                Assert.DoesNotThrow(() => container.Resolve<IGearPresentationTransferService>());
            }

            Object.DestroyImmediate(board);
        }

        [Test]
        public void Install_WithFeatureToggle_DoesNotWarn()
        {
            var board = ScriptableObject.CreateInstance<BoardRulesSO>();
            var toggle = ScriptableObject.CreateInstance<GearEngineFeatureToggleSO>();
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IInventoryService>(new EmptyInventoryService());
            new GearMechanicsInstaller(board, toggle, new GearBoardLoadoutData()).Install(builder);

            using (IObjectResolver container = builder.Build())
            {
                Assert.DoesNotThrow(() => container.Resolve<IBoardService>());
            }

            Object.DestroyImmediate(board);
            Object.DestroyImmediate(toggle);
        }

        [Test]
        public void Install_AllowsOptionalFeatureToggle()
        {
            LogAssert.Expect(LogType.Warning, "[GearMechanicsInstaller] No GearEngineFeatureToggleSO provided. Using runtime default.");

            var board = ScriptableObject.CreateInstance<BoardRulesSO>();
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IInventoryService>(new EmptyInventoryService());
            new GearMechanicsInstaller(board, null, new GearBoardLoadoutData()).Install(builder);

            using (IObjectResolver container = builder.Build())
            {
                GearEngineFeatureToggleSO resolved = container.Resolve<GearEngineFeatureToggleSO>();
                Assert.IsNotNull(resolved);
            }

            Object.DestroyImmediate(board);
        }
    }
}
