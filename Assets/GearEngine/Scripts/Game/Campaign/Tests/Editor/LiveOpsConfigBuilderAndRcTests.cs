using System.IO;
using GearEngine.Campaign.Authoring;
using GearEngine.Cards.Authoring;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Scaffold.LiveOps.Authoring;
using Scaffold.LiveOps.Authoring.Editor.Deployment;
using UnityEngine;

namespace GearEngine.Campaign.Tests.Editor
{
    /// <summary>
    /// Ensures default <see cref="Scaffold.LiveOps.Authoring.ConfigBuilderSO{TConfig}"/> output matches committed
    /// <c>Assets/LiveOps/RemoteConfig/*.rc</c> (refresh via <i>Window → LiveOps → Configs</i> → Deploy, or <c>RcSyncService.Sync</c> in editor code).
    /// </summary>
    public sealed class LiveOpsConfigBuilderAndRcTests
    {
        private const string RemoteConfigDir = "Assets/LiveOps/RemoteConfig";

        private static void AssertRcMatchesInstance(ConfigBuilderSOBase builder, string rcFileName)
        {
            object dto = builder.BuildBoxed();
            string expected = RcEnvelope.Render(builder.ConfigKey, dto);
            string path = Path.Combine(RemoteConfigDir, rcFileName);
            Assert.IsTrue(File.Exists(path), $"Missing {path}");
            string disk = File.ReadAllText(path);
            var expectedToken = JObject.Parse(expected);
            var diskToken = JObject.Parse(disk);
            Assert.IsTrue(
                JToken.DeepEquals(expectedToken, diskToken),
                $"Run Window → LiveOps → Configs → Sync for {builder.ConfigKey}. Expected:\n{expected}\n\nDisk:\n{disk}");
        }

        [Test]
        public void TrackConfigBuilderSO_Default_Build_Matches_TrackRc()
        {
            TrackConfigBuilderSO b = ScriptableObject.CreateInstance<TrackConfigBuilderSO>();
            try
            {
                AssertRcMatchesInstance(b, "Track.rc");
            }
            finally
            {
                Object.DestroyImmediate(b);
            }
        }

        [Test]
        public void CardConfigBuilderSO_Default_Build_Matches_CardRc()
        {
            CardConfigBuilderSO b = ScriptableObject.CreateInstance<CardConfigBuilderSO>();
            try
            {
                AssertRcMatchesInstance(b, "Card.rc");
            }
            finally
            {
                Object.DestroyImmediate(b);
            }
        }

        [Test]
        public void CurrencyConfigBuilderSO_Default_Build_Matches_CurrencyRc()
        {
            CurrencyConfigBuilderSO b = ScriptableObject.CreateInstance<CurrencyConfigBuilderSO>();
            try
            {
                AssertRcMatchesInstance(b, "Currency.rc");
            }
            finally
            {
                Object.DestroyImmediate(b);
            }
        }

        [Test]
        public void InventoryConfigBuilderSO_Default_Build_Matches_InventoryRc()
        {
            InventoryConfigBuilderSO b = ScriptableObject.CreateInstance<InventoryConfigBuilderSO>();
            try
            {
                AssertRcMatchesInstance(b, "Inventory.rc");
            }
            finally
            {
                Object.DestroyImmediate(b);
            }
        }

        [Test]
        public void LoadoutConfigBuilderSO_Default_Build_Matches_LoadoutRc()
        {
            LoadoutConfigBuilderSO b = ScriptableObject.CreateInstance<LoadoutConfigBuilderSO>();
            try
            {
                AssertRcMatchesInstance(b, "Loadout.rc");
            }
            finally
            {
                Object.DestroyImmediate(b);
            }
        }

        [Test]
        public void RoguelikeConfigBuilderSO_Default_Build_Matches_RoguelikeRc()
        {
            RoguelikeConfigBuilderSO b = ScriptableObject.CreateInstance<RoguelikeConfigBuilderSO>();
            try
            {
                AssertRcMatchesInstance(b, "Roguelike.rc");
            }
            finally
            {
                Object.DestroyImmediate(b);
            }
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
