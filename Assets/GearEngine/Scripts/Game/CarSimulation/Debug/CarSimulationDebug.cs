using GearEngine.CarSimulation;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using Scaffold.Entities;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.CarSimulation.Debug
{
    [ExecuteAlways]
    public sealed class CarSimulationDebug : MonoBehaviour, IInitializable
    {
        private TrackSimulation Sim => runner?.ActiveSimulation;

        private CarEntity Car => Sim?.Car;

        private CarMotionState Motion => Sim?.Motion;

        private RaceRuntimeState Race => Sim?.Race;

        [ShowInInspector, ReadOnly, BoxGroup("Race")]
        private float CurrentSpeed => Race?.CurrentSpeed ?? 0f;

        [ShowInInspector, ReadOnly, BoxGroup("Race")]
        private float Progress01 => Race?.Progress01 ?? 0f;

        [ShowInInspector, ReadOnly, BoxGroup("Race")]
        private int CurrentLap => Race?.CurrentLap ?? 0;

        [ShowInInspector, ReadOnly, BoxGroup("Race")]
        private bool IsDrifting => Race?.IsDrifting ?? false;

        [ShowInInspector, ReadOnly, BoxGroup("Motion")]
        private float RawSpeed => Motion?.Speed ?? 0f;

        [ShowInInspector, ReadOnly, BoxGroup("Motion")]
        private float DriftIntensity => Motion?.DriftIntensity ?? 0f;

        [ShowInInspector, ReadOnly, BoxGroup("Motion")]
        private int WaypointIndex => Motion?.WaypointIndex ?? 0;

        [ShowInInspector, ReadOnly, BoxGroup("Motion")]
        private float DistanceAlongPath => Motion?.DistanceAlongPath ?? 0f;

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

        [SerializeField]
        private VariableSO targetAttribute;

        [SerializeField]
        private float modifierValue;

        private EntityModifierEntry activeModifier;

        [Inject]
        private TrackSimulationRunner runner;

        public void Initialize()
        {
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
    }
}
