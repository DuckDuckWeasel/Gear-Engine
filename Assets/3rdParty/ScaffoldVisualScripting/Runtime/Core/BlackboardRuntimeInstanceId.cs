using System;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public struct BlackboardRuntimeInstanceId : IEquatable<BlackboardRuntimeInstanceId>
    {
        public BlackboardRuntimeInstanceId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("A runtime instance ID cannot be null or empty.", nameof(value));
            }

            this.value = value;
        }

        public string Value => value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrWhiteSpace(value);

        [SerializeField] private string value;

        public override bool Equals(object obj)
        {
            return obj is BlackboardRuntimeInstanceId other && Equals(other);
        }

        public bool Equals(BlackboardRuntimeInstanceId other)
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

        public static BlackboardRuntimeInstanceId New()
        {
            return new BlackboardRuntimeInstanceId(Guid.NewGuid().ToString("N"));
        }

        public static bool operator ==(BlackboardRuntimeInstanceId left, BlackboardRuntimeInstanceId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlackboardRuntimeInstanceId left, BlackboardRuntimeInstanceId right)
        {
            return !left.Equals(right);
        }
    }
}
