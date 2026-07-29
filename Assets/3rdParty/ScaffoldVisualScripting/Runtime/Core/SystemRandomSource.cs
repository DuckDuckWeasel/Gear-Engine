using System;

namespace Scaffold.VisualScripting
{
    public sealed class SystemRandomSource : IRandomSource
    {
        public SystemRandomSource() : this(new Random())
        {
        }

        public SystemRandomSource(int seed) : this(new Random(seed))
        {
        }

        private SystemRandomSource(Random random)
        {
            this.random = random;
        }

        private readonly Random random;

        public float NextValue()
        {
            return (float)random.NextDouble();
        }
    }
}
