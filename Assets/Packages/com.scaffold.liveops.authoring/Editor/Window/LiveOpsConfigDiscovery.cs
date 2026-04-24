using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Scaffold.LiveOps.Authoring.Editor.Window
{
    /// <summary>
    /// Discovers all <see cref="ConfigBuilderSOBase"/> assets and flags duplicate <see cref="ConfigBuilderSOBase.ConfigKey"/> groups.
    /// </summary>
    public static class LiveOpsConfigDiscovery
    {
        public sealed class Row
        {
            public ConfigBuilderSOBase Builder { get; set; }

            public string AssetPath { get; set; }

            public bool IsDuplicateConfigKey { get; set; }
        }

        public static List<Row> DiscoverAllRows()
        {
            var list = new List<Row>();
            foreach (string guid in AssetDatabase.FindAssets("t:ConfigBuilderSOBase"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var builder = AssetDatabase.LoadAssetAtPath<ConfigBuilderSOBase>(path);
                if (builder != null)
                {
                    list.Add(new Row { Builder = builder, AssetPath = path });
                }
            }

            list.Sort((a, b) => string.CompareOrdinal(a.Builder.ConfigKey, b.Builder.ConfigKey));
            ApplyDuplicateFlags(list);
            return list;
        }

        public static void ApplyDuplicateFlags(List<Row> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                return;
            }

            foreach (Row r in rows)
            {
                r.IsDuplicateConfigKey = false;
            }

            foreach (IGrouping<string, Row> g in rows.GroupBy(r => r.Builder.ConfigKey))
            {
                if (g.Count() <= 1)
                {
                    continue;
                }

                foreach (Row r in g)
                {
                    r.IsDuplicateConfigKey = true;
                }
            }
        }
    }
}
