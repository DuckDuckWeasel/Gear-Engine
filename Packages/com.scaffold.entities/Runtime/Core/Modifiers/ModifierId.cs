using System;

namespace Scaffold.Entities
{
    /// <summary>
    /// Unique identifier for a modifier applied to an entity variable.
    /// Used to track and remove specific modifiers at runtime.
    /// </summary>
    public readonly struct ModifierId : IEquatable<ModifierId>
    {
        private readonly Guid value;

        private ModifierId(Guid value)
        {
            this.value = value;
        }

        public static ModifierId New() => new ModifierId(Guid.NewGuid());

        public bool Equals(ModifierId other) => value.Equals(other.value);
        public override bool Equals(object obj) => obj is ModifierId other && Equals(other);
        public override int GetHashCode() => value.GetHashCode();
        public static bool operator ==(ModifierId a, ModifierId b) => a.Equals(b);
        public static bool operator !=(ModifierId a, ModifierId b) => !a.Equals(b);
    }
}
