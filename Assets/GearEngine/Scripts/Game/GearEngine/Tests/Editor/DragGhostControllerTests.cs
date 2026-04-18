using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Presentation.UI;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class DragGhostControllerTests
    {
        private GameObject _boardRootGo;

        [TearDown]
        public void TearDown()
        {
            if (_boardRootGo != null)
            {
                Object.DestroyImmediate(_boardRootGo);
                _boardRootGo = null;
            }
        }

        [Test]
        public void CreateGhost_InstantiatesUnderBoardRoot()
        {
            _boardRootGo = new GameObject("BoardRoot");
            Transform boardRoot = _boardRootGo.transform;
            var controller = new DragGhostController(boardRoot);
            GameObject cubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var config = new GearConfigData { VisualPrefab = cubePrefab, RelativeScaleMultiplier = 1f };

            controller.CreateGhost(config);

            Assert.IsNotNull(controller.Ghost);
            Assert.AreSame(boardRoot, controller.Ghost.transform.parent);
            Object.DestroyImmediate(cubePrefab);
        }

        [Test]
        public void CreateGhost_AppliesRelativeScaleMultiplier()
        {
            _boardRootGo = new GameObject("BoardRoot");
            var controller = new DragGhostController(_boardRootGo.transform);
            GameObject cubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            const float scale = 2.25f;
            var config = new GearConfigData { VisualPrefab = cubePrefab, RelativeScaleMultiplier = scale };

            controller.CreateGhost(config);

            Assert.IsNotNull(controller.Ghost);
            Assert.AreEqual(scale, controller.Ghost.transform.localScale.x, 1e-5f);
            Assert.AreEqual(scale, controller.Ghost.transform.localScale.y, 1e-5f);
            Assert.AreEqual(scale, controller.Ghost.transform.localScale.z, 1e-5f);
            Object.DestroyImmediate(cubePrefab);
        }

        [Test]
        public void MoveGhostTo_SetsWorldPosition()
        {
            _boardRootGo = new GameObject("BoardRoot");
            _boardRootGo.transform.position = new Vector3(10f, 20f, 0f);
            var controller = new DragGhostController(_boardRootGo.transform);
            GameObject cubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var config = new GearConfigData { VisualPrefab = cubePrefab, RelativeScaleMultiplier = 1f };
            controller.CreateGhost(config);

            var target = new Vector3(5f, -3f, 7f);
            controller.MoveGhostTo(target);

            Assert.AreEqual(target.x, controller.Ghost.transform.position.x, 1e-5f);
            Assert.AreEqual(target.y, controller.Ghost.transform.position.y, 1e-5f);
            Assert.AreEqual(target.z, controller.Ghost.transform.position.z, 1e-5f);
            Object.DestroyImmediate(cubePrefab);
        }

        [Test]
        public void DestroyGhost_NullifiesGhostReference()
        {
            _boardRootGo = new GameObject("BoardRoot");
            var controller = new DragGhostController(_boardRootGo.transform);
            GameObject cubePrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var config = new GearConfigData { VisualPrefab = cubePrefab, RelativeScaleMultiplier = 1f };
            controller.CreateGhost(config);
            Assert.IsNotNull(controller.Ghost);

            controller.DestroyGhost();

            Assert.IsNull(controller.Ghost);
            Object.DestroyImmediate(cubePrefab);
        }
    }
}
