using GearEngine.GearEngine;
using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Abilities;
using GearEngine.GearEngine.Nodes;

namespace GearEngine.Campaign.Gear
{
    /// <summary>
    /// Base class for gears that apply dynamic active buffs to the CarEntity during the race loop.
    /// Initialized automatically by ActiveRaceViewModel or CarTrackTestViewModel when the race starts.
    /// </summary>
    public abstract class ActiveRaceGearAbilitySO : GearAbilitySO
    {
        protected RaceState RaceContext { get; private set; }
        private readonly System.Collections.Generic.Dictionary<IGridNode, System.Collections.Generic.List<(Scaffold.Entities.VariableSO, Scaffold.Entities.ModifierId)>> nodeModifiers = new System.Collections.Generic.Dictionary<IGridNode, System.Collections.Generic.List<(Scaffold.Entities.VariableSO, Scaffold.Entities.ModifierId)>>();
        private readonly System.Collections.Generic.Dictionary<(Scaffold.Entities.VariableSO, Scaffold.Entities.ModifierId), float> temporaryModifiers = new System.Collections.Generic.Dictionary<(Scaffold.Entities.VariableSO, Scaffold.Entities.ModifierId), float>();

        protected IGearEngineService GearEngineContext { get; private set; }

        public virtual void Initialize(RaceState state, IGearEngineService gearEngine)
        {
            RaceContext = state;
            GearEngineContext = gearEngine;
            nodeModifiers.Clear();
            temporaryModifiers.Clear();
        }

        protected void ApplyModifier(IGridNode owner, Scaffold.Entities.VariableSO variable, float value, float duration = -1f)
        {
            if (RaceContext?.Car == null || variable == null) return;
            
            var modId = RaceContext.Car.AddModifier(variable, new Scaffold.Entities.FloatAddModifier(value));
            var entry = (variable, modId);

            if (!nodeModifiers.ContainsKey(owner))
            {
                nodeModifiers[owner] = new System.Collections.Generic.List<(Scaffold.Entities.VariableSO, Scaffold.Entities.ModifierId)>();
            }
            nodeModifiers[owner].Add(entry);

            if (duration > 0f)
            {
                temporaryModifiers[entry] = duration;
            }
        }

        public override void Tick(IGridNode owner, float deltaTime)
        {
            if (RaceContext?.Car == null || temporaryModifiers.Count == 0 || !nodeModifiers.ContainsKey(owner)) return;

            var modsForNode = nodeModifiers[owner];
            for (int i = modsForNode.Count - 1; i >= 0; i--)
            {
                var mod = modsForNode[i];
                if (temporaryModifiers.TryGetValue(mod, out float timeLeft))
                {
                    timeLeft -= deltaTime;
                    if (timeLeft <= 0f)
                    {
                        RaceContext.Car.RemoveModifier(mod.Item1, mod.Item2);
                        temporaryModifiers.Remove(mod);
                        modsForNode.RemoveAt(i);
                    }
                    else
                    {
                        temporaryModifiers[mod] = timeLeft;
                    }
                }
            }
        }

        public override void OnDeactive(IGridNode owner)
        {
            base.OnDeactive(owner);
            if (RaceContext?.Car != null && nodeModifiers.TryGetValue(owner, out var activeModifiers))
            {
                foreach (var mod in activeModifiers)
                {
                    RaceContext.Car.RemoveModifier(mod.Item1, mod.Item2);
                    temporaryModifiers.Remove(mod);
                }
                nodeModifiers.Remove(owner);
            }
        }

        public override void Execute(IGridNode owner)
        {
            // By default does nothing, derived classes implement dynamic Execute logic
            // e.g. checking race laps and applying temporary buffs using ApplyModifier(...)
        }
    }
}
