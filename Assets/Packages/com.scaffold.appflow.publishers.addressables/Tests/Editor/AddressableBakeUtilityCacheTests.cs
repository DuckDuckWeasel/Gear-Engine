using NUnit.Framework;
using Scaffold.AppFlow.Publishers.Addressables.Editor;
using UnityEngine;

namespace Scaffold.AppFlow.Publishers.Addressables.Tests.Editor
{
    [TestFixture]
    public sealed class AddressableBakeUtilityCacheTests
    {
        [TearDown]
        public void TearDown()
        {
            AddressableBakeUtility.InvalidateCache();
        }

        [Test]
        public void CollectLabelEntryTypes_SameKeyTwice_DoesNotRescan()
        {
            int before = AddressableBakeUtility.CollectLabelEntryTypesScanCount;
            string key = "scan-count-key-" + System.Guid.NewGuid().ToString("N");
            AddressableBakeUtility.CollectLabelEntryTypes(key);
            int afterFirst = AddressableBakeUtility.CollectLabelEntryTypesScanCount;
            Assert.GreaterOrEqual(afterFirst, before + 1, "first call for a new key should increment scan count");
            AddressableBakeUtility.CollectLabelEntryTypes(key);
            Assert.AreEqual(
                afterFirst,
                AddressableBakeUtility.CollectLabelEntryTypesScanCount,
                "second call with the same key must hit the cache and not rescan");
        }

        [Test]
        public void CollectLabelEntryTypes_RepeatKey_ReturnsSameListInstance()
        {
            string key = "repeat-cache-key-" + System.Guid.NewGuid().ToString("N");
            var a = AddressableBakeUtility.CollectLabelEntryTypes(key);
            var b = AddressableBakeUtility.CollectLabelEntryTypes(key);
            Assert.AreSame(a, b);
        }

        [Test]
        public void ValidateLabelEntries_SameLabelAndType_SkipsSecondFullValidationPass()
        {
            string label = "val-label-" + System.Guid.NewGuid().ToString("N");
            var t1 = typeof(Object);
            AddressableBakeUtility.ValidateLabelEntries(label, t1);
            int afterFirst = AddressableBakeUtility.ValidateLabelEntriesPassCount;
            Assert.GreaterOrEqual(afterFirst, 1);
            AddressableBakeUtility.ValidateLabelEntries(label, t1);
            Assert.AreEqual(afterFirst, AddressableBakeUtility.ValidateLabelEntriesPassCount);
        }
    }
}
