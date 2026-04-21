using GearEngine.CarSimulation.Definitions;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Abilities;
using GearEngine.GearEngine.Nodes;

namespace GearEngine.Campaign.Gear
{
    /// <summary>
    /// Base class for gears that permanently alter RoguelikeCarStats (out-of-race modifiers).
    /// </summary>
    public abstract class PassiveRaceGearAbilitySO : GearAbilitySO
    {
        public override void Execute(IGridNode owner)
        {
            // Passive gears typically do not trigger runtime execute mechanics.
            // They just provide baseline stat bonuses.
        }

        /// <summary>
        /// Modifies the provided roguelike stats directly. 
        /// Automatically called during Base Stats recalculation.
        /// </summary>
        public abstract void ApplyPassiveStats(ref RoguelikeCarStats stats, IGridNode owner, IGearEngineService engine);
    }
}
