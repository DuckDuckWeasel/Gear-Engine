using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    public static class TargetingJexlEmitter
    {
        public static string Emit(ConfigProfileSO profile)
        {
            if (profile == null)
            {
                return "true";
            }

            if (profile.IsDefault)
            {
                return "true";
            }

            var parts = new List<string>();
            TargetingRule t = profile.Targeting;
            if (t != null)
            {
                string platforms = BuildPlatformClause(t.Platforms);
                if (platforms != null)
                {
                    parts.Add(platforms);
                }

                string ver = BuildAppVersionClause(t.AppVersion);
                if (ver != null)
                {
                    parts.Add(ver);
                }

                string tags = BuildTagClause(t.Tags);
                if (tags != null)
                {
                    parts.Add(tags);
                }

                if (t.RolloutPercent >= 0 && t.RolloutPercent < 100)
                {
                    int cap = Mathf.Clamp(t.RolloutPercent, 0, 100);
                    parts.Add($"(user.bucket < {cap.ToString(CultureInfo.InvariantCulture)})");
                }

                string schedule = BuildScheduleClause(t.StartUtc, t.EndUtc);
                if (schedule != null)
                {
                    parts.Add(schedule);
                }

                if (!string.IsNullOrWhiteSpace(t.RawJexl))
                {
                    parts.Add("(" + t.RawJexl.Trim() + ")");
                }
            }

            if (parts.Count == 0)
            {
                return "true";
            }

            if (parts.Count == 1)
            {
                return parts[0];
            }

            return string.Join(" && ", parts.Select(p => "(" + p + ")"));
        }

        private static string BuildPlatformClause(string[] platforms)
        {
            if (platforms == null || platforms.Length == 0)
            {
                return null;
            }

            var norm = new List<string>();
            foreach (string p in platforms)
            {
                if (string.IsNullOrEmpty(p))
                {
                    continue;
                }

                string q = p.Trim();
                if (q.Length == 0)
                {
                    continue;
                }

                norm.Add($"(unity.platform == {JsString(q)})");
            }

            if (norm.Count == 0)
            {
                return null;
            }

            return string.Join(" || ", norm);
        }

        private static string BuildAppVersionClause(AppVersionRange range)
        {
            if (range == null)
            {
                return null;
            }

            string min = string.IsNullOrWhiteSpace(range.Min) ? null : range.Min.Trim();
            string max = string.IsNullOrWhiteSpace(range.Max) ? null : range.Max.Trim();
            if (min == null && max == null)
            {
                return null;
            }

            var parts = new List<string>();
            if (min != null)
            {
                parts.Add($"(app.version >= {JsString(min)})");
            }

            if (max != null)
            {
                parts.Add($"(app.version <= {JsString(max)})");
            }

            return string.Join(" && ", parts);
        }

        private static string BuildTagClause(string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                return null;
            }

            var parts = new List<string>();
            foreach (string x in tags)
            {
                if (string.IsNullOrEmpty(x))
                {
                    continue;
                }

                string q = x.Trim();
                if (q.Length == 0)
                {
                    continue;
                }

                parts.Add($"(user.tag == {JsString(q)})");
            }

            if (parts.Count == 0)
            {
                return null;
            }

            return string.Join(" || ", parts);
        }

        private static string BuildScheduleClause(string startUtc, string endUtc)
        {
            if (string.IsNullOrEmpty(startUtc) && string.IsNullOrEmpty(endUtc))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(startUtc) && !string.IsNullOrEmpty(endUtc))
            {
                return
                    $"(now() >= {JsString(startUtc)} && now() <= {JsString(endUtc)})";
            }

            if (!string.IsNullOrEmpty(startUtc))
            {
                return $"(now() >= {JsString(startUtc.Trim())})";
            }

            return $"(now() <= {JsString(endUtc.Trim())})";
        }

        private static string JsString(string s)
        {
            if (s == null)
            {
                return "\"\"";
            }

            s = s.Replace("\\", "\\\\", StringComparison.Ordinal);
            s = s.Replace("\"", "\\\"", StringComparison.Ordinal);
            return "\"" + s + "\"";
        }
    }
}
