using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Config;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    /// <summary>
    /// Gear cards offered in the roguelike roll flow. Separate from the track <see cref="TrackAssetIndex"/> / track labels so track routing
    /// and roguelike pool stay independent systems.
    /// </summary>
    [CreateAssetMenu(fileName = "RoguelikeGearPool", menuName = "GearEngine/Campaign/Roguelike Gear Pool")]
    public sealed class RoguelikeGearPoolSO : ScriptableObject
    {
        [SerializeField]
        private GearConfig[] gears = Array.Empty<GearConfig>();

        /// <summary>
        /// Replaces pool data at runtime (e.g. from tests).
        /// </summary>
        public void SetRuntimeEntries(GearConfig[] gearConfigs)
        {
            gears = gearConfigs != null ? gearConfigs : Array.Empty<GearConfig>();
        }

        public IReadOnlyList<GearConfig> GetRoguelikeGearOptions()
        {
            return gears ?? Array.Empty<GearConfig>();
        }
    }
}
