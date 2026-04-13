using UnityEngine;

namespace GearEngine.GearEngine.Editor
{
    internal readonly struct GearSprites
    {
        public GearSprites(Sprite baseSpr, Sprite coreSpr, Sprite fallback, Sprite rock, Sprite score, Sprite speed)
        {
            Base = baseSpr;
            Core = coreSpr;
            Fallback = fallback;
            Rock = rock;
            Score = score;
            Speed = speed;
        }

        public readonly Sprite Base;
        public readonly Sprite Core;
        public readonly Sprite Fallback;
        public readonly Sprite Rock;
        public readonly Sprite Score;
        public readonly Sprite Speed;
    }
}
