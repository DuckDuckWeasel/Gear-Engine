using System;
using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [Serializable]
    public struct RoguelikeCarStats
    {
        [Range(0f, 100f), Tooltip("Scales Car Engine Max Speed limit.")]
        public float statTopSpeed;

        [Range(0f, 100f), Tooltip("Scales Engine raw acceleration torque.")]
        public float statAcceleration;

        [Range(0f, 100f), Tooltip("Scales Physical Brake stopping power.")]
        public float statBrakingSystem;

        [Range(0f, 100f), Tooltip("Scales physical drift grip logic.")]
        public float statDriftControl;

        [Range(0f, 100f), Tooltip("Scales Nitrous Oxide explosion power.")]
        public float statNitrousBoost;

        [Range(0f, 100f), Tooltip("How faithfully the AI sticks to the track limits.")]
        public float statSteeringGrip;

        [Range(0f, 100f), Tooltip("AI's ability to take Out-In-Out racing lines.")]
        public float statRacingLine;

        [Range(0f, 100f), Tooltip("AI's courage to brake late and read predictive road chords fast.")]
        public float statDriverReflexes;

        public static RoguelikeCarStats Default => new RoguelikeCarStats
        {
            statTopSpeed = 50f,
            statAcceleration = 50f,
            statBrakingSystem = 50f,
            statDriftControl = 50f,
            statNitrousBoost = 50f,
            statSteeringGrip = 50f,
            statRacingLine = 50f,
            statDriverReflexes = 50f
        };
    }
}
