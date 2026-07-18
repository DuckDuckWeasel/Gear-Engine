using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Scaffold;

namespace Scaffold.Tests.PlayMode
{
    /// <summary>
    /// Validates the CommandTrack / parallel execution engine on Block:
    /// single-track back-compat, flow control (If/Else/End) inside a non-primary
    /// track, and several tracks actually running concurrently.
    ///
    /// Flow control here uses small self-contained test Commands (TestIfCommand /
    /// TestElseCommand / TestEndCommand) rather than the real Scaffold Condition/Else/End,
    /// which are ActionBase (IAction) payloads normally hosted inside an InvokeActionCommand
    /// wrapper. That wrapper's command-list scanning (Condition.OnFalse / FindMatchingEndCommand)
    /// compares wrapper Command instances against typeof(Else)/typeof(End), which can never
    /// match since the list holds InvokeActionCommand wrappers, not the wrapped action types.
    /// That's a pre-existing issue independent of the Track work and is out of scope here;
    /// these tests instead validate the exact Block-level pieces this change touched
    /// (ParentTrack scoping, per-track CommandIndex/IndentLevel, Continue(int) routing).
    /// </summary>
    public class BlockTrackExecutionTests
    {
        private readonly List<GameObject> _spawned = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _spawned)
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
            var go = new GameObject("TestFlowchart");
            _spawned.Add(go);
            go.AddComponent<Flowchart>();
            return go.AddComponent<Block>();
        }

        private static TestCommand AddCommand(Block block, CommandTrack track, string tag, List<string> log, int framesToWait = 0)
        {
            var cmd = block.gameObject.AddComponent<TestCommand>();
            cmd.Tag = tag;
            cmd.Log = log;
            cmd.FramesToWait = framesToWait;
            track.Commands.Add(cmd);
            return cmd;
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator SequentialCommands_StillExecuteInOrder()
        {
            var block = CreateBlock();
            var track0 = new CommandTrack("Track 0");
            block.Tracks.Add(track0);

            var log = new List<string>();
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
            var block = CreateBlock();

            // Track 0 is intentionally empty: the flow-control chain lives on Track 1
            // to prove commands are scoped to their own track, not always Track 0.
            var track0 = new CommandTrack("Track 0");
            var track1 = new CommandTrack("Track 1");
            block.Tracks.Add(track0);
            block.Tracks.Add(track1);

            var log = new List<string>();

            var ifCmd = block.gameObject.AddComponent<TestIfCommand>();
            ifCmd.Result = false; // force the Else branch
            track1.Commands.Add(ifCmd);

            AddCommand(block, track1, "insideIf", log);

            var elseCmd = block.gameObject.AddComponent<TestElseCommand>();
            track1.Commands.Add(elseCmd);

            AddCommand(block, track1, "insideElse", log);

            var endCmd = block.gameObject.AddComponent<TestEndCommand>();
            track1.Commands.Add(endCmd);

            AddCommand(block, track1, "afterEnd", log);

            yield return block.Execute();

            CollectionAssert.AreEqual(new[] { "insideElse", "afterEnd" }, log);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator MultipleTracks_ExecuteInParallel_WithWaitAll()
        {
            var block = CreateBlock();
            block.ExecutionMethod = BlockExecutionMethod.AllAtSameTime;
            block.AwaitMode = BlockAwaitMode.WaitAll;

            var trackA = new CommandTrack("Track A");
            var trackB = new CommandTrack("Track B");
            block.Tracks.Add(trackA);
            block.Tracks.Add(trackB);

            var log = new List<string>();
            var slow = AddCommand(block, trackA, "slow-start", log, framesToWait: 3);
            AddCommand(block, trackA, "slow-end", log);
            var fast = AddCommand(block, trackB, "fast", log);

            yield return block.Execute();

            // Both tracks must have started before the slow one finished:
            // proves Track B didn't wait for Track A to complete before running.
            Assert.LessOrEqual(fast.EnterFrame, slow.EnterFrame);
            CollectionAssert.Contains(log, "fast");
            CollectionAssert.Contains(log, "slow-end");
            // "fast" (Track B, no delay) must be logged before "slow-end" (Track A, waits 3 frames).
            Assert.Less(log.IndexOf("fast"), log.IndexOf("slow-end"));
            Assert.AreEqual(ExecutionState.Idle, block.State);
        }

        [UnityTest]
        [Timeout(5000)]
        public IEnumerator MultipleTracks_ExecuteInSequence_WhenConfigured()
        {
            var block = CreateBlock();
            block.ExecutionMethod = BlockExecutionMethod.Sequence;

            var trackA = new CommandTrack("Track A");
            var trackB = new CommandTrack("Track B");
            block.Tracks.Add(trackA);
            block.Tracks.Add(trackB);

            var log = new List<string>();
            var firstTrackAction = AddCommand(block, trackA, "first", log, framesToWait: 2);
            AddCommand(block, trackA, "first-end", log);
            var secondTrackAction = AddCommand(block, trackB, "second", log);

            yield return block.Execute();

            Assert.Greater(secondTrackAction.EnterFrame, firstTrackAction.EnterFrame);
            Assert.Less(log.IndexOf("first-end"), log.IndexOf("second"));
            Assert.AreEqual(ExecutionState.Idle, block.State);
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

                var commands = ParentTrack.Commands;
                for (int i = CommandIndex + 1; i < commands.Count; i++)
                {
                    var cmd = commands[i];
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
