using UnityEngine;

namespace Scaffold.GearEngine.Config
{
    public static class TriggerPatternExtensions
    {
        private static readonly Vector2Int[] fourWayDirs = new[]
        {
            Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
        };

        private static readonly Vector2Int[] eightWayDirs = new[]
        {
            Vector2Int.up, new Vector2Int(1, 1), Vector2Int.right, new Vector2Int(1, -1),
            Vector2Int.down, new Vector2Int(-1, -1), Vector2Int.left, new Vector2Int(-1, 1)
        };

        public static Vector2Int[] GetDirections(this TriggerPattern pattern)
        {
            return pattern == TriggerPattern.EightWay ? eightWayDirs : fourWayDirs;
        }
    }
}
