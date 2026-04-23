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
        public void CarTrackBootstrap_Initialize_OpensCarTrackScreenViewModel()
        {
            var go = new GameObject("BootstrapTest");
            try
            {
                AssertCarTrackScreenViewModelOpenedForBootstrap(go);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private void AssertCarTrackScreenViewModelOpenedForBootstrap(GameObject go)
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                SeedMinimalOpenTrack(trackDef);
                (CarTrackBootstrap bootstrap, CapturingNavigation nav) = CreateBootstrapWithNav(go, carDef, trackDef);
                bootstrap.Initialize();
                Assert.That(nav.LastOpened, Is.InstanceOf<CarTrackScreenViewModel>());
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
            var aiRunner = new SplineCarRunnerService(ScriptableObject.CreateInstance<SplineCarRunnerConfigSO>());
            var raceManager = new RaceManagerService(aiRunner);
            
            InjectPrivateField(bootstrap, "factory", factory);
            InjectPrivateField(bootstrap, "navigation", nav);
            InjectPrivateField(bootstrap, "raceManager", raceManager);
            InjectPrivateField(bootstrap, "aiRunner", aiRunner);
            
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

            public void Open<TViewController>(TViewController controller, NavigationOptions options) where TViewController : IViewController
            {
                LastOpened = controller;
            }

            public void PrepareDependencies(IViewController controller)
            {
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
