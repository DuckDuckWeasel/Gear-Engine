using System.Reflection;
using GearEngine.CarSimulation;
using NUnit.Framework;
using Scaffold.Navigation.Contracts;
using UnityEditor;
using UnityEngine;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class CarTrackBootstrapTests
    {
        [Test]
        public void CarTrackBootstrap_Initialize_OpensTrackViewModel()
        {
            var go = new GameObject("BootstrapTest");
            try
            {
                AssertTrackViewModelOpenedForBootstrap(go);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private void AssertTrackViewModelOpenedForBootstrap(GameObject go)
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                (CarTrackBootstrap bootstrap, CapturingNavigation nav) = CreateBootstrapWithNav(go, carDef, trackDef);
                bootstrap.Initialize();
                Assert.That(nav.LastOpened, Is.InstanceOf<TrackViewModel>());
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        private (CarTrackBootstrap bootstrap, CapturingNavigation nav) CreateBootstrapWithNav(GameObject go, CarDefinition carDef, TrackDefinition trackDef)
        {
            CarTrackBootstrap bootstrap = go.AddComponent<CarTrackBootstrap>();
            SerializedObject bSo = new SerializedObject(bootstrap);
            bSo.FindProperty("carDefinition").objectReferenceValue = carDef;
            bSo.FindProperty("trackDefinition").objectReferenceValue = trackDef;
            bSo.ApplyModifiedPropertiesWithoutUndo();
            var factory = new TrackSimulationFactory();
            var nav = new CapturingNavigation();
            InjectPrivateField(bootstrap, "factory", factory);
            InjectPrivateField(bootstrap, "navigation", nav);
            return (bootstrap, nav);
        }

        private void InjectPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(target, value);
        }

        private sealed class CapturingNavigation : INavigation
        {
            public IViewController LastOpened { get; private set; }

            public IViewController CurrentController => LastOpened;

            public void Open<TViewController>(TViewController controller, bool closeCurrent = false, NavigationOptions options = null) where TViewController : IViewController
            {
                LastOpened = controller;
            }

            public void Close<TViewController>(TViewController controller) where TViewController : IViewController
            {
            }

            public IViewController Return()
            {
                return null;
            }
        }
    }
}
