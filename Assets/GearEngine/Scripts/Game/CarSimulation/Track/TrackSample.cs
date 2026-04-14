using UnityEngine;

namespace GearEngine.CarSimulation.Track
{
    public readonly struct TrackSample
    {
        public TrackSample(float distance, float normalizedT, Vector3 position, Vector3 forward, Vector3 up, float curvature, float signedCurvature)
        {
            Distance = distance;
            NormalizedT = normalizedT;
            Position = position;
            Forward = forward;
            Up = up;
            Curvature = curvature;
            SignedCurvature = signedCurvature;
        }

        public float Distance { get; }
        public float NormalizedT { get; }
        public Vector3 Position { get; }
        public Vector3 Forward { get; }
        public Vector3 Up { get; }
        public float Curvature { get; }
        public float SignedCurvature { get; }
    }
}
