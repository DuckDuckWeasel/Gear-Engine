using System.IO;
using GearEngine.Campaign.Authoring;
using GearEngine.Cards.Authoring;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Scaffold.LiveOps.Authoring;
using Scaffold.LiveOps.Authoring.Editor.Deployment;
using UnityEditor;
using UnityEngine;

namespace GearEngine.Campaign.Tests.Editor
{
    /// <summary>
    /// Ensures default <see cref="Scaffold.LiveOps.Authoring.ConfigBuilderSO{TConfig}"/> DTO in <c>entries</c> matches committed
    /// <c>Assets/LiveOps/RemoteConfig/*.rc</c> (top-level <c>_contentHash</c> / <c>_deployedAt</c> ignored; refresh via
    /// <i>Window → LiveOps → Configs</i> or <c>RcSyncService</c> in editor).
    /// </summary>
    public sealed class LiveOpsConfigBuilderAndRcTests
    {
        private const string RemoteConfigDir = "Assets/LiveOps/RemoteConfig";

        private static T LoadAuthoringAsset<T>(string assetPath) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.IsNotNull(asset, $"Missing authoring asset at {assetPath}");
            return asset;
        }

        private static void AssertRcMatchesInstance(ConfigBuilderSOBase builder, string rcFileName)
        {
            object dto = builder.BuildBoxed();
            JToken expectedConfig = RcEnvelope.GetConfigToken(dto);
            string path = Path.Combine(RemoteConfigDir, rcFileName);
            Assert.IsTrue(File.Exists(path), $"Missing {path}");
            string disk = File.ReadAllText(path);
            var diskRoot = JObject.Parse(disk);
            JToken diskConfig = diskRoot["entries"]?[builder.ConfigKey];
            Assert.IsNotNull(diskConfig, $"Commit {rcFileName} missing entries.{builder.ConfigKey}");
            Assert.IsTrue(
                JToken.DeepEquals(expectedConfig, diskConfig),
                $"Run Window → LiveOps → Configs → Sync for {builder.ConfigKey}. Expected DTO JSON does not match disk entry.");
        }

        [Test]
        public void TrackConfigBuilderSO_Default_Build_Matches_TrackRc()
        {
            TrackConfigBuilderSO b = LoadAuthoringAsset<TrackConfigBuilderSO>(
                "Assets/GearEngine/Data/LiveOps/Authoring/TrackConfigBuilder.asset");
            AssertRcMatchesInstance(b, "Track.rc");
        }

        [Test]
        public void CardConfigBuilderSO_Default_Build_Matches_CardRc()
        {
            CardConfigBuilderSO b = LoadAuthoringAsset<CardConfigBuilderSO>(
                "Assets/GearEngine/Data/LiveOps/Authoring/CardConfigBuilder.asset");
            AssertRcMatchesInstance(b, "Card.rc");
        }

        [Test]
        public void CurrencyConfigBuilderSO_Default_Build_Matches_CurrencyRc()
        {
            CurrencyConfigBuilderSO b = LoadAuthoringAsset<CurrencyConfigBuilderSO>(
                "Assets/GearEngine/Data/LiveOps/Authoring/CurrencyConfigBuilder.asset");
            AssertRcMatchesInstance(b, "Currency.rc");
        }

        [Test]
        public void InventoryConfigBuilderSO_Default_Build_Matches_InventoryRc()
        {
            InventoryConfigBuilderSO b = LoadAuthoringAsset<InventoryConfigBuilderSO>(
                "Assets/GearEngine/Data/LiveOps/Authoring/InventoryConfigBuilder.asset");
            AssertRcMatchesInstance(b, "Inventory.rc");
        }

        [Test]
        public void LoadoutConfigBuilderSO_Default_Build_Matches_LoadoutRc()
        {
            LoadoutConfigBuilderSO b = LoadAuthoringAsset<LoadoutConfigBuilderSO>(
                "Assets/GearEngine/Data/LiveOps/Authoring/LoadoutConfigBuilder.asset");
            AssertRcMatchesInstance(b, "Loadout.rc");
        }

        [Test]
        public void RoguelikeConfigBuilderSO_Default_Build_Matches_RoguelikeRc()
        {
            RoguelikeConfigBuilderSO b = LoadAuthoringAsset<RoguelikeConfigBuilderSO>(
                "Assets/GearEngine/Data/LiveOps/Authoring/RoguelikeConfigBuilder.asset");
            AssertRcMatchesInstance(b, "Roguelike.rc");
        }

        [Test]
        public void Builder_ConfigKeys_Match_ServerModule_Contract()
        {
            TrackConfigBuilderSO t = ScriptableObject.CreateInstance<TrackConfigBuilderSO>();
            CardConfigBuilderSO c = ScriptableObject.CreateInstance<CardConfigBuilderSO>();
            CurrencyConfigBuilderSO y = ScriptableObject.CreateInstance<CurrencyConfigBuilderSO>();
            InventoryConfigBuilderSO i = ScriptableObject.CreateInstance<InventoryConfigBuilderSO>();
            LoadoutConfigBuilderSO l = ScriptableObject.CreateInstance<LoadoutConfigBuilderSO>();
            RoguelikeConfigBuilderSO r = ScriptableObject.CreateInstance<RoguelikeConfigBuilderSO>();
            try
            {
                Assert.AreEqual("TrackConfig", t.ConfigKey);
                Assert.AreEqual("CardConfig", c.ConfigKey);
                Assert.AreEqual("CurrencyConfig", y.ConfigKey);
                Assert.AreEqual("InventoryConfig", i.ConfigKey);
                Assert.AreEqual("LoadoutConfig", l.ConfigKey);
                Assert.AreEqual("RoguelikeConfig", r.ConfigKey);
            }
            finally
            {
                Object.DestroyImmediate(t);
                Object.DestroyImmediate(c);
                Object.DestroyImmediate(y);
                Object.DestroyImmediate(i);
                Object.DestroyImmediate(l);
                Object.DestroyImmediate(r);
            }
        }
    }
}
