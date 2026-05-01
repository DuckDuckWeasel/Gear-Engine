using System;
using GearEngine.GearEngine.Visuals;
using Scaffold.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GearEngine.Campaign.Presentation
{
    public sealed class CardOptionView : ViewComponent<CardOptionViewModel>
    {
        [SerializeField] private TextMeshProUGUI nameLabel;
        [SerializeField] private Button selectButton;
        [SerializeField] private Transform visualContainer;

        private ItemView spawnedVisual;

        protected override void OnBind()
        {
            base.OnBind();
            ApplyNameLabel();
            SubscribeSelectButton();
            SpawnVisual();
        }

        protected override void OnUnbind()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveListener(OnSelectClicked);
            }

            ClearVisual();
            base.OnUnbind();
        }

        private void ApplyNameLabel()
        {
            if (nameLabel != null)
            {
                nameLabel.text = viewModel.Config.name;
            }
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
                Debug.LogError($"[CardOptionView] OnSelectClicked failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SpawnVisual()
        {
            if (visualContainer != null && viewModel?.Config != null)
            {
                ClearVisual();
                var runtimeData = viewModel.Config.CreateRuntimeData();
                spawnedVisual = GearViewSpawner.Spawn(runtimeData, visualContainer);
            }
        }

        private void ClearVisual()
        {
            if (spawnedVisual != null)
            {
                Destroy(spawnedVisual.gameObject);
                spawnedVisual = null;
            }
        }
    }
}
