using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Definitions
{
    [CreateAssetMenu(menuName = "Game/Track/Track Definition", fileName = "TrackDefinition")]
    public sealed class TrackDefinition : ScriptableObject
    {
        public string TrackName => trackName;

        [SerializeField] private string trackName;

        public Spline Spline => spline;

        [SerializeField] private Spline spline = new Spline();

        [Tooltip("Lap count required to finish a race on this track (closed splines only).")]
        [SerializeField]
        [Min(1)]
        private int totalLaps = 3;

        [SerializeField]
        private RaceScoreBracket[] scoreBrackets = new RaceScoreBracket[0];

        public int TotalLaps => totalLaps;

        public RaceScoreBracket[] ScoreBrackets => scoreBrackets;

        private void OnValidate()
        {
            if (totalLaps < 1)
            {
                totalLaps = 1;
            }

            if (scoreBrackets == null)
            {
                return;
            }

            for (int i = 0; i < scoreBrackets.Length; i++)
            {
                if (scoreBrackets[i].TimeToBeatSeconds < 0f)
                {
                    Debug.LogWarning($"[TrackDefinition] '{name}' bracket index {i} has negative timeToBeatSeconds.");
                }

                if (scoreBrackets[i].GoldReward < 0)
                {
                    Debug.LogWarning($"[TrackDefinition] '{name}' bracket index {i} has negative goldReward.");
                }
            }
        }
    }
}
