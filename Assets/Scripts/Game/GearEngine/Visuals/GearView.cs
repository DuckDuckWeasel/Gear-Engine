using UnityEngine;

namespace Game.GearEngine
{
    public class GearView : MonoBehaviour
    {
        private IGridNode targetNode;
        private Transform cachedVisual;
        private BoardConfigSO boardConfig;
        
        public IGridNode TargetNode => targetNode;

        public void Initialize(IGridNode node, GearConfigData configData, BoardConfigSO config)
        {
            targetNode = node;
            boardConfig = config;
            SetupVisual(configData);
        }

        public void Reconfigure(GearConfigData configData)
        {
            ClearCachedVisual();
            SetupVisual(configData);
        }

        private void SetupVisual(GearConfigData configData)
        {
            if (configData?.VisualPrefab != null)
            {
                var instance = Instantiate(configData.VisualPrefab, transform);
                cachedVisual = instance.transform;
                cachedVisual.localPosition = Vector3.zero;
            }
        }

        private void ClearCachedVisual()
        {
            if (cachedVisual != null)
            {
                Destroy(cachedVisual.gameObject);
                cachedVisual = null;
            }
        }

        private void Update()
        {
            if (targetNode == null || boardConfig == null) return;

            Transform target = cachedVisual != null ? cachedVisual : transform;
            
            // Lerp towards the logical position smoothly
            Vector3 logicalWorldPos = boardConfig.GetWorldPosition(targetNode.Position);
            
            // If the view is manually dragged away, it will glide back when released.
            // Lerp smooth position
            transform.localPosition = Vector3.Lerp(transform.localPosition, logicalWorldPos, Time.deltaTime * 20f);

            // Lerp smooth rotation
            Quaternion targetRot = Quaternion.Euler(0, 0, -targetNode.CurrentRotation);
            target.localRotation = Quaternion.Lerp(target.localRotation, targetRot, Time.deltaTime * 15f);
        }
    }
}
