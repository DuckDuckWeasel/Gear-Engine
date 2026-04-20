using UnityEngine;
using Scaffold.Events.Contracts;
using System;
using Sirenix.OdinInspector;

namespace GearEngine.GearEngine.Nodes
{
    public class BaseGearNode : NodeBase
    {
        public BaseGearNode(IGridManager grid, IEventBus eventBus) : base(grid, eventBus)
        {
            triggerAction = OnTriggerReceived;
            eventBus.AddListener(triggerAction);
        }

        [ShowInInspector]
        public float CurrentCharge { get; private set; }

        [SerializeField]
        private Action<DirectionalTriggerEvent> triggerAction;
        [SerializeField]
        private bool hasExecutedThisTick;
        [SerializeField]
        private bool hasRotatedThisTick;

        public override void ResetSimulationState()
        {
            base.ResetSimulationState();
            CurrentCharge = 0f;
            hasExecutedThisTick = false;
            hasRotatedThisTick = false;
        }

        public override void NodeUpdate(float deltaTime, float speedModifier)
        {
            hasExecutedThisTick = false;
            hasRotatedThisTick = false;

            if (ConfigData == null || !IsActive)
            {
                return;
            }

            float speed = ConfigData.BaseRotationSpeed;
            CurrentRotation += speed * speedModifier * deltaTime;
            if (CurrentRotation >= 360f)
            {
                CurrentRotation -= 360f;
            }

            ApplyCharge(ConfigData.ChargeOverTimeAmount * deltaTime);
            TickAbilities(deltaTime);

            CheckAndExecute();
        }

        private void OnTriggerReceived(DirectionalTriggerEvent evt)
        {
            if (!CanProcessTrigger(evt))
            {
                return;
            }

            hasRotatedThisTick = true;
            float currentSign = ApplyTriggerSpin(evt.SourceRotationSign);
            ApplyCharge(ConfigData.ChargeOnTriggerAmount);
            CheckAndExecute();
            RaiseNeighborTriggers(currentSign);
        }

        private bool CanProcessTrigger(DirectionalTriggerEvent evt)
        {
            if (evt.TargetPosition != Position || ConfigData == null)
            {
                return false;
            }

            if (hasRotatedThisTick)
            {
                return false;
            }

            return true;
        }

        private float ApplyTriggerSpin(float sourceRotationSign)
        {
            float currentSign = -sourceRotationSign;
            CurrentRotation += ConfigData.TriggerSpinDegrees * currentSign;
            NormalizeRotation();
            return currentSign;
        }

        private void NormalizeRotation()
        {
            while (CurrentRotation >= 360f)
            {
                CurrentRotation -= 360f;
            }

            while (CurrentRotation < 0f)
            {
                CurrentRotation += 360f;
            }
        }

        private void RaiseNeighborTriggers(float currentSign)
        {
            Vector2Int[] dirs = ConfigData.TriggerPattern.GetDirections();

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int neighborPos = Position + dir;
                if (grid.GetNode(neighborPos) is BaseGearNode)
                {
                    eventBus.Raise(new DirectionalTriggerEvent(neighborPos, 0f, currentSign));
                }
            }
        }

        public void ApplyCharge(float amount)
        {
            if (ConfigData == null)
            {
                return;
            }

            if (CurrentCharge < ConfigData.MaxCharge)
            {
                CurrentCharge += amount;
                if (CurrentCharge > ConfigData.MaxCharge)
                {
                    CurrentCharge = ConfigData.MaxCharge;
                }
            }
        }

        public void SetCharge(float amount)
        {
            if (ConfigData == null) return;
            CurrentCharge = Mathf.Clamp(amount, 0f, ConfigData.MaxCharge);
        }

        private void CheckAndExecute()
        {
            if (CurrentCharge >= ConfigData.MaxCharge && ConfigData.MaxCharge > 0)
            {
                if (hasExecutedThisTick)
                {
                    return;
                }

                CurrentCharge = 0f;
                hasExecutedThisTick = true;

                Debug.Log($"<color=#33ccff>[BaseGearNode]</color> {Position} triggered and fully charged! Executing abilities.");
                ExecuteAbilities();
                eventBus.Raise(new GearRotatedEvent(Position));
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            if (eventBus != null && triggerAction != null)
            {
                eventBus.RemoveListener(triggerAction);
            }
        }
    }
}
