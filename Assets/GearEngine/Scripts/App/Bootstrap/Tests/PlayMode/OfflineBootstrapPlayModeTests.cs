#if UNITY_EDITOR
using System;
using System.Collections;
using System.Threading.Tasks;
using GearEngine.App.Bootstrap;
using NUnit.Framework;
using Scaffold.AppFlow;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace GearEngine.App.Bootstrap.Tests.PlayMode
{
    /// <summary>
    /// End-to-end bootstrap test. Loads the real <c>Main Scene</c> with offline mode forced on,
    /// then waits for <see cref="AppFlowRoot.ReadyTask"/> to complete. Any <c>Debug.LogError</c>
    /// during startup fails the test via Unity's default log assertion.
    /// </summary>
    [TestFixture]
    public sealed class OfflineBootstrapPlayModeTests
    {
        private const string MainScenePath = "Assets/GearEngine/Scenes/Main Scene.unity";
        private const float ReadyTimeoutSeconds = 30f;

        [SetUp]
        public void SetUp()
        {
            GearAppFlowRoot.ForceOfflineModeForTests = true;
        }

        [TearDown]
        public void TearDown()
        {
            GearAppFlowRoot.ForceOfflineModeForTests = null;
        }

        [UnityTest]
        public IEnumerator Bootstrap_InOfflineMode_CompletesStartupWithoutErrors()
        {
            Scene scene = EditorSceneManager.LoadSceneInPlayMode(
                MainScenePath, new LoadSceneParameters(LoadSceneMode.Single));

            yield return new WaitUntil(() => scene.isLoaded);

            AppFlowRoot root = FindRootInScene(scene);
            Assert.That(root, Is.Not.Null, $"No AppFlowRoot found in '{MainScenePath}'.");

            Task ready = root.ReadyTask;
            float deadline = Time.realtimeSinceStartup + ReadyTimeoutSeconds;
            while (!ready.IsCompleted)
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    Assert.Fail($"AppFlowRoot.ReadyTask did not complete within {ReadyTimeoutSeconds}s.");
                }

                yield return null;
            }

            if (ready.IsFaulted)
            {
                Exception ex = ready.Exception?.GetBaseException();
                Assert.Fail($"AppFlowRoot startup faulted: {ex?.GetType().Name}: {ex?.Message}\n{ex?.StackTrace}");
            }

            Assert.That(ready.IsCompletedSuccessfully, Is.True, "AppFlowRoot.ReadyTask did not complete successfully.");
        }

        private static AppFlowRoot FindRootInScene(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                AppFlowRoot found = root.GetComponentInChildren<AppFlowRoot>(includeInactive: true);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
#endif
