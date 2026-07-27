using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Visuals;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class BoardGearAnimator : MonoBehaviour
    {
        private readonly Dictionary<IGridNode, Entry> entries = new Dictionary<IGridNode, Entry>();

        private Func<Vector2Int, Transform> slotFn;
        private BoardLayoutSO layout;
        private string motorCogGearId = string.Empty;
        private Vector2Int? lastMotorPos;
        private Func<bool> isSimulationRunningFn;

        public void Configure(Func<Vector2Int, Transform> getSlot, BoardLayoutSO boardLayout, string motorCogGearId = null, Func<bool> isSimulationRunningFn = null)
        {
            slotFn = getSlot;
            layout = boardLayout;
            this.motorCogGearId = motorCogGearId ?? string.Empty;
            this.isSimulationRunningFn = isSimulationRunningFn;
            lastMotorPos = null;
        }

        public void Track(IGridNode node, GearView view)
        {
            if (node == null || view == null)
            {
                return;
            }

            PrimeEntrySlot(node, view);
            float stagger = ComputeStagger(node.Position, FindMotorPosition());
            CommitEntry(node, view, node.Position, stagger);
            view.SetRotationTarget(-node.CurrentRotation + stagger);
            ApplyChargeVisual(node, view, snap: true);
        }

        public void Untrack(IGridNode node)
        {
            if (node != null)
            {
                entries.Remove(node);
            }
        }

        public void Clear()
        {
            entries.Clear();
            lastMotorPos = null;
        }

        private void Update()
        {
            if (entries.Count == 0)
            {
                return;
            }

            Vector2Int? motor = FindMotorPosition();
            bool motorMoved = motor != lastMotorPos;
            lastMotorPos = motor;
            List<IGridNode> nodes = new List<IGridNode>(entries.Keys);
            foreach (IGridNode node in nodes)
            {
                TickOneNode(node, motor, motorMoved);
            }
        }

        private void PrimeEntrySlot(IGridNode node, GearView view)
        {
            entries[node] = new Entry
            {
                View = view,
                LastPos = node.Position,
                StaggerOffset = 0f,
            };
        }

        private void CommitEntry(IGridNode node, GearView view, Vector2Int lastPos, float stagger)
        {
            entries[node] = new Entry
            {
                View = view,
                LastPos = lastPos,
                StaggerOffset = stagger,
            };
        }

        private void TickOneNode(IGridNode node, Vector2Int? motor, bool motorMoved)
        {
            if (!entries.TryGetValue(node, out Entry e))
            {
                return;
            }

            if (node.Position != e.LastPos || motorMoved)
            {
                RefreshReparentAndStagger(node, ref e, motor);
            }

            if (!entries.TryGetValue(node, out e))
            {
                return;
            }

            e.View.SetRotationTarget(-node.CurrentRotation + e.StaggerOffset);
            ApplyChargeVisual(node, e.View, snap: false);
        }

        private void RefreshReparentAndStagger(IGridNode node, ref Entry e, Vector2Int? motor)
        {
            e.LastPos = node.Position;
            e.StaggerOffset = ComputeStagger(node.Position, motor);
            Transform p = slotFn?.Invoke(node.Position);
            if (p != null)
            {
                e.View.SetReparent(p);
            }

            entries[node] = e;
        }

        private void ApplyChargeVisual(IGridNode node, GearView view, bool snap)
        {
            if (isSimulationRunningFn != null && !isSimulationRunningFn())
            {
                view.SetChargeFillTarget(1f, snap: snap);
            }
            else
            {
                if (node is BaseGearNode baseGear && baseGear.ConfigData != null && baseGear.ConfigData.MaxCharge > 0f)
                {
                    view.SetChargeFillTarget(baseGear.CurrentCharge / baseGear.ConfigData.MaxCharge, snap: snap);
                }
                else
                {
                    view.SetChargeFillTarget(0f, snap: snap);
                }
            }
        }

        private Vector2Int? FindMotorPosition()
        {
            if (string.IsNullOrEmpty(motorCogGearId))
            {
                return null;
            }

            foreach (KeyValuePair<IGridNode, Entry> kvp in entries)
            {
                if (kvp.Key?.ConfigData?.Id == motorCogGearId)
                {
                    return kvp.Key.Position;
                }
            }

            return null;
        }

        private float ComputeStagger(Vector2Int pos, Vector2Int? motorPos)
        {
            if (layout == null || !motorPos.HasValue)
            {
                return 0f;
            }

            int distance = Mathf.Abs(pos.x - motorPos.Value.x) + Mathf.Abs(pos.y - motorPos.Value.y);
            return (distance % 2 == 0) ? 0f : layout.StaggeredRotationOffset;
        }

        private struct Entry
        {
            public GearView View;
            public Vector2Int LastPos;
            public float StaggerOffset;
        }
    }
}
