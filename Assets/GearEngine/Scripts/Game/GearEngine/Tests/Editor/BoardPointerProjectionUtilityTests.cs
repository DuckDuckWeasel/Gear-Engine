using GearEngine.GearEngine.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class BoardPointerProjectionUtilityTests
    {
        private GameObject cameraHost;
        private GameObject boardRootHost;

        [TearDown]
        public void TearDown()
        {
            if (cameraHost != null)
            {
                Object.DestroyImmediate(cameraHost);
                cameraHost = null;
            }

            if (boardRootHost != null)
            {
                Object.DestroyImmediate(boardRootHost);
                boardRootHost = null;
            }
        }

        [Test]
        public void TryProjectScreenPointToPlane_WithTopDownBoard_ReturnsBoardOriginAtScreenCenter()
        {
            Camera camera = CreateTopDownCamera();
            Transform boardRoot = CreateTopDownBoardRoot();

            bool projected = BoardPointerProjectionUtility.TryProjectScreenPointToPlane(
                camera,
                new Vector2(500f, 500f),
                boardRoot,
                out Vector3 worldPoint);

            Assert.IsTrue(projected);
            Assert.That(worldPoint.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(worldPoint.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(worldPoint.z, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void TryProjectScreenPointToPlane_WithTopDownBoard_PreservesBoardLocalAxes()
        {
            Camera camera = CreateTopDownCamera();
            Transform boardRoot = CreateTopDownBoardRoot();

            bool projected = BoardPointerProjectionUtility.TryProjectScreenPointToPlane(
                camera,
                new Vector2(600f, 600f),
                boardRoot,
                out Vector3 worldPoint);

            Assert.IsTrue(projected);

            Vector3 boardLocalPoint = boardRoot.InverseTransformPoint(worldPoint);
            Assert.That(boardLocalPoint.x, Is.EqualTo(1f).Within(0.001f));
            Assert.That(boardLocalPoint.y, Is.EqualTo(1f).Within(0.001f));
            Assert.That(boardLocalPoint.z, Is.EqualTo(0f).Within(0.001f));
        }

        private Camera CreateTopDownCamera()
        {
            cameraHost = new GameObject("TopDownCamera");
            var camera = cameraHost.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.pixelRect = new Rect(0f, 0f, 1000f, 1000f);
            camera.aspect = 1f;
            camera.transform.position = new Vector3(0f, 10f, 0f);
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            return camera;
        }

        private Transform CreateTopDownBoardRoot()
        {
            boardRootHost = new GameObject("BoardRoot");
            boardRootHost.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            return boardRootHost.transform;
        }
    }
}
