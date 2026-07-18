namespace Scaffold
{
    public static class VariableValueReference
    {
        public static T Resolve<T>(VariableBase<T> flowchartVariable, T directValue, VariableValueSO<T> scriptableObjectValue, VariableDataSource source)
        {
            switch (source)
            {
                case VariableDataSource.FlowchartVariable:
                    return flowchartVariable != null ? flowchartVariable.Value : default;
                case VariableDataSource.Direct:
                    return directValue;
                case VariableDataSource.ScriptableObject:
                    return scriptableObjectValue != null ? scriptableObjectValue.Value : default;
                case VariableDataSource.Unspecified:
                default:
                    return flowchartVariable != null ? flowchartVariable.Value : directValue;
            }
        }

        public static void Assign<T>(VariableBase<T> flowchartVariable, ref T directValue, VariableValueSO<T> scriptableObjectValue, VariableDataSource source, T value)
        {
            VariableDataSource resolvedSource = GetResolvedSource(flowchartVariable, source);
            if (resolvedSource == VariableDataSource.FlowchartVariable)
            {
                AssignFlowchartVariable(flowchartVariable, value);
                return;
            }

            if (resolvedSource == VariableDataSource.ScriptableObject)
            {
                AssignScriptableObject(scriptableObjectValue, value);
                return;
            }

            directValue = value;
        }

        public static VariableDataSource GetResolvedSource<T>(VariableBase<T> flowchartVariable, VariableDataSource source)
        {
            return source == VariableDataSource.Unspecified
                ? flowchartVariable != null ? VariableDataSource.FlowchartVariable : VariableDataSource.Direct
                : source;
        }

        public static string Describe<T>(VariableBase<T> flowchartVariable, T directValue, VariableValueSO<T> scriptableObjectValue, VariableDataSource source)
        {
            switch (GetResolvedSource(flowchartVariable, source))
            {
                case VariableDataSource.FlowchartVariable:
                    return flowchartVariable != null ? flowchartVariable.Key : "Null";
                case VariableDataSource.ScriptableObject:
                    return scriptableObjectValue != null ? scriptableObjectValue.name : "Null";
                case VariableDataSource.Direct:
                default:
                    return (object)directValue != null ? directValue.ToString() : "Null";
            }
        }

        private static void AssignFlowchartVariable<T>(VariableBase<T> flowchartVariable, T value)
        {
            if (flowchartVariable != null)
            {
                flowchartVariable.Value = value;
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
