using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Scaffold;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Presentation.UI.Input;

namespace Game.GearEngine.RuntimeTests
{
    public class BlockCallTests
    {
        private GameObject _blackboardObject;
        private Blackboard _blackboard;

        [SetUp]
        public void Setup()
        {
            _blackboardObject = new GameObject("TestBlackboard");
            _blackboard = _blackboardObject.AddComponent<Blackboard>();
            _blackboardObject.AddComponent<EventDispatcher>();
        }

        [TearDown]
        public void Teardown()
        {
            if (_blackboardObject != null)
            {
                Object.DestroyImmediate(_blackboardObject);
            }
        }

        private Block CreateBlock(string name)
        {
            Block block = _blackboardObject.AddComponent<Block>();
            block.BlockName = name;
            return block;
        }

        private InvokeActionCommand AddCommand(Block block, IAction action)
        {
            InvokeActionCommand cmd = _blackboardObject.AddComponent<InvokeActionCommand>();
            cmd.ParentBlock = block;
            cmd.actions.Add(new InvokeActionCommand.ActionWrapper(action));
            block.CommandList.Add(cmd);
            return cmd;
        }

        [UnityTest]
        public IEnumerator Call_WaitUntilFinished_WaitsForTargetBlockToFinish()
        {
            Block blockA = CreateBlock("BlockA");
            Block blockB = CreateBlock("BlockB");

            StringVariable resultVar = _blackboardObject.AddComponent<Scaffold.StringVariable>();
            resultVar.Key = "Result";
            resultVar.Value = "Start";
            _blackboard.Variables.Add(resultVar);

            // Block B: Waits 0.5s, then Sets Result = "BlockB_Done"
            Wait waitB = new Wait();
            TestReflectionUtils.SetProtectedField(waitB, "duration", new FloatData { Value = 0.5f });
            AddCommand(blockB, waitB);

            SetVariable setB = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setB, resultVar, Scaffold.SetOperator.Assign, "BlockB_Done");
            AddCommand(blockB, setB);

            // Block A: Call Block B (WaitUntilFinished), then Set Result = "BlockA_Done"
            Call callAction = new Call();
            TestReflectionUtils.SetupCallAction(callAction, blockB);
            TestReflectionUtils.SetProtectedField(callAction, "callMode", CallMode.WaitUntilFinished);
            AddCommand(blockA, callAction);

            SetVariable setA = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setA, resultVar, Scaffold.SetOperator.Assign, "BlockA_Done");
            AddCommand(blockA, setA);

            _blackboard.ExecuteBlock(blockA);

            // Should be "Start" initially
            Assert.That(resultVar.Value, Is.EqualTo("Start"));

            // Wait 0.2s - Block B is waiting, Block A is waiting for B
            yield return new WaitForSeconds(0.2f);
            Assert.That(resultVar.Value, Is.EqualTo("Start"));

            // Wait until B finishes and returns to A
            yield return new WaitForSeconds(0.5f);
            Assert.That(resultVar.Value, Is.EqualTo("BlockA_Done"));
        }

        [UnityTest]
        public IEnumerator Call_Continue_ExecutesInParallel()
        {
            Block blockA = CreateBlock("BlockA");
            Block blockB = CreateBlock("BlockB");

            StringVariable resultVar = _blackboardObject.AddComponent<Scaffold.StringVariable>();
            resultVar.Key = "Result";
            resultVar.Value = "Start";
            _blackboard.Variables.Add(resultVar);

            // Block B: Waits 0.5s, then Sets Result = "BlockB_Done"
            Wait waitB = new Wait();
            TestReflectionUtils.SetProtectedField(waitB, "duration", new FloatData { Value = 0.5f });
            AddCommand(blockB, waitB);

            SetVariable setB = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setB, resultVar, Scaffold.SetOperator.Assign, "BlockB_Done");
            AddCommand(blockB, setB);

            // Block A: Call Block B (Continue), then Set Result = "BlockA_Done" immediately
            Call callAction = new Call();
            TestReflectionUtils.SetupCallAction(callAction, blockB);
            TestReflectionUtils.SetProtectedField(callAction, "callMode", CallMode.Continue);
            AddCommand(blockA, callAction);

            SetVariable setA = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setA, resultVar, Scaffold.SetOperator.Assign, "BlockA_Done");
            AddCommand(blockA, setA);

            _blackboard.ExecuteBlock(blockA);

            // Block A finishes immediately while Block B is still waiting
            yield return null;
            Assert.That(resultVar.Value, Is.EqualTo("BlockA_Done"));

            // Wait for Block B to finish and overwrite the result
            yield return new WaitForSeconds(0.6f);
            Assert.That(resultVar.Value, Is.EqualTo("BlockB_Done"));
        }

        [UnityTest]
        public IEnumerator Call_Stop_AbortsCurrentBlockExecution()
        {
            Block blockA = CreateBlock("BlockA");
            Block blockB = CreateBlock("BlockB");

            StringVariable resultVar = _blackboardObject.AddComponent<Scaffold.StringVariable>();
            resultVar.Key = "Result";
            resultVar.Value = "Start";
            _blackboard.Variables.Add(resultVar);

            // Block B: Set Result = "BlockB_Done"
            SetVariable setB = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setB, resultVar, Scaffold.SetOperator.Assign, "BlockB_Done");
            AddCommand(blockB, setB);

            // Block A: Call Block B (Stop), then Set Result = "BlockA_Done" (Should never execute)
            Call callAction = new Call();
            TestReflectionUtils.SetupCallAction(callAction, blockB);
            TestReflectionUtils.SetProtectedField(callAction, "callMode", CallMode.Stop);
            AddCommand(blockA, callAction);

            SetVariable setA = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setA, resultVar, Scaffold.SetOperator.Assign, "BlockA_Done");
            AddCommand(blockA, setA);

            _blackboard.ExecuteBlock(blockA);
            yield return null;

            // Block A stopped, Block B executed
            Assert.That(resultVar.Value, Is.EqualTo("BlockB_Done"));
        }
    }
}
