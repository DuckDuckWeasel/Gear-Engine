using System;
using System.Collections.Generic;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using GearEngine.CarSimulation.PhysicsSimulation;
using GearEngine.CarSimulation.SplineSimulation;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Abilities;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using NUnit.Framework;
using Scaffold.Events.Contracts;
using UnityEditor;
using UnityEngine;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class CampaignBiomeAffinityTests
    {
        [Test]
        public void MatchingGearsBoostOnlyTheirTrackBiome()
        {
            TrackThemeDefinition theme = CreateTheme(TrackBiome.Desert);
            TrackDefinition track = CreateTrack(theme);

            try
            {
                StubEngine engine = new StubEngine(
                    new StubNode(TrackBiome.Desert),
                    new StubNode(TrackBiome.Forest),
                    new StubNode(TrackBiome.Desert));
                CampaignStatsService service = new CampaignStatsService(new RaceSessionConfig(), engine);

                Assert.That(service.GetCalculatedStats(track).SpeedCapability, Is.EqualTo(66f));
                Assert.That(service.GetCalculatedStats().SpeedCapability, Is.EqualTo(50f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(track);
                UnityEngine.Object.DestroyImmediate(theme);
            }
        }

        [Test]
        public void MatchingBonusCapsAtThreeGearsAndStatLimit()
        {
            TrackThemeDefinition theme = CreateTheme(TrackBiome.Glacial);
            TrackDefinition track = CreateTrack(theme);

            try
            {
                StubEngine engine = new StubEngine(
                    new StubNode(TrackBiome.Glacial),
                    new StubNode(TrackBiome.Glacial),
                    new StubNode(TrackBiome.Glacial),
                    new StubNode(TrackBiome.Glacial));
                RaceSessionConfig config = new RaceSessionConfig();
                RoguelikeCarStats stats = RoguelikeCarStats.Default;
                stats.SpeedCapability = 95f;
                config.SetRoguelikeStats(stats);

                Assert.That(new CampaignStatsService(config, engine).GetCalculatedStats(track).SpeedCapability, Is.EqualTo(100f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(track);
                UnityEngine.Object.DestroyImmediate(theme);
            }
        }

        [Test]
        public void AffinitySurvivesRuntimeCloneAndReachesSplinePersonality()
        {
            GearItemData gear = new GearItemData { BiomeAffinity = TrackBiome.Mountain };
            GearItemData copy = gear.Clone(null, null);
            RoguelikeCarStats stats = RoguelikeCarStats.Default;
            stats.SpeedCapability = 74f;

            Assert.That(copy.BiomeAffinity, Is.EqualTo(TrackBiome.Mountain));
            StringAssert.Contains("Mountain affinity", copy.Description);
            Assert.That(DriverPersonality.FromStats(stats).SpeedCapability, Is.EqualTo(74f));
        }

        private static TrackThemeDefinition CreateTheme(TrackBiome biome)
        {
            TrackThemeDefinition theme = ScriptableObject.CreateInstance<TrackThemeDefinition>();
            SerializedObject serialized = new SerializedObject(theme);
            serialized.FindProperty("biome").enumValueIndex = (int)biome;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return theme;
        }

        private static TrackDefinition CreateTrack(TrackThemeDefinition theme)
        {
            TrackDefinition track = ScriptableObject.CreateInstance<TrackDefinition>();
            SerializedObject serialized = new SerializedObject(track);
            serialized.FindProperty("theme").objectReferenceValue = theme;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return track;
        }

        private sealed class StubEngine : IGearEngineService
        {
            private readonly IGridNode[] nodes;

            public StubEngine(params IGridNode[] nodes) { this.nodes = nodes; }
            public bool IsRunning => false;
            public void Play() { }
            public void Stop() { }
            public void ResetGridSimulationState() { }
            public IEnumerable<IGridNode> GetAllNodes() => nodes;
        }

        private sealed class StubNode : IGridNode
        {
            public StubNode(TrackBiome biome) { ConfigData = new GearItemData { BiomeAffinity = biome }; }
            public Vector2Int Position => Vector2Int.zero;
            public float CurrentRotation => 0f;
            public GearItemData ConfigData { get; }
            public float LocalSpeedMultiplier { get; set; }
            public bool IsActive { get; set; } = true;
            public bool IsInteractable => true;
            public IEventBus EventBus => null;
            public void SetPosition(Vector2Int position) { }
            public void AddAbility(GearAbilitySO ability, float duration = -1f) { }
            public void RemoveAbility(GearAbilitySO ability) { }
            public void Initialize(Vector2Int position, GearItemData configData) { }
            public void NodeUpdate(float deltaTime, float speedModifier) { }
            public void WindDownUpdate(float deltaTime, float speedModifier) { }
            public IEnumerable<GearAbilitySO> GetAbilities() => Array.Empty<GearAbilitySO>();
            public void ResetSimulationState() { }
            public void Dispose() { }
        }
    }
}
