using System.Reflection;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Visuals;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class GearViewTests
    {
        [Test]
        public void PureVisual_UpdateDoesNotThrow_AndLerpsRotation()
        {
            var root = new GameObject("ViewRoot");
            var gearGo = new GameObject("GearVisual");
            gearGo.transform.SetParent(root.transform, false);
            var gearView = root.AddComponent<GearView>();
            gearView.WireTestReferences(gearGo.transform);

            var config = new GearItemData { RelativeScaleMultiplier = 1f };
            gearView.ApplyConfig(config);
            gearView.SetRotationTarget(90f);
            gearView.SetChargeFillTarget(0.5f, snap: true);

            MethodInfo update = typeof(GearView).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(update);
            for (int i = 0; i < 5; i++)
            {
                update.Invoke(gearView, null);
            }

            Object.DestroyImmediate(root);
        }
    }
}
