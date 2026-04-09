using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.GearEngine
{
    public class GearView : SerializedMonoBehaviour
    {
        [SerializeField]
        private IGridNode targetNode;
        [SerializeField]
        private Transform cachedVisual;
        [SerializeField]
        private BoardConfigSO boardConfig;
        
        public IGridNode TargetNode => targetNode;
        [ShowInInspector]
        public bool IsBeingDragged { get; set; } = false;

        [SerializeField]
        private GameObject chargeVisualObj;
        [SerializeField]
        private Transform chargeFillTransform;
        [SerializeField]
        private float currentVisualFill = 0f;

        [SerializeField]
        private static Sprite squareSpriteCenter;
        [SerializeField]
        private static Sprite squareSpriteBottom;

        private void InitSprites()
        {
            if (squareSpriteCenter == null)
                squareSpriteCenter = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            if (squareSpriteBottom == null)
                squareSpriteBottom = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0f), 4f);
        }

        public void Initialize(IGridNode node, GearConfigData configData, BoardConfigSO config)
        {
            targetNode = node;
            boardConfig = config;
            SetupVisual(configData);
            SetupChargeVisual(configData);
        }

        public void Reconfigure(GearConfigData configData)
        {
            ClearCachedVisual();
            SetupVisual(configData);
            SetupChargeVisual(configData);
        }

        private void SetupVisual(GearConfigData configData)
        {
            if (configData?.VisualPrefab != null)
            {
                GameObject instance = Instantiate(configData.VisualPrefab, transform);
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
            if (chargeVisualObj != null)
            {
                Destroy(chargeVisualObj);
                chargeVisualObj = null;
                chargeFillTransform = null;
            }
        }

        private void SetupChargeVisual(GearConfigData configData)
        {
            if (targetNode is BaseGearNode baseGear && configData != null && configData.MaxCharge > 0)
            {
                InitSprites();

                chargeVisualObj = new GameObject("ChargeVisual");
                chargeVisualObj.transform.SetParent(transform, false);
                chargeVisualObj.transform.localPosition = new Vector3(0, 0, -0.5f); // In front of gear
                chargeVisualObj.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                
                GameObject bgObj = new GameObject("ChargeBackground");
                bgObj.transform.SetParent(chargeVisualObj.transform, false);
                SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
                bgRenderer.sprite = squareSpriteCenter;
                bgRenderer.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                bgRenderer.sortingOrder = 5;

                GameObject fillObj = new GameObject("ChargeFill");
                chargeFillTransform = fillObj.transform;
                chargeFillTransform.SetParent(chargeVisualObj.transform, false);
                chargeFillTransform.localPosition = new Vector3(0f, -0.5f, -0.01f); // bottom aligned
                chargeFillTransform.localScale = new Vector3(1f, 0f, 1f);
                
                SpriteRenderer fillRenderer = fillObj.AddComponent<SpriteRenderer>();
                fillRenderer.sprite = squareSpriteBottom;
                fillRenderer.color = new Color(0.2f, 0.8f, 1f, 0.9f); // Cyan fill
                fillRenderer.sortingOrder = 6;

                currentVisualFill = 0f;
            }
        }

        private void Update()
        {
            if (targetNode == null || boardConfig == null) return;

            Transform target = cachedVisual != null ? cachedVisual : transform;
            
            if (!IsBeingDragged)
            {
                // Lerp towards the logical position smoothly
                Vector3 logicalWorldPos = boardConfig.GetWorldPosition(targetNode.Position);
                
                // If the view is manually dragged away, it will glide back when released.
                // Lerp smooth position
                transform.localPosition = Vector3.Lerp(transform.localPosition, logicalWorldPos, Time.deltaTime * 20f);
            }

            // Lerp smooth rotation
            Quaternion targetRot = Quaternion.Euler(0, 0, -targetNode.CurrentRotation);
            target.localRotation = Quaternion.Lerp(target.localRotation, targetRot, Time.deltaTime * 15f);

            // Update charge fill
            if (chargeFillTransform != null && targetNode is BaseGearNode baseGear && baseGear.ConfigData != null && baseGear.ConfigData.MaxCharge > 0)
            {
                float targetFill = baseGear.CurrentCharge / baseGear.ConfigData.MaxCharge;
                currentVisualFill = Mathf.Lerp(currentVisualFill, targetFill, Time.deltaTime * 10f);
                chargeFillTransform.localScale = new Vector3(1f, currentVisualFill, 1f);
            }
        }
    }
}
