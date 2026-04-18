using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Track/Track Definition", fileName = "TrackDefinition")]
    public sealed class TrackDefinition : ScriptableObject
    {
        public string TrackName => trackName;

        [SerializeField] private string trackName;

        public int TotalLaps => totalLaps;

        [SerializeField] private int totalLaps = 3;

        public float TimeToBeatSeconds => timeToBeatSeconds;

        [SerializeField] private float timeToBeatSeconds = 60f;

        public Spline Spline => spline;

        [SerializeField] private Spline spline = new Spline();

        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(trackName) ? name : trackName;
        }

        internal void SetTotalLapsForTests(int value)
        {
            totalLaps = value;
        }
    }
}
