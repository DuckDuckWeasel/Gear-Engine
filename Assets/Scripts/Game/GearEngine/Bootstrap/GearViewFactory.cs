using System.Collections.Generic;
using UnityEngine;

namespace Game.GearEngine
{
    public class GearViewFactory
    {
        private readonly BoardConfigSO boardConfig;
        private readonly Dictionary<IGridNode, GearView> viewRegistry = new Dictionary<IGridNode, GearView>();

        public GearViewFactory(BoardConfigSO boardConfig)
        {
            this.boardConfig = boardConfig;
        }

        public GearView CreateView(IGridNode node, GearConfigData configData, Transform parent)
        {
            GameObject viewObj = new GameObject($"{node.GetType().Name}_{node.Position}");
            viewObj.transform.SetParent(parent);

            viewObj.transform.localPosition = boardConfig.GetWorldPosition(node.Position);

            var view = viewObj.AddComponent<GearView>();

            view.Initialize(node, configData, boardConfig, this);

            viewRegistry[node] = view;

            return view;
        }

        public GearView GetView(IGridNode node)
        {
            if (node == null)
            {
                return null;
            }

            viewRegistry.TryGetValue(node, out var view);
            return view;
        }

        public void UnregisterView(IGridNode node)
        {
            if (node == null)
            {
                return;
            }

            viewRegistry.Remove(node);
        }

        /// <summary>Active gear views on the board (avoids <c>FindObjectsOfType</c> during drag targeting).</summary>
        public IEnumerable<GearView> EnumerateGearViews() => viewRegistry.Values;
    }
}

