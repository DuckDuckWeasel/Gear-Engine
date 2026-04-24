using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.RemoteConfig.Editor;
using UnityEditor;
using UnityEngine;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    /// <summary>
    /// Fetches the live Remote Config JSON value for a key using <see cref="RemoteConfigWebApiClient"/>.
    /// </summary>
    public static class CloudRemoteConfigSnapshot
    {
        /// <summary>
        /// Returns indented JSON for the remote value of <paramref name="configKey"/>, or an error message.
        /// </summary>
        public static Task<CloudFetchResult> TryFetchValueJsonForKeyAsync(string configKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(configKey))
            {
                return Task.FromResult(new CloudFetchResult(false, null, "Config key is empty."));
            }

            string projectId = CloudProjectSettings.projectId;
            if (string.IsNullOrEmpty(projectId))
            {
                return Task.FromResult(
                    new CloudFetchResult(
                        false,
                        null,
                        "Project is not linked to Unity Gaming Services (Edit → Project Settings → Services)."));
            }

            string envId = ResolveRemoteConfigEnvironmentId();
            if (string.IsNullOrEmpty(envId))
            {
                return Task.FromResult(
                    new CloudFetchResult(
                        false,
                        null,
                        "No Remote Config environment id. Open **Window → Remote Config** and load environments (or pick an environment in Project Settings → Services so Deployment and RC stay aligned)."));
            }

            var tcs = new TaskCompletionSource<CloudFetchResult>();
            bool finished = false;

            void Complete(CloudFetchResult r)
            {
                if (finished)
                {
                    return;
                }

                finished = true;
                RemoteConfigWebApiClient.fetchConfigsFinished -= OnFetched;
                RemoteConfigWebApiClient.rcRequestFailed -= OnFailed;
                tcs.TrySetResult(r);
            }

            void OnFetched(JObject config)
            {
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (config == null || !config.TryGetValue("value", out JToken valueProbe) || valueProbe is not JArray)
                    {
                        Complete(
                            new CloudFetchResult(
                                false,
                                null,
                                "Remote Config returned no settings payload for this environment (nothing to compare). Open **Window → Remote Config** and confirm this environment has settings."));
                        return;
                    }

                    string extracted = ExtractJsonValueForKey(config, configKey);
                    if (extracted == null)
                    {
                        Complete(
                            new CloudFetchResult(
                                false,
                                null,
                                $"Remote has no setting with key '{configKey}'. Add or deploy that key in this environment, or verify the Remote Config environment matches the one you use for deploy."));
                    }
                    else
                    {
                        Complete(new CloudFetchResult(true, extracted, null));
                    }
                }
                catch (OperationCanceledException)
                {
                    Complete(new CloudFetchResult(false, null, "Cancelled."));
                }
                catch (Exception ex)
                {
                    Complete(new CloudFetchResult(false, null, ex.Message));
                }
            }

            void OnFailed(long code, string err)
            {
                Complete(
                    new CloudFetchResult(
                        false,
                        null,
                        $"Remote Config request failed ({code}): {err}"));
            }

            RemoteConfigWebApiClient.fetchConfigsFinished += OnFetched;
            RemoteConfigWebApiClient.rcRequestFailed += OnFailed;

            try
            {
                RemoteConfigWebApiClient.FetchConfigs(
                    projectId,
                    envId,
                    ex =>
                    {
                        if (ex != null)
                        {
                            Complete(new CloudFetchResult(false, null, $"Parse error: {ex.Message}"));
                        }
                    });
            }
            catch (Exception ex)
            {
                Complete(new CloudFetchResult(false, null, ex.Message));
            }

            return tcs.Task;
        }

        /// <summary>
        /// Prefer the environment id from Unity’s Remote Config datastore (same as **Window → Remote Config**),
        /// then fall back to the Deployment API current environment.
        /// </summary>
        private static string ResolveRemoteConfigEnvironmentId()
        {
            string[] guids = AssetDatabase.FindAssets("t:RemoteConfigDataStore");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var ds = AssetDatabase.LoadAssetAtPath<RemoteConfigDataStore>(path);
                if (ds != null && !string.IsNullOrEmpty(ds.currentEnvironmentId))
                {
                    return ds.currentEnvironmentId;
                }
            }

            return Unity.Services.DeploymentApi.Editor.Deployments.Instance?.EnvironmentProvider?.Current;
        }

        /// <summary>For tests and tooling: reads a setting value from a dashboard-style config object.</summary>
        public static string ExtractJsonValueForKey(JObject settingsConfig, string configKey)
        {
            if (settingsConfig == null || !settingsConfig.TryGetValue("value", out JToken valueToken))
            {
                return null;
            }

            if (valueToken is not JArray arr)
            {
                return null;
            }

            foreach (JToken item in arr)
            {
                JObject rs = GetRsObjectFromSettingItem(item);
                if (rs == null)
                {
                    continue;
                }

                if (!string.Equals(rs["key"]?.Value<string>(), configKey, StringComparison.Ordinal))
                {
                    continue;
                }

                JToken val = rs["value"];
                if (val == null || val.Type == JTokenType.Null)
                {
                    return "null";
                }

                if (val.Type == JTokenType.String)
                {
                    string s = val.Value<string>();
                    if (string.IsNullOrEmpty(s))
                    {
                        return "\"\"";
                    }

                    try
                    {
                        return JToken.Parse(s).ToString(Formatting.Indented);
                    }
                    catch
                    {
                        return s;
                    }
                }

                return val.ToString(Formatting.Indented);
            }

            return null;
        }

        /// <summary>
        /// <see cref="RemoteConfigWebApiClient"/> returns each entry in <c>value</c> as a flat
        /// <c>{ "key", "type", "value" }</c> object. The in-editor datastore wraps that as <c>{ "rs": … }</c>.
        /// </summary>
        private static JObject GetRsObjectFromSettingItem(JToken item)
        {
            if (item is not JObject jo)
            {
                return null;
            }

            if (jo["rs"] is JObject wrapped)
            {
                return wrapped;
            }

            return jo["key"] != null ? jo : null;
        }
    }

    public sealed class CloudFetchResult
    {
        public CloudFetchResult(bool ok, string json, string error)
        {
            Ok = ok;
            Json = json;
            Error = error ?? string.Empty;
        }

        public bool Ok { get; }

        public string Json { get; }

        public string Error { get; }
    }
}
