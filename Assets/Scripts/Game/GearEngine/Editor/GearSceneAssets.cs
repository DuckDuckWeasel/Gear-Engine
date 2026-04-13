using GearEngine.GearEngine;
using GearEngine.GearEngine.Presentation;
using UnityEngine;

namespace GearEngine.GearEngine.Editor
{
    internal readonly struct GearSceneAssets
    {
        public GearSceneAssets(GearConfig core, GearConfig baseGear, GameObject emptySlot, BoardConfigSO boardConfig, GearInventoryLoadoutSO loadout, string configFolder)
        {
            Core = core;
            BaseGear = baseGear;
            EmptySlot = emptySlot;
            BoardConfig = boardConfig;
            Loadout = loadout;
            ConfigFolder = configFolder;
        }

        public readonly GearConfig Core;
        public readonly GearConfig BaseGear;
        public readonly GameObject EmptySlot;
        public readonly BoardConfigSO BoardConfig;
        public readonly GearInventoryLoadoutSO Loadout;
        public readonly string ConfigFolder;
    }
}
