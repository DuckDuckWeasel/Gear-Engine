using System;
using System.Collections.Generic;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Presentation.UI.Input;
using NUnit.Framework;
using Scaffold;
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
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
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
        public void Parallel_StartsEveryActionBeforeAnyActionCompletes()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Parallel);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
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
        public void ExecutionProgress_ReportsOnlyOneMeasurableRunningAction()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            DeferredAction firstAction = new DeferredAction { Progress = 0.35f };
            command.actions.Add(firstAction);
            command.actions.Add(new DeferredAction());

            command.OnEnter();

            Assert.That(command.IsActionRunning(0), Is.True);
            Assert.That(command.TryGetActionExecutionProgress(0, out float actionProgress), Is.True);
            Assert.That(actionProgress, Is.EqualTo(0.35f));
            Assert.That(command.TryGetExecutionProgress(out float commandProgress), Is.True);
            Assert.That(commandProgress, Is.EqualTo(0.35f));
        }

        [Test]
        public void ExecutionProgress_DoesNotInventLinearProgressForParallelActions()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Parallel);
            command.actions.Add(new DeferredAction { Progress = 0.25f });
            command.actions.Add(new DeferredAction { Progress = 0.75f });

            command.OnEnter();

            Assert.That(command.TryGetExecutionProgress(out _), Is.False);
        }

        [Test]
        public void ExecutionResults_PersistUntilExecutionFeedbackIsReset()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Selector);
            DeferredAction failure = new DeferredAction();
            DeferredAction success = new DeferredAction();
            command.actions.Add(failure);
            command.actions.Add(success);

            command.OnEnter();
            failure.Complete(ActionExecutionStatus.Failure);
            success.Complete(ActionExecutionStatus.Success);

            Assert.That(
                command.TryGetActionExecutionStatus(
                    0,
                    out CompositeExecutionStatus failureStatus),
                Is.True);
            Assert.That(failureStatus, Is.EqualTo(CompositeExecutionStatus.Failure));
            Assert.That(
                command.TryGetActionExecutionStatus(
                    1,
                    out CompositeExecutionStatus successStatus),
                Is.True);
            Assert.That(successStatus, Is.EqualTo(CompositeExecutionStatus.Success));

            command.ResetExecutionFeedback();

            Assert.That(command.TryGetActionExecutionStatus(0, out _), Is.False);
            Assert.That(command.TryGetActionExecutionStatus(1, out _), Is.False);
        }

        [Test]
        public void Sequence_FailureStopsBeforeTheNextAction()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);

            command.OnEnter();
            firstAction.Complete(ActionExecutionStatus.Failure);

            Assert.That(secondAction.ExecuteCount, Is.Zero);
            Assert.That(command.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Failure));
            Assert.That(command.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void NewExecution_ResetsThePreviousCompositeStatus()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            DeferredAction action = new DeferredAction();
            command.actions.Add(action);
            command.OnEnter();
            action.Complete(ActionExecutionStatus.Failure);
            Assert.That(command.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Failure));

            command.OnEnter();

            Assert.That(command.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(
                command.LastCompositeExecutionStatus,
                Is.EqualTo(CompositeExecutionStatus.Success));
        }

        [Test]
        public void Selector_RunsUntilTheFirstActionSucceeds()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Selector);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            DeferredAction thirdAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);
            command.actions.Add(thirdAction);

            command.OnEnter();
            firstAction.Complete(ActionExecutionStatus.Failure);

            Assert.That(secondAction.ExecuteCount, Is.EqualTo(1));
            secondAction.Complete(ActionExecutionStatus.Success);

            Assert.That(thirdAction.ExecuteCount, Is.Zero);
            Assert.That(command.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(command.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void Selector_FailsAfterEveryEnabledActionFails()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Selector);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);

            command.OnEnter();
            firstAction.Complete(ActionExecutionStatus.Failure);
            secondAction.Complete(ActionExecutionStatus.Failure);

            Assert.That(command.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Failure));
            Assert.That(command.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void UtilitySelector_StartsTheActionWithTheHighestUtility()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.UtilitySelector);
            DeferredAction lowUtilityAction = new DeferredAction();
            DeferredAction highUtilityAction = new DeferredAction();
            DeferredAction mediumUtilityAction = new DeferredAction();
            command.actions.Add(lowUtilityAction);
            command.actions.Add(highUtilityAction);
            command.actions.Add(mediumUtilityAction);
            command.SetActionUtility(0, 1f);
            command.SetActionUtility(1, 3f);
            command.SetActionUtility(2, 2f);

            command.OnEnter();

            Assert.That(lowUtilityAction.ExecuteCount, Is.Zero);
            Assert.That(highUtilityAction.ExecuteCount, Is.EqualTo(1));
            Assert.That(mediumUtilityAction.ExecuteCount, Is.Zero);
        }

        [Test]
        public void UtilitySelector_FallsBackToTheNextHighestUtilityAfterFailure()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.UtilitySelector);
            DeferredAction lowUtilityAction = new DeferredAction();
            DeferredAction highUtilityAction = new DeferredAction();
            DeferredAction mediumUtilityAction = new DeferredAction();
            command.actions.Add(lowUtilityAction);
            command.actions.Add(highUtilityAction);
            command.actions.Add(mediumUtilityAction);
            command.SetActionUtility(0, 1f);
            command.SetActionUtility(1, 3f);
            command.SetActionUtility(2, 2f);

            command.OnEnter();
            highUtilityAction.Complete(ActionExecutionStatus.Failure);

            Assert.That(mediumUtilityAction.ExecuteCount, Is.EqualTo(1));
            Assert.That(lowUtilityAction.ExecuteCount, Is.Zero);

            mediumUtilityAction.Complete(ActionExecutionStatus.Success);

            Assert.That(command.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(command.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void UtilitySelector_FailsAfterEveryEligibleActionFails()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.UtilitySelector);
            DeferredAction lowUtilityAction = new DeferredAction();
            DeferredAction highUtilityAction = new DeferredAction();
            command.actions.Add(lowUtilityAction);
            command.actions.Add(highUtilityAction);
            command.SetActionUtility(0, 1f);
            command.SetActionUtility(1, 2f);

            command.OnEnter();
            highUtilityAction.Complete(ActionExecutionStatus.Failure);
            lowUtilityAction.Complete(ActionExecutionStatus.Failure);

            Assert.That(command.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Failure));
            Assert.That(command.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void UtilitySelector_ReevaluationInterruptsTheRunningActionWhenUtilityChanges()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.UtilitySelector);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);
            command.SetActionUtility(0, 3f);
            command.SetActionUtility(1, 1f);

            command.OnEnter();
            command.SetActionUtility(1, 4f);
            command.ReevaluateUtilitySelection();

            Assert.That(firstAction.InterruptCount, Is.EqualTo(1));
            Assert.That(secondAction.ExecuteCount, Is.EqualTo(1));

            command.SetActionUtility(0, 5f);
            command.ReevaluateUtilitySelection();

            Assert.That(secondAction.InterruptCount, Is.EqualTo(1));
            Assert.That(firstAction.ExecuteCount, Is.EqualTo(2));
        }

        [Test]
        public void UtilitySelector_ReevaluatesABlackboardVariableUtility()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.UtilitySelector);
            DeferredAction constantUtilityAction = new DeferredAction();
            DeferredAction variableUtilityAction = new DeferredAction();
            command.actions.Add(constantUtilityAction);
            command.actions.Add(variableUtilityAction);
            command.SetActionUtility(0, 2f);
            FloatVariable utilityVariable = _hostObject.AddComponent<FloatVariable>();
            utilityVariable.Value = 1f;
            FloatData utilityData = new FloatData(0f)
            {
                floatRef = utilityVariable,
            };
            command.SetActionUtilityData(1, utilityData);

            command.OnEnter();
            utilityVariable.Value = 3f;
            command.ReevaluateUtilitySelection();

            Assert.That(constantUtilityAction.InterruptCount, Is.EqualTo(1));
            Assert.That(variableUtilityAction.ExecuteCount, Is.EqualTo(1));
        }

        [Test]
        public void UtilitySelector_BlockDuringExecutionPreventsUtilityInterruption()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.UtilitySelector);
            DeferredAction blockedAction = new DeferredAction();
            DeferredAction competingAction = new DeferredAction();
            command.actions.Add(blockedAction);
            command.actions.Add(competingAction);
            command.SetActionUtility(0, 3f);
            command.SetActionUtility(1, 1f);
            command.SetUtilityBlockedDuringExecution(0, true);

            command.OnEnter();
            command.SetActionUtility(1, 4f);
            command.ReevaluateUtilitySelection();

            Assert.That(blockedAction.InterruptCount, Is.Zero);
            Assert.That(competingAction.ExecuteCount, Is.Zero);
        }

        [Test]
        public void ParallelWaitAll_WaitsForRemainingActionsAfterFailure()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Parallel);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);

            command.OnEnter();
            firstAction.Complete(ActionExecutionStatus.Failure);

            Assert.That(secondAction.InterruptCount, Is.Zero);
            Assert.That(command.ContinueCount, Is.Zero);

            secondAction.Complete(ActionExecutionStatus.Success);

            Assert.That(command.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Failure));
            Assert.That(command.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void ParallelSelectorWaitAll_WaitsForEveryActionAndSucceedsWhenAnySucceeds()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.ParallelSelector);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            DeferredAction thirdAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);
            command.actions.Add(thirdAction);

            command.OnEnter();
            firstAction.Complete(ActionExecutionStatus.Failure);
            secondAction.Complete(ActionExecutionStatus.Success);

            Assert.That(command.ContinueCount, Is.Zero);
            thirdAction.Complete(ActionExecutionStatus.Failure);

            Assert.That(thirdAction.InterruptCount, Is.Zero);
            Assert.That(command.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(command.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void ParallelWaitAny_ReturnsTheFirstStatusAndLeavesOtherActionsRunning()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Parallel);
            command.AwaitMode = CompositeAwaitMode.WaitAny;
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);

            command.OnEnter();
            firstAction.Complete(ActionExecutionStatus.Failure);

            Assert.That(command.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Failure));
            Assert.That(command.ContinueCount, Is.EqualTo(1));
            Assert.That(secondAction.InterruptCount, Is.Zero);
        }

        [Test]
        public void ParallelWaitNone_ReturnsImmediatelyAfterStartingEveryAction()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Parallel);
            command.AwaitMode = CompositeAwaitMode.WaitNone;
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);

            command.OnEnter();

            Assert.That(firstAction.ExecuteCount, Is.EqualTo(1));
            Assert.That(secondAction.ExecuteCount, Is.EqualTo(1));
            Assert.That(command.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(command.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void RandomOrder_UsesWeightsWithoutRepeatingAnAction()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            command.OrderMode = CompositeOrderMode.Random;
            command.SetRandomValues(0.5f, 0.5f);
            DeferredAction zeroWeightAction = new DeferredAction();
            DeferredAction weightedAction = new DeferredAction();
            command.actions.Add(zeroWeightAction);
            command.actions.Add(weightedAction);
            command.SetActionWeight(0, 0f);
            command.SetActionWeight(1, 100f);

            command.OnEnter();

            Assert.That(weightedAction.ExecuteCount, Is.EqualTo(1));
            Assert.That(zeroWeightAction.ExecuteCount, Is.Zero);

            weightedAction.Complete();

            Assert.That(zeroWeightAction.ExecuteCount, Is.EqualTo(1));
        }

        [Test]
        public void ActionWeights_AutomaticallyBalanceEnabledActions()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            command.actions.Add(new DeferredAction());
            command.actions.Add(new DeferredAction());
            command.actions.Add(new DeferredAction());

            Assert.That(command.GetActionWeight(0), Is.EqualTo(100f / 3f).Within(0.001f));
            Assert.That(command.GetActionWeight(1), Is.EqualTo(100f / 3f).Within(0.001f));
            Assert.That(command.GetActionWeight(2), Is.EqualTo(100f / 3f).Within(0.001f));

            command.SetActionEnabled(2, false);

            Assert.That(command.GetActionWeight(0), Is.EqualTo(50f).Within(0.001f));
            Assert.That(command.GetActionWeight(1), Is.EqualTo(50f).Within(0.001f));
            Assert.That(command.GetActionWeight(2), Is.Zero);
        }

        [Test]
        public void ActionWeightOverrides_ReserveTheirWeightAndBalanceTheRemainder()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            command.actions.Add(new DeferredAction());
            command.actions.Add(new DeferredAction());
            command.actions.Add(new DeferredAction());

            command.SetActionWeight(0, 40f);
            command.SetActionWeight(1, 20f);

            Assert.That(command.HasActionWeightOverride(0), Is.True);
            Assert.That(command.HasActionWeightOverride(1), Is.True);
            Assert.That(command.GetActionWeight(0), Is.EqualTo(40f).Within(0.001f));
            Assert.That(command.GetActionWeight(1), Is.EqualTo(20f).Within(0.001f));
            Assert.That(command.GetActionWeight(2), Is.EqualTo(40f).Within(0.001f));

            command.SetActionWeight(1, 80f);

            Assert.That(command.GetActionWeight(0), Is.EqualTo(100f / 3f).Within(0.001f));
            Assert.That(command.GetActionWeight(1), Is.EqualTo(200f / 3f).Within(0.001f));
            Assert.That(command.GetActionWeight(2), Is.Zero);

            command.ClearActionWeightOverride(1);

            Assert.That(command.HasActionWeightOverride(1), Is.False);
            Assert.That(command.GetActionWeight(0), Is.EqualTo(40f).Within(0.001f));
            Assert.That(command.GetActionWeight(1), Is.EqualTo(30f).Within(0.001f));
            Assert.That(command.GetActionWeight(2), Is.EqualTo(30f).Within(0.001f));
        }

        [Test]
        public void ShuffleOrder_IgnoresWeightsAndUsesAUniformPermutation()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            command.OrderMode = CompositeOrderMode.Shuffle;
            command.SetRandomValues(0f, 0f);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            DeferredAction thirdAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);
            command.actions.Add(thirdAction);
            command.SetActionWeight(0, 100f);
            command.SetActionWeight(1, 0f);
            command.SetActionWeight(2, 0f);

            command.OnEnter();

            Assert.That(firstAction.ExecuteCount, Is.Zero);
            Assert.That(secondAction.ExecuteCount, Is.EqualTo(1));
            Assert.That(thirdAction.ExecuteCount, Is.Zero);
        }

        [TestCase(CompositeOrderMode.Random)]
        [TestCase(CompositeOrderMode.Shuffle)]
        public void RandomizedOrder_CanAvoidTheLastActionFromThePreviousRun(
            CompositeOrderMode orderMode)
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            command.OrderMode = orderMode;
            command.AvoidRepeatingLastAction = true;
            if (orderMode == CompositeOrderMode.Random)
            {
                command.SetRandomValues(0.75f, 0f, 0.25f, 0f);
            }
            else
            {
                command.SetRandomValues(0f, 0.999f);
            }

            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);

            command.OnEnter();
            Assert.That(secondAction.ExecuteCount, Is.EqualTo(1));
            secondAction.Complete();
            firstAction.Complete();

            command.OnEnter();

            Assert.That(secondAction.ExecuteCount, Is.EqualTo(2));
            Assert.That(firstAction.ExecuteCount, Is.EqualTo(1));
        }

        [Test]
        public void OnStopExecuting_InterruptsEveryRunningAction()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Parallel);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);

            command.OnEnter();
            command.OnStopExecuting();

            Assert.That(firstAction.InterruptCount, Is.EqualTo(1));
            Assert.That(secondAction.InterruptCount, Is.EqualTo(1));
            Assert.That(command.ContinueCount, Is.Zero);
        }

        [Test]
        public void PerformInterruption_StopsSelectedActionsInTheCurrentParallelGroup()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Parallel);
            DeferredAction interruptedAction = new DeferredAction();
            PerformInterruption interruptionAction = new PerformInterruption();
            command.actions.Add(interruptedAction);
            command.actions.Add(interruptionAction);
            interruptionAction.TargetActionIds.Add(command.GetActionId(0));

            command.OnEnter();

            Assert.That(interruptedAction.InterruptCount, Is.EqualTo(1));
            Assert.That(command.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(command.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void PerformInterruption_CanStopAnActionInASeparateInvokeAction()
        {
            TestInvokeActionCommand targetCommand = CreateCommand(CompositeExecutionMethod.Parallel);
            DeferredAction interruptedAction = new DeferredAction();
            targetCommand.actions.Add(interruptedAction);

            TestInvokeActionCommand interruptionCommand =
                _hostObject.AddComponent<TestInvokeActionCommand>();
            interruptionCommand.ExecutionMethod = CompositeExecutionMethod.Sequence;
            PerformInterruption interruptionAction = new PerformInterruption
            {
                TargetCommand = targetCommand,
            };
            interruptionCommand.actions.Add(interruptionAction);
            interruptionAction.TargetActionIds.Add(targetCommand.GetActionId(0));

            targetCommand.OnEnter();
            interruptionCommand.OnEnter();

            Assert.That(interruptedAction.InterruptCount, Is.EqualTo(1));
            Assert.That(targetCommand.LastExecutionStatus, Is.EqualTo(ActionExecutionStatus.Success));
            Assert.That(targetCommand.ContinueCount, Is.EqualTo(1));
            Assert.That(interruptionCommand.ContinueCount, Is.EqualTo(1));
        }

        [Test]
        public void ActionIds_FollowTheirActionsWhenTheGroupIsReordered()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);
            string firstActionId = command.GetActionId(0);
            string secondActionId = command.GetActionId(1);

            bool moved = command.TryMoveAction(0, 1);

            Assert.That(moved, Is.True);
            Assert.That(command.actions[0], Is.SameAs(secondAction));
            Assert.That(command.GetActionId(0), Is.EqualTo(secondActionId));
            Assert.That(command.GetActionId(1), Is.EqualTo(firstActionId));
        }

        [Test]
        public void UtilitySettings_FollowTheirActionWhenTheGroupIsReordered()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.UtilitySelector);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
            command.actions.Add(firstAction);
            command.actions.Add(secondAction);
            command.SetActionUtility(0, 7f);
            command.SetUtilityBlockedDuringExecution(0, true);
            command.SetActionWeight(0, 35f);

            bool moved = command.TryMoveAction(0, 1);

            Assert.That(moved, Is.True);
            Assert.That(command.actions[1], Is.SameAs(firstAction));
            Assert.That(command.GetActionUtility(1), Is.EqualTo(7f));
            Assert.That(command.IsUtilityBlockedDuringExecution(1), Is.True);
            Assert.That(command.GetActionWeight(1), Is.EqualTo(35f));
        }

        [Test]
        public void UtilitySettings_ArePreservedWhenAnActionMovesToAnotherGroup()
        {
            TestInvokeActionCommand source = CreateCommand(CompositeExecutionMethod.UtilitySelector);
            TestInvokeActionCommand destination = _hostObject.AddComponent<TestInvokeActionCommand>();
            DeferredAction action = new DeferredAction();
            source.actions.Add(action);
            source.SetActionUtility(0, 8f);
            source.SetUtilityBlockedDuringExecution(0, true);

            bool removed = source.TryRemoveAction(
                0,
                out IAction movedAction,
                out bool enabled,
                out InvokeActionUtilitySettings utilitySettings);
            destination.InsertAction(0, movedAction, enabled, utilitySettings);

            Assert.That(removed, Is.True);
            Assert.That(destination.actions[0], Is.SameAs(action));
            Assert.That(destination.GetActionUtility(0), Is.EqualTo(8f));
            Assert.That(destination.IsUtilityBlockedDuringExecution(0), Is.True);
        }

        [Test]
        public void DisabledAction_IsSkippedWithoutBlockingTheRemainingActions()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            DeferredAction disabledAction = new DeferredAction();
            DeferredAction enabledAction = new DeferredAction();
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
            TestInvokeActionCommand source = CreateCommand(CompositeExecutionMethod.Sequence);
            TestInvokeActionCommand destination = _hostObject.AddComponent<TestInvokeActionCommand>();
            DeferredAction action = new DeferredAction();
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
        public void MovedAction_PreservesTheDisabledStateOfItsSourceInvokeAction()
        {
            TestInvokeActionCommand source = CreateCommand(CompositeExecutionMethod.Sequence);
            TestInvokeActionCommand destination = _hostObject.AddComponent<TestInvokeActionCommand>();
            DeferredAction action = new DeferredAction();
            source.actions.Add(action);
            source.enabled = false;

            bool removed = source.TryRemoveAction(0, out IAction movedAction, out bool enabled);
            destination.InsertAction(0, movedAction, enabled);

            Assert.That(removed, Is.True);
            Assert.That(destination.actions[0], Is.SameAs(action));
            Assert.That(destination.IsActionEnabled(0), Is.False);
        }

        [Test]
        public void MovedAction_PreservesItsEnabledStateWhenReorderedInTheSameGroup()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            DeferredAction firstAction = new DeferredAction();
            DeferredAction secondAction = new DeferredAction();
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
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            DeferredAction existingAction = new DeferredAction();
            DeferredAction addedAction = new DeferredAction();
            command.actions.Add(existingAction);

            command.InsertAction(command.actions.Count, addedAction, true);

            Assert.That(command.actions, Has.Count.EqualTo(2));
            Assert.That(command.actions[0], Is.SameAs(existingAction));
            Assert.That(command.actions[1], Is.SameAs(addedAction));
        }

        [Test]
        public void ActionInsertedIntoAnEmptyInvokeGroup_RemainsVisiblyGrouped()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            DeferredAction action = new DeferredAction();

            command.InsertActionInGroup(0, action, true);

            Assert.That(command.actions, Has.Count.EqualTo(1));
            Assert.That(command.actions[0], Is.SameAs(action));
            Assert.That(command.DisplayAsGroup, Is.True);
        }

        [Test]
        public void RemovingAnActionFromAGroup_PreservesTheGroupPresentationState()
        {
            TestInvokeActionCommand command = CreateCommand(CompositeExecutionMethod.Sequence);
            command.actions.Add(new DeferredAction());
            command.actions.Add(new DeferredAction());

            bool removed = command.TryRemoveAction(0, out _, out _);

            Assert.That(removed, Is.True);
            Assert.That(command.actions, Has.Count.EqualTo(1));
            Assert.That(command.DisplayAsGroup, Is.True);
        }

        [Test]
        public void Condition_FindsEndInsideInvokeActionWrapper()
        {
            _hostObject = new GameObject("WrappedFlowControlTests");
            _hostObject.AddComponent<Blackboard>();
            Block block = _hostObject.AddComponent<Block>();
            CommandTrack track = block.Tracks[0];
            InvokeActionCommand conditionCommand =
                _hostObject.AddComponent<InvokeActionCommand>();
            InvokeActionCommand endCommand =
                _hostObject.AddComponent<InvokeActionCommand>();
            If condition = new If();
            End end = new End();
            conditionCommand.actions.Add(condition);
            endCommand.actions.Add(end);
            track.Commands.Add(conditionCommand);
            track.Commands.Add(endCommand);
            conditionCommand.ParentBlock = block;
            conditionCommand.ParentTrack = track;
            conditionCommand.CommandIndex = 0;
            endCommand.ParentBlock = block;
            endCommand.ParentTrack = track;
            endCommand.CommandIndex = 1;
            condition.SetCommandContext(conditionCommand);
            end.SetCommandContext(endCommand);

            End result = Condition.FindMatchingEndCommand(condition);

            Assert.That(result, Is.SameAs(end));
            Assert.That(result.CommandIndex, Is.EqualTo(1));
        }

        private TestInvokeActionCommand CreateCommand(CompositeExecutionMethod executionMethod)
        {
            _hostObject = new GameObject("InvokeActionCommandTests");
            TestInvokeActionCommand command = _hostObject.AddComponent<TestInvokeActionCommand>();
            command.ExecutionMethod = executionMethod;
            return command;
        }
    }

    public sealed class TestInvokeActionCommand : InvokeActionCommand
    {
        private readonly Queue<float> randomValues = new Queue<float>();

        public int ContinueCount { get; private set; }

        public void SetRandomValues(params float[] values)
        {
            randomValues.Clear();
            foreach (float value in values)
            {
                randomValues.Enqueue(value);
            }
        }

        public override void Continue()
        {
            ContinueCount++;
        }

        protected override float GetRandomValue()
        {
            return randomValues.Count > 0 ? randomValues.Dequeue() : 0f;
        }
    }

    public sealed class DeferredAction :
        IAction,
        IActionWithStatus,
        IInterruptibleAction,
        IActionProgressProvider
    {
        private Action _onComplete;
        private Action<ActionExecutionStatus> _onStatusComplete;

        public int ExecuteCount { get; private set; }
        public int InterruptCount { get; private set; }

        public float Progress { get; set; }

        public void Execute(Action onComplete)
        {
            ExecuteCount++;
            _onComplete = onComplete;
            _onStatusComplete = null;
        }

        public void ExecuteWithStatus(Action<ActionExecutionStatus> onComplete)
        {
            ExecuteCount++;
            _onComplete = null;
            _onStatusComplete = onComplete;
        }

        public void Complete()
        {
            Complete(ActionExecutionStatus.Success);
        }

        public void Complete(ActionExecutionStatus status)
        {
            Action completion = _onComplete;
            Action<ActionExecutionStatus> statusCompletion = _onStatusComplete;
            _onComplete = null;
            _onStatusComplete = null;
            completion?.Invoke();
            statusCompletion?.Invoke(status);
        }

        public void Interrupt()
        {
            InterruptCount++;
            _onComplete = null;
            _onStatusComplete = null;
        }

        public bool TryGetExecutionProgress(out float progress)
        {
            progress = Progress;
            return true;
        }
    }
}
