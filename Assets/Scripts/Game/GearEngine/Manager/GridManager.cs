using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Sirenix.OdinInspector;

namespace GearEngine.GearEngine.Manager
{
    public class GridManager : IGridManager, ITickable
    {
        public float GlobalSpeedModifier { get; set; } = 1.0f;
        public bool IsRunning { get; private set; }

        [ShowInInspector, ReadOnly]
        private Dictionary<Vector2Int, IGridNode> nodes = new Dictionary<Vector2Int, IGridNode>();

        public void Play()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void AddNode(IGridNode node)
        {
            nodes[node.Position] = node;
        }

        public void RemoveNode(Vector2Int pos)
        {
            if (nodes.TryGetValue(pos, out var node))
            {
                node.Dispose();
                nodes.Remove(pos);
            }
        }

        public IGridNode ExtractNode(Vector2Int pos)
        {
            if (nodes.TryGetValue(pos, out var node))
            {
                nodes.Remove(pos);
                return node;
            }
            return null;
        }

        public IGridNode GetNode(Vector2Int pos)
        {
            nodes.TryGetValue(pos, out var node);
            return node;
        }

        public IEnumerable<IGridNode> GetAllNodes()
        {
            return nodes.Values;
        }

        public void Tick()
        {
            float dt = Time.deltaTime;

            if (IsRunning)
            {
                HandleRunningUpdate(dt);
            }
            else
            {
                HandleWindDownUpdate(dt);
            }
        }

        private void HandleRunningUpdate(float dt)
        {
            foreach (var node in nodes.Values)
            {
                node.LocalSpeedMultiplier = 1.0f;
            }

            foreach (var node in nodes.Values)
            {
                if (node is AuraGearNode aura && aura.IsActive)
                {
                    aura.ApplyAura(dt);
                }
            }

            foreach (var node in nodes.Values)
            {
                node.NodeUpdate(dt, GlobalSpeedModifier);
            }
        }

        private void HandleWindDownUpdate(float dt)
        {
            foreach (var node in nodes.Values)
            {
                node.WindDownUpdate(dt, GlobalSpeedModifier);
            }
        }
    }
}
