using Scaffold.MVVM;
using UnityEngine;
using UnityEngine.Splines;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed class CarView : ViewComponent<CarViewModel>
    {
        public SplineContainer SplineContainer { get; set; }

        [SerializeField]
        private PrometeoCarController prometeoController;

        private bool runnerAttached;

        protected override void OnBind()
        {
            if (prometeoController == null)
            {
                Debug.LogError("[CarView] Missing PrometeoCarController. Cannot bind AI logic.");
                return;
            }

            if (SplineContainer == null)
            {
                Debug.LogError("[CarView] SplineContainer not set before Bind.");
                return;
            }

            SetupStartTransform();

            if (viewModel.ShouldAttachRunnerOnBind)
            {
                AttachRunner();
            }
        }

        /// <summary>Attaches the spline runner after preview bind (e.g. when the user starts a race).</summary>
        public void AttachRunner()
        {
            if (runnerAttached || viewModel == null)
            {
                return;
            }

            if (prometeoController == null)
            {
                Debug.LogError("[CarView] AttachRunner: PrometeoCarController is missing.");
                return;
            }

            if (SplineContainer == null)
            {
                Debug.LogError("[CarView] AttachRunner: SplineContainer is not set.");
                return;
            }

            if (viewModel.RunnerService != null)
            {
                if (viewModel.RunnerService is GearEngine.CarSimulation.PhysicsSimulation.SplineCarRunnerService)
                {
                    var initParams = new GearEngine.CarSimulation.PhysicsSimulation.PhysicsInitParams
                    {
                        Entity = viewModel.Car,
                        Track = SplineContainer,
                        CarTransform = prometeoController.transform,
                        Controller = prometeoController,
                        Stats = viewModel.Session.Config.RoguelikeStats
                    };
                    viewModel.RunnerService.InitializeRun(initParams);
                }
                else if (viewModel.RunnerService is GearEngine.CarSimulation.SplineSimulation.SplineEvaluateRunnerService)
                {
                    var initParams = new GearEngine.CarSimulation.SplineSimulation.SplineInitParams
                    {
                        Entity = viewModel.Car,
                        Track = SplineContainer,
                        CarTransform = prometeoController.transform,
                        Personality = GearEngine.CarSimulation.SplineSimulation.DriverPersonality.Default,
                        LaneProfile = null
                    };
                    viewModel.RunnerService.InitializeRun(initParams);
                }
                else
                {
                    Debug.LogError($"[CarView] Unknown RunnerService type: {viewModel.RunnerService.GetType().Name}");
                }
            }

            runnerAttached = true;
            TrySetupEditorDebug();
        }

        private void Update()
        {
            if (viewModel != null)
            {
                viewModel.TickTelemetry();
            }
        }

        private void SetupStartTransform()
        {
            if (SplineContainer != null && SplineContainer.Spline != null && SplineContainer.Spline.Count > 0)
            {
                var startParam = 0f;
                Vector3 startPos = SplineContainer.transform.TransformPoint(
                    UnityEngine.Splines.SplineUtility.EvaluatePosition(SplineContainer.Spline, startParam));

                Vector3 startForward = SplineContainer.transform.TransformDirection(
                    UnityEngine.Splines.SplineUtility.EvaluateTangent(SplineContainer.Spline, startParam)).normalized;

                Vector3 startUp = SplineContainer.transform.TransformDirection(
                    UnityEngine.Splines.SplineUtility.EvaluateUpVector(SplineContainer.Spline, startParam)).normalized;

                transform.position = startPos;
                if (startForward != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(startForward, startUp);
                }
            }
        }

        private void TrySetupEditorDebug()
        {
#if UNITY_EDITOR
            System.Type debugType = System.Type.GetType("GearEngine.CarSimulation.Debug.CarSimulationDebug, Game.CarSimulation.Debug");
            if (debugType != null)
            {
                var debugComponent = gameObject.GetComponent(debugType) ?? gameObject.AddComponent(debugType);
                System.Reflection.MethodInfo setupMethod = debugType.GetMethod("Setup", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (setupMethod != null)
                {
                    var parameters = setupMethod.GetParameters();
                    if (parameters.Length == 2 && parameters[1].ParameterType.IsInstanceOfType(viewModel.RunnerService))
                    {
                        setupMethod.Invoke(debugComponent, new object[] { viewModel.Session, viewModel.RunnerService });
                    }
                }
            }
#endif
        }
    }
}
