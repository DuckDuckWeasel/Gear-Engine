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
    public class EventHandlerTests
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
        public IEnumerator GameStarted_ExecutesBlockAfterFrames()
        {
            Block block = CreateBlock("StartBlock");

            StringVariable resultVar = _blackboardObject.AddComponent<Scaffold.StringVariable>();
            resultVar.Key = "State";
            resultVar.Value = "Idle";
            _blackboard.Variables.Add(resultVar);

            SetVariable setVar = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setVar, resultVar, Scaffold.SetOperator.Assign, "Started");
            AddCommand(block, setVar);

            GameStarted gameStarted = _blackboardObject.AddComponent<GameStarted>();
            gameStarted.ParentBlock = block;
            TestReflectionUtils.SetProtectedField(gameStarted, "waitForFrames", 2);

            // Trigger manual Start since UnityTest doesn't always trigger it automatically on AddComponent in some versions
            TestReflectionUtils.SetProtectedField(gameStarted, "waitForFrames", 2);
            System.Reflection.MethodInfo method = gameStarted.GetType().GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(gameStarted, null);
            }

            Assert.That(resultVar.Value, Is.EqualTo("Idle"));

            // Wait 1 frame
            yield return null;
            Assert.That(resultVar.Value, Is.EqualTo("Idle"));

            // Wait another frame
            yield return null;
            // The Coroutine should have executed ExecuteBlock() by now
            Assert.That(resultVar.Value, Is.EqualTo("Started"));
        }
    }
}
