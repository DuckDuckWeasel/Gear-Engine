using GearEngine.Actions.Input;
using NUnit.Framework;
using Scaffold.Events;
using Scaffold.Events.Contracts;
using Scaffold.Input;
using Scaffold.Input.Contracts;
using System.Reflection;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace GearEngine.GearEngine.Tests.Editor
{
    public class WaitForInputActionBaseTests
    {
        [Test]
        public void InitializeInputService_ReplacesPartialInjectionWithOneGlobalContext()
        {
            IEventBus orphanEventBus = new EventController();
            IInputFilterService orphanInputService = new InputFilterService(orphanEventBus);
            IEventBus installedEventBus = new EventController();
            IInputFilterService installedInputService = new InputFilterService(installedEventBus);
            TestWaitForInputAction action = new TestWaitForInputAction();
            action.SetInputService(orphanInputService);

            action.Initialize();

            Assert.That(action.InputService, Is.SameAs(installedInputService));
            Assert.That(action.EventBus, Is.SameAs(installedEventBus));
        }

        [Test]
        public void PrimaryButtonState_RecognizesInputSystemMousePressAndRelease()
        {
            Mouse mouse = InputSystem.AddDevice<Mouse>();

            try
            {
                InputSystem.QueueStateEvent(
                    mouse,
                    new MouseState().WithButton(MouseButton.Left));
                InputSystem.Update();

                Assert.That(InvokeInputStateMethod("IsPrimaryButtonPressedThisFrame"), Is.True);

                InputSystem.QueueStateEvent(mouse, new MouseState());
                InputSystem.Update();

                Assert.That(InvokeInputStateMethod("IsPrimaryButtonReleasedThisFrame"), Is.True);
            }
            finally
            {
                InputSystem.RemoveDevice(mouse);
            }
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
            public IInputFilterService InputService => _inputService;

            public IEventBus EventBus => _eventBus;

            public void SetInputService(IInputFilterService inputService)
            {
                _inputService = inputService;
            }

            public void Initialize()
            {
                InitializeInputService();
            }
        }
    }
}
