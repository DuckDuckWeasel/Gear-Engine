using System;
using GearEngine.GearEngine.Config;
using Scaffold.MVVM;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GearEngine.GearEngine.Presentation.UI
{
    public partial class TrashZoneViewModel : ViewModel
    {
        [ObservableProperty]
        private bool isActive;

        [ObservableProperty]
        private string rewardText = string.Empty;

        public void HandleDragStarted(object data)
        {
            if (data is GearConfigData gearData && gearData.IsDeletable)
            {
                RewardText = $"+{gearData.DeleteRewardAmount}";
                IsActive = true;
            }
            else
            {
                IsActive = false;
            }
        }

        public void HandleDragEnded()
        {
            IsActive = false;
        }

        public event Action<GearConfigData> OnGearDropped;

        public void HandleGearDropped(GearConfigData data)
        {
            OnGearDropped?.Invoke(data);
            HandleDragEnded();
        }
    }
}
