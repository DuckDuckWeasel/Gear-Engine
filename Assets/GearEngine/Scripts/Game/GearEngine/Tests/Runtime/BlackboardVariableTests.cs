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
        private GameObject blackboardObject;
        private Blackboard blackboard;
        private Block block;

        [SetUp]
        public void Setup()
        {
            blackboardObject = new GameObject("TestBlackboard");
            blackboard = blackboardObject.AddComponent<Blackboard>();
            blackboardObject.AddComponent<EventDispatcher>();

            block = blackboardObject.AddComponent<Block>();
            block.BlockName = "VarBlock";
        }

        [TearDown]
        public void Teardown()
        {
            if (blackboardObject != null)
            {
                Object.DestroyImmediate(blackboardObject);
            }
        }

        private InvokeActionCommand AddCommand(IAction action)
        {
            InvokeActionCommand cmd = blackboardObject.AddComponent<InvokeActionCommand>();
            cmd.ParentBlock = block;
            cmd.actions.Add(new InvokeActionCommand.ActionWrapper(action));
            block.CommandList.Add(cmd);
            return cmd;
        }

        [UnityTest]
        public IEnumerator Blackboard_SetVariable_ReflectsInComponent()
        {
            IntegerVariable resultVar = blackboardObject.AddComponent<Scaffold.IntegerVariable>();
            resultVar.Key = "Score";
            resultVar.Value = 0;
            blackboard.Variables.Add(resultVar);

            // Set Score = 100
            SetVariable setVar = new SetVariable();
            TestReflectionUtils.SetupSetVariableActionInt(setVar, resultVar, Scaffold.SetOperator.Assign, 100);
            AddCommand(setVar);

            blackboard.ExecuteBlock(block);
            yield return null;

            Assert.That(resultVar.Value, Is.EqualTo(100));

            // Set Score += 50
            SetVariable addVar = new SetVariable();
            TestReflectionUtils.SetupSetVariableActionInt(addVar, resultVar, Scaffold.SetOperator.Add, 50);
            AddCommand(addVar);

            blackboard.ExecuteBlock(block); // Executes both again, so it will set to 100, then add 50
            yield return null;

            Assert.That(resultVar.Value, Is.EqualTo(150));
        }

        [UnityTest]
        public IEnumerator Blackboard_CrossNodeReference_InjectsCorrectly()
        {
            // We create a float variable
            FloatVariable timeVar = blackboardObject.AddComponent<Scaffold.FloatVariable>();
            timeVar.Key = "WaitTime";
            timeVar.Value = 0.5f;
            blackboard.Variables.Add(timeVar);

            StringVariable stringVar = blackboardObject.AddComponent<Scaffold.StringVariable>();
            stringVar.Key = "State";
            stringVar.Value = "Start";
            blackboard.Variables.Add(stringVar);

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

            blackboard.ExecuteBlock(block);
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

        [Test]
        public void VariablesWithTheSameKey_OnDifferentBlackboardsRemainIndependent()
        {
            IntegerVariable firstVariable = blackboardObject.AddComponent<IntegerVariable>();
            firstVariable.Key = "Score";
            firstVariable.Value = 10;
            blackboard.Variables.Add(firstVariable);

            GameObject secondObject = new GameObject("SecondBlackboard");

            try
            {
                Blackboard secondBlackboard = secondObject.AddComponent<Blackboard>();
                IntegerVariable secondVariable = secondObject.AddComponent<IntegerVariable>();
                secondVariable.Key = "Score";
                secondVariable.Value = 20;
                secondBlackboard.Variables.Add(secondVariable);

                firstVariable.Value = 30;

                Assert.That(firstVariable.Value, Is.EqualTo(30));
                Assert.That(secondVariable.Value, Is.EqualTo(20));
            }
            finally
            {
                Object.DestroyImmediate(secondObject);
            }
        }
    }
}
