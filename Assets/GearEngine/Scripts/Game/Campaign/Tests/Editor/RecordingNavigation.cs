using System.Collections.Generic;
using Scaffold.Navigation.Contracts;

namespace GearEngine.Campaign.Tests.Editor
{
    internal sealed class RecordingNavigation : INavigation
    {
        public readonly List<object> OpenedControllers = new List<object>();
        public readonly List<bool> OpenedCloseCurrent = new List<bool>();
        public readonly List<NavigationOptions> OpenedOptions = new List<NavigationOptions>();

        public int ReturnCallCount { get; private set; }

        public IViewController CurrentController { get; set; }

        public void Open<TViewController>(TViewController controller, bool closeCurrent = false, NavigationOptions options = null)
            where TViewController : IViewController
        {
            if (controller != null)
            {
                OpenedControllers.Add(controller);
                OpenedCloseCurrent.Add(closeCurrent);
                OpenedOptions.Add(options);
                CurrentController = controller;
            }
        }

        public void Open<TViewController>(TViewController controller, NavigationOptions options) where TViewController : IViewController
        {
            if (controller != null)
            {
                OpenedControllers.Add(controller);
                OpenedCloseCurrent.Add(false);
                OpenedOptions.Add(options);
                CurrentController = controller;
            }
        }

        public void PrepareDependencies(IViewController controller)
        {
        }

        public void Close<TViewController>(TViewController controller) where TViewController : IViewController
        {
        }

        public IViewController Return()
        {
            ReturnCallCount++;
            return null;
        }
    }
}
