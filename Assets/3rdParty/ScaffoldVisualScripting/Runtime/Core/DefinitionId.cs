using System;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public struct DefinitionId : IEquatable<DefinitionId>
    {
        public static DefinitionId Empty => default;

        public DefinitionId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A definition ID cannot be null or empty.", nameof(value));
            }

            this.value = value;
        }

        public string Value => value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrWhiteSpace(value);

        [SerializeField] private string value;

        public override bool Equals(object obj)
        {
            return obj is DefinitionId other && Equals(other);
        }

        public bool Equals(DefinitionId other)
        {
            return string.Equals(value, other.value, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return value == null ? 0 : StringComparer.Ordinal.GetHashCode(value);
        }

        public override string ToString()
        {
            return Value;
        }

        public static DefinitionId New()
        {
            return new DefinitionId(Guid.NewGuid().ToString("N"));
        }

        public static bool operator ==(DefinitionId left, DefinitionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DefinitionId left, DefinitionId right)
        {
            return !left.Equals(right);
        }
    }
}
