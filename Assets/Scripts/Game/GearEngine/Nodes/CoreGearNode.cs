using UnityEngine;
using Scaffold.Events.Contracts;

namespace Game.GearEngine
{
    public class CoreGearNode : NodeBase
    {
        private float internalProgress = 0f;

        public CoreGearNode(IGridManager grid, IEventBus eventBus) : base(grid, eventBus) { }

        public override void NodeUpdate(float deltaTime, float speedModifier)
        {
            if (ConfigData == null || !IsActive) return;

            float speed = ConfigData.BaseRotationSpeed;
            // Accumulate progress invisibly based on speed
            float deltaProgress = speed * LocalSpeedMultiplier * speedModifier * deltaTime;
            internalProgress += deltaProgress;

            // Fluid over-time charge to neighbors continues to happen based on time
            ApplyOverTimeChargeToNeighbors(deltaTime);

            CheckSnapAndTrigger();
        }

        private void CheckSnapAndTrigger()
        {
            float segmentAngle = ConfigData.TriggerPattern == TriggerPattern.EightWay ? 45f : 90f;

            // When accumulation hits the threshold for the next step, SNAP rotation like a clock
            while (internalProgress >= segmentAngle)
            {
                internalProgress -= segmentAngle;
                
                CurrentRotation += segmentAngle;
                if (CurrentRotation >= 360f)
                {
                    CurrentRotation -= 360f;
                }

                // Calculate which segment we just snapped to (0, 1, 2, 3...)
                int currentSegment = Mathf.FloorToInt(CurrentRotation / segmentAngle);

                Vector2Int targetDir = GetDirectionForSegment(currentSegment);
                Vector2Int targetPos = Position + targetDir;
                
                Debug.Log($"<color=#ffcc00>[CoreGearNode]</color> Tick! Snapped to {CurrentRotation}° -> Fired at {targetPos}");

                eventBus.Raise(new DirectionalTriggerEvent(targetPos, ConfigData.ChargeOnTriggerAmount));
            }
        }

        private void ApplyOverTimeChargeToNeighbors(float deltaTime)
        {
            float charge = ConfigData.ChargeOverTimeAmount * deltaTime;
            if (charge <= 0) return;

            var dirs = GetTriggerDirections();
            foreach (var dir in dirs)
            {
                if (grid.GetNode(Position + dir) is BaseGearNode neighbor)
                {
                    neighbor.ApplyCharge(charge);
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
