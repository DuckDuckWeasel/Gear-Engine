using GearEngine.CarSimulation.Definitions;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    /// <summary>
    /// Authoring asset for default roguelike car stats (<see cref="RaceSessionConfig"/>) when starting a campaign race. Lap count comes from <see cref="TrackDefinition.TotalLaps"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "RaceSessionDefaults", menuName = "GearEngine/Campaign/Race Session Defaults")]
    public sealed class RaceSessionDefaultsSO : ScriptableObject
    {
        [SerializeField]
        private RaceSessionConfig template = new RaceSessionConfig();

        public RaceSessionConfig Template => template;
    }
}
