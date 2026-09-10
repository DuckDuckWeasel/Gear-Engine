using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.App.Bootstrap.Tests.Editor
{
    [TestFixture]
    public sealed class MinimumDelayTaskTests
    {
        [Test]
        public void InitializeAsync_UsesUnityPlayerLoopInsteadOfThreadPoolDelay()
        {
            string sourcePath = Path.Combine(
                Application.dataPath,
                "GearEngine/Scripts/App/Bootstrap/Layers/MinimumDelayTask.cs");
            string source = File.ReadAllText(sourcePath);

            Assert.That(source, Does.Contain("Awaitable.NextFrameAsync"));
            Assert.That(source, Does.Not.Contain("Task.Delay"));
        }
    }
}
