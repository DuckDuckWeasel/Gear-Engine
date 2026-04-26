using System;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring
{
    /// <summary>LiveOps config profile: one asset maps to one UGS Game Override (non-default) or the base Settings (default).</summary>
    [CreateAssetMenu(menuName = "LiveOps/Authoring/Config Profile", fileName = "ConfigProfile")]
    public sealed class ConfigProfileSO : ScriptableObject
    {
        [Header("Profile")]
        [SerializeField]
        private string profileId = "default";

        [Tooltip("If true, variants using this profile publish to the environment Settings (like legacy single-key configs). No Game Override is created.")]
        [SerializeField]
        private bool isDefault;

        [Header("Notes")]
        [SerializeField, TextArea(2, 5)]
        private string notes;

        [SerializeField]
        private TargetingRule targeting = new TargetingRule();

        public string ProfileId => string.IsNullOrEmpty(profileId) ? "default" : profileId;

        public bool IsDefault => isDefault;

        public string Notes => notes ?? string.Empty;

        public TargetingRule Targeting => targeting;
    }

    /// <summary>Server-side JEXL targeting for a Game Override. Unused when <see cref="ConfigProfileSO.IsDefault"/> is true.</summary>
    [Serializable]
    public sealed class TargetingRule
    {
        [Tooltip("e.g. iOS, Android, WebGL — OR match. Empty = any platform.")]
        [SerializeField]
        private string[] platforms;

        [SerializeField]
        private AppVersionRange appVersion = new AppVersionRange();

        [Tooltip("OR match on a custom attribute (e.g. user tag / cohort). Empty = any.")]
        [SerializeField]
        private string[] tags;

        [Tooltip("Optional: Unity Analytics audience names. If set, the Game Override file uses Audience.Targets (required by schema). If empty, uses [\"all\"].")]
        [SerializeField]
        private string[] audienceTargets;

        [Tooltip("If set, users must be in the rollout bucket; combined with server rollout on the override.")]
        [Range(0, 100)]
        [SerializeField]
        private int rolloutPercent = 100;

        [Tooltip("Override window (ISO 8601 in UTC in generated file; optional in UI).")]
        [SerializeField]
        private string startUtc;

        [SerializeField]
        private string endUtc;

        [Tooltip("Extra JEXL, ANDed with the generated condition.")]
        [TextArea(1, 4)]
        [SerializeField]
        private string rawJexl;

        public string[] Platforms => platforms;

        public AppVersionRange AppVersion => appVersion;

        public string[] Tags => tags;

        public string[] AudienceTargets => audienceTargets;

        public int RolloutPercent => rolloutPercent;

        public string StartUtc => startUtc;

        public string EndUtc => endUtc;

        public string RawJexl => rawJexl;
    }

    /// <summary>App version min/max (semantic version strings, inclusive).</summary>
    [Serializable]
    public sealed class AppVersionRange
    {
        [SerializeField]
        private string min;

        [SerializeField]
        private string max;

        public string Min => min;

        public string Max => max;
    }
}
