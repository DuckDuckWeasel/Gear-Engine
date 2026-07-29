using UnityEngine;

namespace Scaffold.VisualScripting.Unity
{
    public sealed class UnityTimeSource : ITimeSource
    {
        public float DeltaTime => Time.deltaTime;

        public double ElapsedSeconds => Time.timeAsDouble;

        public long Frame => Time.frameCount;
    }
}
