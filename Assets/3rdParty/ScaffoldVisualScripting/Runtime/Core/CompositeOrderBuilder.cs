using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scaffold.VisualScripting
{
    internal sealed class CompositeOrderBuilder
    {
        public CompositeOrderBuilder(IReadOnlyList<ICompositeTask> tasks, Func<float> getRandomValue)
        {
            this.tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
            this.getRandomValue = getRandomValue ?? throw new ArgumentNullException(nameof(getRandomValue));
        }

        private readonly IReadOnlyList<ICompositeTask> tasks;
        private readonly Func<float> getRandomValue;

        public List<int> Build(ActionListExecutionMethod method, ActionListOrderMode orderMode)
        {
            List<int> order = CreateOrderedIndexes();
            if (!CompositeExecutionDescription.SupportsOrder(method) || orderMode == ActionListOrderMode.Ordered)
            {
                return order;
            }

            return orderMode == ActionListOrderMode.Shuffle ? Shuffle(order) : BuildWeighted(order);
        }

        private List<int> CreateOrderedIndexes()
        {
            List<int> order = new List<int>();
            for (int index = 0; index < tasks.Count; index++)
            {
                order.Add(index);
            }

            return order;
        }

        private List<int> Shuffle(List<int> order)
        {
            for (int sourceIndex = order.Count - 1; sourceIndex > 0; sourceIndex--)
            {
                int destinationIndex = GetRandomIndex(sourceIndex + 1);
                int value = order[sourceIndex];
                order[sourceIndex] = order[destinationIndex];
                order[destinationIndex] = value;
            }

            return order;
        }

        private List<int> BuildWeighted(List<int> remaining)
        {
            List<int> order = new List<int>();
            while (remaining.Count > 0)
            {
                int selectedIndex = SelectWeightedIndex(remaining);
                order.Add(remaining[selectedIndex]);
                remaining.RemoveAt(selectedIndex);
            }

            return order;
        }

        private int SelectWeightedIndex(IReadOnlyList<int> indexes)
        {
            float totalWeight = GetTotalWeight(indexes);
            if (totalWeight <= 0f)
            {
                return GetRandomIndex(indexes.Count);
            }

            return FindTargetWeightIndex(indexes, totalWeight);
        }

        private float GetTotalWeight(IReadOnlyList<int> indexes)
        {
            float total = 0f;
            for (int index = 0; index < indexes.Count; index++)
            {
                total += GetEffectiveWeight(indexes[index]);
            }

            return total;
        }

        private int FindTargetWeightIndex(IReadOnlyList<int> indexes, float totalWeight)
        {
            float target = GetClampedRandomValue() * totalWeight;
            float cumulative = 0f;
            for (int index = 0; index < indexes.Count; index++)
            {
                cumulative += GetEffectiveWeight(indexes[index]);
                if (target < cumulative)
                {
                    return index;
                }
            }

            return indexes.Count - 1;
        }

        private float GetEffectiveWeight(int taskIndex)
        {
            GetWeightBalance(out float overrideTotal, out int automaticCount);
            ICompositeTask task = tasks[taskIndex];
            if (overrideTotal >= 100f)
            {
                return task.HasWeightOverride && overrideTotal > 0f ? task.Weight / overrideTotal * 100f : 0f;
            }

            return task.HasWeightOverride ? task.Weight : GetAutomaticWeight(overrideTotal, automaticCount);
        }

        private void GetWeightBalance(out float overrideTotal, out int automaticCount)
        {
            overrideTotal = 0f;
            automaticCount = 0;
            foreach (ICompositeTask task in tasks)
            {
                AddTaskWeight(task, ref overrideTotal, ref automaticCount);
            }
        }

        private void AddTaskWeight(ICompositeTask task, ref float overrideTotal, ref int automaticCount)
        {
            if (task == null || !task.IsEnabled)
            {
                return;
            }

            if (task.HasWeightOverride)
            {
                overrideTotal += Mathf.Clamp(task.Weight, 0f, 100f);
                return;
            }

            automaticCount++;
        }

        private float GetAutomaticWeight(float overrideTotal, int automaticCount)
        {
            return automaticCount > 0 ? (100f - overrideTotal) / automaticCount : 0f;
        }

        private int GetRandomIndex(int count)
        {
            return count <= 1 ? 0 : Mathf.Min(Mathf.FloorToInt(GetClampedRandomValue() * count), count - 1);
        }

        private float GetClampedRandomValue()
        {
            return Mathf.Clamp(getRandomValue(), 0f, 0.999999f);
        }
    }
}
