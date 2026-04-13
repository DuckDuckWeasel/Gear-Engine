using GearEngine.GearEngine.Nodes;
using UnityEngine;
using VContainer;

namespace GearEngine.GearEngine.Bootstrap
{
    public class GearNodeFactory
    {
        public GearNodeFactory(IObjectResolver resolver)
        {
            this.resolver = resolver;
        }

        private readonly IObjectResolver resolver;

        public IGridNode CreateNode(Vector2Int position, GearConfigData configData)
        {
            IGridNode node = ResolveNodeForCategory(configData.Category);
            node.Initialize(position, configData);
            return node;
        }

        private IGridNode ResolveNodeForCategory(GearCategory category)
        {
            switch (category)
            {
                case GearCategory.Core:
                    return resolver.Resolve<CoreGearNode>();
                case GearCategory.Aura:
                    return resolver.Resolve<AuraGearNode>();
                case GearCategory.Base:
                default:
                    return resolver.Resolve<BaseGearNode>();
            }
        }
    }
}
