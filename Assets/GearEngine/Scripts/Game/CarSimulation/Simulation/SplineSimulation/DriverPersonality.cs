using System;
using GearEngine.CarSimulation.PhysicsSimulation;
using UnityEngine;

namespace GearEngine.CarSimulation.SplineSimulation
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
        [Range(0f, 100f), Tooltip("100 = max speed (200 km/h), 0 = slow (10 km/h).")]
        public float SpeedCapability;

        [Range(0f, 100f), Tooltip("100 = always take the perfect racing line, 0 = always take invalid lines.")]
        public float CorneringSkill;

        [Range(0f, 100f), Tooltip("100 = always drifts in curves, 0 = perfect grip (no drift).")]
        public float Drift;

        [Range(0f, 100f), Tooltip("100 = extremely precise even on bad lines, 0 = hugs the absolute wrong edge of the track.")]
        public float Precision;

        [Range(0f, 100f), Tooltip("100 = perfectly smooth ride, 0 = weaves on straights and suspension bounces heavily.")]
        public float Smoothness;

        /// <summary>Default middle-of-the-road personality.</summary>
        public static DriverPersonality Default => new DriverPersonality
        {
            SpeedCapability = 50f,
            CorneringSkill = 50f,
            Drift = 50f,
            Precision = 50f,
            Smoothness = 50f
        };

        public static DriverPersonality FromStats(RoguelikeCarStats stats)
        {
            return new DriverPersonality
            {
                SpeedCapability = Mathf.Clamp(stats.SpeedCapability, 0f, 100f),
                CorneringSkill = Mathf.Clamp(stats.CorneringSkill, 0f, 100f),
                Drift = Mathf.Clamp(stats.Drift, 0f, 100f),
                Precision = Mathf.Clamp(stats.Precision, 0f, 100f),
                Smoothness = Mathf.Clamp(stats.Smoothness, 0f, 100f)
            };
        }

        public static DriverPersonality CreateOpponent(int index, int seed)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            System.Random random = new System.Random(seed);
            return new DriverPersonality
            {
                SpeedCapability = Mathf.Clamp(42f + index * 8f + random.Next(-3, 4), 0f, 100f),
                CorneringSkill = 50f + random.Next(-10, 11),
                Drift = 50f + random.Next(-10, 11),
                Precision = 50f + random.Next(-10, 11),
                Smoothness = 50f + random.Next(-10, 11)
            };
        }
    }
}
