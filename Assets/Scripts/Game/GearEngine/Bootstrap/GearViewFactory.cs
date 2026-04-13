using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Bootstrap
{
    public class GearViewFactory
    {
        private readonly Dictionary<IGridNode, GearView> viewRegistry = new Dictionary<IGridNode, GearView>();

        public GearView CreateView(
            IGridNode node,
            GearConfigData configData,
            Transform parent,
            Vector3 localPosition)
        {
            GameObject viewObj = new GameObject($"{node.GetType().Name}_{node.Position}");
            viewObj.transform.SetParent(parent);
            viewObj.transform.localPosition = localPosition;

            var view = viewObj.AddComponent<GearView>();

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
