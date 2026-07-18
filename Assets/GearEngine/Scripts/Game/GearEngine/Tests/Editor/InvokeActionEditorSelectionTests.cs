using GearEngine.GearEngine.Presentation.UI.Input;
using NUnit.Framework;
using Scaffold;
using Scaffold.EditorUtils;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    public class InvokeActionEditorSelectionTests
    {
        private GameObject hostObject;
        private InvokeActionCommand command;

        [SetUp]
        public void SetUp()
        {
            hostObject = new GameObject("InvokeActionEditorSelectionTests");
            command = hostObject.AddComponent<InvokeActionCommand>();
            command.actions.Add(new CameraZoom());
            command.actions.Add(new SendAnalyticsEvent());
        }

        [TearDown]
        public void TearDown()
        {
            InvokeActionEditorSelection.Clear(command);
            Object.DestroyImmediate(hostObject);
        }

        [Test]
        public void Select_StoresTheNestedActionSelectedFromTheBlockList()
        {
            InvokeActionEditorSelection.Select(command, 1);

            Assert.That(InvokeActionEditorSelection.GetSelectedIndex(command), Is.EqualTo(1));
        }

        [Test]
        public void GetDisplayName_UsesTheActionCommandInfoName()
        {
            Assert.That(InvokeActionEditorUtility.GetDisplayName(command.actions[1]), Is.EqualTo("Send Event"));
        }
    }
}
