using UnityEngine;

namespace Game.GearEngine
{
    public class GearViewFactory
    {
        public GearView CreateView(IGridNode node, GearConfigData configData, Transform parent)
        {
            string nodeType = node.GetType().Name;
            var viewObj = new GameObject($"{nodeType}_{node.Position}");
            
            viewObj.transform.SetParent(parent);
            viewObj.transform.localPosition = new Vector3(node.Position.x * 1.5f, node.Position.y * 1.5f, 0);

            var view = viewObj.AddComponent<GearView>();
            view.Initialize(node, configData);

            return view;
        }
    }
}
