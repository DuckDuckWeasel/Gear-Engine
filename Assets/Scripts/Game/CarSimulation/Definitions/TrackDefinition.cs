using UnityEngine;
using UnityEngine.Splines;

namespace Game.CarSimulation
{
    [CreateAssetMenu(menuName = "Game/Track/Track Definition", fileName = "TrackDefinition")]
    public sealed class TrackDefinition : ScriptableObject
    {
        [SerializeField] private string trackName;
        [SerializeField] private Spline spline = new Spline();

        public string TrackName => trackName;
        public Spline Spline => spline;
    }
}
