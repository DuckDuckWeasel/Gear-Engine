using System;
using System.Reflection;
using GearEngine.SceneFoundation.Bootstrap;
using NUnit.Framework;
using Scaffold.Navigation;
using UnityEditor;
using UnityEngine;
using VContainer;

namespace GearEngine.SceneFoundation.Tests.Editor
{
    public sealed class SceneFoundationScopeTests
    {
        private const string NavigationSettingsPath = "Assets/Navigation/Navigation Settings.asset";

        [Test]
        public void Configure_MissingNavigationSettings_ThrowsInvalidOperationException()
        {
            var go = new GameObject("SceneFoundationTest");
            try
            {
                var scope = go.AddComponent<MinimalSceneScope>();
                AssignNavigationViewHolderOnly(scope, new GameObject("Holder").transform);

                var builder = new ContainerBuilder();
                InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
                    InvokeProtectedConfigure(scope, builder));
                Assert.That(ex.Message, Does.Contain("navigationSettings"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Configure_MissingNavigationViewHolder_ThrowsInvalidOperationException()
        {
            var go = new GameObject("SceneFoundationTest");
            try
            {
                var scope = go.AddComponent<MinimalSceneScope>();
                AssignNavigationSettings(scope, LoadNavigationSettingsOrSkip());

                var builder = new ContainerBuilder();
                InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
                    InvokeProtectedConfigure(scope, builder));
                Assert.That(ex.Message, Does.Contain("navigationViewHolder"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void Configure_WithValidFoundation_CallsInstallFeatureServices()
        {
            var nav = LoadNavigationSettingsOrSkip();
            if (nav == null)
            {
                Assert.Ignore($"Navigation settings not found at {NavigationSettingsPath}.");
            }

            var go = new GameObject("SceneFoundationTest");
            try
            {
                var scope = go.AddComponent<TrackingSceneScope>();
                AssignNavigationSettings(scope, nav);
                AssignNavigationViewHolderOnly(scope, new GameObject("Holder").transform);

                var builder = new ContainerBuilder();
                Assert.DoesNotThrow(() => InvokeProtectedConfigure(scope, builder));
                Assert.That(scope.InstallFeatureServicesCalled, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private static NavigationSettings LoadNavigationSettingsOrSkip()
        {
            return AssetDatabase.LoadAssetAtPath<NavigationSettings>(NavigationSettingsPath);
        }

        private static void InvokeProtectedConfigure(SceneFoundationScope scope, IContainerBuilder builder)
        {
            MethodInfo configure = typeof(SceneFoundationScope).GetMethod(
                "Configure",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(configure, Is.Not.Null);
            configure.Invoke(scope, new object[] { builder });
        }

        private static void AssignNavigationSettings(SceneFoundationScope scope, NavigationSettings settings)
        {
            SerializedObject so = new SerializedObject(scope);
            SerializedProperty prop = so.FindProperty("navigationSettings");
            Assert.That(prop, Is.Not.Null);
            prop.objectReferenceValue = settings;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignNavigationViewHolderOnly(SceneFoundationScope scope, Transform holder)
        {
            SerializedObject so = new SerializedObject(scope);
            SerializedProperty prop = so.FindProperty("navigationViewHolder");
            Assert.That(prop, Is.Not.Null);
            prop.objectReferenceValue = holder;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private sealed class MinimalSceneScope : SceneFoundationScope
        {
            protected override void InstallFeatureServices(IContainerBuilder builder)
            {
            }
        }

        private sealed class TrackingSceneScope : SceneFoundationScope
        {
            public bool InstallFeatureServicesCalled { get; private set; }

            protected override void InstallFeatureServices(IContainerBuilder builder)
            {
                InstallFeatureServicesCalled = true;
            }
        }
    }
}
