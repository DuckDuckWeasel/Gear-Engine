using System;
using VContainer;
using VContainer.Unity;

namespace Scaffold.VisualScripting.Unity
{
    public sealed class BlackboardRuntimeInstaller : IInstaller
    {
        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            RegisterCoreServices(builder);
            RegisterUnityServices(builder);
            RegisterRuntimeFactories(builder);
        }

        private void RegisterCoreServices(IContainerBuilder builder)
        {
            builder.Register<SerializedGraphCloner>(Lifetime.Singleton);
            builder.Register<BlackboardDefinitionValidator>(Lifetime.Singleton);
            builder.Register<BlackboardEventBus>(Lifetime.Singleton).As<IBlackboardEventBus>();
            builder.Register<BlackboardRegistry>(Lifetime.Singleton).As<IBlackboardRegistry>();
            builder.Register<PublicVariableRegistry>(Lifetime.Singleton).As<IPublicVariableRegistry>();
            builder.Register<GlobalVariableStore>(Lifetime.Singleton).As<IGlobalVariableStore>();
            builder.Register<IRandomSource>(_ => new SystemRandomSource(), Lifetime.Transient);
        }

        private void RegisterUnityServices(IContainerBuilder builder)
        {
            builder.Register<UnityBlackboardLogger>(Lifetime.Singleton).As<IBlackboardLogger>();
            builder.Register<UnityTimeSource>(Lifetime.Singleton).As<ITimeSource>();
            builder.Register<UnityVariableValueSerializer>(Lifetime.Singleton).As<IVariableValueSerializer>();
            builder.Register<UnityPlayerPrefsBlackboardSaveService>(Lifetime.Singleton).As<IBlackboardSaveService>();
            builder.RegisterComponentOnNewGameObject<UnityCoroutineRunner>(Lifetime.Singleton, "ScaffoldBlackboardCoroutineRunner").AsSelf();
            builder.Register<UnityFrameScheduler>(Lifetime.Transient).As<IFrameScheduler>();
        }

        private void RegisterRuntimeFactories(IContainerBuilder builder)
        {
            builder.RegisterFactory<IFrameScheduler>(resolver => () => resolver.Resolve<IFrameScheduler>(), Lifetime.Singleton);
            builder.Register<UnityBlackboardRuntimeServicesFactory>(Lifetime.Singleton).As<IBlackboardRuntimeServicesFactory>();
            builder.Register<BlackboardFactory>(Lifetime.Transient);
        }
    }
}
