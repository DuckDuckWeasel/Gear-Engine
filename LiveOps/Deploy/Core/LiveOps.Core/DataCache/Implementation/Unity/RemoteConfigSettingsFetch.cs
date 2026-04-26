using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Unity.Services.Apis.RemoteConfig;
using Unity.Services.CloudCode.Apis;
using Unity.Services.CloudCode.Core;
using Unity.Services.RemoteConfig.Model;

namespace LiveOps.ModuleFetchData.Unity
{
    /// <summary>
    /// Fetches Remote Config with <see cref="RequestAttributes"/> when the Game API client exposes
    /// <c>AssignSettingsAsync(IExecutionContext, string, SettingsDeliveryRequest)</c> (Game Overrides / JEXL).
    /// Falls back to GET when not available.
    /// </summary>
    internal static class RemoteConfigSettingsFetch
    {
        public static async Task<ApiResponse<SettingsDeliveryResponse>> FetchAsync(
            IGameApiClient gameApiClient,
            IExecutionContext context,
            string accessToken,
            string projectId,
            string environmentId,
            string playerId)
        {
            object remoteApi = gameApiClient.RemoteConfigSettings;
            Type apiType = remoteApi.GetType();
            MethodInfo assign = apiType.GetMethod(
                "AssignSettingsAsync",
                BindingFlags.Public | BindingFlags.Instance,
                binder: null,
                types: new[] { typeof(IExecutionContext), typeof(string), typeof(SettingsDeliveryRequest) },
                modifiers: null);

            if (assign != null)
            {
                var attrs = BuildRequestAttributes(playerId);
                var request = new SettingsDeliveryRequest(
                    projectId: projectId,
                    userId: playerId,
                    customUserId: null,
                    environmentId: environmentId,
                    configType: "settings",
                    key: null,
                    type: null,
                    isDebugBuild: false,
                    packageVersion: "2.0.0",
                    attributionMetadata: string.Empty,
                    attributes: attrs);

                object taskObj = assign.Invoke(
                    remoteApi,
                    new object[] { context, accessToken, request });
                if (taskObj is Task<ApiResponse<SettingsDeliveryResponse>> typed)
                {
                    return await typed.ConfigureAwait(false);
                }
            }

            return await gameApiClient.RemoteConfigSettings.AssignSettingsGetAsync(
                context,
                accessToken,
                projectId,
                environmentId).ConfigureAwait(false);
        }

        private static RequestAttributes BuildRequestAttributes(string playerId)
        {
            int bucket = Math.Abs((playerId ?? string.Empty).GetHashCode(StringComparison.Ordinal) % 100);
            return new RequestAttributes(
                unity: new UnityAttributes(platform: "CloudCode", appVersion: null),
                app: new Dictionary<string, string>
                {
                    { "version", "0.0.0" },
                },
                user: new Dictionary<string, string>
                {
                    { "tag", string.Empty },
                    { "bucket", bucket.ToString(System.Globalization.CultureInfo.InvariantCulture) },
                });
        }
    }
}
