using VContainer;
using VContainer.Unity;

namespace GearEngine.GearEngine.Presentation.UI.Tags.Highlight
{
    public class TutorialFocusInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            builder.Register<TutorialFocusService>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
        }
    }
}
