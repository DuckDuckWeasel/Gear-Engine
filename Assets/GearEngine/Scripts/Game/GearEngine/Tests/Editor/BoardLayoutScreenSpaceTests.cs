using GearEngine.GearEngine.Config;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    public sealed class BoardLayoutScreenSpaceTests
    {
        private BoardLayoutSO layout;
        private BoardRulesSO rules;

        [SetUp]
        public void SetUp()
        {
            layout = ScriptableObject.CreateInstance<BoardLayoutSO>();
            layout.CellSpacing = 100f;
            rules = ScriptableObject.CreateInstance<BoardRulesSO>();
            rules.GridWidth = 4;
            rules.GridHeight = 3;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(layout);
            Object.DestroyImmediate(rules);
        }

        [Test]
        public void GetCellLocalPosition_ReferencePixels_CentersGrid()
        {
            Vector3 first = layout.GetCellLocalPosition(Vector2Int.zero, rules);
            Vector3 last = layout.GetCellLocalPosition(new Vector2Int(3, 2), rules);

            Assert.That(first, Is.EqualTo(new Vector3(-150f, -100f, 0f)));
            Assert.That(last, Is.EqualTo(new Vector3(150f, 100f, 0f)));
        }

        [Test]
        public void TryGetGridPosition_OutsideGrid_ReturnsFalse()
        {
            bool accepted = layout.TryGetGridPosition(
                new Vector2(251f, 0f),
                rules,
                out _);

            Assert.IsFalse(accepted);
        }

        [Test]
        public void TryGetGridPosition_InsideCell_ReturnsCoordinate()
        {
            bool accepted = layout.TryGetGridPosition(
                new Vector2(145f, 95f),
                rules,
                out Vector2Int coordinate);

            Assert.IsTrue(accepted);
            Assert.That(coordinate, Is.EqualTo(new Vector2Int(3, 2)));
        }
    }
}
