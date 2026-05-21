using UnityEngine;
using VContainer;

namespace GearEngine.GearEngine.Bootstrap
{
    public class GearNodeFactory : IGearNodeFactory
    {
        public GearNodeFactory(IObjectResolver resolver)
        {
            this.resolver = resolver;
        }

        private readonly IObjectResolver resolver;

        public IGridNode CreateNode(Vector2Int position, GearItemData configData)
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
