using UnityEngine;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Car/Track Simulation Tuning", fileName = "TrackSimulationTuning")]
    public sealed class TrackSimulationTuning : ScriptableObject
    {
        public float GripScale => gripScale;

        public float LookAheadMinMetres => lookAheadMinMetres;

        public float LookAheadSpeedFactor => lookAheadSpeedFactor;

        public float AheadProbeStep => aheadProbeStep;

        public float CurvatureEpsilon => curvatureEpsilon;

        [SerializeField] private float gripScale = 0.12f;

        [SerializeField] private float lookAheadMinMetres = 8f;

        [SerializeField] private float lookAheadSpeedFactor = 0.75f;

        [SerializeField] private float aheadProbeStep = 0.25f;

        [SerializeField] private float curvatureEpsilon = 1e-5f;
    }
}
