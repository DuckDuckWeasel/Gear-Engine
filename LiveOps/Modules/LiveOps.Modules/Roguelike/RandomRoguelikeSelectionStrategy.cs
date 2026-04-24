using System;
using System.Collections.Generic;

namespace LiveOps.Modules.Roguelike
{
    public sealed class RandomRoguelikeSelectionStrategy : IRoguelikeSelectionStrategy
    {
        private readonly Random rng;

        public RandomRoguelikeSelectionStrategy()
            : this(new Random())
        {
        }

        public RandomRoguelikeSelectionStrategy(Random rng)
        {
            this.rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        public IReadOnlyList<string> Select(IReadOnlyList<string> pool, int count)
        {
            if (pool == null || pool.Count == 0 || count <= 0)
            {
                return Array.Empty<string>();
            }

            int take = Math.Min(count, pool.Count);
            List<string> bag = new List<string>(pool.Count);
            for (int i = 0; i < pool.Count; i++)
            {
                string id = pool[i];
                if (!string.IsNullOrEmpty(id))
                {
                    bag.Add(id);
                }
            }

            if (bag.Count == 0)
            {
                return Array.Empty<string>();
            }

            take = Math.Min(take, bag.Count);
            List<string> picked = new List<string>(take);
            for (int i = 0; i < take; i++)
            {
                int idx = rng.Next(bag.Count);
                picked.Add(bag[idx]);
                bag.RemoveAt(idx);
            }

            return picked;
        }
    }
}
