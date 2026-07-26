using System.Reflection;
using NUnit.Framework;
using Scaffold;
using Scaffold.EditorUtils;

namespace GearEngine.GearEngine.Tests.Editor
{
    public class InvokeActionPropertyVisibilityTests
    {
        [Test]
        public void ShowUIFocus_LayoutFieldsFollowOverridePresetLayout()
        {
            ShowUIFocus action = new ShowUIFocus();

            Assert.That(
                InvokeActionEditorUtility.IsPropertyVisible(action, "_overridePresetLayout"),
                Is.True);
            Assert.That(
                InvokeActionEditorUtility.IsPropertyVisible(action, "_indicatorAnchor"),
                Is.False);

            FieldInfo overrideField = typeof(ShowUIFocus).GetField(
                "_overridePresetLayout",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(overrideField, Is.Not.Null);
            overrideField.SetValue(action, true);

            Assert.That(
                InvokeActionEditorUtility.IsPropertyVisible(action, "_indicatorAnchor"),
                Is.True);
        }
    }
}
