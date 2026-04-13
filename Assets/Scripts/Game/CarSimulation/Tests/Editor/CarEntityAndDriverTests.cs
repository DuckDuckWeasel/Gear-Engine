using GearEngine.CarSimulation;
using NUnit.Framework;
using Scaffold.Entities;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class CarEntityAndDriverTests
    {
        [Test]
        public void CarEntity_Create_ProducesRuntimeInstanceWithoutPrefabHost()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            try
            {
                CarEntity car = CarEntity.Create(carDef);
                Assert.That(car, Is.Not.Null);
                Assert.That(car.Instance, Is.Not.Null);
                Assert.That(car.Definition, Is.SameAs(carDef));
            }
            finally
            {
                Object.DestroyImmediate(carDef);
            }
        }

        [Test]
        public void CarSplineDriver_Bind_DoesNotThrowBeforeUnityStart()
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var speed = ScriptableObject.CreateInstance<AttributeSO>();
            var so = new UnityEditor.SerializedObject(speed);
            so.FindProperty("valueType").enumValueIndex = (int)AttributeValueType.Float;
            so.ApplyModifiedPropertiesWithoutUndo();

            var go = new GameObject("DriverBindTest");
            try
            {
                var splineAnimate = go.AddComponent<SplineAnimate>();
                var driver = go.AddComponent<CarSplineDriver>();
                var driverSerialized = new UnityEditor.SerializedObject(driver);
                driverSerialized.FindProperty("splineAnimate").objectReferenceValue = splineAnimate;
                driverSerialized.FindProperty("speedAttribute").objectReferenceValue = speed;
                driverSerialized.ApplyModifiedPropertiesWithoutUndo();

                CarEntity car = CarEntity.Create(carDef);
                var container = go.AddComponent<SplineContainer>();

                Assert.DoesNotThrow(() => driver.Bind(car, container));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(speed);
                Object.DestroyImmediate(carDef);
            }
        }
    }
}
