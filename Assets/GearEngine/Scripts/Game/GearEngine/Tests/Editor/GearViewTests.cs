using System.Reflection;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class GearViewTests
    {
        [Test]
        public void PureVisual_UpdateDoesNotThrow_AndLerpsRotation()
        {
            GameObject root = new GameObject("ViewRoot", typeof(RectTransform));
            GameObject gearGo = new GameObject("GearVisual", typeof(RectTransform), typeof(Image));
            gearGo.transform.SetParent(root.transform, false);
            GearView gearView = root.AddComponent<GearView>();
            gearView.WireTestReferences(
                gearGo.transform,
                gearGo.GetComponent<Image>());

            GearItemData config = new GearItemData { RelativeScaleMultiplier = 1f };
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

        [Test]
        public void ChargeFill_TwoViews_UseIndependentMaterialInstances()
        {
            Material source = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/GearEngine/Data/Gear/ChargeFillMaterial.mat");
            Assert.IsNotNull(source);

            GearView first = CreateViewWithChargeImage(source, out Image firstImage);
            GearView second = CreateViewWithChargeImage(source, out Image secondImage);

            first.SetChargeFillTarget(0.25f, snap: true);
            second.SetChargeFillTarget(0.75f, snap: true);

            Assert.AreNotSame(firstImage.material, secondImage.material);
            Assert.That(firstImage.material.GetFloat("_FillAmount"), Is.EqualTo(0.25f).Within(0.001f));
            Assert.That(secondImage.material.GetFloat("_FillAmount"), Is.EqualTo(0.75f).Within(0.001f));

            Object.DestroyImmediate(first.gameObject);
            Object.DestroyImmediate(second.gameObject);
        }

        private static GearView CreateViewWithChargeImage(Material source, out Image chargeImage)
        {
            GameObject root = new GameObject("ViewRoot", typeof(RectTransform));
            GameObject visual = new GameObject("GearVisual", typeof(RectTransform), typeof(Image));
            visual.transform.SetParent(root.transform, false);
            GameObject charge = new GameObject("ChargeVisual", typeof(RectTransform), typeof(Image));
            charge.transform.SetParent(root.transform, false);
            chargeImage = charge.GetComponent<Image>();
            chargeImage.material = source;

            GearView view = root.AddComponent<GearView>();
            view.WireTestReferences(
                visual.transform,
                visual.GetComponent<Image>(),
                chargeImage);
            return view;
        }
    }
}
