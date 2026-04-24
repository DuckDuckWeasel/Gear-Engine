using Scaffold.LiveOps.Authoring.Editor.Deployment;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scaffold.LiveOps.Authoring.Editor.Window
{
    /// <summary>Shared Remote Config disk-status icons (Unity light meter) for list + detail views.</summary>
    internal static class LiveOpsConfigStatusLights
    {
        private const string IconInSync = "lightMeter/greenLight";

        private const string IconDrift = "lightMeter/orangeLight";

        private const string IconMissing = "lightMeter/redLight";

        private const string IconDuplicate = "lightMeter/redLight";

        private const string IconNeutral = "lightMeter/lightRim";

        public static void ApplyToImage(Image image, RowStatus disk, bool isDuplicateKey)
        {
            string iconName = isDuplicateKey
                ? IconDuplicate
                : disk switch
                {
                    RowStatus.InSync => IconInSync,
                    RowStatus.Drift => IconDrift,
                    RowStatus.Missing => IconMissing,
                    _ => IconNeutral,
                };

            GUIContent g = EditorGUIUtility.IconContent(iconName);
            image.image = g != null ? g.image as Texture2D : null;
            image.tooltip = StatusTooltip(disk, isDuplicateKey);
        }

        public static string ShortStatusLabel(RowStatus disk)
        {
            return disk switch
            {
                RowStatus.InSync => "In sync",
                RowStatus.Drift => "Drift",
                RowStatus.Missing => "Missing .rc",
                _ => disk.ToString(),
            };
        }

        public static string StatusTooltip(RowStatus disk, bool isDuplicateKey)
        {
            if (isDuplicateKey)
            {
                return "Duplicate ConfigKey — another builder asset uses the same key.";
            }

            return $"{ShortStatusLabel(disk)} — disk .rc vs builder output.";
        }
    }
}
