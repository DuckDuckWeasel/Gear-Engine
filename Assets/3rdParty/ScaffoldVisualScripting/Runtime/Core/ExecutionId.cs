using System;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public struct ExecutionId : IEquatable<ExecutionId>
    {
        private ExecutionId(string value)
        {
            this.value = value;
        }

        public bool IsEmpty => string.IsNullOrEmpty(value);

        [SerializeField] private string value;

        public override bool Equals(object obj)
        {
            return obj is ExecutionId other && Equals(other);
        }

        public bool Equals(ExecutionId other)
        {
            return string.Equals(value, other.value, StringComparison.Ordinal);
        }

        public override int GetHashCode()
        {
            return value == null ? 0 : StringComparer.Ordinal.GetHashCode(value);
        }

        public override string ToString()
        {
            return value ?? string.Empty;
        }

        public static ExecutionId New()
        {
            return new ExecutionId(Guid.NewGuid().ToString("N"));
        }

        public static bool operator ==(ExecutionId left, ExecutionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ExecutionId left, ExecutionId right)
        {
            return !left.Equals(right);
        }
    }
}
