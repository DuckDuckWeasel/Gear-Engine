using System;
using GearEngine.CarSimulation.Definitions;
using Scaffold.MVVM;

namespace GearEngine.Campaign.Presentation
{
    public sealed class TrackScoreBandViewModel : ViewModel
    {
        public TrackScoreBandViewModel(int position, TrackScoreBand band)
        {
            if (band == null)
            {
                throw new ArgumentNullException(nameof(band));
            }

            if (position < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(position));
            }

            Position = position;
            MaxRaceTimeSeconds = band.MaxRaceTimeSeconds;
            RewardValue = band.RewardValue;
        }

        public int Position { get; }

        public float MaxRaceTimeSeconds { get; }

        public int RewardValue { get; }
    }
}
