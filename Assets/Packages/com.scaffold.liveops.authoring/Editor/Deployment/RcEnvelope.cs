using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Scaffold.LiveOps.Authoring.Editor.Deployment
{
    /// <summary>
    /// Writes Remote Config <c>.rc</c> assets in the shape expected by <c>com.unity.services.deployment</c>:
    /// <c>{ "entries": { "&lt;ConfigKey&gt;": &lt;jsonValue&gt; } }</c>.
    /// </summary>
    public static class RcEnvelope
    {
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver(),
        };

        private static readonly JsonSerializer Serializer = JsonSerializer.Create(SerializerSettings);

        public static string Render(string configKey, object dto, DateTime? deployedAtUtc = null)
        {
            return RenderInternal(configKey, dto, deployedAtUtc ?? DateTime.UtcNow);
        }

        private static string RenderInternal(string configKey, object dto, DateTime deployedAtUtc)
        {
            JToken configToken = JToken.FromObject(dto, Serializer);
            string hash = ComputeContentHashFromToken(configToken);
            return BuildEnvelope(configKey, configToken, hash, deployedAtUtc);
        }

        public static string RenderWithTimestamp(string configKey, object dto, DateTime deployedAtUtc) =>
            RenderInternal(configKey, dto, deployedAtUtc);

        public static JToken GetConfigToken(object dto) => JToken.FromObject(dto, Serializer);

        public static string ComputeContentHashFromToken(JToken configToken)
        {
            if (configToken == null)
            {
                return Sha256Hex(string.Empty);
            }

            // Canonical UTF-8 of compact JSON (stable sort from Newtonsoft object graph).
            string canonical = configToken.ToString(Formatting.None);
            return Sha256Hex(canonical);
        }

        private static string BuildEnvelope(string configKey, JToken configToken, string contentHash, DateTime deployedAtUtc)
        {
            var envelope = new JObject
            {
                ["_contentHash"] = contentHash,
                ["_deployedAt"] = deployedAtUtc.ToString("o", System.Globalization.CultureInfo.InvariantCulture),
                ["entries"] = new JObject
                {
                    [configKey] = configToken,
                },
            };

            return envelope.ToString(Formatting.Indented) + "\n";
        }

        private static string Sha256Hex(string input)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            using var sha = SHA256.Create();
            byte[] h = sha.ComputeHash(bytes);
            var sb = new StringBuilder(h.Length * 2);
            foreach (byte b in h)
            {
                sb.Append(b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }
    }
}
