using UnityEngine;

namespace GearEngine.CarSimulation.Simulation
{
    public readonly struct CurveSample
    {
        public CurveSample(float curveAmount, float curveDirection, Vector3 position, Vector3 tangent, Vector3 up)
        {
            CurveAmount = curveAmount;
            CurveDirection = curveDirection;
            Position = position;
            Tangent = tangent;
            Up = up;
        }

        public float CurveAmount { get; }

        public float CurveDirection { get; }

        public Vector3 Position { get; }

        public Vector3 Tangent { get; }

        public Vector3 Up { get; }
    }
}
