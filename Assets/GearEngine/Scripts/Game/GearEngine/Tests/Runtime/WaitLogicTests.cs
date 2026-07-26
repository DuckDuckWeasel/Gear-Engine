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
    public class WaitLogicTests
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
            _block.BlockName = "WaitBlock";
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
        public IEnumerator Wait_DelaysExecutionForSpecifiedDuration()
        {
            StringVariable resultVar = _blackboardObject.AddComponent<Scaffold.StringVariable>();
            resultVar.Key = "Result";
            resultVar.Value = "Start";
            _blackboard.Variables.Add(resultVar);

            // Wait 0.5s
            Wait waitAction = new Wait();
            TestReflectionUtils.SetProtectedField(waitAction, "duration", new FloatData { Value = 0.5f });
            AddCommand(waitAction);

            // Set Result = "Done"
            SetVariable setVar = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setVar, resultVar, Scaffold.SetOperator.Assign, "Done");
            AddCommand(setVar);

            _blackboard.ExecuteBlock(_block);

            // Should still be Start
            yield return null;
            Assert.That(resultVar.Value, Is.EqualTo("Start"));

            // After 0.2s, still Start
            yield return new WaitForSeconds(0.2f);
            Assert.That(resultVar.Value, Is.EqualTo("Start"));

            // After 0.6s total, should be Done
            yield return new WaitForSeconds(0.4f);
            Assert.That(resultVar.Value, Is.EqualTo("Done"));
        }
    }
}
