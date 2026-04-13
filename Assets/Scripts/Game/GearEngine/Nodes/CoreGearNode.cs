using UnityEngine;
using Scaffold.Events.Contracts;

namespace GearEngine.GearEngine.Nodes
{
    public class CoreGearNode : NodeBase
    {
        [SerializeField]
        private float lastFiredRotation = 0f;
        [SerializeField]
        private float slowdownTimer = 0f;

        public CoreGearNode(IGridManager grid, IEventBus eventBus) : base(grid, eventBus) { }

        public override void NodeUpdate(float deltaTime, float speedModifier)
        {
            if (ConfigData == null || !IsActive) return;

            float currentSpeedMod = speedModifier;
            if (slowdownTimer > 0)
            {
                currentSpeedMod *= ConfigData.SnapSlowdownMultiplier; // Config-driven stutter drag
                slowdownTimer -= deltaTime;
            }

            float speed = ConfigData.BaseRotationSpeed;
            float rotationDelta = speed * LocalSpeedMultiplier * currentSpeedMod * deltaTime;
            
            // Apply continuously for buttery smooth view rendering
            CurrentRotation += rotationDelta;

            // Fluid over-time charge to neighbors continues to happen based on time
            ApplyOverTimeChargeToNeighbors(deltaTime, speed, currentSpeedMod);

            CheckSnapAndTrigger();
        }

        private void CheckSnapAndTrigger()
        {
            float segmentAngle = ConfigData.TriggerPattern == TriggerPattern.EightWay ? 45f : 90f;

            // Has our continuous smooth rotation crossed the mathematical threshold for the next segment?
            while (CurrentRotation - lastFiredRotation >= segmentAngle)
            {
                lastFiredRotation += segmentAngle;
                
                // Calculate which segment we just snapped to logically
                int currentSegment = Mathf.FloorToInt(lastFiredRotation / segmentAngle);

                Vector2Int targetDir = GetDirectionForSegment(currentSegment);
                Vector2Int targetPos = Position + targetDir;
                
                IGridNode hitNode = grid.GetNode(targetPos);
                if (hitNode != null)
                {
                    Debug.Log($"<color=#ffcc00>[CoreGearNode]</color> Smooth Tick! Logically crossed {lastFiredRotation}° -> Fired at {targetPos}");

                    float currentSign = Mathf.Sign(ConfigData.BaseRotationSpeed * LocalSpeedMultiplier);
                    eventBus.Raise(new DirectionalTriggerEvent(targetPos, ConfigData.ChargeOnTriggerAmount, currentSign));
                    
                    slowdownTimer = ConfigData.SnapSlowdownDuration; // Config-driven stutter duration
                }
            }

            // Loop reset safeguard
            if (CurrentRotation >= 360f && lastFiredRotation >= 360f)
            {
                CurrentRotation -= 360f;
                lastFiredRotation -= 360f;
            }
        }

        private void ApplyOverTimeChargeToNeighbors(float deltaTime, float speed, float speedModifier)
        {
            float charge = ConfigData.ChargeOverTimeAmount * deltaTime;

            var dirs = ConfigData.TriggerPattern.GetDirections();
            foreach (var dir in dirs)
            {
                if (grid.GetNode(Position + dir) is BaseGearNode neighbor)
                {
                    if (charge > 0)
                    {
                        neighbor.ApplyCharge(charge);
                    }
                }
            }
        }



        private Vector2Int GetDirectionForSegment(int segment)
        {
            var dirs = ConfigData.TriggerPattern.GetDirections();
            if (segment >= 0 && segment < dirs.Length) return dirs[segment];
            return Vector2Int.up;
        }

        public override void WindDownUpdate(float deltaTime, float speedModifier)
        {
            if (ConfigData == null) return;

            // Smoothly snap to closest 90-degree orthogonal rest state in the direction of rotation
            float targetRotation;
            if (LastRotationDelta > 0)
                targetRotation = Mathf.Ceil(CurrentRotation / 90f) * 90f;
            else if (LastRotationDelta < 0)
                targetRotation = Mathf.Floor(CurrentRotation / 90f) * 90f;
            else
                targetRotation = Mathf.Round(CurrentRotation / 90f) * 90f;

            float smoothSpeed = 5f;
            CurrentRotation = Mathf.LerpAngle(CurrentRotation, targetRotation, deltaTime * smoothSpeed);
        }
    }
}
