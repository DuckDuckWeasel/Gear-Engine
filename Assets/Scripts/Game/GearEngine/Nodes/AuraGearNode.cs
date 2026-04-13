using UnityEngine;

namespace GearEngine.GearEngine.Nodes
{
    public class AuraGearNode : NodeBase
    {
        public AuraGearNode(IGridManager grid, Scaffold.Events.Contracts.IEventBus eventBus) : base(grid, eventBus)
        {
        }

        public void ApplyAura(float deltaTime)
        {
            if (ConfigData == null)
            {
                return;
            }

            float bonusCharge = ConfigData.ChargeOverTimeAmount * deltaTime;
            float speedBoostMultiplier = 1f + (ConfigData.ChargeOnTriggerAmount / 100f);

            Vector2Int[] dirs = ConfigData.TriggerPattern.GetDirections();

            foreach (Vector2Int dir in dirs)
            {
                ApplyAuraToNeighbor(dir, bonusCharge, speedBoostMultiplier);
            }
        }

        private void ApplyAuraToNeighbor(Vector2Int dir, float bonusCharge, float speedBoostMultiplier)
        {
            IGridNode neighbor = grid.GetNode(Position + dir);
            if (neighbor is BaseGearNode baseGear && bonusCharge > 0)
            {
                baseGear.ApplyCharge(bonusCharge);
                return;
            }

            if (neighbor is CoreGearNode coreGear)
            {
                coreGear.LocalSpeedMultiplier *= speedBoostMultiplier;
            }
        }

        public override void NodeUpdate(float deltaTime, float speedModifier)
        {
            if (ConfigData == null)
            {
                return;
            }

            CurrentRotation += ConfigData.BaseRotationSpeed * speedModifier * deltaTime;
            if (CurrentRotation >= 360f)
            {
                CurrentRotation -= 360f;
            }
        }
    }
}
