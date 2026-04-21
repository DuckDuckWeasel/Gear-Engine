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

        public static string Render(string configKey, object dto)
        {
            var envelope = new JObject
            {
                ["entries"] = new JObject { [configKey] = JToken.FromObject(dto, Serializer) },
            };

            return envelope.ToString(Formatting.Indented) + "\n";
        }
    }
}
