using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    /// <summary>Disk status of a builder vs its generated <c>.rc</c> file.</summary>
    public enum RowStatus
    {
        Missing,
        InSync,
        Drift,
    }

    /// <summary>
    /// Reads and writes <c>Assets/LiveOps/RemoteConfig/&lt;Stem&gt;.rc</c> from <see cref="ConfigBuilderSOBase"/> assets.
    /// </summary>
    public static class RcSyncService
    {
        public const string DefaultOutputDirectory = "Assets/LiveOps/RemoteConfig";

        /// <summary>Resolved path for the builder's <c>.rc</c> file.</summary>
        public static string GetRcPath(ConfigBuilderSOBase builder, string outputDirectory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            string dir = string.IsNullOrEmpty(outputDirectory) ? DefaultOutputDirectory : outputDirectory;
            string stem = builder.ConfigKey.EndsWith("Config", StringComparison.Ordinal)
                ? builder.ConfigKey.Substring(0, builder.ConfigKey.Length - "Config".Length)
                : builder.ConfigKey;

            return Path.Combine(dir, $"{stem}.rc").Replace('\\', '/');
        }

        /// <summary>Full envelope JSON that would be written by <see cref="Sync"/>.</summary>
        public static string RenderEnvelopeJson(ConfigBuilderSOBase builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            object dto = builder.BuildBoxed();
            return RcEnvelope.Render(builder.ConfigKey, dto);
        }

        public static RowStatus GetStatus(ConfigBuilderSOBase builder, string outputDirectory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            string path = GetRcPath(builder, outputDirectory);
            if (!File.Exists(path))
            {
                return RowStatus.Missing;
            }

            string expected = RenderEnvelopeJson(builder);
            string disk = File.ReadAllText(path);
            return Normalize(expected) == Normalize(disk) ? RowStatus.InSync : RowStatus.Drift;
        }

        public static void Sync(ConfigBuilderSOBase builder, string outputDirectory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
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

        public static void Pull(ConfigBuilderSOBase builder, string outputDirectory = null)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            try
            {
                string path = GetRcPath(builder, outputDirectory);
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[LiveOps Config] No file at {path}.");
                    return;
                }

                var envelope = JObject.Parse(File.ReadAllText(path));
                JToken inner = envelope["entries"]?[builder.ConfigKey];
                if (inner == null)
                {
                    Debug.LogWarning($"[LiveOps Config] Key '{builder.ConfigKey}' missing in {path}.");
                    return;
                }

                JsonSerializer ser = JsonSerializer.Create(RcEnvelope.SerializerSettings);
                object dto = inner.ToObject(builder.ConfigType, ser);
                builder.ApplyBoxed(dto);
                EditorUtility.SetDirty((UnityEngine.Object)builder);
                AssetDatabase.SaveAssets();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LiveOps Config] Pull failed for '{builder.ConfigKey}': {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        internal static string Normalize(string text)
        {
            return text?.Replace("\r\n", "\n", StringComparison.Ordinal).Trim() ?? string.Empty;
        }
    }
}
