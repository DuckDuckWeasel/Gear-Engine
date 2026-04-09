using NUnit.Framework;
using UnityEngine;
using Scaffold.Events;
namespace Game.GearEngine.Tests {
    public class GearChainTest {
        [Test]
        public void TestChain() {
            var eventBus = new EventController();
            var grid = new GridManager();
            var coreData = new GearConfigData { Id = "core", BaseRotationSpeed = 100f, TriggerPattern = TriggerPattern.FourWay };
            var core = new CoreGearNode(grid, eventBus); core.Initialize(new Vector2Int(0,0), coreData);
            
            var baseData1 = new GearConfigData { Id = "b1", TriggerSpinDegrees = 90f };
            var b1 = new BaseGearNode(grid, eventBus); b1.Initialize(new Vector2Int(1,0), baseData1);
            
            var baseData2 = new GearConfigData { Id = "b2", TriggerSpinDegrees = 90f };
            var b2 = new BaseGearNode(grid, eventBus); b2.Initialize(new Vector2Int(2,0), baseData2);

            grid.AddNode(core); grid.AddNode(b1); grid.AddNode(b2);
            
            // force core snap
            core.NodeUpdate(1f, 1f); 
            Debug.Log($"B1 rot: {b1.CurrentRotation}, B2 rot: {b2.CurrentRotation}");
            Assert.AreEqual(90f, b1.CurrentRotation);
            Assert.AreEqual(-90f, b2.CurrentRotation);
        }
    }
}
