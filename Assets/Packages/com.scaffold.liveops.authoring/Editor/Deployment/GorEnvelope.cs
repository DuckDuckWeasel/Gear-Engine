using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using ConfigBuilderSOBase = Scaffold.LiveOps.Authoring.ConfigBuilderSOBase;
using ConfigProfileSO = Scaffold.LiveOps.Authoring.ConfigProfileSO;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    /// <summary>
    /// Writes UGS Game Overrides config files (schema: game-overrides.schema.json).
    /// One profile → one display name in <c>Overrides</c>; values are per Remote Config key (JSON values as JSON strings in <c>RemoteConfig.Entries</c>).
    /// </summary>
    public static class GorEnvelope
    {
        public static string Render(
            string overrideDisplayName,
            ConfigProfileSO profile,
            IReadOnlyList<ConfigBuilderSOBase> builders,
            string jexlCondition,
            DateTime deployedAtUtc,
            int rolloutPercent)
        {
            if (string.IsNullOrEmpty(overrideDisplayName))
            {
                throw new ArgumentException("Override display name is required.", nameof(overrideDisplayName));
            }

            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (builders == null || builders.Count == 0)
            {
                throw new ArgumentException("At least one builder is required for a game override.", nameof(builders));
            }

            if (jexlCondition == null)
            {
                jexlCondition = "true";
            }

            var rcEntries = new JObject();
            foreach (ConfigBuilderSOBase b in builders.OrderBy(x => x.ConfigKey, StringComparer.Ordinal))
            {
                JToken t = RcEnvelope.GetConfigToken(b.BuildBoxed());
                string jsonString = t.ToString(Formatting.None, RcSerializer());
                if (b.ConfigKey != null)
                {
                    rcEntries[b.ConfigKey] = jsonString;
                }
            }

            string contentHash = RcEnvelope.ComputeContentHashFromToken(rcEntries);
            string desc =
                "LiveOps profile: "
                + (profile.ProfileId ?? string.Empty)
                + " | _contentHash="
                + contentHash
                + " | _deployedAt="
                + deployedAtUtc.ToString("o", CultureInfo.InvariantCulture);

            JObject scheduling = BuildScheduling(profile);

            int rollout = profile.IsDefault ? 100 : Mathf.Clamp(rolloutPercent, 0, 100);

            var variant = new JObject
            {
                ["RemoteConfig"] = new JObject
                {
                    ["Entries"] = rcEntries,
                },
            };

            JArray targets = new JArray();
            if (profile.Targeting != null
                && profile.Targeting.AudienceTargets != null
                && profile.Targeting.AudienceTargets.Length > 0)
            {
                foreach (string t in profile.Targeting.AudienceTargets)
                {
                    if (!string.IsNullOrEmpty(t))
                    {
                        targets.Add(t);
                    }
                }
            }

            if (targets.Count == 0)
            {
                targets.Add("all");
            }

            var @override = new JObject
            {
                ["Description"] = desc,
                ["Audience"] = new JObject
                {
                    ["Targets"] = targets,
                },
                ["Rollout"] = rollout,
                ["Condition"] = new JObject
                {
                    ["Value"] = jexlCondition,
                },
                ["Variants"] = new JArray { variant },
            };

            if (scheduling != null)
            {
                @override["Scheduling"] = scheduling;
            }

            var root = new JObject
            {
                ["$schema"] = "https://ugs-config-schemas.unity3d.com/v1/game-overrides.schema.json",
                ["Overrides"] = new JObject
                {
                    [SanitizeKey(overrideDisplayName)] = @override,
                },
            };

            return root.ToString(Formatting.Indented) + "\n";
        }

        private static JObject BuildScheduling(ConfigProfileSO profile)
        {
            if (profile?.Targeting == null)
            {
                return null;
            }

            string start = profile.Targeting.StartUtc;
            string end = profile.Targeting.EndUtc;
            if (string.IsNullOrEmpty(start) && string.IsNullOrEmpty(end))
            {
                return null;
            }

            return new JObject
            {
                ["StartDate"] = string.IsNullOrEmpty(start) ? null : JValue.CreateString(start),
                ["EndDate"] = string.IsNullOrEmpty(end) ? null : JValue.CreateString(end),
            };
        }

        private static string SanitizeKey(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "override";
            }

            s = s.Trim();
            s = Regex.Replace(s, "[^A-Za-z0-9_\\- ]+", "_", RegexOptions.CultureInvariant);
            return s.Length == 0 ? "override" : s;
        }

        private static JsonSerializer RcSerializer() => JsonSerializer.Create(RcEnvelope.SerializerSettings);
    }
}
