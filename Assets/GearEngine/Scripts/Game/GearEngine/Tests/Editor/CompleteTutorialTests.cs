using NUnit.Framework;
using Scaffold;
using Scaffold.Tutorial.Controllers;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    public sealed class CompleteTutorialTests
    {
        [Test]
        public void Execute_WithProgressController_ReportsCompletion()
        {
            GameObject gameObject = new GameObject("Tutorial");
            try
            {
                Blackboard blackboard = gameObject.AddComponent<Blackboard>();
                TutorialProgressController progressController =
                    gameObject.AddComponent<TutorialProgressController>();
                bool completed = false;
                progressController.OnTutorialCompleted += (_, skipped) =>
                    completed = !skipped;

                CompleteTutorial action = new CompleteTutorial();
                action.SetBlackboard(blackboard);
                action.Execute(() => { });

                Assert.That(completed, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
