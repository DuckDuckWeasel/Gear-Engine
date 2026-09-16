using GearEngine.CarSimulation.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class RaceResultModelTests
    {
        [Test]
        public void WhenTrackHasTiers_ScoreAndGoldMatchTierReward()
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithTiersForTests(
                new TrackTierConfig(30f, 1000, 900),
                new TrackTierConfig(15f, 2000, 100));

            try
            {
                RaceResultModel result = new RaceResultModel(raceTime: 20f, lapCount: 3, track, driftScore: 1200);

                Assert.That(result.HighestAchievedTier, Is.EqualTo(1));
                Assert.That(result.Gold.Amount, Is.EqualTo(910));
                Assert.That(result.IsGoodResult, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(track);
            }
        }

        [Test]
        public void WhenTrackHasNoTiers_UsesLegacyScoreAndScaledGold()
        {
            TrackDefinition track = ScriptableObject.CreateInstance<TrackDefinition>();

            try
            {
                RaceResultModel result = new RaceResultModel(raceTime: 10f, lapCount: 1, track, driftScore: 0);

                Assert.That(result.HighestAchievedTier, Is.EqualTo(0));
                Assert.That(result.Gold.Amount, Is.EqualTo(4500));
            }
            finally
            {
                Object.DestroyImmediate(track);
            }
        }
    }
}
