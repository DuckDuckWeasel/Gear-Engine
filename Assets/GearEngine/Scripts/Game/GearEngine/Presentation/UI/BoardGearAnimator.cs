using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Visuals;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    /// <summary>
    /// Single-instance per-board orchestrator. Drives rotation, charge fill, and reparenting for tracked gears.
    /// </summary>
    public class BoardGearAnimator : MonoBehaviour
    {
        private struct Entry
        {
            public GearView View;
            public Vector2Int LastPos;
            public float StaggerOffset;
        }

        private readonly Dictionary<IGridNode, Entry> entries = new Dictionary<IGridNode, Entry>();

        private Func<Vector2Int, Transform> slotFn;
        private BoardLayoutSO layout;

        public void Configure(Func<Vector2Int, Transform> getSlot, BoardLayoutSO boardLayout)
        {
            slotFn = getSlot;
            layout = boardLayout;
        }

        public void Track(IGridNode node, GearView view)
        {
            if (node == null || view == null)
            {
                return;
            }

            float stagger = ComputeStagger(node.Position);
            entries[node] = new Entry
            {
                View = view,
                LastPos = node.Position,
                StaggerOffset = stagger,
            };

            view.SetRotationTarget(-node.CurrentRotation + stagger);

            if (node is BaseGearNode baseGear && baseGear.ConfigData != null && baseGear.ConfigData.MaxCharge > 0f)
            {
                view.SetChargeFillTarget(baseGear.CurrentCharge / baseGear.ConfigData.MaxCharge, snap: true);
            }
            else
            {
                view.ClearChargeFillTarget();
            }
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
        }

        private void Update()
        {
            if (entries.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<IGridNode, Entry> kvp in entries)
            {
                IGridNode node = kvp.Key;
                Entry e = kvp.Value;

                if (node.Position != e.LastPos)
                {
                    e.LastPos = node.Position;
                    e.StaggerOffset = ComputeStagger(node.Position);
                    Transform p = slotFn?.Invoke(node.Position);
                    if (p != null)
                    {
                        e.View.SetReparent(p);
                    }

                    entries[node] = e;
                }

                e.View.SetRotationTarget(-node.CurrentRotation + e.StaggerOffset);

                if (node is BaseGearNode baseGear && baseGear.ConfigData != null && baseGear.ConfigData.MaxCharge > 0f)
                {
                    e.View.SetChargeFillTarget(baseGear.CurrentCharge / baseGear.ConfigData.MaxCharge);
                }
            }
        }

        private float ComputeStagger(Vector2Int pos)
        {
            if (layout == null)
            {
                return 0f;
            }

            return ((pos.x + pos.y) % 2 == 0) ? layout.StaggeredRotationOffset : 0f;
        }
    }
}
