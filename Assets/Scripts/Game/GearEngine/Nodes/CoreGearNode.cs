using UnityEngine;
using Scaffold.Events.Contracts;

namespace GearEngine.GearEngine.Nodes
{
    public class CoreGearNode : NodeBase
    {
        public CoreGearNode(IGridManager grid, IEventBus eventBus) : base(grid, eventBus)
        {
        }

        [SerializeField]
        private float lastFiredRotation = 0f;
        [SerializeField]
        private float slowdownTimer = 0f;

        public override void NodeUpdate(float deltaTime, float speedModifier)
        {
            if (ConfigData == null || !IsActive)
            {
                return;
            }

            float currentSpeedMod = speedModifier;
            if (slowdownTimer > 0)
            {
                currentSpeedMod *= ConfigData.SnapSlowdownMultiplier;
                slowdownTimer -= deltaTime;
            }

            float speed = ConfigData.BaseRotationSpeed;
            float rotationDelta = speed * LocalSpeedMultiplier * currentSpeedMod * deltaTime;

            CurrentRotation += rotationDelta;

            ApplyOverTimeChargeToNeighbors(deltaTime, speed, currentSpeedMod);

            CheckSnapAndTrigger();
        }

        private void CheckSnapAndTrigger()
        {
            float segmentAngle = ConfigData.TriggerPattern == TriggerPattern.EightWay ? 45f : 90f;

            while (CurrentRotation - lastFiredRotation >= segmentAngle)
            {
                lastFiredRotation += segmentAngle;
                TryFireTriggerForSegment(segmentAngle);
            }

            if (CurrentRotation >= 360f && lastFiredRotation >= 360f)
            {
                CurrentRotation -= 360f;
                lastFiredRotation -= 360f;
            }
        }

        private void TryFireTriggerForSegment(float segmentAngle)
        {
            int currentSegment = Mathf.FloorToInt(lastFiredRotation / segmentAngle);
            Vector2Int targetDir = GetDirectionForSegment(currentSegment);
            Vector2Int targetPos = Position + targetDir;

            IGridNode hitNode = grid.GetNode(targetPos);
            if (hitNode == null)
            {
                return;
            }

            Debug.Log($"<color=#ffcc00>[CoreGearNode]</color> Smooth Tick! Logically crossed {lastFiredRotation}° -> Fired at {targetPos}");

            float currentSign = Mathf.Sign(ConfigData.BaseRotationSpeed * LocalSpeedMultiplier);
            eventBus.Raise(new DirectionalTriggerEvent(targetPos, ConfigData.ChargeOnTriggerAmount, currentSign));

            slowdownTimer = ConfigData.SnapSlowdownDuration;
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
            if (segment >= 0 && segment < dirs.Length)
            {
                return dirs[segment];
            }

            return Vector2Int.up;
        }

        public override void WindDownUpdate(float deltaTime, float speedModifier)
        {
            if (ConfigData == null)
            {
                return;
            }

            float targetRotation = ComputeWindDownTargetRotation();
            float smoothSpeed = 5f;
            CurrentRotation = Mathf.LerpAngle(CurrentRotation, targetRotation, deltaTime * smoothSpeed);
        }

        private float ComputeWindDownTargetRotation()
        {
            if (LastRotationDelta > 0)
            {
                return Mathf.Ceil(CurrentRotation / 90f) * 90f;
            }

            if (LastRotationDelta < 0)
            {
                return Mathf.Floor(CurrentRotation / 90f) * 90f;
            }

            return Mathf.Round(CurrentRotation / 90f) * 90f;
        }
    }
}
