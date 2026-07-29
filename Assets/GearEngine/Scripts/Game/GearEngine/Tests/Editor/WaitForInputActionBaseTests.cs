using GearEngine.Actions.Input;
using NUnit.Framework;
using Scaffold.Events;
using Scaffold.Events.Contracts;
using Scaffold.Input;
using Scaffold.Input.Contracts;
using System.Reflection;
using UnityEngine.InputSystem;

namespace GearEngine.GearEngine.Tests.Editor
{
    public class WaitForInputActionBaseTests : InputTestFixture
    {
        [Test]
        public void InitializeInputService_ReplacesPartialStateWithCoherentLocalServices()
        {
            IEventBus orphanEventBus = new EventController();
            IInputFilterService orphanInputService = new InputFilterService(orphanEventBus);
            TestWaitForInputAction action = new TestWaitForInputAction();
            action.SetInputService(orphanInputService);

            action.Initialize();

            Assert.That(action.InputService, Is.Not.Null);
            Assert.That(action.InputService, Is.Not.SameAs(orphanInputService));
            Assert.That(action.EventBus, Is.Not.Null);
        }

        [Test]
        public void PrimaryButtonState_RecognizesInputSystemMousePressAndRelease()
        {
            Mouse mouse = InputSystem.AddDevice<Mouse>();

            Press(mouse.leftButton);
            Assert.That(
                InvokeInputStateMethod("IsPrimaryButtonPressedThisFrame"),
                Is.True);

            Release(mouse.leftButton);
            Assert.That(
                InvokeInputStateMethod("IsPrimaryButtonReleasedThisFrame"),
                Is.True);
        }

        private static bool InvokeInputStateMethod(string methodName)
        {
            MethodInfo method = typeof(InputFilterService).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.That(method, Is.Not.Null);
            return (bool)method.Invoke(null, null);
        }

        private sealed class TestWaitForInputAction : WaitForInputActionBase
        {
            public IInputFilterService InputService => inputService;

            public IEventBus EventBus => eventBus;

            public void SetInputService(IInputFilterService inputService)
            {
                this.inputService = inputService;
            }

            public void Initialize()
            {
                InitializeInputService();
            }
        }
    }
}
