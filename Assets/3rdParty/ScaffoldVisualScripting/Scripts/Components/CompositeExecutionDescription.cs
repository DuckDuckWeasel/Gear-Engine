namespace Scaffold
{
    public static class CompositeExecutionDescription
    {
        public static bool SupportsAwait(CompositeExecutionMethod executionMethod)
        {
            return executionMethod == CompositeExecutionMethod.Parallel ||
                   executionMethod == CompositeExecutionMethod.ParallelSelector;
        }

        public static bool SupportsOrder(CompositeExecutionMethod executionMethod)
        {
            return executionMethod == CompositeExecutionMethod.Sequence ||
                   executionMethod == CompositeExecutionMethod.Selector;
        }

        public static bool SupportsWeight(
            CompositeExecutionMethod executionMethod,
            CompositeOrderMode orderMode)
        {
            return SupportsOrder(executionMethod) && orderMode == CompositeOrderMode.Random;
        }

        public static string GetExecutionTooltip(
            CompositeExecutionMethod executionMethod,
            CompositeAwaitMode awaitMode,
            CompositeOrderMode orderMode)
        {
            string description = GetExecutionDescription(executionMethod);
            if (SupportsAwait(executionMethod))
            {
                return description + " " + GetAwaitDescription(executionMethod, awaitMode);
            }

            if (SupportsOrder(executionMethod))
            {
                return description + " " + GetOrderDescription(orderMode);
            }

            return description;
        }

        public static string GetAwaitTooltip(
            CompositeExecutionMethod executionMethod,
            CompositeAwaitMode awaitMode)
        {
            return GetAwaitDescription(executionMethod, awaitMode);
        }

        public static string GetOrderTooltip(
            CompositeExecutionMethod executionMethod,
            CompositeOrderMode orderMode)
        {
            return GetExecutionDescription(executionMethod) + " " + GetOrderDescription(orderMode);
        }

        private static string GetExecutionDescription(CompositeExecutionMethod executionMethod)
        {
            switch (executionMethod)
            {
                case CompositeExecutionMethod.Sequence:
                    return "Runs tasks one at a time and fails when a task fails.";
                case CompositeExecutionMethod.Parallel:
                    return "Starts every task together; all tasks must succeed.";
                case CompositeExecutionMethod.Selector:
                    return "Runs tasks one at a time until one succeeds.";
                case CompositeExecutionMethod.ParallelSelector:
                    return "Starts every task together; at least one task must succeed.";
                case CompositeExecutionMethod.UtilitySelector:
                    return "Runs the eligible task with the highest utility and reevaluates while it runs.";
                default:
                    return "Controls how child tasks execute.";
            }
        }

        private static string GetAwaitDescription(
            CompositeExecutionMethod executionMethod,
            CompositeAwaitMode awaitMode)
        {
            switch (awaitMode)
            {
                case CompositeAwaitMode.WaitAll:
                    return executionMethod == CompositeExecutionMethod.Parallel
                        ? "Wait All completes after every task and fails if any task failed."
                        : "Wait All completes after every task and succeeds if any task succeeded.";
                case CompositeAwaitMode.WaitAny:
                    return "Wait Any returns the first completed task's status; remaining tasks continue in the background.";
                case CompositeAwaitMode.WaitNone:
                    return "Wait None returns success immediately after launch; all tasks continue in the background.";
                default:
                    return "Controls when the composite returns after starting parallel tasks.";
            }
        }

        private static string GetOrderDescription(CompositeOrderMode orderMode)
        {
            switch (orderMode)
            {
                case CompositeOrderMode.Ordered:
                    return "Ordered preserves the list order.";
                case CompositeOrderMode.Random:
                    return "Random builds a new weighted order without repeating a task; Weight is relative from 0 to 100%.";
                case CompositeOrderMode.Shuffle:
                    return "Shuffle builds a uniform random order without using weights.";
                default:
                    return "Controls the order used for this execution.";
            }
        }
    }
}
