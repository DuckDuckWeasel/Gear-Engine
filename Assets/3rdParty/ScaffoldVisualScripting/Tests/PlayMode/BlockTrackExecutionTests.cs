using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Scaffold;

namespace Scaffold.Tests.PlayMode
{
    /// <summary>
    /// Validates the command-level composite execution engine on Block:
    /// single-track compatibility, flow control inside a non-primary track,
    /// parallel execution, selectors, weighted random order, shuffle, and utility.
    ///
    /// Flow control uses small self-contained test Commands so the suite can isolate the
    /// Block scheduler. Invoke Action wrapper discovery is covered by the Game edit-mode suite.
    /// </summary>
    public class BlockTrackExecutionTests
    {
        private readonly List<GameObject> _spawned = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in _spawned)
            {
                if (go != null)
                {
                    Object.Destroy(go);
                }
            }
            _spawned.Clear();
        }

        private Block CreateBlock()
        {
            GameObject go = new GameObject("TestBlackboard");
            _spawned.Add(go);
            go.AddComponent<Blackboard>();
            return go.AddComponent<TestCompositeBlock>();
        }

        private static TestCommand AddCommand(Block block, CommandTrack track, string tag, List<string> log, int framesToWait = 0)
        {
            TestCommand cmd = block.gameObject.AddComponent<TestCommand>();
            cmd.Tag = tag;
            cmd.Log = log;
            cmd.FramesToWait = framesToWait;
            track.Commands.Add(cmd);
            return cmd;
        }

        [Test]
        public void Blackboard_IsCompatibleWithExistingBlackboardConsumers()
        {
            GameObject go = new GameObject("Blackboard");
            _spawned.Add(go);

            Blackboard blackboard = go.AddComponent<Blackboard>();

            Assert.That(go.GetComponent<Blackboard>(), Is.SameAs(blackboard));
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator SequentialCommands_StillExecuteInOrder()
        {
            Block block = CreateBlock();
            CommandTrack track0 = new CommandTrack("Track 0");
            block.Tracks.Add(track0);

            List<string> log = new List<string>();
            AddCommand(block, track0, "A", log);
            AddCommand(block, track0, "B", log);
            AddCommand(block, track0, "C", log);

            yield return block.Execute();

            CollectionAssert.AreEqual(new[] { "A", "B", "C" }, log);
            Assert.AreEqual(ExecutionState.Idle, block.State);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator IfElseEnd_WorksInsideNonPrimaryTrack()
        {
            Block block = CreateBlock();

            // Track 0 is intentionally empty: the flow-control chain lives on Track 1
            // to prove commands are scoped to their own track, not always Track 0.
            CommandTrack track0 = new CommandTrack("Track 0");
            CommandTrack track1 = new CommandTrack("Track 1");
            block.Tracks.Add(track0);
            block.Tracks.Add(track1);

            List<string> log = new List<string>();

            TestIfCommand ifCmd = block.gameObject.AddComponent<TestIfCommand>();
            ifCmd.Result = false; // force the Else branch
            track1.Commands.Add(ifCmd);

            AddCommand(block, track1, "insideIf", log);

            TestElseCommand elseCmd = block.gameObject.AddComponent<TestElseCommand>();
            track1.Commands.Add(elseCmd);

            AddCommand(block, track1, "insideElse", log);

            TestEndCommand endCmd = block.gameObject.AddComponent<TestEndCommand>();
            track1.Commands.Add(endCmd);

            AddCommand(block, track1, "afterEnd", log);

            yield return block.Execute();

            CollectionAssert.AreEqual(new[] { "insideElse", "afterEnd" }, log);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator MultipleTracks_ExecuteInParallel_WithWaitAll()
        {
            Block block = CreateBlock();
            block.ExecutionMethod = CompositeExecutionMethod.Parallel;
            block.AwaitMode = CompositeAwaitMode.WaitAll;

            CommandTrack trackA = new CommandTrack("Track A");
            CommandTrack trackB = new CommandTrack("Track B");
            block.Tracks.Add(trackA);
            block.Tracks.Add(trackB);

            List<string> log = new List<string>();
            TestCommand slow = AddCommand(block, trackA, "slow-start", log, framesToWait: 3);
            AddCommand(block, trackA, "slow-end", log);
            TestCommand fast = AddCommand(block, trackB, "fast", log);

            yield return block.Execute();

            // Every visible command is a parallel child, regardless of its track.
            Assert.LessOrEqual(fast.EnterFrame, slow.EnterFrame);
            CollectionAssert.Contains(log, "fast");
            CollectionAssert.Contains(log, "slow-end");
            Assert.AreEqual(ExecutionState.Idle, block.State);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator MultipleTracks_ExecuteInSequence_WhenConfigured()
        {
            Block block = CreateBlock();
            block.ExecutionMethod = CompositeExecutionMethod.Sequence;

            CommandTrack trackA = new CommandTrack("Track A");
            CommandTrack trackB = new CommandTrack("Track B");
            block.Tracks.Add(trackA);
            block.Tracks.Add(trackB);

            List<string> log = new List<string>();
            TestCommand firstTrackAction = AddCommand(block, trackA, "first", log, framesToWait: 2);
            AddCommand(block, trackA, "first-end", log);
            TestCommand secondTrackAction = AddCommand(block, trackB, "second", log);

            yield return block.Execute();

            Assert.Greater(secondTrackAction.EnterFrame, firstTrackAction.EnterFrame);
            Assert.Less(log.IndexOf("first-end"), log.IndexOf("second"));
            Assert.AreEqual(ExecutionState.Idle, block.State);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator Selector_UsesTheSharedCompositeStatusAcrossTracks()
        {
            Block block = CreateBlock();
            block.Tracks.Clear();
            block.ExecutionMethod = CompositeExecutionMethod.Selector;
            CommandTrack failingTrack = new CommandTrack("Failing");
            CommandTrack successfulTrack = new CommandTrack("Successful");
            CommandTrack unusedTrack = new CommandTrack("Unused");
            block.Tracks.Add(failingTrack);
            block.Tracks.Add(successfulTrack);
            block.Tracks.Add(unusedTrack);
            StatusCommand failure = AddStatusCommand(
                block,
                failingTrack,
                CompositeExecutionStatus.Failure);
            StatusCommand success = AddStatusCommand(
                block,
                successfulTrack,
                CompositeExecutionStatus.Success);
            StatusCommand unused = AddStatusCommand(
                block,
                unusedTrack,
                CompositeExecutionStatus.Success);

            yield return block.Execute();

            Assert.That(failure.ExecuteCount, Is.EqualTo(1));
            Assert.That(success.ExecuteCount, Is.EqualTo(1));
            Assert.That(unused.ExecuteCount, Is.Zero);
            Assert.That(
                block.LastCompositeExecutionStatus,
                Is.EqualTo(CompositeExecutionStatus.Success));
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator SelectorRandom_UsesWeightedOrderBeforeSelectingTheFirstSuccess()
        {
            TestCompositeBlock block = (TestCompositeBlock)CreateBlock();
            block.Tracks.Clear();
            block.ExecutionMethod = CompositeExecutionMethod.Selector;
            block.OrderMode = CompositeOrderMode.Random;
            block.SetRandomValues(0.5f, 0.5f);
            CommandTrack track = new CommandTrack("Track");
            block.Tracks.Add(track);
            StatusCommand zeroWeightFailure = AddStatusCommand(
                block,
                track,
                CompositeExecutionStatus.Failure);
            StatusCommand weightedSuccess = AddStatusCommand(
                block,
                track,
                CompositeExecutionStatus.Success);
            zeroWeightFailure.CompositeWeight = 0f;
            weightedSuccess.CompositeWeight = 100f;

            yield return block.Execute();

            Assert.That(zeroWeightFailure.ExecuteCount, Is.Zero);
            Assert.That(weightedSuccess.ExecuteCount, Is.EqualTo(1));
            Assert.That(
                block.LastCompositeExecutionStatus,
                Is.EqualTo(CompositeExecutionStatus.Success));
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator SelectorShuffle_IgnoresWeightsBeforeSelectingTheFirstSuccess()
        {
            TestCompositeBlock block = (TestCompositeBlock)CreateBlock();
            block.Tracks.Clear();
            block.ExecutionMethod = CompositeExecutionMethod.Selector;
            block.OrderMode = CompositeOrderMode.Shuffle;
            block.SetRandomValues(0f);
            CommandTrack track = new CommandTrack("Track");
            block.Tracks.Add(track);
            StatusCommand weightedFailure = AddStatusCommand(
                block,
                track,
                CompositeExecutionStatus.Failure);
            StatusCommand zeroWeightSuccess = AddStatusCommand(
                block,
                track,
                CompositeExecutionStatus.Success);
            weightedFailure.CompositeWeight = 100f;
            zeroWeightSuccess.CompositeWeight = 0f;

            yield return block.Execute();

            Assert.That(weightedFailure.ExecuteCount, Is.Zero);
            Assert.That(zeroWeightSuccess.ExecuteCount, Is.EqualTo(1));
            Assert.That(
                block.LastCompositeExecutionStatus,
                Is.EqualTo(CompositeExecutionStatus.Success));
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator ParallelSelectorWaitAll_SucceedsWhenAnyCommandSucceeds()
        {
            Block block = CreateBlock();
            block.Tracks.Clear();
            block.ExecutionMethod = CompositeExecutionMethod.ParallelSelector;
            block.AwaitMode = CompositeAwaitMode.WaitAll;
            CommandTrack track = new CommandTrack("Track");
            block.Tracks.Add(track);
            StatusCommand failure = AddStatusCommand(
                block,
                track,
                CompositeExecutionStatus.Failure);
            StatusCommand success = AddStatusCommand(
                block,
                track,
                CompositeExecutionStatus.Success);

            yield return block.Execute();

            Assert.That(failure.ExecuteCount, Is.EqualTo(1));
            Assert.That(success.ExecuteCount, Is.EqualTo(1));
            Assert.That(
                block.LastCompositeExecutionStatus,
                Is.EqualTo(CompositeExecutionStatus.Success));
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator StopAllBlocks_ResetsCompletedCommandFeedback()
        {
            Block block = CreateBlock();
            block.Tracks.Clear();
            block.ExecutionMethod = CompositeExecutionMethod.Selector;
            CommandTrack track = new CommandTrack("Track");
            block.Tracks.Add(track);
            StatusCommand failure = AddStatusCommand(
                block,
                track,
                CompositeExecutionStatus.Failure);
            StatusCommand success = AddStatusCommand(
                block,
                track,
                CompositeExecutionStatus.Success);

            yield return block.Execute();

            Assert.That(
                block.TryGetCommandExecutionStatus(failure, out CompositeExecutionStatus failureStatus),
                Is.True);
            Assert.That(failureStatus, Is.EqualTo(CompositeExecutionStatus.Failure));
            Assert.That(
                block.TryGetCommandExecutionStatus(success, out CompositeExecutionStatus successStatus),
                Is.True);
            Assert.That(successStatus, Is.EqualTo(CompositeExecutionStatus.Success));

            block.GetBlackboard().StopAllBlocks();

            Assert.That(block.TryGetCommandExecutionStatus(failure, out _), Is.False);
            Assert.That(block.TryGetCommandExecutionStatus(success, out _), Is.False);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator StopAllBlocksAndRestartBlock_RestartsTheSelectedBlockAfterStoppingIt()
        {
            Block block = CreateBlock();
            CommandTrack track = block.Tracks[0];
            List<string> log = new List<string>();
            AddCommand(block, track, "running", log, framesToWait: 10);
            Blackboard blackboard = block.GetBlackboard();

            Assert.That(blackboard.ExecuteBlock(block), Is.True);
            yield return null;
            Assert.That(block.IsExecuting(), Is.True);

            blackboard.StopAllBlocksAndRestartBlock(block);

            yield return null;
            yield return null;

            Assert.That(block.GetExecutionCount(), Is.EqualTo(2));
            Assert.That(block.IsExecuting(), Is.True);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator RandomOrder_UsesCommandWeightsThroughTheSharedRunner()
        {
            TestCompositeBlock block = (TestCompositeBlock)CreateBlock();
            block.Tracks.Clear();
            block.ExecutionMethod = CompositeExecutionMethod.Sequence;
            block.OrderMode = CompositeOrderMode.Random;
            block.SetRandomValues(0.5f, 0.5f);
            CommandTrack track = new CommandTrack("Track");
            block.Tracks.Add(track);
            List<string> log = new List<string>();
            TestCommand zeroWeightCommand = AddCommand(block, track, "zero", log);
            TestCommand weightedCommand = AddCommand(block, track, "weighted", log);
            zeroWeightCommand.CompositeWeight = 0f;
            weightedCommand.CompositeWeight = 100f;

            yield return block.Execute();

            CollectionAssert.AreEqual(new[] { "weighted", "zero" }, log);
        }

        [Test]
        public void CommandWeights_AutomaticallyBalanceEnabledCommands()
        {
            Block block = CreateBlock();
            block.Tracks.Clear();
            CommandTrack track = new CommandTrack("Track");
            block.Tracks.Add(track);
            List<string> log = new List<string>();
            TestCommand first = AddCommand(block, track, "first", log);
            TestCommand second = AddCommand(block, track, "second", log);
            TestCommand third = AddCommand(block, track, "third", log);

            Assert.That(block.GetCommandWeight(first), Is.EqualTo(100f / 3f).Within(0.001f));
            Assert.That(block.GetCommandWeight(second), Is.EqualTo(100f / 3f).Within(0.001f));
            Assert.That(block.GetCommandWeight(third), Is.EqualTo(100f / 3f).Within(0.001f));

            third.enabled = false;

            Assert.That(block.GetCommandWeight(first), Is.EqualTo(50f).Within(0.001f));
            Assert.That(block.GetCommandWeight(second), Is.EqualTo(50f).Within(0.001f));
            Assert.That(block.GetCommandWeight(third), Is.Zero);
        }

        [Test]
        public void CommandWeightOverrides_ReserveWeightAndAutomaticCommandsShareTheRemainder()
        {
            Block block = CreateBlock();
            block.Tracks.Clear();
            CommandTrack track = new CommandTrack("Track");
            block.Tracks.Add(track);
            List<string> log = new List<string>();
            TestCommand first = AddCommand(block, track, "first", log);
            TestCommand second = AddCommand(block, track, "second", log);
            TestCommand automatic = AddCommand(block, track, "automatic", log);

            first.CompositeWeight = 40f;
            second.CompositeWeight = 20f;

            Assert.That(first.HasCompositeWeightOverride, Is.True);
            Assert.That(block.GetCommandWeight(first), Is.EqualTo(40f).Within(0.001f));
            Assert.That(block.GetCommandWeight(second), Is.EqualTo(20f).Within(0.001f));
            Assert.That(block.GetCommandWeight(automatic), Is.EqualTo(40f).Within(0.001f));

            second.ClearCompositeWeightOverride();

            Assert.That(second.HasCompositeWeightOverride, Is.False);
            Assert.That(block.GetCommandWeight(first), Is.EqualTo(40f).Within(0.001f));
            Assert.That(block.GetCommandWeight(second), Is.EqualTo(30f).Within(0.001f));
            Assert.That(block.GetCommandWeight(automatic), Is.EqualTo(30f).Within(0.001f));
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator UtilitySelector_UsesCommandUtilityThroughTheSharedRunner()
        {
            Block block = CreateBlock();
            block.Tracks.Clear();
            block.ExecutionMethod = CompositeExecutionMethod.UtilitySelector;
            CommandTrack track = new CommandTrack("Track");
            block.Tracks.Add(track);
            List<string> log = new List<string>();
            TestCommand lowUtilityCommand = AddCommand(block, track, "low", log);
            TestCommand highUtilityCommand = AddCommand(block, track, "high", log);
            lowUtilityCommand.CompositeUtility = 1f;
            highUtilityCommand.CompositeUtility = 5f;

            yield return block.Execute();

            CollectionAssert.AreEqual(new[] { "high" }, log);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator ParallelWaitAny_ReturnsWhileOtherCommandsContinue()
        {
            Block block = CreateBlock();
            block.ExecutionMethod = CompositeExecutionMethod.Parallel;
            block.AwaitMode = CompositeAwaitMode.WaitAny;
            CommandTrack track = block.Tracks[0];
            List<string> log = new List<string>();
            AddCommand(block, track, "fast", log);
            TestCommand slow = AddCommand(block, track, "slow", log, framesToWait: 3);

            yield return block.Execute();

            Assert.That(block.State, Is.EqualTo(ExecutionState.Idle));
            Assert.That(slow.IsExecuting, Is.True);
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            Assert.That(slow.IsExecuting, Is.False);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator ParallelWaitNone_ReturnsImmediatelyWhileEveryCommandContinues()
        {
            Block block = CreateBlock();
            block.ExecutionMethod = CompositeExecutionMethod.Parallel;
            block.AwaitMode = CompositeAwaitMode.WaitNone;
            CommandTrack track = block.Tracks[0];
            List<string> log = new List<string>();
            TestCommand first = AddCommand(block, track, "first", log, framesToWait: 3);
            TestCommand second = AddCommand(block, track, "second", log, framesToWait: 3);

            yield return block.Execute();

            Assert.That(block.State, Is.EqualTo(ExecutionState.Idle));
            Assert.That(first.IsExecuting, Is.True);
            Assert.That(second.IsExecuting, Is.True);
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            Assert.That(first.IsExecuting, Is.False);
            Assert.That(second.IsExecuting, Is.False);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator Shuffle_IgnoresWeightsAndRunsEveryCommandOnce()
        {
            TestCompositeBlock block = (TestCompositeBlock)CreateBlock();
            block.ExecutionMethod = CompositeExecutionMethod.Sequence;
            block.OrderMode = CompositeOrderMode.Shuffle;
            block.SetRandomValues(0f);
            CommandTrack track = block.Tracks[0];
            List<string> log = new List<string>();
            TestCommand first = AddCommand(block, track, "first", log);
            TestCommand second = AddCommand(block, track, "second", log);
            first.CompositeWeight = 100f;
            second.CompositeWeight = 0f;

            yield return block.Execute();

            CollectionAssert.AreEqual(new[] { "second", "first" }, log);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator Shuffle_CanAvoidTheLastCommandFromThePreviousRun()
        {
            TestCompositeBlock block = (TestCompositeBlock)CreateBlock();
            block.ExecutionMethod = CompositeExecutionMethod.Sequence;
            block.OrderMode = CompositeOrderMode.Shuffle;
            block.AvoidRepeatingLastCommand = true;
            block.SetRandomValues(0f, 0.999f);
            CommandTrack track = block.Tracks[0];
            List<string> log = new List<string>();
            AddCommand(block, track, "first", log);
            AddCommand(block, track, "second", log);

            yield return block.Execute();
            CollectionAssert.AreEqual(new[] { "second", "first" }, log);
            log.Clear();

            yield return block.Execute();

            CollectionAssert.AreEqual(new[] { "second", "first" }, log);
        }

        private static StatusCommand AddStatusCommand(
            Block block,
            CommandTrack track,
            CompositeExecutionStatus status)
        {
            StatusCommand command = block.gameObject.AddComponent<StatusCommand>();
            command.Status = status;
            track.Commands.Add(command);
            return command;
        }

        /// <summary>
        /// Minimal test-only Command: logs its tag on enter, then optionally waits N frames before continuing.
        /// </summary>
        private class TestCommand : Command
        {
            public string Tag;
            public List<string> Log;
            public int FramesToWait;
            public int EnterFrame = -1;

            public override void OnEnter()
            {
                EnterFrame = Time.frameCount;
                Log?.Add(Tag);

                if (FramesToWait <= 0)
                {
                    Continue();
                }
                else
                {
                    StartCoroutine(DelayedContinue());
                }
            }

            private IEnumerator DelayedContinue()
            {
                for (int i = 0; i < FramesToWait; i++)
                {
                    yield return null;
                }
                Continue();
            }
        }

        private class StatusCommand : Command, ICompositeExecutionStatusProvider
        {
            public CompositeExecutionStatus Status;

            public int ExecuteCount { get; private set; }

            public CompositeExecutionStatus LastCompositeExecutionStatus => Status;

            public override void OnEnter()
            {
                ExecuteCount++;
                Continue();
            }
        }

        private class TestCompositeBlock : Block
        {
            private readonly Queue<float> randomValues = new Queue<float>();

            public void SetRandomValues(params float[] values)
            {
                randomValues.Clear();
                foreach (float value in values)
                {
                    randomValues.Enqueue(value);
                }
            }

            protected override float GetCompositeRandomValue()
            {
                return randomValues.Count > 0 ? randomValues.Dequeue() : 0f;
            }
        }

        /// <summary>
        /// Minimal test-only stand-in for an "If": jumps to the next same-indent Else/End
        /// within its own track when Result is false, matching Condition.OnFalse's shape.
        /// </summary>
        private class TestIfCommand : Command
        {
            public bool Result = true;

            public override void OnEnter()
            {
                if (Result)
                {
                    Continue();
                    return;
                }

                List<Command> commands = ParentTrack.Commands;
                for (int i = CommandIndex + 1; i < commands.Count; i++)
                {
                    Command cmd = commands[i];
                    if (cmd.IndentLevel != IndentLevel)
                    {
                        continue;
                    }
                    if (cmd is TestElseCommand || cmd is TestEndCommand)
                    {
                        Continue(cmd.CommandIndex + 1);
                        return;
                    }
                }

                StopParentBlock();
            }

            public override bool OpenBlock()
            {
                return true;
            }
        }

        private class TestElseCommand : Command
        {
            public override void OnEnter()
            {
                Continue();
            }

            public override bool OpenBlock()
            {
                return true;
            }

            public override bool CloseBlock()
            {
                return true;
            }
        }

        private class TestEndCommand : Command
        {
            public override void OnEnter()
            {
                Continue();
            }

            public override bool CloseBlock()
            {
                return true;
            }
        }
    }
}
