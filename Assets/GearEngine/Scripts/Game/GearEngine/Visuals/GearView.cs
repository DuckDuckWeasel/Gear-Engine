using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GearEngine.GearEngine.Visuals
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
        

        
        private float baseRotationOffset = 0f;
        private Vector2Int lastKnownPosition = new Vector2Int(-999, -999);

        [SerializeField]
        private GameObject chargeVisualObj;
        [SerializeField]
        private Transform chargeFillTransform;
        [SerializeField]
        private SpriteRenderer chargeFillRenderer;
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

        private GearViewFactory ownerFactory;

        public void Initialize(IGridNode node, GearConfigData configData, BoardConfigSO config, GearViewFactory factory)
        {
            targetNode = node;
            boardConfig = config;
            ownerFactory = factory;

            RecalculateRotationOffset();

            SetupVisual(configData);
            SetupChargeVisual(configData);
        }

        private void OnDestroy()
        {
            if (ownerFactory != null && targetNode != null)
            {
                ownerFactory.UnregisterView(targetNode);
            }
        }

        public void RecalculateRotationOffset()
        {
            baseRotationOffset = 0f;
            if (targetNode != null && boardConfig != null && (targetNode.Position.x + targetNode.Position.y) % 2 == 0)
            {
                baseRotationOffset = boardConfig.StaggeredRotationOffset;
            }
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
                float finalScale = configData.RelativeScaleMultiplier;
                cachedVisual.localScale = new Vector3(finalScale, finalScale, finalScale);
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
                chargeFillRenderer = null;
            }
        }

        private void SetupChargeVisual(GearConfigData configData)
        {
            if (configData == null)
            {
                return;
            }

            bool hasIcon = configData.UIIcon != null;
            bool hasCharge = targetNode is BaseGearNode && configData.MaxCharge > 0;

            if (!hasIcon && !hasCharge)
            {
                return;
            }

            // Attempt to link to an existing prefab component first to avoid code-driven setup
            if (cachedVisual != null)
            {
                Transform existingCharge = cachedVisual.Find("ChargeVisual");
                if (existingCharge != null)
                {
                    chargeVisualObj = existingCharge.gameObject;
                    chargeFillTransform = existingCharge;
                    chargeFillRenderer = existingCharge.GetComponent<SpriteRenderer>();
                    
                    // The charge view must be a child of the standard parent (GearView), not the rotating basic gear prefab!
                    existingCharge.SetParent(transform, true);
                }
            }

            // Fallback: Create dynamically ONLY if not found in the prefab
            if (chargeVisualObj == null)
            {
                InitSprites();

                chargeVisualObj = new GameObject("ChargeVisual");
                chargeFillTransform = chargeVisualObj.transform;
                chargeVisualObj.transform.SetParent(transform, false);
                chargeVisualObj.transform.localPosition = new Vector3(0, 0, -0.5f); // In front of gear
                chargeVisualObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                
                chargeFillRenderer = chargeVisualObj.AddComponent<SpriteRenderer>();
                chargeFillRenderer.sortingOrder = 6;
                chargeFillRenderer.sprite = hasIcon ? configData.UIIcon : squareSpriteCenter;
                chargeFillRenderer.color = hasIcon ? Color.white : new Color(0.2f, 0.8f, 1f, 0.9f); // Cyan fill for geometry fallback

                // Only apply fill shader natively if we built it from code
                if (hasCharge)
                {
                    Shader fillShader = Shader.Find("GearEngine/Sprites/SpriteFillGrayscale");
                    if (fillShader != null)
                    {
                        Material mat = new Material(fillShader);
                        mat.SetFloat("_FillAmount", 0f);
                        chargeFillRenderer.material = mat;
                    }
                }
            }
            else
            {
                // If found in prefab, simply override the sprite for dynamic UI icons, respecting the prefab's shader/color setup
                if (hasIcon && chargeFillRenderer != null)
                {
                    chargeFillRenderer.sprite = configData.UIIcon;
                }
            }

            if (hasCharge)
            {
                currentVisualFill = 0f;
            }
        }

        private void Update()
        {
            if (targetNode == null || boardConfig == null) return;

            if (targetNode.Position != lastKnownPosition)
            {
                lastKnownPosition = targetNode.Position;
                RecalculateRotationOffset();
            }

            Transform target = transform;
            if (cachedVisual != null)
            {
                Transform gearChild = cachedVisual.Find("GearVisual");
                target = gearChild != null ? gearChild : cachedVisual;
            }

            // Lerp towards the logical position smoothly
            Vector3 logicalWorldPos = boardConfig.GetWorldPosition(targetNode.Position);
            transform.localPosition = Vector3.Lerp(transform.localPosition, logicalWorldPos, Time.deltaTime * 20f);

            // Lerp smooth rotation including base stagger offset
            // We flip the rotation because Unity 2D standard rotates counter-clockwise with positive Z
            Quaternion targetRot = Quaternion.Euler(0, 0, (-targetNode.CurrentRotation) + baseRotationOffset);
            target.localRotation = Quaternion.Lerp(target.localRotation, targetRot, Time.deltaTime * 15f);

            // Update charge fill
            if (chargeFillRenderer != null && targetNode is BaseGearNode baseGear && baseGear.ConfigData != null && baseGear.ConfigData.MaxCharge > 0)
            {
                float targetFill = baseGear.CurrentCharge / baseGear.ConfigData.MaxCharge;
                currentVisualFill = Mathf.Lerp(currentVisualFill, targetFill, Time.deltaTime * 10f);
                
                if (chargeFillRenderer.material != null)
                {
                    chargeFillRenderer.material.SetFloat("_FillAmount", currentVisualFill);
                }
            }
        }
    }
}
