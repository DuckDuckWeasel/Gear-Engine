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
        public void BindForDisplay_DoesNotRequireBoardLayout_UpdateDoesNotThrow()
        {
            var root = new GameObject("ViewRoot");
            var gearGo = new GameObject("GearVisual");
            gearGo.transform.SetParent(root.transform, false);
            var gearView = root.AddComponent<GearView>();
            gearView.WireTestReferences(gearGo.transform);

            var config = new GearConfigData { RelativeScaleMultiplier = 1f };
            gearView.BindForDisplay(config, DisplayOptions.Ghost(0.6f));

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
