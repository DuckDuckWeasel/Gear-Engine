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
    public class ActionLogicTests
    {
        private GameObject _blackboardObject;
        private Blackboard _blackboard;
        private Block _block;

        [SetUp]
        public void Setup()
        {
            _blackboardObject = new GameObject("TestBlackboard");
            _blackboard = _blackboardObject.AddComponent<Blackboard>();
            _blackboardObject.AddComponent<EventDispatcher>();

            _block = _blackboardObject.AddComponent<Block>();
            _block.BlockName = "LogicBlock";
        }

        [TearDown]
        public void Teardown()
        {
            if (_blackboardObject != null)
            {
                Object.DestroyImmediate(_blackboardObject);
            }
        }

        private InvokeActionCommand AddCommand()
        {
            InvokeActionCommand cmd = _blackboardObject.AddComponent<InvokeActionCommand>();
            cmd.ParentBlock = _block;
            _block.CommandList.Add(cmd);
            return cmd;
        }

        [UnityTest]
        public IEnumerator If_ExecutesWhenConditionIsTrue_AndSkipsElse()
        {
            Scaffold.BooleanVariable boolVar = _blackboardObject.AddComponent<Scaffold.BooleanVariable>();
            boolVar.Key = "Condition";
            boolVar.Value = true;
            _blackboard.Variables.Add(boolVar);

            Scaffold.StringVariable resultVar = _blackboardObject.AddComponent<Scaffold.StringVariable>();
            resultVar.Key = "Result";
            resultVar.Value = "None";
            _blackboard.Variables.Add(resultVar);

            // If action
            InvokeActionCommand cmdIf = AddCommand();
            If ifAction = new If();
            TestReflectionUtils.SetupIfAction(ifAction, boolVar, Scaffold.CompareOperator.Equals, true);
            cmdIf.actions.Add(new InvokeActionCommand.ActionWrapper(ifAction));

            // Set result "IfTrue"
            InvokeActionCommand cmdTrue = AddCommand();
            SetVariable setTrue = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setTrue, resultVar, Scaffold.SetOperator.Assign, "IfTrue");
            cmdTrue.actions.Add(new InvokeActionCommand.ActionWrapper(setTrue));

            // Else action
            InvokeActionCommand cmdElse = AddCommand();
            Else elseAction = new Else();
            cmdElse.actions.Add(new InvokeActionCommand.ActionWrapper(elseAction));

            // Set result "IfFalse"
            InvokeActionCommand cmdFalse = AddCommand();
            SetVariable setFalse = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setFalse, resultVar, Scaffold.SetOperator.Assign, "IfFalse");
            cmdFalse.actions.Add(new InvokeActionCommand.ActionWrapper(setFalse));

            // End action
            InvokeActionCommand cmdEnd = AddCommand();
            End endAction = new End();
            cmdEnd.actions.Add(new InvokeActionCommand.ActionWrapper(endAction));

            _blackboard.ExecuteBlock(_block);
            yield return null;

            Assert.That(resultVar.Value, Is.EqualTo("IfTrue"));
        }

        [UnityTest]
        public IEnumerator If_SkipsWhenConditionIsFalse_AndExecutesElse()
        {
            Scaffold.BooleanVariable boolVar = _blackboardObject.AddComponent<Scaffold.BooleanVariable>();
            boolVar.Key = "Condition";
            boolVar.Value = false;
            _blackboard.Variables.Add(boolVar);

            Scaffold.StringVariable resultVar = _blackboardObject.AddComponent<Scaffold.StringVariable>();
            resultVar.Key = "Result";
            resultVar.Value = "None";
            _blackboard.Variables.Add(resultVar);

            // If action
            InvokeActionCommand cmdIf = AddCommand();
            If ifAction = new If();
            TestReflectionUtils.SetupIfAction(ifAction, boolVar, Scaffold.CompareOperator.Equals, true);
            cmdIf.actions.Add(new InvokeActionCommand.ActionWrapper(ifAction));

            // Set result "IfTrue"
            InvokeActionCommand cmdTrue = AddCommand();
            SetVariable setTrue = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setTrue, resultVar, Scaffold.SetOperator.Assign, "IfTrue");
            cmdTrue.actions.Add(new InvokeActionCommand.ActionWrapper(setTrue));

            // Else action
            InvokeActionCommand cmdElse = AddCommand();
            Else elseAction = new Else();
            cmdElse.actions.Add(new InvokeActionCommand.ActionWrapper(elseAction));

            // Set result "IfFalse"
            InvokeActionCommand cmdFalse = AddCommand();
            SetVariable setFalse = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setFalse, resultVar, Scaffold.SetOperator.Assign, "IfFalse");
            cmdFalse.actions.Add(new InvokeActionCommand.ActionWrapper(setFalse));

            // End action
            InvokeActionCommand cmdEnd = AddCommand();
            End endAction = new End();
            cmdEnd.actions.Add(new InvokeActionCommand.ActionWrapper(endAction));

            _blackboard.ExecuteBlock(_block);
            yield return null;

            Assert.That(resultVar.Value, Is.EqualTo("IfFalse"));
        }
    }
}
