using System;

namespace GearEngine.CarSimulation.PhysicsSimulation.Cinematic
{
    public interface ICinematicFinishStrategy
    {
        void Initialize(SplineCarRunnerContext ctx);
        void Tick(SplineCarRunnerContext ctx, float deltaTime);
    }
}
