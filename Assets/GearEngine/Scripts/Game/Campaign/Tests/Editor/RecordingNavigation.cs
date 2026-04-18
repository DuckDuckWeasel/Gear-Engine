using System.Collections.Generic;
using Scaffold.Navigation.Contracts;

namespace GearEngine.Campaign.Tests.Editor
{
    internal sealed class RecordingNavigation : INavigation
    {
        public readonly List<object> OpenedControllers = new List<object>();

        public int ReturnCallCount { get; private set; }

        public IViewController CurrentController => null;

        public void Open<TViewController>(TViewController controller, bool closeCurrent = false, NavigationOptions options = null)
            where TViewController : IViewController
        {
            if (controller != null)
            {
                OpenedControllers.Add(controller);
            }
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
