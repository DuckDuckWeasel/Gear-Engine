using System;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Presentation.UI.Input;
using NUnit.Framework;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    public class InvokeActionCommandTests
    {
        private GameObject _hostObject;

        [TearDown]
        public void TearDown()
        {
            if (_hostObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_hostObject);
            }
        }

        [Test]
        public void Sequence_StartsTheNextActionOnlyAfterThePreviousActionCompletes()
        {
            var command = CreateCommand(InvokeActionExecutionMethod.Sequence);
            var firstAction = new DeferredAction();
            var secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);

            command.OnEnter();

            Assert.That(firstAction.ExecuteCount, Is.EqualTo(1));
            Assert.That(secondAction.ExecuteCount, Is.Zero);

            firstAction.Complete();

            Assert.That(secondAction.ExecuteCount, Is.EqualTo(1));
            Assert.That(command.ContinueCount, Is.Zero);

            secondAction.Complete();

            Assert.That(command.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void AllAtSameTime_StartsEveryActionBeforeAnyActionCompletes()
        {
            var command = CreateCommand(InvokeActionExecutionMethod.AllAtSameTime);
            var firstAction = new DeferredAction();
            var secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);

            command.OnEnter();

            Assert.That(firstAction.ExecuteCount, Is.EqualTo(1));
            Assert.That(secondAction.ExecuteCount, Is.EqualTo(1));

            firstAction.Complete();
            Assert.That(command.ContinueCount, Is.Zero);

            secondAction.Complete();
            Assert.That(command.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void DisabledAction_IsSkippedWithoutBlockingTheRemainingActions()
        {
            var command = CreateCommand(InvokeActionExecutionMethod.Sequence);
            var disabledAction = new DeferredAction();
            var enabledAction = new DeferredAction();
            command.actions.Add(disabledAction);
            command.actions.Add(enabledAction);
            command.SetActionEnabled(0, false);

            command.OnEnter();

            Assert.That(disabledAction.ExecuteCount, Is.Zero);
            Assert.That(enabledAction.ExecuteCount, Is.EqualTo(1));

            enabledAction.Complete();

            Assert.That(command.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void MovedAction_PreservesItsEnabledStateWhenInsertedIntoAnotherGroup()
        {
            var source = CreateCommand(InvokeActionExecutionMethod.Sequence);
            var destination = _hostObject.AddComponent<TestInvokeActionCommand>();
            var action = new DeferredAction();
            source.actions.Add(action);
            source.SetActionEnabled(0, false);

            bool removed = source.TryRemoveAction(0, out IAction movedAction, out bool enabled);
            destination.InsertAction(0, movedAction, enabled);

            Assert.That(removed, Is.True);
            Assert.That(source.actions, Is.Empty);
            Assert.That(destination.actions[0], Is.SameAs(action));
            Assert.That(destination.IsActionEnabled(0), Is.False);
        }

        [Test]
        public void MovedAction_PreservesItsEnabledStateWhenReorderedInTheSameGroup()
        {
            var command = CreateCommand(InvokeActionExecutionMethod.Sequence);
            var firstAction = new DeferredAction();
            var secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);
            command.SetActionEnabled(0, false);

            bool moved = command.TryMoveAction(0, 1);

            Assert.That(moved, Is.True);
            Assert.That(command.actions[0], Is.SameAs(secondAction));
            Assert.That(command.actions[1], Is.SameAs(firstAction));
            Assert.That(command.IsActionEnabled(1), Is.False);
        }

        [Test]
        public void AddedAction_AppendsWithoutRemovingTheExistingAction()
        {
            var command = CreateCommand(InvokeActionExecutionMethod.Sequence);
            var existingAction = new DeferredAction();
            var addedAction = new DeferredAction();
            command.actions.Add(existingAction);

            command.InsertAction(command.actions.Count, addedAction, true);

            Assert.That(command.actions, Has.Count.EqualTo(2));
            Assert.That(command.actions[0], Is.SameAs(existingAction));
            Assert.That(command.actions[1], Is.SameAs(addedAction));
        }

        [Test]
        public void RemovingAnActionFromAGroup_PreservesTheGroupPresentationState()
        {
            var command = CreateCommand(InvokeActionExecutionMethod.Sequence);
            command.actions.Add(new DeferredAction());
            command.actions.Add(new DeferredAction());

            bool removed = command.TryRemoveAction(0, out _, out _);

            Assert.That(removed, Is.True);
            Assert.That(command.actions, Has.Count.EqualTo(1));
            Assert.That(command.DisplayAsGroup, Is.True);
        }

        private TestInvokeActionCommand CreateCommand(InvokeActionExecutionMethod executionMethod)
        {
            _hostObject = new GameObject("InvokeActionCommandTests");
            var command = _hostObject.AddComponent<TestInvokeActionCommand>();
            command.ExecutionMethod = executionMethod;
            return command;
        }
    }

    public sealed class TestInvokeActionCommand : InvokeActionCommand
    {
        public int ContinueCount { get; private set; }

        public override void Continue()
        {
            ContinueCount++;
        }
    }

    public sealed class DeferredAction : IAction
    {
        private Action _onComplete;

        public int ExecuteCount { get; private set; }

        public void Execute(Action onComplete)
        {
            ExecuteCount++;
            _onComplete = onComplete;
        }

        public void Complete()
        {
            _onComplete?.Invoke();
        }
    }
}
