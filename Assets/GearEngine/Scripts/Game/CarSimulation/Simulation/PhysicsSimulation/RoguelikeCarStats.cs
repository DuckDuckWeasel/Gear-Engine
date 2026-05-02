using System;
using UnityEngine;

namespace GearEngine.CarSimulation.PhysicsSimulation
{
    [Serializable]
    public struct RoguelikeCarStats
    {
        [Range(0f, 10f), Tooltip("10 = max speed, 0 = slow.")]
        public float SpeedCapability;

        [Range(0f, 10f), Tooltip("10 = perfect racing line, 0 = invalid lines.")]
        public float CorneringSkill;

        [Range(0f, 10f), Tooltip("10 = always drifts, 0 = perfect grip.")]
        public float Drift;

        [Range(0f, 10f), Tooltip("10 = precise, 0 = hugs wrong edge.")]
        public float Precision;

        [Range(0f, 10f), Tooltip("10 = smooth ride, 0 = bounces heavily.")]
        public float Smoothness;

        public static RoguelikeCarStats Default => new RoguelikeCarStats
        {
            SpeedCapability = 5f,
            CorneringSkill = 5f,
            Drift = 5f,
            Precision = 5f,
            Smoothness = 5f
        };
    }
}
