using UnityEngine;

namespace Game.GearEngine
{
    public class GearViewFactory
    {
        private BoardConfigSO boardConfig;

        public GearViewFactory(BoardConfigSO boardConfig)
        {
            this.boardConfig = boardConfig;
        }

        public GearView CreateView(IGridNode node, GearConfigData configData, Transform parent)
        {
            // The Root Node GameObject acts as a pure structural container and pivot.
            // The Visual Prefab will be instantiated cleanly inside it by the GearView.Initialize() call.
            GameObject viewObj = new GameObject($"{node.GetType().Name}_{node.Position}");
            viewObj.transform.SetParent(parent);
            
            // Standardize local positioning using new centered grid constants
            viewObj.transform.localPosition = boardConfig.GetWorldPosition(node.Position);

            // Fetch or attach the MVC logic binding component to this empty wrapper
            var view = viewObj.AddComponent<GearView>();
            
            view.Initialize(node, configData, boardConfig);

            return view;
        }
    }
}
