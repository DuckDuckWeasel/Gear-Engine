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
            GameObject viewObj;
            
            if (configData.VisualPrefab != null)
            {
                viewObj = UnityEngine.Object.Instantiate(configData.VisualPrefab, parent);
                viewObj.name = $"{node.GetType().Name}_{node.Position}";
            }
            else
            {
                Debug.LogWarning($"<color=#ffaa00>[GearViewFactory]</color> {node.GetType().Name} lacks a VisualPrefab! Spawning generic fallback empty.");
                viewObj = new GameObject($"{node.GetType().Name}_{node.Position}_Fallback");
                viewObj.transform.SetParent(parent);
            }
            
            // Standardize local positioning using new centered grid constants
            viewObj.transform.localPosition = boardConfig.GetWorldPosition(node.Position);

            // Fetch or attach the MVC logic binding component
            var view = viewObj.GetComponent<GearView>();
            if (view == null)
            {
                view = viewObj.AddComponent<GearView>();
            }
            
            view.Initialize(node, configData, boardConfig);

            return view;
        }
    }
}
