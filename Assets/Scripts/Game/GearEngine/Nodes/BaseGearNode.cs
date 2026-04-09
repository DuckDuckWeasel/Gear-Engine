using UnityEngine;
using Scaffold.Events.Contracts;
using System;

namespace Game.GearEngine
{
    public class BaseGearNode : NodeBase
    {
        private Action<DirectionalTriggerEvent> triggerAction;
        private bool hasExecutedThisTick;
        private float maxDrivenSpeedThisTick = 0f;

        public float CurrentCharge { get; private set; }

        public BaseGearNode(IGridManager grid, IEventBus eventBus)
            : base(grid, eventBus)
        {
            triggerAction = OnTriggerReceived;
            eventBus.AddListener(triggerAction);
        }

        public override void NodeUpdate(float deltaTime, float speedModifier)
        {
            hasExecutedThisTick = false; // Reset threshold every tick

            if (ConfigData == null || !IsActive) return;

            // Apply driven rotation from the strongest neighbor driving us
            if (Mathf.Abs(maxDrivenSpeedThisTick) > 0.01f)
            {
                CurrentRotation += maxDrivenSpeedThisTick * deltaTime;
                if (CurrentRotation >= 360f) CurrentRotation -= 360f;
            }
            
            // Reset for the upcoming/ongoing physics frame
            maxDrivenSpeedThisTick = 0f;

            float speed = ConfigData.BaseRotationSpeed;
            CurrentRotation += speed * speedModifier * deltaTime;
            if (CurrentRotation >= 360f) CurrentRotation -= 360f;

            ApplyCharge(ConfigData.ChargeOverTimeAmount * deltaTime);
            TickAbilities(deltaTime);

            CheckAndExecute();
        }

        public void ApplyCharge(float amount)
        {
            if (ConfigData == null) return;
            
            if (CurrentCharge < ConfigData.MaxCharge)
            {
                CurrentCharge += amount;
                if (CurrentCharge > ConfigData.MaxCharge)
                {
                    CurrentCharge = ConfigData.MaxCharge;
                }
            }
        }

        public void ApplyDrivenRotation(float deltaTime, float drivingSpeed)
        {
            // Only adopt the fastest driving speed we receive this frame
            if (Mathf.Abs(drivingSpeed) > Mathf.Abs(maxDrivenSpeedThisTick))
            {
                maxDrivenSpeedThisTick = drivingSpeed;
            }
        }

        private void OnTriggerReceived(DirectionalTriggerEvent evt)
        {
            if (evt.TargetPosition != Position || ConfigData == null) return;

            ApplyCharge(evt.ChargeOnTriggerAmount);
            CheckAndExecute();
        }

        private void CheckAndExecute()
        {
            if (CurrentCharge >= ConfigData.MaxCharge)
            {
                if (hasExecutedThisTick)
                {
                    // Delay execution of overflowing charge to the exact next tick
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
