using UnityEngine;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.PhysicsSimulation.Cinematic;

namespace GearEngine.CarSimulation.PhysicsSimulation.Cinematic
{
    public class AkiraSlideCinematicStrategy : ICinematicFinishStrategy
    {
        private float slideDirection;
        private float timer;
        private Vector3 initialVelocityDirection;
        private float initialSpeed;
        private Quaternion initialRotation;
        private Quaternion targetRotation;

        public void Initialize(SplineCarRunnerContext ctx)
        {
            timer = 0f;
            
            // Determine slide direction based on steering or random if straight
            if (ctx.targetCar != null && ctx.upcomingWaypoints != null && ctx.upcomingWaypoints.Length > 0)
            {
                float dotRight = Vector3.Dot(ctx.targetCar.transform.right, (ctx.upcomingWaypoints[0] - ctx.targetCar.transform.position).normalized);
                slideDirection = dotRight > 0 ? 1f : -1f;
            }
            else
            {
                slideDirection = 1f;
            }

            if (ctx.targetCarRb != null)
            {
                initialSpeed = ctx.targetCarRb.linearVelocity.magnitude;
                initialVelocityDirection = initialSpeed > 1f ? ctx.targetCarRb.linearVelocity.normalized : ctx.targetCar.transform.forward;
                initialRotation = ctx.targetCarRb.rotation;
                
                // Akira slide is roughly 75 to 85 degrees sideways
                float driftAngle = 80f * slideDirection;
                targetRotation = Quaternion.LookRotation(initialVelocityDirection, Vector3.up) * Quaternion.Euler(0, driftAngle, 0);

                // Make the car kinematic to completely bypass Prometeo physics friction
                ctx.targetCarRb.isKinematic = true;
            }

            Debug.Log($"[AkiraSlide] Initialized with direction {slideDirection}");
        }

        public void Tick(SplineCarRunnerContext ctx, float deltaTime)
        {
            timer += deltaTime;

            // Stop normal Prometeo inputs
            ctx.aiThrottle.buttonPressed = false;
            ctx.aiReverse.buttonPressed = false;
            ctx.aiLeft.buttonPressed = false;
            ctx.aiRight.buttonPressed = false;
            ctx.aiBrake.buttonPressed = true; // Force handbrake visual/particles

            if (ctx.targetCarRb != null)
            {
                // Phase 1: Snap to Akira Drift Angle (0 to 0.4s)
                float snapDuration = 0.4f;
                if (timer <= snapDuration)
                {
                    float t = timer / snapDuration;
                    // Ease out cubic
                    t = 1f - Mathf.Pow(1f - t, 3f);
                    
                    ctx.targetCarRb.MoveRotation(Quaternion.Slerp(initialRotation, targetRotation, t));
                    
                    // Manually move the car position because it's kinematic now
                    float currentSpeed = Mathf.Lerp(initialSpeed, initialSpeed * 0.8f, t);
                    ctx.targetCarRb.MovePosition(ctx.targetCarRb.position + initialVelocityDirection * (currentSpeed * deltaTime));
                }
                // Phase 2: Slide to a halt with slight wobble (0.4s onwards)
                else
                {
                    float timeSinceSnap = timer - snapDuration;
                    
                    // Decelerate manually
                    float decayFactor = Mathf.Clamp01(timeSinceSnap / 1.0f); // 1.0s to fully stop
                    float currentSpeed = Mathf.Lerp(initialSpeed * 0.8f, 0f, decayFactor);
                    
                    ctx.targetCarRb.MovePosition(ctx.targetCarRb.position + initialVelocityDirection * (currentSpeed * deltaTime));

                    // Add a slight wobble to the Pitch and Roll for dramatic effect
                    float wobbleFrequency = 15f;
                    float wobbleAmplitude = Mathf.Lerp(3f, 0f, decayFactor); 
                    float wobbleAngle = Mathf.Sin(timeSinceSnap * wobbleFrequency) * wobbleAmplitude;
                    
                    Quaternion wobbleRot = targetRotation * Quaternion.Euler(wobbleAngle, 0, wobbleAngle * slideDirection);
                    ctx.targetCarRb.MoveRotation(wobbleRot);
                }

                // Force drift state and particles
                ctx.targetCar.isDrifting = true;
                ctx.targetCar.isTractionLocked = true;
                ctx.targetCar.DriftCarPS();
            }
        }
    }
}
