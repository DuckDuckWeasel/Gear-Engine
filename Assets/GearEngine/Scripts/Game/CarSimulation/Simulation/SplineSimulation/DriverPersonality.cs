using System;
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
        [Range(0f, 10f), Tooltip("10 = max speed (200 km/h), 0 = slow (10 km/h).")]
        public float SpeedCapability;

        [Range(0f, 10f), Tooltip("10 = always take the perfect racing line, 0 = always take invalid lines.")]
        public float CorneringSkill;

        [Range(0f, 10f), Tooltip("10 = always drifts in curves, 0 = perfect grip (no drift).")]
        public float Drift;

        [Range(0f, 10f), Tooltip("10 = extremely precise even on bad lines, 0 = hugs the absolute wrong edge of the track.")]
        public float Precision;

        [Range(0f, 10f), Tooltip("10 = perfectly smooth ride, 0 = weaves on straights and suspension bounces heavily.")]
        public float Smoothness;

        /// <summary>Default middle-of-the-road personality.</summary>
        public static DriverPersonality Default => new DriverPersonality
        {
            SpeedCapability = 5f,
            CorneringSkill = 5f,
            Drift = 5f,
            Precision = 5f,
            Smoothness = 5f
        };
    }
}
