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

        [Test]
        public void ApplyConfig_PreservesGearBodyAndAssignsIconToChargeImage()
        {
            Texture2D bodyTexture = new Texture2D(4, 4);
            Texture2D iconTexture = new Texture2D(4, 4);
            Sprite bodySprite = Sprite.Create(
                bodyTexture,
                new Rect(0f, 0f, 4f, 4f),
                new Vector2(0.5f, 0.5f));
            Sprite iconSprite = Sprite.Create(
                iconTexture,
                new Rect(0f, 0f, 4f, 4f),
                new Vector2(0.5f, 0.5f));
            GearView view = CreateViewWithChargeImage(source: null, out Image chargeImage);
            Image bodyImage = view.transform.GetChild(0).GetComponent<Image>();
            bodyImage.sprite = bodySprite;

            view.ApplyConfig(new GearItemData { UIIcon = iconSprite });

            Assert.That(bodyImage.sprite, Is.SameAs(bodySprite));
            Assert.That(chargeImage.sprite, Is.SameAs(iconSprite));

            Object.DestroyImmediate(view.gameObject);
            Object.DestroyImmediate(bodySprite);
            Object.DestroyImmediate(iconSprite);
            Object.DestroyImmediate(bodyTexture);
            Object.DestroyImmediate(iconTexture);
        }

        [Test]
        public void BasePrefab_EnlargesGearBodyAndIconWithinSlot()
        {
            GearView prefab = AssetDatabase.LoadAssetAtPath<GearView>(
                "Assets/GearEngine/Prefabs/Gears/Gears/BaseGearView.prefab");
            Assert.IsNotNull(prefab);

            GearView instance = Object.Instantiate(prefab);
            instance.ApplyConfig(new GearItemData { RelativeScaleMultiplier = 1f });

            RectTransform gearVisual = instance.transform.Find("GearVisual") as RectTransform;
            RectTransform chargeVisual = instance.transform.Find("ChargeVisual") as RectTransform;
            Assert.IsNotNull(gearVisual);
            Assert.IsNotNull(chargeVisual);
            Assert.That(gearVisual.localScale.x, Is.EqualTo(1.38f).Within(0.001f));
            Assert.That(chargeVisual.localScale.x, Is.EqualTo(1.15f).Within(0.001f));
            Assert.That(chargeVisual.anchorMin, Is.EqualTo(new Vector2(0.05f, 0.05f)));
            Assert.That(chargeVisual.anchorMax, Is.EqualTo(new Vector2(0.95f, 0.95f)));

            Object.DestroyImmediate(instance.gameObject);
        }

        [Test]
        public void CorePrefab_UsesLargerScaleToMeshWithStandardGears()
        {
            GearView prefab = AssetDatabase.LoadAssetAtPath<GearView>(
                "Assets/GearEngine/Prefabs/Gears/Gears/CoreGearView.prefab");
            Assert.IsNotNull(prefab);

            GearView instance = Object.Instantiate(prefab);
            instance.ApplyConfig(new GearItemData { RelativeScaleMultiplier = 1f });

            RectTransform gearVisual = instance.transform.Find("GearVisual") as RectTransform;
            RectTransform chargeVisual = instance.transform.Find("ChargeVisual") as RectTransform;
            Assert.IsNotNull(gearVisual);
            Assert.IsNotNull(chargeVisual);
            Assert.That(gearVisual.localScale.x, Is.EqualTo(1.55f).Within(0.001f));
            Assert.That(chargeVisual.localScale.x, Is.EqualTo(1.15f).Within(0.001f));

            Object.DestroyImmediate(instance.gameObject);
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
