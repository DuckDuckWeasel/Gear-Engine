using System.Reflection;
using GearEngine.CarSimulation.Definitions;
using GearEngine.Race;
using GearEngine.Race.Bootstrap;
using NUnit.Framework;
using Scaffold.Navigation.Contracts;
using UnityEditor;
using UnityEngine;

namespace GearEngine.Race.Tests.Editor
{
    public sealed class RaceBootstrapTests
    {
        [Test]
        public void RaceBootstrap_Initialize_OpensRaceViewModel()
        {
            var go = new GameObject("RaceBootstrapTest");
            try
            {
                AssertRaceViewModelOpenedForBootstrap(go);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        private static void AssertRaceViewModelOpenedForBootstrap(GameObject go)
        {
            var carDef = ScriptableObject.CreateInstance<CarDefinition>();
            var trackDef = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                (RaceBootstrap bootstrap, CapturingNavigation nav) = CreateBootstrapWithNav(go, carDef, trackDef);
                bootstrap.Initialize();
                Assert.That(nav.LastOpened, Is.InstanceOf<RaceViewModel>());
            }
            finally
            {
                Object.DestroyImmediate(carDef);
                Object.DestroyImmediate(trackDef);
            }
        }

        private static (RaceBootstrap bootstrap, CapturingNavigation nav) CreateBootstrapWithNav(
            GameObject go,
            CarDefinition carDef,
            TrackDefinition trackDef)
        {
            RaceBootstrap bootstrap = go.AddComponent<RaceBootstrap>();
            SerializedObject bSo = new SerializedObject(bootstrap);
            SerializedProperty startProp = bSo.FindProperty("startData");
            Assert.That(startProp, Is.Not.Null);
            startProp.FindPropertyRelative("trackDefinition").objectReferenceValue = trackDef;
            startProp.FindPropertyRelative("carDefinition").objectReferenceValue = carDef;
            bSo.ApplyModifiedPropertiesWithoutUndo();

            var nav = new CapturingNavigation();
            InjectPrivateField(bootstrap, "navigation", nav);
            return (bootstrap, nav);
        }

        private static void InjectPrivateField(object target, string name, object value)
        {
            FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(target, value);
        }

        private sealed class CapturingNavigation : INavigation
        {
            public IViewController LastOpened { get; private set; }

            public IViewController CurrentController => LastOpened;

            public void Open<TViewController>(TViewController controller, bool closeCurrent = false, NavigationOptions options = null)
                where TViewController : IViewController
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
