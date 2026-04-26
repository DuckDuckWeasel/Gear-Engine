using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scaffold.LiveOps.Authoring.Editor.Window;
using UnityEditor;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    /// <summary>Disk status of a builder vs its generated file.</summary>
    public enum RowStatus
    {
        Missing,
        InSync,
        Drift,
    }

    /// <summary>
    /// Reads and writes <c>Assets/LiveOps/RemoteConfig</c> from <see cref="ConfigBuilderSOBase"/> assets
    /// (default Variants: <c>.rc</c>; non-default: aggregated <c>_overrides/*.gor</c> per profile).
    /// </summary>
    public static class RcSyncService
    {
        public const string DefaultOutputDirectory = "Assets/LiveOps/RemoteConfig";

        public const string OverridesSubDirectory = "_overrides";

        /// <summary>Resolved path for the builder's <c>.rc</c> file (default Variants only).</summary>
        public static string GetRcPath(ConfigBuilderSOBase builder, string outputDirectory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (!builder.IsDefaultVariant)
            {
                return GetGorPathForBuilder(builder, outputDirectory);
            }

            string dir = string.IsNullOrEmpty(outputDirectory) ? DefaultOutputDirectory : outputDirectory;
            string stem = builder.ConfigKey.EndsWith("Config", StringComparison.Ordinal)
                ? builder.ConfigKey.Substring(0, builder.ConfigKey.Length - "Config".Length)
                : builder.ConfigKey;

            return Path.Combine(dir, $"{stem}.rc").Replace('\\', '/');
        }

        public static string GetGorPathForProfile(ConfigProfileSO profile, string outputDirectory = null)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (profile.IsDefault)
            {
                return null;
            }

            string dir = string.IsNullOrEmpty(outputDirectory) ? DefaultOutputDirectory : outputDirectory;
            string name = string.IsNullOrEmpty(profile.ProfileId) ? "profile" : SanitizeFileStem(profile.ProfileId);
            return Path.Combine(dir, OverridesSubDirectory, name + ".gor").Replace('\\', '/');
        }

        private static string GetGorPathForBuilder(ConfigBuilderSOBase builder, string outputDirectory)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder.IsDefaultVariant)
            {
                return GetRcPath(builder, outputDirectory);
            }

            if (builder.Profile == null)
            {
                return null;
            }

            return GetGorPathForProfile(builder.Profile, outputDirectory);
        }

        private static string SanitizeFileStem(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "profile";
            }

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                s = s.Replace(c, '_');
            }

            s = s.Trim();
            return s.Length == 0 ? "profile" : s;
        }

        /// <summary>Full <c>.rc</c> envelope (default variants), or a single-line note for non-default (use <see cref="RenderLocalConfigPayloadJson"/> for diff).</summary>
        public static string RenderEnvelopeJson(ConfigBuilderSOBase builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder.IsDefaultVariant)
            {
                return RcEnvelope.Render(builder.ConfigKey, builder.BuildBoxed());
            }

            // Non-default variants live in a shared .gor; the window shows the per-key JSON via RenderLocalConfigPayloadJson.
            return "{\n  \"_note\": \"This variant is deployed inside Assets/LiveOps/RemoteConfig/_overrides/<Profile>.gor\"\n}\n";
        }

        /// <summary>Indented JSON for the DTO (Remote Config value for this key) — for Config tab / Diff “local” column.</summary>
        public static string RenderLocalConfigPayloadJson(ConfigBuilderSOBase builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return JToken.FromObject(builder.BuildBoxed(), JsonSerializer.Create(RcEnvelope.SerializerSettings))
                .ToString(Formatting.Indented);
        }

        public static RowStatus GetStatus(ConfigBuilderSOBase builder, string outputDirectory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder.IsDefaultVariant)
            {
                return GetRcFileStatus(
                    GetRcPath(builder, outputDirectory),
                    builder.ConfigKey,
                    RcEnvelope.GetConfigToken(builder.BuildBoxed()));
            }

            if (builder.Profile == null)
            {
                return RowStatus.Missing;
            }

            return GetGorKeyStatus(
                GetGorPathForProfile(builder.Profile, outputDirectory),
                builder.ConfigKey,
                RcEnvelope.GetConfigToken(builder.BuildBoxed()));
        }

        public static void SyncForBuilder(ConfigBuilderSOBase builder, string outputDirectory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder.IsDefaultVariant)
            {
                SyncDefault(builder, outputDirectory);
                return;
            }

            if (builder.Profile == null)
            {
                Debug.LogError("[LiveOps Config] Non-default variant requires a Config Profile reference.");
                return;
            }

            List<Row> all = DiscoverAllBuilderRows();
            SyncProfileOverride(builder.Profile, all, outputDirectory);
        }

        public static void Sync(ConfigBuilderSOBase builder, string outputDirectory = null) => SyncForBuilder(builder, outputDirectory);

        public static void SyncAll(IReadOnlyList<LiveOpsConfigDiscovery.Row> rows, string outputDirectory = null)
        {
            if (rows == null)
            {
                return;
            }

            // 1) All default
            foreach (LiveOpsConfigDiscovery.Row r in rows)
            {
                if (r?.Builder == null)
                {
                    continue;
                }

                if (r.Builder.IsDefaultVariant)
                {
                    SyncDefault(r.Builder, outputDirectory);
                }
            }

            // 2) All distinct non-default profiles
            HashSet<ConfigProfileSO> profs = new HashSet<ConfigProfileSO>();
            foreach (LiveOpsConfigDiscovery.Row r in rows)
            {
                if (r?.Builder == null || r.Builder.IsDefaultVariant)
                {
                    continue;
                }

                if (r.Builder.Profile == null)
                {
                    continue;
                }

                if (!r.Builder.Profile.IsDefault)
                {
                    profs.Add(r.Builder.Profile);
                }
            }

            foreach (ConfigProfileSO p in profs)
            {
                SyncProfileOverride(p, rows, outputDirectory);
            }
        }

        public static void SyncDefault(ConfigBuilderSOBase builder, string outputDirectory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (!builder.IsDefaultVariant)
            {
                SyncForBuilder(builder, outputDirectory);
                return;
            }

            try
            {
                object dto = builder.BuildBoxed();
                string text = RcEnvelope.Render(builder.ConfigKey, dto);
                string path = GetRcPath(builder, outputDirectory);
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(path, text);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveOps Config] Sync failed for '{builder.ConfigKey}': {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public static void SyncProfileOverride(
            ConfigProfileSO profile,
            IReadOnlyList<LiveOpsConfigDiscovery.Row> allRows,
            string outputDirectory = null)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (profile.IsDefault)
            {
                return;
            }

            string path = GetGorPathForProfile(profile, outputDirectory);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            (string text, string error) = BuildExpectedGorFileText(profile, CollectBuildersForProfile(profile, allRows));
            if (error != null)
            {
                Debug.LogError($"[LiveOps Config] Cannot sync .gor for profile '{profile.ProfileId}': {error}");
                return;
            }

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            try
            {
                File.WriteAllText(path, text);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveOps Config] .gor write failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public static IReadOnlyList<string> CollectDeployPathsForVariant(ConfigBuilderSOBase builder, string outputDirectory = null)
        {
            if (builder == null)
            {
                return Array.Empty<string>();
            }

            if (builder.IsDefaultVariant)
            {
                return new[] { GetRcPath(builder, outputDirectory) };
            }

            if (builder.Profile == null)
            {
                return Array.Empty<string>();
            }

            return new[] { GetGorPathForProfile(builder.Profile, outputDirectory) };
        }

        public static IReadOnlyList<string> CollectDeployPathsForProfile(
            ConfigProfileSO profile,
            out bool isSettingsOnly,
            string outputDirectory = null)
        {
            isSettingsOnly = profile != null && profile.IsDefault;
            if (profile == null || profile.IsDefault)
            {
                return Array.Empty<string>();
            }

            return new[] { GetGorPathForProfile(profile, outputDirectory) };
        }

        public static IReadOnlyList<string> CollectAllDeployPaths(
            IReadOnlyList<LiveOpsConfigDiscovery.Row> rows,
            string outputDirectory = null)
        {
            if (rows == null)
            {
                return Array.Empty<string>();
            }

            var set = new HashSet<string>(StringComparer.Ordinal);
            var gorProfiles = new HashSet<ConfigProfileSO>();
            foreach (LiveOpsConfigDiscovery.Row r in rows)
            {
                if (r?.Builder == null)
                {
                    continue;
                }

                if (r.Builder.IsDefaultVariant)
                {
                    set.Add(GetRcPath(r.Builder, outputDirectory));
                }
                else if (r.Builder.Profile != null && !r.Builder.Profile.IsDefault)
                {
                    string g = GetGorPathForProfile(r.Builder.Profile, outputDirectory);
                    if (!string.IsNullOrEmpty(g))
                    {
                        set.Add(g);
                    }

                    gorProfiles.Add(r.Builder.Profile);
                }
            }

            return set.OrderBy(s => s, StringComparer.Ordinal).ToList();
        }

        public static void Pull(ConfigBuilderSOBase builder, string outputDirectory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (builder.IsDefaultVariant)
            {
                PullFromRc(builder, outputDirectory);
                return;
            }

            PullFromGor(builder, outputDirectory);
        }

        private static void PullFromRc(ConfigBuilderSOBase builder, string outputDirectory)
        {
            try
            {
                string path = GetRcPath(builder, outputDirectory);
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[LiveOps Config] No file at {path}.");
                    return;
                }

                JObject root = JObject.Parse(File.ReadAllText(path));
                TryApplyConfigFromEnvelope(root, builder);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveOps Config] Pull failed for '{builder.ConfigKey}': {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private static void PullFromGor(ConfigBuilderSOBase builder, string outputDirectory)
        {
            if (builder?.Profile == null)
            {
                return;
            }

            string gorPath = GetGorPathForProfile(builder.Profile, outputDirectory);
            if (string.IsNullOrEmpty(gorPath) || !File.Exists(gorPath))
            {
                Debug.LogWarning($"[LiveOps Config] No .gor at {gorPath}.");
                return;
            }

            try
            {
                JObject root = JObject.Parse(File.ReadAllText(gorPath));
                JObject overrides = (JObject)root["Overrides"];
                if (overrides == null || overrides.Count == 0)
                {
                    return;
                }

                JProperty first = overrides.Properties().FirstOrDefault();
                if (first?.Value is not JObject odef)
                {
                    return;
                }

                JArray variants = odef["Variants"] as JArray;
                if (variants == null || variants.Count == 0)
                {
                    return;
                }

                JObject v0 = variants[0] as JObject;
                JObject remote = v0?["RemoteConfig"] as JObject;
                JObject entries = remote?["Entries"] as JObject;
                if (entries == null || !entries.TryGetValue(builder.ConfigKey, out JToken strTok))
                {
                    return;
                }

                if (strTok.Type != JTokenType.String)
                {
                    return;
                }

                JToken configJson = JToken.Parse(strTok.Value<string>() ?? "null");
                builder.ApplyBoxed(
                    configJson.ToObject(builder.ConfigType, JsonSerializer.Create(RcEnvelope.SerializerSettings)));
                EditorUtility.SetDirty((UnityEngine.Object)builder);
                AssetDatabase.SaveAssets();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveOps Config] Pull from .gor failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private static void TryApplyConfigFromEnvelope(JObject root, ConfigBuilderSOBase builder)
        {
            JToken inner = root["entries"]?[builder.ConfigKey];
            if (inner == null)
            {
                Debug.LogWarning($"[LiveOps Config] Key '{builder.ConfigKey}' missing in envelope.");
                return;
            }

            JsonSerializer ser = JsonSerializer.Create(RcEnvelope.SerializerSettings);
            object dto = inner.ToObject(builder.ConfigType, ser);
            builder.ApplyBoxed(dto);
            EditorUtility.SetDirty((UnityEngine.Object)builder);
            AssetDatabase.SaveAssets();
        }

        public static List<LiveOpsConfigDiscovery.Row> DiscoverAllBuilderRows() => LiveOpsConfigDiscovery.DiscoverAllRows();

        internal static string Normalize(string text)
        {
            return text?.Replace("\r\n", "\n", StringComparison.Ordinal).Trim() ?? string.Empty;
        }

        private static List<ConfigBuilderSOBase> CollectBuildersForProfile(
            ConfigProfileSO profile,
            IReadOnlyList<LiveOpsConfigDiscovery.Row> allRows)
        {
            if (allRows == null)
            {
                return new List<ConfigBuilderSOBase>();
            }

            return allRows
                .Select(r => r?.Builder)
                .Where(
                    b => b != null
                        && b.Profile == profile
                        && !b.IsDefaultVariant
                        && b.Profile != null
                        && !b.Profile.IsDefault)
                .ToList();
        }

        private static (string expectedFullText, string error) BuildExpectedGorFileText(
            ConfigProfileSO profile,
            IReadOnlyList<ConfigBuilderSOBase> builders)
        {
            if (profile == null)
            {
                return (null, "Profile is null.");
            }

            if (profile.IsDefault)
            {
                return (null, "Not a Game Override profile.");
            }

            if (builders == null || builders.Count == 0)
            {
                return (null, "No variants reference this profile.");
            }

            if (string.IsNullOrEmpty(profile.ProfileId))
            {
                return (null, "ProfileId is empty.");
            }

            string jexl = TargetingJexlEmitter.Emit(profile);
            int rollout = profile.Targeting != null ? profile.Targeting.RolloutPercent : 100;
            string text;
            try
            {
                text = GorEnvelope.Render(
                    profile.ProfileId,
                    profile,
                    (IReadOnlyList<ConfigBuilderSOBase>)builders,
                    jexl,
                    DateTime.UtcNow,
                    rollout);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }

            return (text, null);
        }

        private static RowStatus GetRcFileStatus(string path, string configKey, JToken expectedConfigToken)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path) || string.IsNullOrEmpty(configKey))
            {
                return RowStatus.Missing;
            }

            JObject root;
            try
            {
                root = JObject.Parse(File.ReadAllText(path));
            }
            catch
            {
                return RowStatus.Drift;
            }

            JToken diskEntry = root["entries"]?[configKey];
            if (diskEntry == null)
            {
                return RowStatus.Drift;
            }

            return JToken.DeepEquals(expectedConfigToken, diskEntry) ? RowStatus.InSync : RowStatus.Drift;
        }

        private static RowStatus GetGorKeyStatus(string gorPath, string configKey, JToken configToken)
        {
            if (string.IsNullOrEmpty(gorPath) || !File.Exists(gorPath) || string.IsNullOrEmpty(configKey))
            {
                return RowStatus.Missing;
            }

            string disk = File.ReadAllText(gorPath);
            JObject root;
            try
            {
                root = JObject.Parse(disk);
            }
            catch
            {
                return RowStatus.Drift;
            }

            JObject overrides = root["Overrides"] as JObject;
            if (overrides == null || overrides.Count == 0)
            {
                return RowStatus.Drift;
            }

            JObject odef = overrides.Properties().Select(p => p.Value).OfType<JObject>().FirstOrDefault();
            if (odef == null)
            {
                return RowStatus.Drift;
            }

            JArray variants = odef["Variants"] as JArray;
            if (variants == null || variants.Count == 0)
            {
                return RowStatus.Drift;
            }

            JObject v0 = variants[0] as JObject;
            JObject remote = v0?["RemoteConfig"] as JObject;
            JObject entries = remote?["Entries"] as JObject;
            if (entries == null)
            {
                return RowStatus.Drift;
            }

            if (!entries.TryGetValue(configKey, out JToken ev))
            {
                return RowStatus.Drift;
            }

            string onDisk;
            if (ev.Type == JTokenType.String)
            {
                onDisk = ev.Value<string>();
            }
            else
            {
                onDisk = ev.ToString(Formatting.None);
            }

            JToken onDiskToken;
            try
            {
                onDiskToken = JToken.Parse(string.IsNullOrEmpty(onDisk) ? "null" : onDisk);
            }
            catch
            {
                return RowStatus.Drift;
            }

            return JToken.DeepEquals(configToken, onDiskToken) ? RowStatus.InSync : RowStatus.Drift;
        }
    }
}
