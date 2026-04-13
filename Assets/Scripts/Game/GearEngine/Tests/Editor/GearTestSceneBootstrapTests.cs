using NUnit.Framework;
using Scaffold.GearEngine.Presentation;
using Scaffold.Navigation.Contracts;
using UnityEngine;

namespace Scaffold.GearEngine.Tests.Editor
{
    public sealed class GearTestSceneBootstrapTests
    {
        [Test]
        public void GearTestSceneBootstrap_Initialize_OpensGearEngineViewModel()
        {
            var go = new GameObject("GearBootstrapTest");
            try
            {
                GearTestSceneBootstrap bootstrap = go.AddComponent<GearTestSceneBootstrap>();
                var nav = new CapturingNavigation();
                bootstrap.Construct(nav);
                bootstrap.Initialize();
                Assert.That(nav.LastOpened, Is.InstanceOf<GearEngineViewModel>());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
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
