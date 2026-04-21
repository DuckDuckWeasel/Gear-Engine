using System;
using System.Collections.Generic;
using Scaffold.LiveOps.Authoring;
using UnityEditor;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    /// <summary>
    /// Discovers all <see cref="ConfigBuilderSOBase"/> assets, renders <c>.rc</c> JSON, and compares to on-disk files.
    /// </summary>
    public sealed class ConfigDeploymentWindow : EditorWindow
    {
        private const string OutputDir = "Assets/LiveOps/RemoteConfig";

        private readonly List<DeploymentRow> rows = new List<DeploymentRow>();

        [MenuItem("Window/LiveOps/Config Deployment")]
        public static void Open()
        {
            GetWindow<ConfigDeploymentWindow>("LiveOps Config");
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            rows.Clear();
            foreach (string guid in AssetDatabase.FindAssets("t:ConfigBuilderSOBase"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var so = AssetDatabase.LoadAssetAtPath<ConfigBuilderSOBase>(path);
                if (so != null)
                {
                    rows.Add(new DeploymentRow(so, OutputDir));
                }
            }

            rows.Sort((a, b) => string.CompareOrdinal(a.Builder.ConfigKey, b.Builder.ConfigKey));
            foreach (DeploymentRow row in rows)
            {
                row.RecomputeStatus();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Remote Config .rc authoring", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Sync writes Assets/LiveOps/RemoteConfig/<Name>.rc from each builder. Deploy with Window → Deployment.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh"))
                {
                    Refresh();
                }

                if (GUILayout.Button("Sync All Out-of-Sync"))
                {
                    foreach (DeploymentRow r in rows)
                    {
                        if (r.Status == RowStatus.Drift || r.Status == RowStatus.Missing)
                        {
                            r.Sync();
                        }
                    }
                }
            }

            EditorGUILayout.Space(6f);

            foreach (DeploymentRow row in rows)
            {
                row.DrawRow();
            }
        }
    }
}
