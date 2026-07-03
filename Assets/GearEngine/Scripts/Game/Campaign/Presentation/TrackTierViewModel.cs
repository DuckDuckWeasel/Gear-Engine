using System;
using GearEngine.CarSimulation.Definitions;
using Scaffold.MVVM;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GearEngine.Campaign.Presentation
{
    public sealed partial class TrackTierViewModel : ViewModel
    {
        public TrackTierViewModel(int tierNumber, TrackTierConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            TierNumber = tierNumber;
            TargetTimeSeconds = config.TargetTimeSeconds;
            TargetScore = config.TargetScore;
            GoldReward = config.GoldReward;
        }

        public int TierNumber { get; }
        public float TargetTimeSeconds { get; }
        public int TargetScore { get; }
        public int GoldReward { get; }

        public string TargetDescription => $"< {TargetTimeSeconds:F1}s OR > {TargetScore} pts";
        public string RewardDescription => $"{GoldReward} Gold";
    }
}
