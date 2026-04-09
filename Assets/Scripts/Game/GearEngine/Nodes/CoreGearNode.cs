using UnityEngine;
using Scaffold.Events.Contracts;

namespace Game.GearEngine
{
    public class CoreGearNode : NodeBase
    {
        private float internalProgress = 0f;

        private float lastFiredRotation = 0f;

        public CoreGearNode(IGridManager grid, IEventBus eventBus) : base(grid, eventBus) { }

        public override void NodeUpdate(float deltaTime, float speedModifier)
        {
            if (ConfigData == null || !IsActive) return;

            float speed = ConfigData.BaseRotationSpeed;
            float rotationDelta = speed * LocalSpeedMultiplier * speedModifier * deltaTime;
            
            // Apply continuously for buttery smooth view rendering
            CurrentRotation += rotationDelta;

            // Fluid over-time charge to neighbors continues to happen based on time
            ApplyOverTimeChargeToNeighbors(deltaTime, speed, speedModifier);

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
                
                Debug.Log($"<color=#ffcc00>[CoreGearNode]</color> Smooth Tick! Logically crossed {lastFiredRotation}° -> Fired at {targetPos}");

                eventBus.Raise(new DirectionalTriggerEvent(targetPos, ConfigData.ChargeOnTriggerAmount));
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
            if (charge <= 0) return;

            var dirs = GetTriggerDirections();
            foreach (var dir in dirs)
            {
                if (grid.GetNode(Position + dir) is BaseGearNode neighbor)
                {
                    neighbor.ApplyCharge(charge);
                    
                    // Drive neighbor rotation seamlessly!
                    neighbor.ApplyDrivenRotation(deltaTime, speed * LocalSpeedMultiplier * speedModifier);
                }
            }
        }



        private Vector2Int[] GetTriggerDirections()
        {
            if (ConfigData.TriggerPattern == TriggerPattern.EightWay)
            {
                return new[]
                {
                    Vector2Int.up, new Vector2Int(1, 1), Vector2Int.right, new Vector2Int(1, -1),
                    Vector2Int.down, new Vector2Int(-1, -1), Vector2Int.left, new Vector2Int(-1, 1)
                };
            }
            return new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        }

        private Vector2Int GetDirectionForSegment(int segment)
        {
            var dirs = GetTriggerDirections();
            if (segment >= 0 && segment < dirs.Length) return dirs[segment];
            return Vector2Int.up;
        }

        public override void WindDownUpdate(float deltaTime, float speedModifier)
        {
            if (ConfigData == null) return;

            // Wait, what's closest orthogonal angle for CoreGear?
            // CoreGear rotates tick-by-tick. Visually it's snapped.
            // If it accumulated some internal progress, we just let it rot to 0 or leave it snapped.
            // Let's give it a smooth snap toward 0 degree (starting position).
            float smoothSpeed = 5f;
            CurrentRotation = Mathf.LerpAngle(CurrentRotation, 0f, deltaTime * smoothSpeed);
            internalProgress = 0f; // drain progress
        }
    }
}
