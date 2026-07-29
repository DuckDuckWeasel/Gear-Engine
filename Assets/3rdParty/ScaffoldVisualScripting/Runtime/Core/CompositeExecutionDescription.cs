namespace Scaffold.VisualScripting
{
    public static class CompositeExecutionDescription
    {
        public static bool SupportsAwait(ActionListExecutionMethod method)
        {
            return method == ActionListExecutionMethod.Parallel || method == ActionListExecutionMethod.ParallelSelector;
        }

        public static bool SupportsOrder(ActionListExecutionMethod method)
        {
            return method == ActionListExecutionMethod.Sequence || method == ActionListExecutionMethod.Selector;
        }

        public static bool SupportsWeight(ActionListExecutionMethod method, ActionListOrderMode orderMode)
        {
            return SupportsOrder(method) && orderMode == ActionListOrderMode.Random;
        }
    }
}
