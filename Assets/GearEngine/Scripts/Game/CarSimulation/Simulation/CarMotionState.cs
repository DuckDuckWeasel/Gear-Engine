using UnityEngine;

namespace GearEngine.CarSimulation.Simulation
{
    internal sealed class CarMotionState
    {
        public Vector3 Position;

        public float YawDegrees;

        public int WaypointIndex;

        public float DistanceAlongPath;

        public float Speed;

        public float SlipAngle;

        public float DriftIntensity;

        public float PendingSpeedBoost;

        public void Reset()
        {
            Position = Vector3.zero;
            YawDegrees = 0f;
            WaypointIndex = 0;
            DistanceAlongPath = 0f;
            Speed = 0f;
            SlipAngle = 0f;
            DriftIntensity = 0f;
            PendingSpeedBoost = 0f;
        }
    }
}
