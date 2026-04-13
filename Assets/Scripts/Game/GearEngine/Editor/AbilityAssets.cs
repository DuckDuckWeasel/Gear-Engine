using GearEngine.GearEngine.Abilities;

namespace GearEngine.GearEngine.Editor
{
    internal readonly struct AbilityAssets
    {
        public AbilityAssets(DestroySelfAbility d, ScoreAbility s, SpeedBoostAbility sp)
        {
            DestroySelf = d;
            Score = s;
            SpeedBoost = sp;
        }

        public readonly DestroySelfAbility DestroySelf;
        public readonly ScoreAbility Score;
        public readonly SpeedBoostAbility SpeedBoost;
    }
}
