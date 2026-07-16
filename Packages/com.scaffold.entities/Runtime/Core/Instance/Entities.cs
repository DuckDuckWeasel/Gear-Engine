namespace Scaffold.Entities
{
    public static class Entities
    {
        public static EntityInstance<TDefinition> Local<TDefinition>(TDefinition definition) where TDefinition : IEntityDefinition
            => new EntityInstanceImpl<TDefinition>(definition);

        private class EntityInstanceImpl<T> : EntityInstance<T> where T : IEntityDefinition
        {
            public EntityInstanceImpl(T definition) : base(definition) { }
        }
    }
}
