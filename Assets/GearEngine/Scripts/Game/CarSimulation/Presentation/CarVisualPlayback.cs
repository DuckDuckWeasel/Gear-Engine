using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.Entity;
using GearEngine.CarSimulation.Simulation;
using UnityEngine;

namespace GearEngine.CarSimulation.Presentation
{
    public sealed class CarVisualPlayback
    {
        public CarVisualPlayback(CarVisualState state, CarVisualConfig config, LapSimulationConfig lapConfig)
        {
            this.state = state ?? throw new System.ArgumentNullException(nameof(state));
            this.config = config ?? throw new System.ArgumentNullException(nameof(config));
            this.lapConfig = lapConfig ?? throw new System.ArgumentNullException(nameof(lapConfig));
        }

        public CarVisualState State => state;

        private readonly CarVisualState state;
        private readonly CarVisualConfig config;
        private readonly LapSimulationConfig lapConfig;

        public void Tick(float dt, CarEntity car, CarVariableSet vars, CurveSample curve)
        {
            if (dt <= 0f)
            {
                return;
            }

            float handlingStat = CarRaceStats.ReadHandling(car, vars, lapConfig);
            ApplyCornerMotion(dt, curve, handlingStat);
        }

        private void ApplyCornerMotion(float dt, CurveSample curve, float handlingStat)
        {
            float targetCornerEffect = curve.CurveAmount * (1f - handlingStat) * config.DriftStrength * curve.CurveDirection;
            bool sameDirection = Mathf.Approximately(state.CornerEffect, 0f) || Mathf.Sign(targetCornerEffect) == Mathf.Sign(state.CornerEffect);
            bool targetIncreasesMagnitude = sameDirection && Mathf.Abs(targetCornerEffect) > Mathf.Abs(state.CornerEffect);
            float rate = targetIncreasesMagnitude ? config.CornerResponse : config.DriftRecoverRate;
            state.CornerEffect = Mathf.MoveTowards(state.CornerEffect, targetCornerEffect, rate * dt);
            state.LateralOffset = state.CornerEffect * config.MaxVisualOffset;
            state.SlipAngle = state.CornerEffect * config.MaxSlipAngle;
            state.IsDrifting = Mathf.Abs(state.CornerEffect) > config.DriftThreshold;
        }
    }
}
