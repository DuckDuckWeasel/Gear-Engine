using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Scaffold.LiveOps.Authoring.Editor.Window
{
    public static class ConfigProfileDiscovery
    {
        public sealed class Row
        {
            public ConfigProfileSO Profile { get; set; }

            public string AssetPath { get; set; }
        }

        public static List<Row> DiscoverAll()
        {
            var list = new List<Row>();
            foreach (string guid in AssetDatabase.FindAssets("t:ConfigProfileSO"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prof = AssetDatabase.LoadAssetAtPath<ConfigProfileSO>(path);
                if (prof != null)
                {
                    list.Add(new Row { Profile = prof, AssetPath = path });
                }
            }

            list.Sort(
                (a, b) => string.CompareOrdinal(
                    a.Profile != null ? a.Profile.ProfileId : string.Empty,
                    b.Profile != null ? b.Profile.ProfileId : string.Empty,
                    StringComparison.Ordinal));
            return list;
        }
    }
}
