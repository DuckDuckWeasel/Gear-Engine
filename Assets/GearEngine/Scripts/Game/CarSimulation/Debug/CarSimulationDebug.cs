using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using Scaffold.Entities;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Debug
{
    [ExecuteAlways]
    public sealed class CarSimulationDebug : MonoBehaviour, IInitializable
    {
        [SerializeField]
        private VariableSO targetAttribute;

        [SerializeField]
        private float modifierValue;

        private EntityModifierEntry activeModifier;

        [Inject]
        private IRaceSessionRunner runner;

        private LapRaceSession Session => runner?.ActiveSession;

        private CarEntity Car => Session?.Car;

        [ShowInInspector, ReadOnly, BoxGroup("Race")]
        private float CurrentSpeed => Session?.CurrentSpeed ?? 0f;

        [ShowInInspector, ReadOnly, BoxGroup("Race")]
        private float NormalizedProgress => Session?.NormalizedProgress ?? 0f;

        [ShowInInspector, ReadOnly, BoxGroup("Race")]
        private int CurrentLap => Session?.CurrentLap ?? 0;

        [ShowInInspector, ReadOnly, BoxGroup("Race")]
        private float RaceTime => Session?.RaceTime ?? 0f;

        [ShowInInspector, ReadOnly, BoxGroup("Race")]
        private SimulationLifecycleState Phase => Session?.Phase ?? SimulationLifecycleState.Created;

        [ShowInInspector, ReadOnly, BoxGroup("Attribute")]
        private float CurrentAttributeValue
        {
            get
            {
                if (Car == null || targetAttribute == null)
                {
                    return 0f;
                }

                return Car.TryGetValue(targetAttribute, out float v) ? v : 0f;
            }
        }

        [Button, BoxGroup("Modifiers")]
        private void AddModifier()
        {
            if (Car == null || targetAttribute == null)
            {
                throw new System.Exception("[CarSimulationDebug] Car or target attribute is null.");
            }

            activeModifier = new EntityModifierEntry(targetAttribute, new FloatVariableValue { Value = modifierValue });
            Car.AddModifier(activeModifier);
        }

        [Button, BoxGroup("Modifiers")]
        private void RemoveModifier()
        {
            if (Car == null || activeModifier == null)
            {
                return;
            }

            Car.RemoveModifier(activeModifier);
            activeModifier = null;
        }

        [Button, BoxGroup("Modifiers")]
        private void ClearAllModifiers()
        {
            Car?.ClearModifiers();
            activeModifier = null;
        }

        public void Initialize()
        {
        }
    }
}
