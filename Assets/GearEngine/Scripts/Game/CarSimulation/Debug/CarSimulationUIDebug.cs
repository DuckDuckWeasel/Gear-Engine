using UnityEngine;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.Definitions;
using Scaffold.Entities;

namespace GearEngine.CarSimulation.Debug
{
    /// <summary>
    /// Classe de debug visual em tempo real (OnGUI) para manipular valores da CarEntity 
    /// e forçar comandos manuais como freio e drift.
    /// </summary>
    public sealed class CarSimulationUIDebug : MonoBehaviour
    {
        [Header("Target Configuration")]
        [SerializeField] private VariableSO targetAttribute;
        
        [Header("Runtime Bindings")]
        [SerializeField] private CarSimulationDebug simulationDebug;

        private SplineCarRunnerContext Context => simulationDebug?.Context;

        private bool forceBrake = false;
        private bool forceHandbrake = false;

        private void Reset()
        {
            if (simulationDebug == null)
            {
                simulationDebug = GetComponent<CarSimulationDebug>();
            }
        }

        private void Awake()
        {
            if (simulationDebug == null)
            {
                simulationDebug = GetComponent<CarSimulationDebug>();
            }
        }

        private void OnGUI()
        {
            if (simulationDebug == null || simulationDebug.Session == null)
            {
                GUI.Label(new Rect(10, 10, 300, 30), "Car UIDebug: Aguardando Corrida...");
                return;
            }

            var ctx = Context;
            if (ctx == null) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 500), GUI.skin.box);
            GUILayout.Label("<b>CAR SIMULATION UI DEBUG</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(5);

            GUILayout.Label("<b>NATIVE ENTITY VARIABLES</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
            
            if (ctx.Variables != null && ctx.entity != null)
            {
                DrawNativeStat(ctx, ctx.Variables.Speed, "Speed");
                DrawNativeStat(ctx, ctx.Variables.Acceleration, "Acceleration");
                DrawNativeStat(ctx, ctx.Variables.Handling, "Handling");
                DrawNativeStat(ctx, ctx.Variables.Stability, "Stability");
                DrawNativeStat(ctx, ctx.Variables.Recovery, "Recovery");
                DrawNativeStat(ctx, ctx.Variables.DriftPenalty, "Drift Penalty");
            }
            else
            {
                GUILayout.Label("Nenhum VariableSet configurado no contexto.", new GUIStyle(GUI.skin.label) { normal = new GUIStyleState { textColor = Color.yellow }});
            }

            GUILayout.Space(15);
            GUILayout.Label("<b>CONTROLES DE MANUAL OVERRIDE</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });

            GUILayout.EndArea();
        }

        private void DrawNativeStat(SplineCarRunnerContext ctx, VariableSO variable, string label)
        {
            if (variable == null) return;
            
            ctx.entity.TryGetValue(variable, out float currentValue);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {currentValue:F1}", GUILayout.Width(130));
            
            if (GUILayout.Button("- 10", GUILayout.Width(50)))
            {
                ctx.entity.AddModifier(new EntityModifierEntry(variable, new FloatVariableValue { Value = -10f }));
            }
            if (GUILayout.Button("+ 10", GUILayout.Width(50)))
            {
                ctx.entity.AddModifier(new EntityModifierEntry(variable, new FloatVariableValue { Value = 10f }));
            }
            GUILayout.EndHorizontal();
        }
    }
}
