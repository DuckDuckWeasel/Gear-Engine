#nullable enable
using UnityEngine;

namespace Scaffold.Entities
{
    public partial class EntityComponent<TDefinition> : EntityComponent where TDefinition : IEntityDefinition
    {
        [SerializeField] private TDefinition definition = default!;

        private EntityInstance<TDefinition>? instance;

        public TDefinition Definition => definition;

        public EntityInstance<TDefinition> Instance
            => instance ??= Entities.Local(definition);
    }
}
