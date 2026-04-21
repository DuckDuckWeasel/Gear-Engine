using System;
using UnityEngine;

namespace GearEngine.GearEngine
{
    [Serializable]
    public sealed class GearEngineStartData
    {
        [SerializeField] private GearBoardLoadoutData boardLoadout = new GearBoardLoadoutData();

        public GearBoardLoadoutData BoardLoadout => boardLoadout;

        public GearBoardLoadoutData GetBoardLoadoutData() => boardLoadout;
    }
}
