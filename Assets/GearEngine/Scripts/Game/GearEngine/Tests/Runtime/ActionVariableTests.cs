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
    public class ActionVariableTests
    {
        private GameObject _blackboardObject;
        private Blackboard _blackboard;
        private Block _block;
        private InvokeActionCommand _command;

        [SetUp]
        public void Setup()
        {
            _blackboardObject = new GameObject("TestBlackboard");
            _blackboard = _blackboardObject.AddComponent<Blackboard>();
            _blackboardObject.AddComponent<EventDispatcher>();

            _block = _blackboardObject.AddComponent<Block>();
            _block.BlockName = "MainBlock";

            _command = _blackboardObject.AddComponent<InvokeActionCommand>();
            _command.ParentBlock = _block;
            _block.CommandList.Add(_command);
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
        public IEnumerator SetVariable_AssignsStringProperly()
        {
            Scaffold.StringVariable variable = _blackboardObject.AddComponent<Scaffold.StringVariable>();
            variable.Key = "MyVar";
            variable.Value = "Empty";
            _blackboard.Variables.Add(variable);

            SetVariable action = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(action, variable, Scaffold.SetOperator.Assign, "UpdatedValue");

            _command.actions.Add(new InvokeActionCommand.ActionWrapper(action));

            _blackboard.ExecuteBlock(_block);
            yield return null;

            Assert.That(variable.Value, Is.EqualTo("UpdatedValue"));
        }

        [UnityTest]
        public IEnumerator SetVariable_AddsIntegerProperly()
        {
            Scaffold.IntegerVariable variable = _blackboardObject.AddComponent<Scaffold.IntegerVariable>();
            variable.Key = "MyInt";
            variable.Value = 10;
            _blackboard.Variables.Add(variable);

            SetVariable action = new SetVariable();
            TestReflectionUtils.SetupSetVariableActionInt(action, variable, Scaffold.SetOperator.Add, 5);

            _command.actions.Add(new InvokeActionCommand.ActionWrapper(action));

            _blackboard.ExecuteBlock(_block);
            yield return null;

            Assert.That(variable.Value, Is.EqualTo(15));
        }
    }
}
