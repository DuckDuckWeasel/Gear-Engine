using UnityEngine;

namespace Game.GearEngine
{
    public class GearView : MonoBehaviour
    {
        private IGridNode targetNode;
        private Transform cachedVisual;

        public void Initialize(IGridNode node, GearConfigData configData)
        {
            targetNode = node;
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
            if (targetNode == null) return;

            Transform target = cachedVisual != null ? cachedVisual : transform;
            
            // Visual Improvement: Instead of a raw snap, we Lerp towards the ideal logical rotation.
            // This creates a fast, "springy" mechanical tick effect for Core Gears that snap in steps.
            Quaternion targetRot = Quaternion.Euler(0, 0, -targetNode.CurrentRotation);
            
            float smoothSpeed = 15f; 
            target.localRotation = Quaternion.Lerp(target.localRotation, targetRot, Time.deltaTime * smoothSpeed);
        }
    }
}
