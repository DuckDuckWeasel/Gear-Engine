using System;
using UnityEngine;

namespace GearEngine.Campaign.Presentation
{
    [Serializable]
    public struct BandSlotPrefabConfig
    {
        [Tooltip("Inclusive minimum position")]
        public int MinPosition;
        [Tooltip("Inclusive maximum position")]
        public int MaxPosition;
        public TrackScoreBandSlotView Prefab;

        public bool Contains(int position)
        {
            return position >= MinPosition && position <= MaxPosition;
        }
    }
}
