using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GearEngine.Campaign.Authoring;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Scaffold.LiveOps.Authoring;
using Scaffold.LiveOps.Authoring.Editor.Deployment;
using Scaffold.LiveOps.Authoring.Editor.Window;
using UnityEditor;
using UnityEngine;

namespace GearEngine.Campaign.Tests.Editor
{
    public sealed class RcSyncServiceAndDiscoveryTests
    {
        [Test]
        public void GetStatus_ReportsDrift_WhenRcDoesNotMatchBuilder()
        {
            const string dir = "Assets/TempLiveOpsRcSyncTest";
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, "Currency.rc").Replace('\\', '/');
            try
            {
                File.WriteAllText(path, "{ \"entries\": { \"WrongKey\": 1 } }\n");
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                CurrencyConfigBuilderSO b = ScriptableObject.CreateInstance<CurrencyConfigBuilderSO>();
                try
                {
                    Assert.AreEqual(RowStatus.Drift, RcSyncService.GetStatus(b, dir));
                }
                finally
                {
                    Object.DestroyImmediate(b);
                }
            }
            finally
            {
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.DeleteAsset(dir);
            }
        }

        [Test]
        public void ApplyDuplicateFlags_MarksRows_WhenSameConfigKey()
        {
            var rows = new List<LiveOpsConfigDiscovery.Row>();
            TrackConfigBuilderSO a = ScriptableObject.CreateInstance<TrackConfigBuilderSO>();
            TrackConfigBuilderSO b = ScriptableObject.CreateInstance<TrackConfigBuilderSO>();
            try
            {
                rows.Add(new LiveOpsConfigDiscovery.Row { Builder = a, AssetPath = "a" });
                rows.Add(new LiveOpsConfigDiscovery.Row { Builder = b, AssetPath = "b" });
                LiveOpsConfigDiscovery.ApplyDuplicateFlags(rows);
                Assert.IsTrue(rows.TrueForAll(r => r.IsDuplicateConfigKey));
            }
            finally
            {
                Object.DestroyImmediate(a);
                Object.DestroyImmediate(b);
            }
        }

        [Test]
        public void CloudRemoteConfigSnapshot_ExtractJsonValueForKey_FindsSetting_WrappedRsShape()
        {
            var settings = new JObject
            {
                ["value"] = new JArray
                {
                    new JObject
                    {
                        ["rs"] = new JObject
                        {
                            ["key"] = "CurrencyConfig",
                            ["type"] = "json",
                            ["value"] = new JObject { ["entries"] = new JArray() },
                        },
                    },
                },
            };

            string json = CloudRemoteConfigSnapshot.ExtractJsonValueForKey(settings, "CurrencyConfig");
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("entries", System.StringComparison.Ordinal));
        }

        [Test]
        public void CloudRemoteConfigSnapshot_ExtractJsonValueForKey_FindsSetting_FlatApiShape()
        {
            // Shape returned by RemoteConfigWebApiClient before the editor wraps entries in { "rs": ... }.
            var settings = new JObject
            {
                ["value"] = new JArray
                {
                    new JObject
                    {
                        ["key"] = "CurrencyConfig",
                        ["type"] = "json",
                        ["value"] = new JObject { ["entries"] = new JArray() },
                    },
                },
            };

            string json = CloudRemoteConfigSnapshot.ExtractJsonValueForKey(settings, "CurrencyConfig");
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("entries", System.StringComparison.Ordinal));
        }

        private sealed class RecordingDeployer : IRemoteDeployer
        {
            public List<string> Paths { get; } = new List<string>();

            public Task<DeployOutcome> DeployAsync(
                IReadOnlyList<string> rcPaths,
                CancellationToken cancellationToken = default,
                System.IProgress<string> statusProgress = null)
            {
                Paths.AddRange(rcPaths);
                return Task.FromResult(new DeployOutcome(true, "test", DeployTransport.Api));
            }
        }

        [Test]
        public async Task IRemoteDeployer_RecordsPaths()
        {
            var d = new RecordingDeployer();
            await d.DeployAsync(new[] { "Assets/LiveOps/RemoteConfig/Currency.rc" }, CancellationToken.None);
            Assert.AreEqual(1, d.Paths.Count);
            Assert.AreEqual("Assets/LiveOps/RemoteConfig/Currency.rc", d.Paths[0]);
        }
    }
}
