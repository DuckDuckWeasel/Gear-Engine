using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Scaffold.LiveOps.Authoring.Editor.Window
{
    /// <summary>
    /// Discovers all <see cref="ConfigBuilderSOBase"/> assets and flags duplicate
    /// <see cref="ConfigBuilderSOBase"/>(<see cref="ConfigBuilderSOBase.ConfigKey"/>, <see cref="ConfigBuilderSOBase.ProfileId"/>) groups.
    /// </summary>
    public static class LiveOpsConfigDiscovery
    {
        public sealed class Row
        {
            public ConfigBuilderSOBase Builder { get; set; }

            public string AssetPath { get; set; }

            public bool IsDuplicateVariant { get; set; }

            /// <summary>Compatibility name for the same check (duplicate <c>(ConfigKey, ProfileId)</c>).</summary>
            public bool IsDuplicateConfigKey
            {
                get => IsDuplicateVariant;
                set => IsDuplicateVariant = value;
            }
        }

        public sealed class VariantListItem
        {
            public bool IsGroup { get; set; }

            public string GroupConfigKey { get; set; }

            public Row VariantRow { get; set; }
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

            list.Sort(
                (a, b) =>
                {
                    int c = string.CompareOrdinal(a.Builder.ConfigKey, b.Builder.ConfigKey);
                    return c != 0 ? c : string.CompareOrdinal(a.Builder.ProfileId, b.Builder.ProfileId);
                });
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
                r.IsDuplicateVariant = false;
            }

            foreach (IGrouping<string, Row> g in rows.GroupBy(r => VariantKey(r)))
            {
                if (g.Count() <= 1)
                {
                    continue;
                }

                foreach (Row r in g)
                {
                    r.IsDuplicateVariant = true;
                }
            }
        }

        private static string VariantKey(Row r) => r.Builder.ConfigKey + "\u0001" + r.Builder.ProfileId;

        public static List<VariantListItem> BuildVariantListItems(IReadOnlyList<Row> rows)
        {
            var result = new List<VariantListItem>();
            if (rows == null || rows.Count == 0)
            {
                return result;
            }

            foreach (IGrouping<string, Row> g in rows.GroupBy(r => r.Builder.ConfigKey).OrderBy(x => x.Key, System.StringComparer.Ordinal))
            {
                result.Add(
                    new VariantListItem
                    {
                        IsGroup = true,
                        GroupConfigKey = g.Key,
                    });
                foreach (Row r in g.OrderBy(x => x.Builder.ProfileId, System.StringComparer.Ordinal))
                {
                    result.Add(
                        new VariantListItem
                        {
                            IsGroup = false,
                            VariantRow = r,
                        });
                }
            }

            return result;
        }
    }
}
