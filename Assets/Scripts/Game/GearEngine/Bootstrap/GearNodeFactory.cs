using UnityEngine;
using VContainer;

namespace Scaffold.GearEngine.Bootstrap
{
    public class GearNodeFactory
    {
        private readonly IObjectResolver resolver;

        public GearNodeFactory(IObjectResolver resolver)
        {
            this.resolver = resolver;
        }

        public IGridNode CreateNode(Vector2Int position, GearConfigData configData)
        {
            IGridNode node = null;
            switch (configData.Category)
            {
                case GearCategory.Core:
                    node = resolver.Resolve<CoreGearNode>();
                    break;
                case GearCategory.Aura:
                    node = resolver.Resolve<AuraGearNode>();
                    break;
                case GearCategory.Base:
                default:
                    node = resolver.Resolve<BaseGearNode>();
                    break;
            }
            node.Initialize(position, configData);
            return node;
        }
    }
}
