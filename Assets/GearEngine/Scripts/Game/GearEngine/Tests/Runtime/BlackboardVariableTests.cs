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
    public class BlackboardVariableTests
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
            _block.BlockName = "VarBlock";
        }

        [TearDown]
        public void Teardown()
        {
            if (_blackboardObject != null)
            {
                Object.DestroyImmediate(_blackboardObject);
            }
        }

        private InvokeActionCommand AddCommand(IAction action)
        {
            InvokeActionCommand cmd = _blackboardObject.AddComponent<InvokeActionCommand>();
            cmd.ParentBlock = _block;
            cmd.actions.Add(new InvokeActionCommand.ActionWrapper(action));
            _block.CommandList.Add(cmd);
            return cmd;
        }

        [UnityTest]
        public IEnumerator Blackboard_SetVariable_ReflectsInComponent()
        {
            IntegerVariable resultVar = _blackboardObject.AddComponent<Scaffold.IntegerVariable>();
            resultVar.Key = "Score";
            resultVar.Value = 0;
            _blackboard.Variables.Add(resultVar);

            // Set Score = 100
            SetVariable setVar = new SetVariable();
            TestReflectionUtils.SetupSetVariableActionInt(setVar, resultVar, Scaffold.SetOperator.Assign, 100);
            AddCommand(setVar);

            _blackboard.ExecuteBlock(_block);
            yield return null;

            Assert.That(resultVar.Value, Is.EqualTo(100));

            // Set Score += 50
            SetVariable addVar = new SetVariable();
            TestReflectionUtils.SetupSetVariableActionInt(addVar, resultVar, Scaffold.SetOperator.Add, 50);
            AddCommand(addVar);

            _blackboard.ExecuteBlock(_block); // Executes both again, so it will set to 100, then add 50
            yield return null;

            Assert.That(resultVar.Value, Is.EqualTo(150));
        }

        [UnityTest]
        public IEnumerator Blackboard_CrossNodeReference_InjectsCorrectly()
        {
            // We create a float variable
            FloatVariable timeVar = _blackboardObject.AddComponent<Scaffold.FloatVariable>();
            timeVar.Key = "WaitTime";
            timeVar.Value = 0.5f;
            _blackboard.Variables.Add(timeVar);

            StringVariable stringVar = _blackboardObject.AddComponent<Scaffold.StringVariable>();
            stringVar.Key = "State";
            stringVar.Value = "Start";
            _blackboard.Variables.Add(stringVar);

            // Wait using the float variable reference
            Wait waitAction = new Wait();
            FloatData floatData = new FloatData();
            floatData.floatRef = timeVar;
            floatData.source = VariableDataSource.BlackboardVariable;
            TestReflectionUtils.SetProtectedField(waitAction, "duration", floatData);
            AddCommand(waitAction);

            SetVariable setVar = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setVar, stringVar, Scaffold.SetOperator.Assign, "End");
            AddCommand(setVar);

            _blackboard.ExecuteBlock(_block);
            yield return null;

            // Still Start
            Assert.That(stringVar.Value, Is.EqualTo("Start"));

            // Wait 0.2s -> Still Start
            yield return new WaitForSeconds(0.2f);
            Assert.That(stringVar.Value, Is.EqualTo("Start"));

            // Wait total 0.6s -> Should be End
            yield return new WaitForSeconds(0.4f);
            Assert.That(stringVar.Value, Is.EqualTo("End"));
        }
    }
}
