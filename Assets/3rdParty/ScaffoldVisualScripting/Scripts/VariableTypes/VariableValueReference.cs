namespace Scaffold
{
    public static class VariableValueReference
    {
        public static T Resolve<T>(VariableBase<T> blackboardVariable, T directValue, VariableValueSO<T> scriptableObjectValue, VariableDataSource source)
        {
            switch (source)
            {
                case VariableDataSource.BlackboardVariable:
                    return blackboardVariable != null ? blackboardVariable.Value : default;
                case VariableDataSource.Direct:
                    return directValue;
                case VariableDataSource.ScriptableObject:
                    return scriptableObjectValue != null ? scriptableObjectValue.Value : default;
                case VariableDataSource.Unspecified:
                default:
                    return blackboardVariable != null ? blackboardVariable.Value : directValue;
            }
        }

        public static void Assign<T>(VariableBase<T> blackboardVariable, ref T directValue, VariableValueSO<T> scriptableObjectValue, VariableDataSource source, T value)
        {
            VariableDataSource resolvedSource = GetResolvedSource(blackboardVariable, source);
            if (resolvedSource == VariableDataSource.BlackboardVariable)
            {
                AssignBlackboardVariable(blackboardVariable, value);
                return;
            }

            if (resolvedSource == VariableDataSource.ScriptableObject)
            {
                AssignScriptableObject(scriptableObjectValue, value);
                return;
            }

            directValue = value;
        }

        public static VariableDataSource GetResolvedSource<T>(VariableBase<T> blackboardVariable, VariableDataSource source)
        {
            return source == VariableDataSource.Unspecified
                ? blackboardVariable != null ? VariableDataSource.BlackboardVariable : VariableDataSource.Direct
                : source;
        }

        public static string Describe<T>(VariableBase<T> blackboardVariable, T directValue, VariableValueSO<T> scriptableObjectValue, VariableDataSource source)
        {
            switch (GetResolvedSource(blackboardVariable, source))
            {
                case VariableDataSource.BlackboardVariable:
                    return blackboardVariable != null ? blackboardVariable.Key : "Null";
                case VariableDataSource.ScriptableObject:
                    return scriptableObjectValue != null ? scriptableObjectValue.name : "Null";
                case VariableDataSource.Direct:
                default:
                    return (object)directValue != null ? directValue.ToString() : "Null";
            }
        }

        private static void AssignBlackboardVariable<T>(VariableBase<T> blackboardVariable, T value)
        {
            if (blackboardVariable != null)
            {
                blackboardVariable.Value = value;
            }
        }

        private static void AssignScriptableObject<T>(VariableValueSO<T> scriptableObjectValue, T value)
        {
            if (scriptableObjectValue != null)
            {
                scriptableObjectValue.Value = value;
            }
        }
    }
}
