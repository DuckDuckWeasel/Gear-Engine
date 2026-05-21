using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Config;
using GearEngine.Core.Config;
using UnityEngine;

namespace GearEngine.Campaign.Services
{
    /// <summary>
    /// Gear perks offered in the roguelike roll flow. Separate from the track <see cref="TrackAssetIndex"/> / track labels so track routing
    /// and roguelike pool stay independent systems.
    /// </summary>
    [CreateAssetMenu(fileName = "RoguelikeGearPool", menuName = "GearEngine/Campaign/Roguelike Gear Pool")]
    public sealed class RoguelikeGearPoolSO : BaseCatalogSO<GearItem>
    {
        protected override string GetId(GearItem item)
        {
            return item?.Id;
        }

        public IReadOnlyList<GearItem> GetRoguelikeGearOptions()
        {
            return All;
        }
    }
}
