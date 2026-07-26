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
    public class BlackboardExecutionTests
    {
        private GameObject _blackboardObject;
        private Blackboard _blackboard;

        [SetUp]
        public void Setup()
        {
            _blackboardObject = new GameObject("TestBlackboard");
            _blackboard = _blackboardObject.AddComponent<Blackboard>();

            // Required dependencies for Actions
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
        public IEnumerator Blackboard_CanExecuteSingleBlock_WithGenericAction()
        {
            // 1. Create a Block
            Block block = _blackboardObject.AddComponent<Block>();
            block.BlockName = "StartBlock";

            // 2. Add InvokeActionCommand to the Block
            InvokeActionCommand invokeCommand = _blackboardObject.AddComponent<InvokeActionCommand>();
            invokeCommand.ParentBlock = block;
            block.CommandList.Add(invokeCommand);

            // Ensure the variable exists in the blackboard
            Scaffold.StringVariable variable = _blackboardObject.AddComponent<Scaffold.StringVariable>();
            variable.Key = "TestVar";
            variable.Value = "Initial";
            _blackboard.Variables.Add(variable);

            // 3. Create an Action that sets a variable so we can verify execution
            SetVariable action = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(action, variable, Scaffold.SetOperator.Assign, "Executed");

            // Add action via wrapper
            invokeCommand.actions.Add(new InvokeActionCommand.ActionWrapper(action));

            // 4. Execute the Block
            _blackboard.ExecuteBlock(block);

            // 5. Yield a frame to let Coroutines / UniTask run
            yield return null;

            // 6. Verify the variable was changed
            Assert.That(variable.Value, Is.EqualTo("Executed"));
        }
    }
}
