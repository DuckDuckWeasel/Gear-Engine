using GameModuleDTO.Modules.Tracks;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class TrackGameDataTests
    {
        [Test]
        public void Ctor_CopiesOrderedTrackIdsFromConfig()
        {
            TrackConfig config = JsonConvert.DeserializeObject<TrackConfig>(
                "{\"entries\":[{\"id\":\"a\",\"advanceScore\":100},{\"id\":\"b\",\"advanceScore\":200}]}");
            var persistence = new TrackPersistence { CurrentTrackId = "a" };

            var data = new TrackGameData(persistence, config);

            Assert.That(data.OrderedTrackIds.Count, Is.EqualTo(2));
            Assert.That(data.OrderedTrackIds[0], Is.EqualTo("a"));
            Assert.That(data.CurrentTrackId, Is.EqualTo("a"));
        }
    }
}
