using GearEngine.CarSimulation.Definitions;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    /// <summary>
    /// Authoring asset for default <see cref="RaceSessionConfig"/> used when starting a campaign race.
    /// </summary>
    [CreateAssetMenu(fileName = "RaceSessionDefaults", menuName = "GearEngine/Campaign/Race Session Defaults")]
    public sealed class RaceSessionDefaultsSO : ScriptableObject
    {
        [SerializeField]
        private RaceSessionConfig template = new RaceSessionConfig();

        public RaceSessionConfig Template => template;
    }
}
