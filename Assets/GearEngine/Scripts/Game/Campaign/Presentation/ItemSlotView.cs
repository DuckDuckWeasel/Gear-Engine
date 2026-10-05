using System;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GearEngine.GearEngine.Config;

namespace GearEngine.Campaign.Presentation
{
    public sealed class ItemSlotView : ViewComponent<ItemSlotViewModel>
    {
        private static readonly Color32 s_readableTitleColor = new Color32(44, 57, 69, 255);

        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private TextMeshProUGUI descriptionLabel;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button selectButton;
        [SerializeField] private Material grayscaleMaterial;
        [SerializeField] private Image rarityBackgroundImage;
        [SerializeField] private bool useRarityTextColor = true;
        [SerializeField] private bool useAuthoredTitleLayout;

        [SerializeField] private Image[] rarityStars = Array.Empty<Image>();
        [SerializeField] private Sprite filledRarityStar;
        [SerializeField] private Sprite emptyRarityStar;

        protected override void OnBind()
        {
            base.OnBind();
            if (viewModel?.Item == null)
            {
                return;
            }

            ApplyPerkData();
            for (int i = 0; i < rarityStars.Length; i++)
            {
                rarityStars[i].sprite = i <= (int)viewModel.Item.Rarity ? filledRarityStar : emptyRarityStar;
            }
            SubscribeSelectButton();
        }

        protected override void OnUnbind()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(OnSelectClicked);
            }
            base.OnUnbind();
        }

        private void ApplyPerkData()
        {
            if (viewModel?.Item == null)
            {
                return;
            }

            RarityConfigSO visualConfig = viewModel.Item.RarityConfig;

            if (nameLabel != null)
            {
                ConfigureNameLabel(visualConfig);

                if (viewModel.Amount > 1)
                {
                    nameLabel.text = $"x{viewModel.Amount} {viewModel.Item.Name}";
                }
                else
                {
                    nameLabel.text = viewModel.Item.Name;
                }
            }

            if (descriptionLabel != null)
            {
                descriptionLabel.text = viewModel.Item.Description;
            }

            if (iconImage != null && viewModel.Item.Icon != null)
            {
                iconImage.sprite = viewModel.Item.Icon;
                iconImage.rectTransform.localScale = Vector3.one * viewModel.IconScale;
                iconImage.gameObject.SetActive(true);
            }
            else if (iconImage != null)
            {
                iconImage.gameObject.SetActive(false);
            }

            if (rarityBackgroundImage != null)
            {
                if (visualConfig != null && visualConfig.CardSprite != null)
                {
                    rarityBackgroundImage.sprite = visualConfig.CardSprite;
                }
                rarityBackgroundImage.color = Color.white;
            }

            Image[] allImages = GetComponentsInChildren<Image>(true);
            foreach (Image img in allImages)
            {
                if (viewModel.IsOwned)
                {
                    img.material = null;
                }
                else if (grayscaleMaterial != null)
                {
                    img.material = grayscaleMaterial;
                }
            }
        }

        private void ConfigureNameLabel(RarityConfigSO visualConfig)
        {
            if (!useAuthoredTitleLayout)
            {
                RectTransform labelRect = nameLabel.rectTransform;
                labelRect.anchorMin = new Vector2(0f, 1f);
                labelRect.anchorMax = new Vector2(1f, 1f);
                labelRect.anchoredPosition = new Vector2(0f, -75f);
                labelRect.sizeDelta = new Vector2(-80f, 105f);
            }

            Color preferredColor = useRarityTextColor && visualConfig != null
                ? visualConfig.TextColor
                : s_readableTitleColor;
            nameLabel.color = GetReadableTitleColor(preferredColor);
            nameLabel.fontStyle = FontStyles.Bold;
            nameLabel.alignment = TextAlignmentOptions.Center;
            nameLabel.enableAutoSizing = true;
            nameLabel.fontSizeMin = 20f;
            nameLabel.fontSizeMax = 30f;
            nameLabel.textWrappingMode = TextWrappingModes.Normal;
            nameLabel.overflowMode = TextOverflowModes.Overflow;
        }

        private static Color GetReadableTitleColor(Color preferredColor)
        {
            float luminance = 0.2126f * preferredColor.linear.r +
                0.7152f * preferredColor.linear.g +
                0.0722f * preferredColor.linear.b;
            return luminance > 0.35f ? s_readableTitleColor : preferredColor;
        }

        private void SubscribeSelectButton()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(OnSelectClicked);
                Bind<bool, bool>(() => viewModel.CanPick, interactable => selectButton.interactable = interactable);
            }
        }

        private void OnSelectClicked()
        {
            try
            {
                viewModel?.Pick();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ItemSlotView] OnSelectClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
