using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using VContainer;

namespace Game.GearEngine.Tests
{
    [TestFixture]
    public class GearMechanicsInstallerTests
    {
        [Test]
        public void Install_BuildsContainer_ThatInjectsGearBootstrapDependencies()
        {
            var board = ScriptableObject.CreateInstance<BoardConfigSO>();
            board.GridWidth = 2;
            board.GridHeight = 2;

            var go = new GameObject("GearBootstrapInjectionTest");
            try
            {
                var bootstrap = go.AddComponent<GearBootstrap>();
                var loadout = ScriptableObject.CreateInstance<GearInventoryLoadoutSO>();

                var builder = new ContainerBuilder();
                new GearMechanicsInstaller(board, bootstrap, loadout).Install(builder);
                using (builder.Build())
                {
                    var boardConfigField = typeof(GearBootstrap).GetField(
                        "boardConfig",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    var gridField = typeof(GearBootstrap).GetField(
                        "grid",
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    Assert.IsNotNull(
                        boardConfigField?.GetValue(bootstrap),
                        "BoardConfigSO must be injected into GearBootstrap after container build.");
                    Assert.IsNotNull(
                        gridField?.GetValue(bootstrap),
                        "IGridManager must be injected into GearBootstrap after container build.");
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}
