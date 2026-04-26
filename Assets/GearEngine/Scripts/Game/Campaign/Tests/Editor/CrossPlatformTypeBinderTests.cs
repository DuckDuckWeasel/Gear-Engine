using LiveOps.DTO.Json;
using NUnit.Framework;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class CrossPlatformTypeBinderTests
    {
        [Test]
        public void BindToType_AcceptsGameLiveOpsTracksDtoAssemblyForTrackGameData()
        {
            var binder = new CrossPlatformTypeBinder();
            System.Type resolved = binder.BindToType(
                "Game.LiveOps.Tracks.DTO",
                "LiveOps.Modules.DTO.Tracks.TrackGameData");
            Assert.That(resolved, Is.Not.Null);
            Assert.That(resolved.FullName, Is.EqualTo("LiveOps.Modules.DTO.Tracks.TrackGameData"));
        }
    }
}
