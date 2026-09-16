using System;

namespace GearEngine.Campaign
{
    public sealed class RaceStandingEntry
    {
        public RaceStandingEntry(string name, float timeSeconds, bool isPlayer = false)
        {
            Name = name;
            TimeSeconds = timeSeconds;
            IsPlayer = isPlayer;
        }

        public string Name { get; }
        public float TimeSeconds { get; }
        public bool IsPlayer { get; }
        public string FormattedTime => $"{(int)TimeSpan.FromSeconds(TimeSeconds).TotalMinutes:00}:{TimeSpan.FromSeconds(TimeSeconds).Seconds:00}.{(int)(TimeSeconds * 100) % 100:00}";
    }
}
