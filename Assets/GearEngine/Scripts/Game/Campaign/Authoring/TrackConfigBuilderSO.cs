using System;
using System.Collections.Generic;
using LiveOps.Modules.DTO.Tracks;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using Scaffold.LiveOps.Authoring;
using UnityEngine;

namespace GearEngine.Campaign.Authoring
{
    [CreateAssetMenu(menuName = "LiveOps/Authoring/Track Config Builder", fileName = "TrackConfigBuilder")]
    public sealed class TrackConfigBuilderSO : ConfigBuilderSO<TrackConfig>
    {
        [Header("Asset source (same tracks as the liveops.tracks addressable label at runtime)")]
        [Tooltip("Assign the track definitions to export (Oval, Circle, …). Must match the label-driven TrackDefinition list in Addressables / Asset publishers.")]
        [SerializeField]
        private List<TrackDefinition> trackDefinitions = new();

        [Header("Asset-independent fields")]
        [SerializeField]
        private int defaultBaseReward = 10;

        public override string ConfigKey => nameof(TrackConfig);

        public override TrackConfig Build()
        {
            var cfg = new TrackConfig();
            if (trackDefinitions == null || trackDefinitions.Count == 0)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    "[TrackConfigBuilder] No `trackDefinitions` assigned. Add your `TrackDefinition` assets (the same as under label liveops.tracks), then use Window → LiveOps → Configs → Sync.",
                    this);
#endif
                return cfg;
            }

            int index = 0;
            foreach (TrackDefinition track in trackDefinitions)
            {
                if (track == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[TrackConfigBuilder] List row {index}: TrackDefinition reference is missing — skipped for Remote Config.",
                        this);
#endif
                    index++;
                    continue;
                }

                if (string.IsNullOrEmpty(track.name))
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[TrackConfigBuilder] List row {index}: Track id is empty (asset `name` must be set) — skipped.",
                        this);
#endif
                    index++;
                    continue;
                }

                var dtoEntry = new TrackConfigEntry
                {
                    Id = track.name,
                    BaseReward = defaultBaseReward,
                };

                if (track.ScoreBands != null)
                {
                    foreach (TrackScoreBand band in track.ScoreBands)
                    {
                        dtoEntry.Bands.Add(
                            new TrackScoreBandConfig
                            {
                                MaxRaceTimeSeconds = band.MaxRaceTimeSeconds,
                                Reward = band.RewardValue,
                            });
                    }
                }

                cfg.AddEntry(dtoEntry);
                index++;
            }

#if UNITY_EDITOR
            if (cfg.Entries.Count == 0 && trackDefinitions.Count > 0)
            {
                Debug.LogWarning(
                    "[TrackConfigBuilder] Lists track rows but none were exported to TrackConfig (missing `TrackDefinition` or empty names). Fix the list, then Sync in Window → LiveOps → Configs.",
                    this);
            }
#endif

            return cfg;
        }

        public override void Apply(TrackConfig pulled)
        {
            if (pulled == null)
            {
                return;
            }

            var byId = new Dictionary<string, TrackDefinition>(StringComparer.Ordinal);
            if (trackDefinitions != null)
            {
                foreach (TrackDefinition t in trackDefinitions)
                {
                    if (t == null || string.IsNullOrEmpty(t.name) || byId.ContainsKey(t.name))
                    {
                        continue;
                    }

                    byId[t.name] = t;
                }
            }

            foreach (TrackConfigEntry entry in pulled.Entries)
            {
                if (byId.TryGetValue(entry.Id, out TrackDefinition t))
                {
                    _ = t;
                    defaultBaseReward = entry.BaseReward;
                }
            }
        }
    }
}
