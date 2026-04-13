using UnityEngine;
using Scaffold.Events.Contracts;
using System;
using Sirenix.OdinInspector;

namespace GearEngine.GearEngine.Nodes
{
    public class BaseGearNode : NodeBase
    {
        [SerializeField]
        private Action<DirectionalTriggerEvent> triggerAction;
        [SerializeField]
        private bool hasExecutedThisTick;
        [SerializeField]
        private bool hasRotatedThisTick;

        [ShowInInspector]
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
            hasRotatedThisTick = false; // Reset mechanical trigger lock
            
            if (ConfigData == null || !IsActive) return;

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

        private void OnTriggerReceived(DirectionalTriggerEvent evt)
        {
            if (evt.TargetPosition != Position || ConfigData == null) return;
            if (hasRotatedThisTick) return;

            hasRotatedThisTick = true;

            // Visual feedback chunk: jump by config degrees and let GearView lerp it smoothly!
            // Inverse gear ratio mechanics -> + becomes -, - becomes +
            float currentSign = -evt.SourceRotationSign;
            CurrentRotation += ConfigData.TriggerSpinDegrees * currentSign;
            
            while (CurrentRotation >= 360f) CurrentRotation -= 360f;
            while (CurrentRotation < 0f) CurrentRotation += 360f;

            ApplyCharge(ConfigData.ChargeOnTriggerAmount);
            CheckAndExecute();

            // Cascade mechanics to adjacent interlocking gears
            Vector2Int[] dirs = ConfigData.TriggerPattern.GetDirections();

            foreach (var dir in dirs)
            {
                var neighborPos = Position + dir;
                if (grid.GetNode(neighborPos) is BaseGearNode)
                {
                    eventBus.Raise(new DirectionalTriggerEvent(neighborPos, 0f, currentSign));
                }
            }
        }

        private void CheckAndExecute()
        {
            if (CurrentCharge >= ConfigData.MaxCharge && ConfigData.MaxCharge > 0)
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
