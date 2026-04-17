using System.Reflection;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Bootstrap;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Presentation;
using GearEngine.CarSimulation.Simulation;
using NUnit.Framework;
using Scaffold.Navigation.Contracts;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Tests
{
    public sealed class CarTrackBootstrapTests
    {
        private static void SeedMinimalOpenTrack(TrackDefinition trackDef)
        {
            trackDef.Spline.Knots = new[] { new BezierKnot(Vector3.zero), new BezierKnot(Vector3.right * 10f) };
            trackDef.Spline.Closed = false;
        }

        [Test]
        public void CarTrackBootstrap_Initialize_OpensTrackListViewModel()
        {
            var go = new GameObject("BootstrapTest");
            try
            {
                AssertTrackListViewModelOpenedForBootstrap(go);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private void AssertTrackListViewModelOpenedForBootstrap(GameObject go)
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                SeedMinimalOpenTrack(trackDef);
                (CarTrackBootstrap bootstrap, CapturingNavigation nav) = CreateBootstrapWithNav(go, carDef, trackDef);
                bootstrap.Initialize();
                Assert.That(nav.LastOpened, Is.InstanceOf<TrackListViewModel>());
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
            SerializedProperty listProp = bSo.FindProperty("carDefinitions");
            listProp.arraySize = 1;
            listProp.GetArrayElementAtIndex(0).objectReferenceValue = carDef;
            bSo.FindProperty("trackDefinition").objectReferenceValue = trackDef;
            bSo.ApplyModifiedPropertiesWithoutUndo();
            var factory = new TrackSimulationFactory();
            var nav = new CapturingNavigation();
            InjectPrivateField(bootstrap, "factory", factory);
            InjectPrivateField(bootstrap, "navigation", nav);
            InjectPrivateField(bootstrap, "raceSessionRunner", new RaceSessionRunner());
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
