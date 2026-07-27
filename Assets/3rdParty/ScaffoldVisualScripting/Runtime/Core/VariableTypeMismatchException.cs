using System;

namespace Scaffold.VisualScripting
{
    public sealed class VariableTypeMismatchException : InvalidOperationException
    {
        public VariableTypeMismatchException(DefinitionId definitionId, Type expectedType, Type actualType) : base(CreateMessage(definitionId, expectedType, actualType))
        {
        }

        private static string CreateMessage(DefinitionId definitionId, Type expectedType, Type actualType)
        {
            string actualName = actualType == null ? "null" : actualType.FullName;
            return $"Variable '{definitionId}' expects '{expectedType.FullName}' but received '{actualName}'.";
        }
    }
}
