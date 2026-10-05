using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Scaffold.CloudCode;
using UnityEngine;
using SdkCloudCode = Unity.Services.CloudCode;

namespace GearEngine.App.Bootstrap.Layers
{
    internal sealed class LazyCloudCodeService : ICloudCodeService
    {
        private static readonly JsonSerializerSettings s_jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        public async Task<T> CallEndpointAsync<T>(
            string module,
            string endpoint,
            object payload = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                ValidateRequest(module, endpoint);
                cancellationToken.ThrowIfCancellationRequested();

                SdkCloudCode.ICloudCodeService sdkService = SdkCloudCode.CloudCodeService.Instance;
                if (sdkService == null)
                {
                    throw new InvalidOperationException("Unity Cloud Code is not initialized.");
                }

                Dictionary<string, object> arguments = payload == null
                    ? new Dictionary<string, object>()
                    : new Dictionary<string, object> { { "request", payload } };
                string response = await sdkService.CallModuleEndpointAsync(module, endpoint, arguments);
                cancellationToken.ThrowIfCancellationRequested();
                return JsonConvert.DeserializeObject<T>(response, s_jsonSettings);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[LazyCloudCodeService] Request {module}/{endpoint} failed. {exception}");
                throw;
            }
        }

        private static void ValidateRequest(string module, string endpoint)
        {
            if (string.IsNullOrWhiteSpace(module))
            {
                throw new ArgumentException("Module name is required.", nameof(module));
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint name is required.", nameof(endpoint));
            }
        }
    }
}
