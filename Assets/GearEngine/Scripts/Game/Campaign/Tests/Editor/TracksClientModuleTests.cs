using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign;
using GearEngine.Campaign.Bootstrap.LiveOps;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using GearEngine.Currency;
using LiveOps.DTO.GameModule;
using LiveOps.DTO.ModuleRequest;
using LiveOps.Modules.DTO.Currency;
using LiveOps.Modules.DTO.ModuleRequests;
using LiveOps.Modules.DTO.Tracks;
using NUnit.Framework;
using Scaffold.LiveOps;
using UnityEngine;

namespace GearEngine.Campaign.Tests.Editor
{
    /// <summary>
    /// Regression coverage for the online code path of <see cref="TracksClientModule"/>. These tests
    /// exercise the assumptions the offline-stub work depends on: that LiveOps always delivers a
    /// non-null <see cref="TrackGameData"/> so the module can repair / read it without null guards.
    /// </summary>
    public sealed class TracksClientModuleTests
    {
        [Test]
        public void OnInitialized_WithValidCurrentTrackId_ExposesThatTrack()
        {
            using var ctx = new Ctx("track_01", new[] { "track_01", "track_02", "track_03" });

            TrackDefinition resolved = ctx.Module.CurrentTrack;

            Assert.That(resolved, Is.SameAs(ctx.Tracks["track_01"]));
            Assert.That(ctx.Module.GetTrackProgress().CurrentTrackIndex, Is.EqualTo(0));
        }

        [Test]
        public void OnInitialized_WithUnknownCurrentTrackId_RepairsToFirstOrderedInCatalog()
        {
            using var ctx = new Ctx("track_missing", new[] { "track_missing", "track_02", "track_03" });

            Assert.That(ctx.Module.CurrentTrack, Is.SameAs(ctx.Tracks["track_02"]));
            Assert.That(ctx.Data.CurrentTrackId, Is.EqualTo("track_02"));
        }

        [Test]
        public void OnInitialized_WithEmptyCurrentTrackId_RepairsToFirstResolvable()
        {
            using var ctx = new Ctx(string.Empty, new[] { "track_01", "track_02" });

            Assert.That(ctx.Module.CurrentTrack, Is.SameAs(ctx.Tracks["track_01"]));
            Assert.That(ctx.Data.CurrentTrackId, Is.EqualTo("track_01"));
        }

        [Test]
        public void GetOrderedTracks_ReturnsEntriesFromModuleData()
        {
            using var ctx = new Ctx("track_02", new[] { "track_01", "track_02", "track_03" });

            IReadOnlyList<TrackEntry> ordered = ctx.Module.GetOrderedTracks();

            Assert.That(ordered.Count, Is.EqualTo(3));
            Assert.That(ctx.Module.GetTrackProgress().CurrentTrackIndex, Is.EqualTo(1));
        }

        [Test]
        public async Task RecordResultAsync_AdvancesCurrentTrack_WhenResponseHasNextTrackId()
        {
            using var ctx = new Ctx("track_01", new[] { "track_01", "track_02", "track_03" });
            ctx.LiveOps.NextRecordResponse = new RecordRaceResultResponse
            {
                NewBestTimeSec = 9.5f,
                NextTrackId = "track_02",
            };

            await ctx.Module.RecordResultAsync(new RaceResultModel(9.5f, lapCount: 1, track: ctx.Tracks["track_01"]));

            Assert.That(ctx.Data.CurrentTrackId, Is.EqualTo("track_02"));
            Assert.That(ctx.Module.GetTrackProgress().CurrentTrackIndex, Is.EqualTo(1));
            Assert.That(ctx.Data.BestTimeSec["track_01"], Is.EqualTo(9.5f));
        }

        private sealed class Ctx : IDisposable
        {
            public Ctx(string currentTrackId, IReadOnlyList<string> orderedIds)
            {
                var trackList = new List<TrackDefinition>();
                Tracks = new Dictionary<string, TrackDefinition>();
                foreach (string id in new[] { "track_01", "track_02", "track_03" })
                {
                    var t = ScriptableObject.CreateInstance<TrackDefinition>();
                    t.name = id;
                    Tracks[id] = t;
                    trackList.Add(t);
                }

                car = ScriptableObject.CreateInstance<CarDefinition>();
                var index = new TrackAssetIndex(trackList, car);

                Data = BuildData(currentTrackId, orderedIds);
                LiveOps = new FakeLiveOps { Track = Data };

                var currency = new CurrencyClientModule(LiveOps);
                Module = new TracksClientModule(LiveOps, currency, index);
                Module.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
            }

            public TracksClientModule Module { get; }
            public FakeLiveOps LiveOps { get; }
            public TrackGameData Data { get; }
            public Dictionary<string, TrackDefinition> Tracks { get; }

            private readonly CarDefinition car;

            public void Dispose()
            {
                foreach (TrackDefinition t in Tracks.Values)
                {
                    UnityEngine.Object.DestroyImmediate(t);
                }

                if (car != null)
                {
                    UnityEngine.Object.DestroyImmediate(car);
                }
            }

            private static TrackGameData BuildData(string currentTrackId, IReadOnlyList<string> orderedIds)
            {
                string ordered = string.Join(",", orderedIds);
                string json = $"{{\"currentTrackId\":\"{currentTrackId}\",\"orderedTrackIds\":[\"{ordered.Replace(",", "\",\"")}\"],\"bestTimeSec\":{{}}}}";
                return Newtonsoft.Json.JsonConvert.DeserializeObject<TrackGameData>(json);
            }
        }

        private sealed class FakeLiveOps : ILiveOpsService
        {
            public TrackGameData Track { get; set; }
            public RecordRaceResultResponse NextRecordResponse { get; set; }

            public T GetModuleData<T>() where T : class, IGameModuleData
            {
                if (typeof(T) == typeof(TrackGameData))
                {
                    return Track as T;
                }

                if (typeof(T) == typeof(CurrencyGameData))
                {
                    return null;
                }

                throw new InvalidOperationException($"Test fake has no module for {typeof(T).Name}.");
            }

            public Task<TResponse> CallAsync<TResponse>(ModuleRequest<TResponse> request, CancellationToken cancellationToken = default)
                where TResponse : ModuleResponse
            {
                if (request is RecordRaceResultRequest)
                {
                    return Task.FromResult((TResponse)(object)NextRecordResponse);
                }

                throw new InvalidOperationException($"Test fake has no handler for {request?.GetType().Name}.");
            }
        }
    }
}
