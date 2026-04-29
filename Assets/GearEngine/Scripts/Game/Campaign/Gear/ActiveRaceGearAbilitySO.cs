using System.Collections.Generic;
using GearEngine.GearEngine;
using GearEngine.CarSimulation.Simulation;
using GearEngine.GearEngine.Abilities;
using GearEngine.GearEngine.Nodes;
using Scaffold.Entities;

namespace GearEngine.Campaign.Gear
{
    /// <summary>
    /// Base class for gears that apply dynamic active buffs to the CarEntity during the race loop.
    /// Initialized automatically by ActiveRaceViewModel or CarTrackTestViewModel when the race starts.
    /// </summary>
    public abstract class ActiveRaceGearAbilitySO : GearAbilitySO
    {
        protected RaceState RaceContext { get; private set; }

        private readonly Dictionary<IGridNode, List<AppliedRaceModifier>> nodeModifiers =
            new Dictionary<IGridNode, List<AppliedRaceModifier>>();

        private readonly Dictionary<ModifierId, float> temporaryModifiers = new Dictionary<ModifierId, float>();

        protected IGearEngineService GearEngineContext { get; private set; }

        private readonly struct AppliedRaceModifier
        {
            public AppliedRaceModifier(Variable key, ModifierId id)
            {
                Key = key;
                Id = id;
            }

            public Variable Key { get; }
            public ModifierId Id { get; }
        }

        public virtual void Initialize(RaceState state, IGearEngineService gearEngine)
        {
            RaceContext = state;
            GearEngineContext = gearEngine;
            nodeModifiers.Clear();
            temporaryModifiers.Clear();
        }

        protected void ApplyModifier(IGridNode owner, VariableSO variable, float value, float duration = -1f)
        {
            if (RaceContext?.Car == null || variable == null)
            {
                return;
            }

            var entry = new EntityModifierEntry(variable, new FloatAddModifier(value));
            ModifierId modifierId = RaceContext.Car.AddModifier(entry);

            if (!nodeModifiers.TryGetValue(owner, out List<AppliedRaceModifier>? list))
            {
                list = new List<AppliedRaceModifier>();
                nodeModifiers[owner] = list;
            }

            list.Add(new AppliedRaceModifier(variable, modifierId));

            if (duration > 0f)
            {
                temporaryModifiers[modifierId] = duration;
            }
        }

        public override void Tick(IGridNode owner, float deltaTime)
        {
            if (RaceContext?.Car == null || temporaryModifiers.Count == 0 || !nodeModifiers.TryGetValue(owner, out List<AppliedRaceModifier>? modsForNode))
            {
                return;
            }

            for (int i = modsForNode.Count - 1; i >= 0; i--)
            {
                AppliedRaceModifier applied = modsForNode[i];
                if (temporaryModifiers.TryGetValue(applied.Id, out float timeLeft))
                {
                    timeLeft -= deltaTime;
                    if (timeLeft <= 0f)
                    {
                        RaceContext.Car.RemoveModifier(applied.Key, applied.Id);
                        temporaryModifiers.Remove(applied.Id);
                        modsForNode.RemoveAt(i);
                    }
                    else
                    {
                        temporaryModifiers[applied.Id] = timeLeft;
                    }
                }
            }
        }

        public override void OnDeactive(IGridNode owner)
        {
            base.OnDeactive(owner);
            if (RaceContext?.Car != null && nodeModifiers.TryGetValue(owner, out List<AppliedRaceModifier>? activeModifiers))
            {
                foreach (AppliedRaceModifier applied in activeModifiers)
                {
                    RaceContext.Car.RemoveModifier(applied.Key, applied.Id);
                    temporaryModifiers.Remove(applied.Id);
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
