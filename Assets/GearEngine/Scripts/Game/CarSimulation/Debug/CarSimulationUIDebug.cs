using UnityEngine;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.SplineSimulation;
using GearEngine.CarSimulation.Simulation;
using GearEngine.CarSimulation.Presentation;
using Scaffold.Entities;

namespace GearEngine.CarSimulation.Debug
{
    /// <summary>
    /// Generic OnGUI visual debugger to read stats from the active simulation (Physics or Spline).
    /// Used for manipulating CarEntity variables, injecting stats or modifying driver behavior.
    /// </summary>
    public sealed class CarSimulationUIDebug : MonoBehaviour
    {
        private bool showDebug = false;
        private Rect windowRect = new Rect(10, 10, 320, 400);

        // Spline specific
        private float statSpeed = 5f;
        private float statCornering = 5f;
        private float statDrift = 5f;
        private float statPrecision = 5f;
        private float statSmoothness = 5f;

        private void OnGUI()
        {
            if (!showDebug)
            {
                if (GUI.Button(new Rect(10, 10, 120, 30), "Show Debug UI"))
                    showDebug = true;
                return;
            }

            if (GUI.Button(new Rect(10, 10, 120, 30), "Hide Debug UI"))
                showDebug = false;

            windowRect = GUI.Window(0, windowRect, DrawWindow, "Car Simulation Debug");
        }

        private void DrawWindow(int windowID)
        {
            GUILayout.Space(10);
            
            var splineBootstrap = Object.FindFirstObjectByType<SplineEvaluateBootstrap>();
            if (splineBootstrap != null && splineBootstrap.ActiveDriver != null)
            {
                DrawSplineDebug(splineBootstrap);
            }
            else
            {
                var trackView = Object.FindFirstObjectByType<CarTrackTestView>();
                if (trackView != null)
                {
                    DrawPhysicsDebug(trackView);
                }
                else
                {
                    GUILayout.Label("Aguardando inicialização da corrida...");
                }
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        private void DrawSplineDebug(SplineEvaluateBootstrap bootstrap)
        {
            var driver = bootstrap.ActiveDriver;
            var state = driver.State;
            
            GUILayout.Label("<b>[ SPLINE SIMULATION ]</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
            
            GUILayout.Label($"Speed: {state.Speed * 3.6f:F0} km/h");
            GUILayout.Label($"Lap: {state.CompletedLaps + 1} | {state.T * 100f:F1}%");
            string mode = state.IsDrifting ? "DRIFT" : state.IsBraking ? "BRAKE" : state.IsAccelerating ? "ACCEL" : "COAST";
            GUILayout.Label($"State: {mode}");
            GUILayout.Label($"Curve Mode: {(state.IsInCurveSequence ? state.ActiveCurveMode.ToString() : "None")}");

            GUILayout.Space(10);
            GUILayout.Label("<b>STAT MODIFIERS (Personality)</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
            
            DrawSlider("Speed", ref statSpeed, 0f, 10f);
            DrawSlider("Cornering", ref statCornering, 0f, 10f);
            DrawSlider("Drift", ref statDrift, 0f, 10f);
            DrawSlider("Precision", ref statPrecision, 0f, 10f);
            DrawSlider("Smoothness", ref statSmoothness, 0f, 10f);

            if (GUI.changed)
            {
                bootstrap.UpdatePersonality(new DriverPersonality
                {
                    SpeedCapability = statSpeed,
                    CorneringSkill = statCornering,
                    Drift = statDrift,
                    Precision = statPrecision,
                    Smoothness = statSmoothness
                });
            }
        }

        private void DrawPhysicsDebug(CarTrackTestView trackView)
        {
            // Access CarTrackScreenViewModel via reflection
            var viewModelProperty = typeof(Scaffold.MVVM.View<CarTrackScreenViewModel>).GetProperty("ViewModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var vm = viewModelProperty?.GetValue(trackView) as CarTrackScreenViewModel;
            
            if (vm == null || vm.Sessions == null || vm.Sessions.Count == 0)
            {
                GUILayout.Label("Aguardando corrida (CarTrackScreenViewModel)...");
                return;
            }

            RaceState session = vm.Sessions[0];
            var car = session.Car;
            
            GUILayout.Label("<b>[ PHYSICS SIMULATION ]</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
            GUILayout.Label($"Speed: {session.CurrentSpeed:F0} km/h");
            GUILayout.Label($"Lap: {session.CurrentLap} | {session.NormalizedProgress * 100f:F1}%");
            GUILayout.Label($"Phase: {session.Phase}");

            GUILayout.Space(10);
            GUILayout.Label("<b>NATIVE ENTITY VARIABLES</b>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });

            var simDebug = Object.FindFirstObjectByType<CarSimulationDebug>();
            if (simDebug != null && simDebug.Context?.Variables != null)
            {
                var vars = simDebug.Context.Variables;
                DrawNativeStat(car, vars.SpeedCapability, "Speed Capability");
                DrawNativeStat(car, vars.CorneringSkill, "Cornering Skill");
                DrawNativeStat(car, vars.Drift, "Drift");
                DrawNativeStat(car, vars.Precision, "Precision");
                DrawNativeStat(car, vars.Smoothness, "Smoothness");
            }
            else
            {
                GUILayout.Label("Variáveis (VariableSet) não encontradas em CarSimulationDebug.");
            }
        }

        private void DrawSlider(string label, ref float val, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(80));
            val = GUILayout.HorizontalSlider(val, min, max, GUILayout.Width(150));
            GUILayout.Label(val.ToString("F1"), GUILayout.Width(30));
            GUILayout.EndHorizontal();
        }

        private void DrawNativeStat(CarEntity car, VariableSO variable, string label)
        {
            if (variable == null || car == null) return;
            
            car.TryGetVariable(variable, out float currentValue);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {currentValue:F1}", GUILayout.Width(120));
            
            if (GUILayout.Button("- 10", GUILayout.Width(40)))
                car.AddModifier(variable, new FloatAddModifier(-10f));
            if (GUILayout.Button("+ 10", GUILayout.Width(40)))
                car.AddModifier(variable, new FloatAddModifier(10f));
            
            GUILayout.EndHorizontal();
        }
    }
}
