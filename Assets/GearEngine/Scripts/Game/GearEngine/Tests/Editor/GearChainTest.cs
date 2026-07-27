using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Manager;
using GearEngine.GearEngine.Nodes;
using Scaffold.Events;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class GearChainTest
    {
        [Test]
        public void CoreHit_RotatesEveryConnectedNeighborOfStruckCog()
        {
            EventController eventBus = new EventController();
            GridManager grid = new GridManager();
            GearItemData coreData = new GearItemData
            {
                Id = "core",
                BaseRotationSpeed = 100f,
                TriggerPattern = TriggerPattern.FourWay,
            };
            GearItemData connectedData = new GearItemData
            {
                TriggerPattern = TriggerPattern.FourWay,
                TriggerSpinDegrees = 90f,
            };

            CoreGearNode core = CreateCore(grid, eventBus, coreData);
            BaseGearNode struck = CreateConnectedGear(
                grid,
                eventBus,
                connectedData,
                new Vector2Int(1, 0));
            BaseGearNode right = CreateConnectedGear(
                grid,
                eventBus,
                connectedData,
                new Vector2Int(2, 0));
            BaseGearNode up = CreateConnectedGear(
                grid,
                eventBus,
                connectedData,
                new Vector2Int(1, 1));
            BaseGearNode down = CreateConnectedGear(
                grid,
                eventBus,
                connectedData,
                new Vector2Int(1, -1));

            core.NodeUpdate(1f, 1f);

            Assert.That(struck.CurrentRotation, Is.EqualTo(270f));
            Assert.That(right.CurrentRotation, Is.EqualTo(90f));
            Assert.That(up.CurrentRotation, Is.EqualTo(90f));
            Assert.That(down.CurrentRotation, Is.EqualTo(90f));

            down.Dispose();
            up.Dispose();
            right.Dispose();
            struck.Dispose();
            core.Dispose();
        }

        private static CoreGearNode CreateCore(
            GridManager grid,
            EventController eventBus,
            GearItemData config)
        {
            CoreGearNode node = new CoreGearNode(grid, eventBus);
            node.Initialize(Vector2Int.zero, config);
            grid.AddNode(node);
            return node;
        }

        private static BaseGearNode CreateConnectedGear(
            GridManager grid,
            EventController eventBus,
            GearItemData config,
            Vector2Int position)
        {
            BaseGearNode node = new BaseGearNode(grid, eventBus);
            node.Initialize(position, config);
            grid.AddNode(node);
            return node;
        }
    }
}
