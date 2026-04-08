using UnityEngine;
using VContainer;

namespace Game.GearEngine
{
    public class GearNodeFactory
    {
        private readonly IObjectResolver resolver;

        public GearNodeFactory(IObjectResolver resolver)
        {
            this.resolver = resolver;
        }

        public IGridNode CreateCoreGear(Vector2Int position, GearConfigData configData)
        {
            var node = resolver.Resolve<CoreGearNode>();
            node.Initialize(position, configData);
            return node;
        }

        public IGridNode CreateBaseGear(Vector2Int position, GearConfigData configData)
        {
            var node = resolver.Resolve<BaseGearNode>();
            node.Initialize(position, configData);
            return node;
        }

        public IGridNode CreateAuraGear(Vector2Int position, GearConfigData configData)
        {
            var node = resolver.Resolve<AuraGearNode>();
            node.Initialize(position, configData);
            return node;
        }
    }
}
