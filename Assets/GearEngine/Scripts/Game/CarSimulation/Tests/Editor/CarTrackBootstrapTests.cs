using System.Collections.Generic;
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
        public void Initialize_OpensTrackListViewModel()
        {
            var go = new GameObject("BootstrapTest");
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
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void Initialize_CreatesOneSessionPerCarDefinition()
        {
            var go = new GameObject("BootstrapTest");
            var carDef1 = ScriptableObject.CreateInstance<CarDefinition>();
            var carDef2 = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                SeedMinimalOpenTrack(trackDef);
                CarTrackBootstrap bootstrap = go.AddComponent<CarTrackBootstrap>();
                SerializedObject bSo = new SerializedObject(bootstrap);
                SerializedProperty listProp = bSo.FindProperty("carDefinitions");
                listProp.arraySize = 2;
                listProp.GetArrayElementAtIndex(0).objectReferenceValue = carDef1;
                listProp.GetArrayElementAtIndex(1).objectReferenceValue = carDef2;
                bSo.FindProperty("trackDefinition").objectReferenceValue = trackDef;
                bSo.ApplyModifiedPropertiesWithoutUndo();

                var nav = new CapturingNavigation();
                InjectPrivateField(bootstrap, "factory", new TrackSimulationFactory());
                InjectPrivateField(bootstrap, "navigation", nav);
                bootstrap.Initialize();

                var vm = (TrackListViewModel)nav.LastOpened;
                Assert.That(vm.Sessions.Count, Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(carDef1);
                Object.DestroyImmediate(carDef2);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void Initialize_AllSessionsStartInCreatedPhase()
        {
            var go = new GameObject("BootstrapTest");
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                SeedMinimalOpenTrack(trackDef);
                (CarTrackBootstrap bootstrap, CapturingNavigation nav) = CreateBootstrapWithNav(go, carDef, trackDef);
                bootstrap.Initialize();

                var vm = (TrackListViewModel)nav.LastOpened;
                foreach (LapRaceSession session in vm.Sessions)
                {
                    Assert.That(session.Phase, Is.EqualTo(SimulationLifecycleState.Created),
                        "Sessions must not auto-start; clock is only started explicitly by the race flow.");
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void Initialize_CreatesOneRunnerPerCarDefinition()
        {
            var go = new GameObject("BootstrapTest");
            var carDef1 = ScriptableObject.CreateInstance<CarDefinition>();
            var carDef2 = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                SeedMinimalOpenTrack(trackDef);
                CarTrackBootstrap bootstrap = go.AddComponent<CarTrackBootstrap>();
                SerializedObject bSo = new SerializedObject(bootstrap);
                SerializedProperty listProp = bSo.FindProperty("carDefinitions");
                listProp.arraySize = 2;
                listProp.GetArrayElementAtIndex(0).objectReferenceValue = carDef1;
                listProp.GetArrayElementAtIndex(1).objectReferenceValue = carDef2;
                bSo.FindProperty("trackDefinition").objectReferenceValue = trackDef;
                bSo.ApplyModifiedPropertiesWithoutUndo();

                var nav = new CapturingNavigation();
                InjectPrivateField(bootstrap, "factory", new TrackSimulationFactory());
                InjectPrivateField(bootstrap, "navigation", nav);
                bootstrap.Initialize();

                var runners = (List<IRaceSessionRunner>)GetPrivateField(bootstrap, "runners");
                Assert.That(runners.Count, Is.EqualTo(2),
                    "CarTrackBootstrap must create one IRaceSessionRunner per CarDefinition for its multi-session tick loop.");
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(carDef1);
                Object.DestroyImmediate(carDef2);
                Object.DestroyImmediate(trackDef);
            }
        }

        [Test]
        public void Initialize_EachRunnerHasSessionAssigned()
        {
            var go = new GameObject("BootstrapTest");
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                SeedMinimalOpenTrack(trackDef);
                (CarTrackBootstrap bootstrap, CapturingNavigation nav) = CreateBootstrapWithNav(go, carDef, trackDef);
                bootstrap.Initialize();

                var runners = (List<IRaceSessionRunner>)GetPrivateField(bootstrap, "runners");
                Assert.That(runners.Count, Is.GreaterThan(0));
                foreach (IRaceSessionRunner runner in runners)
                {
                    Assert.That(runner.ActiveSession, Is.Not.Null,
                        "Each runner must have an assigned LapRaceSession after Initialize.");
                }
            }
            finally
            {
                Object.DestroyImmediate(go);
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
            return (bootstrap, nav);
        }

        private static void InjectPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(target, value);
        }

        private static object GetPrivateField(object target, string name)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Field '{name}' not found on {target.GetType().Name}.");
            return field.GetValue(target);
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
