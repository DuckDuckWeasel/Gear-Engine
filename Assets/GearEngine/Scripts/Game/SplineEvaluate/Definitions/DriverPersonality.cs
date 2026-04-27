using System;
using UnityEngine;

namespace GearEngine.SplineEvaluate.Definitions
{
    /// <summary>
    /// Per-car personality that controls how aggressively/conservatively the car
    /// follows the spline. Each stat ranges from 0 (conservative) to 10 (aggressive).
    /// The 5 stats are blended through <see cref="LaneProfile"/> curves to produce
    /// the final lateral offset from the centerline at every point on the track.
    /// </summary>
    [Serializable]
    public struct DriverPersonality
    {
        [Range(0f, 10f), Tooltip("How early and deep the car cuts into corners (inside offset at apex).")]
        public float Aggression;

        [Range(0f, 10f), Tooltip("How wide the exit line is through corners (outside offset post-apex, simulates controlled oversteer).")]
        public float DriftTendency;

        [Range(0f, 10f), Tooltip("General lane variation amplitude on straights (Perlin-based wandering).")]
        public float LineWidth;

        [Range(0f, 10f), Tooltip("Reduces random variation. 10 = robotic precision, 0 = human-like inconsistency.")]
        public float Consistency;

        [Range(0f, 10f), Tooltip("Late braking + tighter entry. Affects both speed model (shorter lookahead) and lateral entry offset.")]
        public float Risk;

        /// <summary>Default middle-of-the-road personality (all stats at 5).</summary>
        public static DriverPersonality Default => new DriverPersonality
        {
            Aggression = 5f,
            DriftTendency = 5f,
            LineWidth = 5f,
            Consistency = 5f,
            Risk = 5f
        };
    }
}
