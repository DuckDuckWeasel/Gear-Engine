using System;

namespace GearEngine.Campaign
{
    public sealed class RaceStandingEntry
    {
        public RaceStandingEntry(string name, float timeSeconds, bool isPlayer = false, bool hasRecordedTime = true)
        {
            Name = name;
            TimeSeconds = timeSeconds;
            IsPlayer = isPlayer;
            HasRecordedTime = hasRecordedTime;
        }

        public string Name { get; }
        public float TimeSeconds { get; }
        public bool IsPlayer { get; }
        public bool HasRecordedTime { get; }
        public string FormattedTime => !HasRecordedTime ? "--:--.--" : $"{(int)TimeSpan.FromSeconds(TimeSeconds).TotalMinutes:00}:{TimeSpan.FromSeconds(TimeSeconds).Seconds:00}.{(int)(TimeSeconds * 100) % 100:00}";
    }
}
