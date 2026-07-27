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
        private GameObject blackboardObject;
        private Blackboard blackboard;

        [SetUp]
        public void Setup()
        {
            blackboardObject = new GameObject("TestBlackboard");
            blackboard = blackboardObject.AddComponent<Blackboard>();
            blackboardObject.AddComponent<EventDispatcher>();
        }

        [TearDown]
        public void Teardown()
        {
            if (blackboardObject != null)
            {
                Object.DestroyImmediate(blackboardObject);
            }
        }

        private Block CreateBlock(string name)
        {
            Block block = blackboardObject.AddComponent<Block>();
            block.BlockName = name;
            return block;
        }

        private InvokeActionCommand AddCommand(Block block, IAction action)
        {
            InvokeActionCommand cmd = blackboardObject.AddComponent<InvokeActionCommand>();
            cmd.ParentBlock = block;
            cmd.actions.Add(new InvokeActionCommand.ActionWrapper(action));
            block.CommandList.Add(cmd);
            return cmd;
        }

        [UnityTest]
        public IEnumerator GameStarted_YieldsConfiguredFramesBeforeExecutingBlock()
        {
            Block block = CreateBlock("StartBlock");

            StringVariable resultVar = blackboardObject.AddComponent<Scaffold.StringVariable>();
            resultVar.Key = "State";
            resultVar.Value = "Idle";
            blackboard.Variables.Add(resultVar);

            SetVariable setVar = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(setVar, resultVar, Scaffold.SetOperator.Assign, "Started");
            AddCommand(block, setVar);

            GameStarted gameStarted = blackboardObject.AddComponent<GameStarted>();
            gameStarted.ParentBlock = block;
            block._EventHandler = gameStarted;
            TestReflectionUtils.SetProtectedField(gameStarted, "waitForFrames", 2);
            gameStarted.enabled = false;

            Assert.That(resultVar.Value, Is.EqualTo("Idle"));

            System.Reflection.MethodInfo method = gameStarted.GetType().GetMethod(
                "GameStartCoroutine",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            Assert.That(method, Is.Not.Null);

            IEnumerator startupRoutine = (IEnumerator)method.Invoke(gameStarted, null);

            Assert.That(startupRoutine.MoveNext(), Is.True);
            Assert.That(startupRoutine.Current, Is.TypeOf<WaitForEndOfFrame>());
            Assert.That(resultVar.Value, Is.EqualTo("Idle"));

            Assert.That(startupRoutine.MoveNext(), Is.True);
            Assert.That(startupRoutine.Current, Is.TypeOf<WaitForEndOfFrame>());
            Assert.That(resultVar.Value, Is.EqualTo("Idle"));

            Assert.That(startupRoutine.MoveNext(), Is.False);

            yield return null;

            Assert.That(resultVar.Value, Is.EqualTo("Started"));
        }

        [UnityTest]
        public IEnumerator MessageReceived_ExecutesOnlyForTheConfiguredMessage()
        {
            Block block = CreateBlock("MessageBlock");

            StringVariable resultVar = blackboardObject.AddComponent<StringVariable>();
            resultVar.Key = "State";
            resultVar.Value = "Idle";
            blackboard.Variables.Add(resultVar);

            SetVariable setVar = new SetVariable();
            TestReflectionUtils.SetupSetVariableAction(
                setVar,
                resultVar,
                SetOperator.Assign,
                "MessageReceived");
            AddCommand(block, setVar);

            MessageReceived messageReceived = blackboardObject.AddComponent<MessageReceived>();
            messageReceived.ParentBlock = block;
            block._EventHandler = messageReceived;
            TestReflectionUtils.SetProtectedField(messageReceived, "message", "Begin");

            blackboard.SendScaffoldMessage("Other");
            yield return null;

            Assert.That(resultVar.Value, Is.EqualTo("Idle"));

            blackboard.SendScaffoldMessage("Begin");
            yield return null;

            Assert.That(resultVar.Value, Is.EqualTo("MessageReceived"));
        }
    }
}
