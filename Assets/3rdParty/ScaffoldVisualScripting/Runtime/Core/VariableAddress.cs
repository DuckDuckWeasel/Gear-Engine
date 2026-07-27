using System;

namespace Scaffold.VisualScripting
{
    [Serializable]
    public struct VariableAddress : IEquatable<VariableAddress>
    {
        public VariableAddress(BlackboardRuntimeInstanceId runtimeInstanceId, DefinitionId definitionId)
        {
            RuntimeInstanceId = runtimeInstanceId;
            DefinitionId = definitionId;
        }

        public BlackboardRuntimeInstanceId RuntimeInstanceId { get; }

        public DefinitionId DefinitionId { get; }

        public override bool Equals(object obj)
        {
            return obj is VariableAddress other && Equals(other);
        }

        public bool Equals(VariableAddress other)
        {
            return RuntimeInstanceId == other.RuntimeInstanceId && DefinitionId == other.DefinitionId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (RuntimeInstanceId.GetHashCode() * 397) ^ DefinitionId.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{RuntimeInstanceId}/{DefinitionId}";
        }

        public static bool operator ==(VariableAddress left, VariableAddress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VariableAddress left, VariableAddress right)
        {
            return !left.Equals(right);
        }
    }
}
