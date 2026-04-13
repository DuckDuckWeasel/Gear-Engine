using Sirenix.OdinInspector;
using UnityEngine;

namespace GearEngine.GearEngine.Visuals
{
    public class GearView : SerializedMonoBehaviour
    {
        public IGridNode TargetNode => targetNode;

        [SerializeField]
        private IGridNode targetNode;

        [ShowInInspector]
        public bool IsBeingDragged { get; set; } = false;

        [SerializeField]
        private Transform cachedVisual;
        [SerializeField]
        private BoardConfigSO boardConfig;
        private float baseRotationOffset = 0f;
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

        private void OnDestroy()
        {
            if (ownerFactory != null && targetNode != null)
            {
                ownerFactory.UnregisterView(targetNode);
            }
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
                chargeFillRenderer = null;
            }
        }

        private void SetupChargeVisual(GearConfigData configData)
        {
            if (targetNode is not BaseGearNode || configData == null || configData.MaxCharge <= 0)
            {
                return;
            }

            EnsureSquareSpritesExist();
            CreateChargeVisualObjects(configData);
        }

        private void Update()
        {
            if (targetNode == null || boardConfig == null)
            {
                return;
            }

            Transform target = cachedVisual != null ? cachedVisual : transform;

            if (!IsBeingDragged)
            {
                Vector3 logicalWorldPos = boardConfig.GetWorldPosition(targetNode.Position);
                transform.localPosition = Vector3.Lerp(transform.localPosition, logicalWorldPos, Time.deltaTime * 20f);
            }

            Quaternion targetRot = Quaternion.Euler(0, 0, (-targetNode.CurrentRotation) + baseRotationOffset);
            target.localRotation = Quaternion.Lerp(target.localRotation, targetRot, Time.deltaTime * 15f);

            UpdateChargeFillDisplay();
        }

        private void EnsureSquareSpritesExist()
        {
            if (squareSpriteCenter == null)
            {
                squareSpriteCenter = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            }

            if (squareSpriteBottom == null)
            {
                squareSpriteBottom = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0f), 4f);
            }
        }

        private void CreateChargeVisualObjects(GearConfigData configData)
        {
            chargeVisualObj = new GameObject("ChargeVisual");
            chargeFillTransform = chargeVisualObj.transform;
            chargeVisualObj.transform.SetParent(transform, false);
            chargeVisualObj.transform.localPosition = new Vector3(0, 0, -0.5f);
            chargeVisualObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            chargeFillRenderer = chargeVisualObj.AddComponent<SpriteRenderer>();
            chargeFillRenderer.sprite = configData.UIIcon != null ? configData.UIIcon : squareSpriteCenter;
            chargeFillRenderer.color = new Color(0.2f, 0.8f, 1f, 0.9f);
            chargeFillRenderer.sortingOrder = 6;
            BuildChargeFillMaterialForRenderer(chargeFillRenderer);
            currentVisualFill = 0f;
        }

        private void UpdateChargeFillDisplay()
        {
            if (chargeFillRenderer == null || targetNode is not BaseGearNode baseGear || baseGear.ConfigData == null || baseGear.ConfigData.MaxCharge <= 0)
            {
                return;
            }

            float targetFill = baseGear.CurrentCharge / baseGear.ConfigData.MaxCharge;
            currentVisualFill = Mathf.Lerp(currentVisualFill, targetFill, Time.deltaTime * 10f);

            if (chargeFillRenderer.material != null)
            {
                chargeFillRenderer.material.SetFloat("_FillAmount", currentVisualFill);
            }
        }

        private static void BuildChargeFillMaterialForRenderer(SpriteRenderer renderer)
        {
            Shader fillShader = Shader.Find("GearEngine/Sprites/SpriteFillGrayscale");
            if (fillShader == null)
            {
                return;
            }

            Material mat = new Material(fillShader);
            mat.SetFloat("_FillAmount", 0f);
            renderer.material = mat;
        }
    }
}
