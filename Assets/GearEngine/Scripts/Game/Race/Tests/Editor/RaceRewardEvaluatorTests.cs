using GearEngine.CarSimulation.Definitions;
using GearEngine.Race.Rewards;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.Race.Tests.Editor
{
    public sealed class RaceRewardEvaluatorTests
    {
        [Test]
        public void Evaluate_PicksStrictestQualifyingBracket()
        {
            var track = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                typeof(TrackDefinition).GetField("scoreBrackets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(track, new[]
                    {
                        new RaceScoreBracket("Bronze", 120f, 10),
                        new RaceScoreBracket("Gold", 60f, 100),
                        new RaceScoreBracket("Silver", 90f, 50),
                    });

                RaceRewardEvaluation r = RaceRewardEvaluator.Evaluate(track, finishTimeSeconds: 70f, lapsCompleted: 3);

                Assert.That(r.MatchedBracket, Is.True);
                Assert.That(r.RankId, Is.EqualTo("Silver"));
                Assert.That(r.GoldReward, Is.EqualTo(50));
            }
            finally
            {
                Object.DestroyImmediate(track);
            }
        }

        [Test]
        public void Evaluate_NoBracket_WhenTooSlow()
        {
            var track = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                typeof(TrackDefinition).GetField("scoreBrackets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(track, new[] { new RaceScoreBracket("Gold", 60f, 100) });

                RaceRewardEvaluation r = RaceRewardEvaluator.Evaluate(track, finishTimeSeconds: 61f, lapsCompleted: 3);

                Assert.That(r.MatchedBracket, Is.False);
                Assert.That(r.GoldReward, Is.EqualTo(0));
            }
            finally
            {
                Object.DestroyImmediate(track);
            }
        }

        [Test]
        public void Evaluate_NoBracket_WhenLapsIncomplete()
        {
            var track = ScriptableObject.CreateInstance<TrackDefinition>();
            try
            {
                typeof(TrackDefinition).GetField("scoreBrackets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(track, new[] { new RaceScoreBracket("Gold", 60f, 100) });

                RaceRewardEvaluation r = RaceRewardEvaluator.Evaluate(track, finishTimeSeconds: 30f, lapsCompleted: 1);

                Assert.That(r.MatchedBracket, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(track);
            }
        }
    }
}
