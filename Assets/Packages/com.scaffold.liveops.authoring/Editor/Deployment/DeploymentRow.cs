using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    internal enum RowStatus
    {
        Missing,
        InSync,
        Drift,
    }

    internal sealed class DeploymentRow
    {
        private readonly string outputDirectory;

        internal DeploymentRow(ConfigBuilderSOBase builder, string outputDirectory)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.outputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));
        }

        internal ConfigBuilderSOBase Builder { get; }

        private string RcAssetPath()
        {
            string stem = Builder.ConfigKey.EndsWith("Config", StringComparison.Ordinal)
                ? Builder.ConfigKey.Substring(0, Builder.ConfigKey.Length - "Config".Length)
                : Builder.ConfigKey;

            return Path.Combine(outputDirectory, $"{stem}.rc").Replace('\\', '/');
        }

        internal RowStatus Status { get; private set; }

        internal void RecomputeStatus()
        {
            string path = RcAssetPath();
            if (!File.Exists(path))
            {
                Status = RowStatus.Missing;
                return;
            }

            string expected = RcEnvelope.Render(Builder.ConfigKey, Builder.BuildBoxed());
            string disk = File.ReadAllText(path);
            Status = Normalize(expected) == Normalize(disk) ? RowStatus.InSync : RowStatus.Drift;
        }

        internal void Sync()
        {
            object dto = Builder.BuildBoxed();
            string text = RcEnvelope.Render(Builder.ConfigKey, dto);
            string path = RcAssetPath();
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(path, text);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            RecomputeStatus();
        }

        internal void Pull()
        {
            string path = RcAssetPath();
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[LiveOps Config] No file at {path}.");
                return;
            }

            var envelope = JObject.Parse(File.ReadAllText(path));
            JToken inner = envelope["entries"]?[Builder.ConfigKey];
            if (inner == null)
            {
                Debug.LogWarning($"[LiveOps Config] Key '{Builder.ConfigKey}' missing in {path}.");
                return;
            }

            JsonSerializer ser = JsonSerializer.Create(RcEnvelope.SerializerSettings);
            object dto = inner.ToObject(Builder.ConfigType, ser);
            Builder.ApplyBoxed(dto);
            EditorUtility.SetDirty((UnityEngine.Object)Builder);
            AssetDatabase.SaveAssets();
            RecomputeStatus();
        }

        internal void DrawRow()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(Builder.ConfigKey, GUILayout.Width(160f));
                EditorGUILayout.LabelField(Status.ToString(), GUILayout.Width(72f));
                if (GUILayout.Button("Sync", GUILayout.Width(56f)))
                {
                    Sync();
                }

                if (GUILayout.Button("Pull", GUILayout.Width(44f)))
                {
                    Pull();
                }
            }
        }

        private static string Normalize(string text)
        {
            return text?.Replace("\r\n", "\n", StringComparison.Ordinal).Trim() ?? string.Empty;
        }
    }
}
