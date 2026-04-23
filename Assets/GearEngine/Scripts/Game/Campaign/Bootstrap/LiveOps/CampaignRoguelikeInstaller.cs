using System;
using GearEngine.Campaign.Services;
using Scaffold.AppFlow;
using Scaffold.LiveOps;
using VContainer;
using VContainer.Unity;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class CampaignRoguelikeInstaller : IInstaller
    {
        private readonly RoguelikeGearPoolSO roguelikeGearPool;

        public CampaignRoguelikeInstaller(RoguelikeGearPoolSO roguelikeGearPool)
        {
            this.roguelikeGearPool = roguelikeGearPool ?? throw new ArgumentNullException(nameof(roguelikeGearPool));
        }

        public void Install(IContainerBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.RegisterInstance(roguelikeGearPool);

            builder.Register<RoguelikeClientModule>(Lifetime.Singleton)
                .AsSelf()
                .As<IGameClientModule>()
                .As<IAsyncInitializable>();
        }
    }
}
