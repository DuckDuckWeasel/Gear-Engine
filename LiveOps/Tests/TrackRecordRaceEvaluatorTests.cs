using System.Collections.Generic;
using GameModule.Modules.Tracks;
using GameModuleDTO.Modules.Tracks;
using GameModuleDTO.ModuleRequests;
using Newtonsoft.Json;
using Xunit;

namespace LiveOps.Tests
{
    public sealed class TrackRecordRaceEvaluatorTests
    {
        [Fact]
        public void Evaluate_SlowerThanAllBands_UsesBaseReward_NotAdvanced()
        {
            TrackConfig config = JsonConvert.DeserializeObject<TrackConfig>(
                "{\"entries\":[{\"id\":\"t1\",\"baseReward\":5,\"bands\":[{\"maxSec\":30,\"r\":100}]}]}");
            Assert.True(config.TryGet("t1", out TrackConfigEntry entry));
            var persistence = new TrackPersistence();

            RecordRaceResultResponse r = TrackRecordRaceEvaluator.Evaluate(entry, config, "t1", 99f, persistence);

            Assert.Equal(-1, r.MatchedBandIndex);
            Assert.Equal(5, r.Reward);
            Assert.False(r.Advanced);
            Assert.Equal(99f, r.NewBestTimeSec);
            Assert.Single(persistence.BestTimeSec);
            Assert.Equal(99f, persistence.BestTimeSec["t1"]);
        }

        [Fact]
        public void Evaluate_MatchesFirstBand_AdvancedAndReward()
        {
            TrackConfig config = JsonConvert.DeserializeObject<TrackConfig>(
                "{\"entries\":[" +
                "{\"id\":\"t1\",\"baseReward\":2,\"bands\":[{\"maxSec\":60,\"r\":50},{\"maxSec\":90,\"r\":10}]}," +
                "{\"id\":\"t2\",\"baseReward\":0,\"bands\":[]}" +
                "]}");
            config.TryGet("t1", out TrackConfigEntry entry);
            var persistence = new TrackPersistence { CurrentTrackId = "t1" };

            RecordRaceResultResponse r = TrackRecordRaceEvaluator.Evaluate(entry, config, "t1", 45f, persistence);

            Assert.Equal(0, r.MatchedBandIndex);
            Assert.Equal(50, r.Reward);
            Assert.True(r.Advanced);
            Assert.Equal("t2", r.NextTrackId);
            Assert.Equal("t2", persistence.CurrentTrackId);
            Assert.Equal(45f, r.NewBestTimeSec);
        }

        [Fact]
        public void Evaluate_ImprovesBestTime()
        {
            TrackConfig config = JsonConvert.DeserializeObject<TrackConfig>(
                "{\"entries\":[{\"id\":\"t1\",\"baseReward\":0,\"bands\":[{\"maxSec\":100,\"r\":1}]}]}");
            config.TryGet("t1", out TrackConfigEntry entry);
            var persistence = new TrackPersistence
            {
                BestTimeSec = new Dictionary<string, float> { ["t1"] = 80f },
            };

            RecordRaceResultResponse r = TrackRecordRaceEvaluator.Evaluate(entry, config, "t1", 90f, persistence);

            Assert.Equal(80f, r.NewBestTimeSec);
            Assert.Equal(80f, persistence.BestTimeSec["t1"]);
        }
    }
}
