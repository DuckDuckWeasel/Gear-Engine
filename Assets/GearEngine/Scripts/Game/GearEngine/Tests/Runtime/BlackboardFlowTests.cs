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
    public class BlackboardFlowTests
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

        [UnityTest]
        public IEnumerator CallAction_TransitionsToTargetBlock_AndReturns()
        {
            // Block 1 (Caller)
            Block callerBlock = _blackboardObject.AddComponent<Block>();
            callerBlock.BlockName = "CallerBlock";

            // Block 2 (Target)
            Block targetBlock = _blackboardObject.AddComponent<Block>();
            targetBlock.BlockName = "TargetBlock";

            // Add Command to Caller
            InvokeActionCommand callerCommand = _blackboardObject.AddComponent<InvokeActionCommand>();
            callerCommand.ParentBlock = callerBlock;
            callerBlock.CommandList.Add(callerCommand);

            // Add Call Action to Caller
            Call callAction = new Call();
            TestReflectionUtils.SetupCallAction(callAction, targetBlock);
            callerCommand.actions.Add(new InvokeActionCommand.ActionWrapper(callAction));

            // Variable to verify Target execution
            Scaffold.StringVariable variable = _blackboardObject.AddComponent<Scaffold.StringVariable>();
            variable.Key = "TestVar";
            variable.Value = "Initial";
            _blackboard.Variables.Add(variable);

            // Add Command to Target
            InvokeActionCommand targetCommand = _blackboardObject.AddComponent<InvokeActionCommand>();
            targetCommand.ParentBlock = targetBlock;
            targetBlock.CommandList.Add(targetCommand);

            // Add SetVariable Action to Target
            // Add SetVariable Action to Target
            SetVariable setVarAction = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setVarAction, variable, Scaffold.SetOperator.Assign, "TargetExecuted");
            targetCommand.actions.Add(new InvokeActionCommand.ActionWrapper(setVarAction));

            // Execute Caller
            _blackboard.ExecuteBlock(callerBlock);

            // Wait 2 frames to ensure the call transition and execution complete
            yield return null;
            yield return null;

            // Verify the variable was changed by the target block
            Assert.That(variable.Value, Is.EqualTo("TargetExecuted"));
        }
    }
}
