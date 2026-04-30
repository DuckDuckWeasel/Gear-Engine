using GearEngine.CarSimulation;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.Definitions;
using Scaffold.Entities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GearEngine.CarSimulation.Debug
{
    [RequireComponent(typeof(CarSimulationUIDebug))]
    public sealed class CarSimulationDebug : SerializedMonoBehaviour
    {
        [SerializeField]
        private VariableSO targetAttribute;

        [SerializeField]
        private float modifierValue;

        private EntityModifierEntry activeModifier;

        [ShowInInspector, BoxGroup("Race")]
        public RaceState Session { get; private set; }

        private CarEntity Car => Session?.Car;

        [ShowInInspector, BoxGroup("Race")]
        private float CurrentSpeed => Session?.CurrentSpeed ?? 0f;

        [ShowInInspector, BoxGroup("Race")]
        private float NormalizedProgress => Session?.NormalizedProgress ?? 0f;

        [ShowInInspector, BoxGroup("Race")]
        private int CurrentLap => Session?.CurrentLap ?? 0;

        [ShowInInspector, BoxGroup("Race")]
        private float RaceTime => Session?.RaceTime ?? 0f;

        [ShowInInspector, BoxGroup("Race")]
        private SimulationLifecycleState Phase => Session?.Phase ?? SimulationLifecycleState.Created;

        [ShowInInspector, BoxGroup("Attribute")]
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

        private SplineCarRunnerService runner;

        public SplineCarRunnerService Runner => runner;
        public SplineCarRunnerContext Context => runner?.GetDebugContext(Car);

        // [ShowInInspector, BoxGroup("Progression Stats")]
        // public RoguelikeCarStats SourceStats => Context?.sourceStats ?? RoguelikeCarStats.Default;

        [ShowInInspector, BoxGroup("Effective Stats")]
        private float StatTopSpeed => ExtractEntityStat(Context?.Variables?.Speed);
        [ShowInInspector, BoxGroup("Effective Stats")]
        private float StatAcceleration => ExtractEntityStat(Context?.Variables?.Acceleration);
        [ShowInInspector, BoxGroup("Effective Stats")]
        private float StatSteeringGrip => ExtractEntityStat(Context?.Variables?.Handling);
        [ShowInInspector, BoxGroup("Effective Stats")]
        private float StatRacingLine => ExtractEntityStat(Context?.Variables?.Stability);
        [ShowInInspector, BoxGroup("Effective Stats")]
        private float StatDriverReflexes => ExtractEntityStat(Context?.Variables?.Recovery);
        [ShowInInspector, BoxGroup("Effective Stats")]
        private float StatDriftControl => ExtractEntityStat(Context?.Variables?.DriftPenalty);

        private float ExtractEntityStat(VariableSO varSo)
        {
            if (varSo == null || Car == null) return 0f;
            return Car.TryGetValue(varSo, out float v) ? v : 0f;
        }

        [ShowInInspector, BoxGroup("Runtime Mechanics")]
        private float MaxSimulationSpeed => Context?.maxSimulationSpeed ?? 0f;

        [ShowInInspector, BoxGroup("Runtime Mechanics")]
        private float SafeCornerSpeed => Context?.safeCornerSpeed ?? 0f;

        [ShowInInspector, BoxGroup("Runtime Mechanics")]
        private float ArcadeSteerAssist => Context?.arcadeSteerAssist ?? 0f;

        [ShowInInspector, BoxGroup("Runtime Mechanics")]
        private int CalculatedAcceleration => Context?.calculatedAcceleration ?? 0;

        [ShowInInspector, BoxGroup("Runtime Mechanics")]
        private int CalculatedBrakeForce => Context?.calculatedBrakeForce ?? 0;

        [ShowInInspector, BoxGroup("Runtime Mechanics")]
        private int CalculatedDriftGrip => Context?.calculatedDriftGrip ?? 0;

        [ShowInInspector, BoxGroup("Runtime Mechanics")]
        private float CurrentSimulationMultiplier => Context?.currentSimulationMultiplier ?? 0f;

        public void Setup(RaceState session, SplineCarRunnerService runner)
        {
            Session = session;
            this.runner = runner;
        }

        [Button, BoxGroup("Modifiers")]
        private void AddModifier()
        {
            if (Car == null || targetAttribute == null)
            {
                UnityEngine.Debug.LogError("[CarSimulationDebug] Car or target attribute is null.");
                return;
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

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || runner == null || Car == null) return;
            
            if (runner.GetDebugTelemetry(Car, out Vector3[] waypoints, out bool requiresHandbrake, out bool isBrakingForCurve, out Transform carTransform))
            {
                if (waypoints == null || waypoints.Length == 0 || carTransform == null) return;

                Gizmos.color = Color.magenta;
                for (int i = 0; i < waypoints.Length; i++) {
                    Gizmos.DrawWireSphere(waypoints[i], 1f);
                    if (i == 0) {
                        Gizmos.DrawLine(carTransform.position, waypoints[i]);
                    } else {
                        Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
                    }
                }

                Gizmos.color = requiresHandbrake ? Color.red : (isBrakingForCurve ? new Color(1f, 0.5f, 0f) : Color.green);
                Vector3 dir = carTransform.forward * CurrentSpeed;
                Gizmos.DrawRay(carTransform.position, dir * 0.2f);
            }
        }
    }
}
