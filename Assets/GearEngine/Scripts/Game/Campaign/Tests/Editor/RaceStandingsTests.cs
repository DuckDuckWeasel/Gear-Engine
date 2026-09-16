using System.Linq;
using GearEngine.CarSimulation.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class RaceStandingsTests
    {
        [TestCase(50f, 1, 3)]
        [TestCase(63f, 2, 3)]
        [TestCase(70f, 3, 3)]
        [TestCase(80f, 4, 4)]
        [TestCase(60f, 2, 3)]
        public void TimeDeterminesPositionAndVisibleRows(float time, int position, int visible)
        {
            RaceStandingsModel standings = new RaceStandingsModel(time, null);
            Assert.That(standings.PlayerPosition, Is.EqualTo(position));
            Assert.That(standings.VisibleRowCount, Is.EqualTo(visible));
            Assert.That(standings.Entries.Count(entry => entry.IsPlayer), Is.EqualTo(1));
            Assert.That(standings.Entries.Take(visible).Count(entry => entry.IsPlayer), Is.EqualTo(1));
        }

        [TestCase(80f, 70f, 4, 3)]
        [TestCase(71f, 70f, 3, 3)]
        [TestCase(80f, 50f, 4, 1)]
        public void PreviousTimeDefinesAnimationStart(float previous, float time, int before, int after)
        {
            RaceStandingsModel standings = new RaceStandingsModel(time, null, previous);
            Assert.That(standings.PreviousPosition, Is.EqualTo(before));
            Assert.That(standings.PlayerPosition, Is.EqualTo(after));
        }

        [TestCase(80f, 5000, 4, 3, true)]
        [TestCase(50f, 0, 1, 0, true)]
        [TestCase(80f, 0, 4, 0, false)]
        [TestCase(50f, 5000, 1, 3, true)]
        public void StarsAndGearEligibilityAreIndependentOfPlacement(float time, int score, int position, int stars, bool gear)
        {
            TrackDefinition track = CampaignTestUtilities.CreateTrackWithTiersForTests(
                new TrackTierConfig(25f, 5000, 300), new TrackTierConfig(33f, 3000, 200), new TrackTierConfig(40f, 1000, 100));
            try
            {
                RaceResultModel result = new RaceResultModel(time, 3, track, score);
                Assert.That(result.Standings.PlayerPosition, Is.EqualTo(position));
                Assert.That(result.HighestAchievedTier, Is.EqualTo(stars));
                Assert.That(result.HasGearReward, Is.EqualTo(gear));
                Assert.That(result.Tiers.Select(tier => tier.TargetScore), Is.Ordered);
            }
            finally { Object.DestroyImmediate(track); }
        }

        [Test]
        public void RivalsAreStableAcrossRuns()
        {
            RaceStandingsModel first = new RaceStandingsModel(45f, null);
            RaceStandingsModel next = new RaceStandingsModel(90f, null);
            Assert.That(first.Entries.Where(entry => !entry.IsPlayer).Select(entry => entry.FormattedTime),
                Is.EqualTo(next.Entries.Where(entry => !entry.IsPlayer).Select(entry => entry.FormattedTime)));
        }
    }
}
