using GameModuleDTO.Modules.Tracks;
using GearEngine.Campaign.Services;
using GearEngine.CarSimulation.Definitions;
using Scaffold.LiveOps.Authoring;
using UnityEngine;

namespace GearEngine.Campaign.Authoring
{
    [CreateAssetMenu(menuName = "LiveOps/Authoring/Track Config Builder", fileName = "TrackConfigBuilder")]
    public sealed class TrackConfigBuilderSO : ConfigBuilderSO<TrackConfig>
    {
        [Header("Asset source")]
        [SerializeField]
        private TrackCatalogSO catalog;

        [Header("Asset-independent fields")]
        [SerializeField]
        private int defaultBaseReward = 10;

        public override string ConfigKey => nameof(TrackConfig);

        public override TrackConfig Build()
        {
            var cfg = new TrackConfig();
            if (catalog == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning(
                    "[TrackConfigBuilder] Catalog is not assigned. Assign `CampaignTrackCatalog` (or your `TrackCatalogSO`) on this asset, then use Window → LiveOps → Config Deployment → Sync.",
                    this);
#endif
                return cfg;
            }

            int index = 0;
            foreach (TrackEntry entry in catalog.Entries)
            {
                if (entry?.Track == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[TrackConfigBuilder] Catalog row {index}: TrackDefinition reference is missing — skipped for Remote Config.",
                        this);
#endif
                    index++;
                    continue;
                }

                if (string.IsNullOrEmpty(entry.TrackId))
                {
#if UNITY_EDITOR
                    Debug.LogWarning(
                        $"[TrackConfigBuilder] Catalog row {index}: Track id is empty (rename the track asset so `TrackDefinition.name` is set) — skipped.",
                        this);
#endif
                    index++;
                    continue;
                }

                var dtoEntry = new TrackConfigEntry
                {
                    Id = entry.TrackId,
                    BaseReward = defaultBaseReward,
                };

                foreach (TrackScoreBand band in entry.Track.ScoreBands)
                {
                    dtoEntry.Bands.Add(
                        new TrackScoreBandConfig
                        {
                            MaxRaceTimeSeconds = band.MaxRaceTimeSeconds,
                            Reward = band.RewardValue,
                        });
                }

                cfg.AddEntry(dtoEntry);
                index++;
            }

#if UNITY_EDITOR
            if (cfg.Entries.Count == 0 && catalog.Entries.Count > 0)
            {
                Debug.LogWarning(
                    "[TrackConfigBuilder] Catalog lists track rows but none were exported to TrackConfig (missing Track refs or empty ids). Fix the catalog, then Sync `Track.rc`.",
                    this);
            }
#endif

            return cfg;
        }

        public override void Apply(TrackConfig pulled)
        {
            if (pulled == null || catalog == null)
            {
                return;
            }

            foreach (TrackConfigEntry entry in pulled.Entries)
            {
                if (catalog.GetTrack(entry.Id) == null)
                {
                    continue;
                }

                defaultBaseReward = entry.BaseReward;
            }
        }
    }
}
