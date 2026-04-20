using GearEngine.Campaign.Bootstrap;
using GearEngine.Campaign.Bootstrap.Cards;
using GearEngine.Currency.Bootstrap;
using GearEngine.LayeredScope;
using Scaffold.CloudCode.Container;
using Scaffold.LiveOps.Container;
using VContainer;

namespace GearEngine.App.Bootstrap.Layers
{
    public sealed class LiveOpsLayer : IScopeLayer
    {
        public void Install(IContainerBuilder builder)
        {
            new CloudCodeInstaller().Install(builder);
            new LiveOpsInstaller().Install(builder);
            new CurrencyClientInstaller().Install(builder);
            new MetaLiveOpsClientInstaller().Install(builder);
            new CardsClientInstaller().Install(builder);
        }
    }
}
