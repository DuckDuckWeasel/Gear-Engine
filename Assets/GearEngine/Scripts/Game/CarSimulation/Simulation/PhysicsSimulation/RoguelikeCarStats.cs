using System;
using UnityEngine;

namespace GearEngine.CarSimulation.PhysicsSimulation
{
    [Serializable]
    public struct RoguelikeCarStats
    {
        [Range(0f, 100f), Tooltip("100 = max speed, 0 = slow.")]
        public float SpeedCapability;

        [Range(0f, 100f), Tooltip("100 = perfect racing line, 0 = invalid lines.")]
        public float CorneringSkill;

        [Range(0f, 100f), Tooltip("100 = always drifts, 0 = perfect grip.")]
        public float Drift;

        [Range(0f, 100f), Tooltip("100 = precise, 0 = hugs wrong edge.")]
        public float Precision;

        [Range(0f, 100f), Tooltip("100 = smooth ride, 0 = bounces heavily.")]
        public float Smoothness;

        public static RoguelikeCarStats Default => new RoguelikeCarStats
        {
            SpeedCapability = 50f,
            CorneringSkill = 50f,
            Drift = 50f,
            Precision = 50f,
            Smoothness = 50f
        };
    }
}
