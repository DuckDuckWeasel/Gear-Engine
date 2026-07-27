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
    public class ActionTransformTests
    {
        private GameObject _blackboardObject;
        private Blackboard _blackboard;
        private Block _block;
        private InvokeActionCommand _command;
        private GameObject _targetObject;

        [SetUp]
        public void Setup()
        {
            _blackboardObject = new GameObject("TestBlackboard");
            _blackboard = _blackboardObject.AddComponent<Blackboard>();
            _blackboardObject.AddComponent<EventDispatcher>();

            _block = _blackboardObject.AddComponent<Block>();
            _block.BlockName = "TransformBlock";

            _command = _blackboardObject.AddComponent<InvokeActionCommand>();
            _command.ParentBlock = _block;
            _block.CommandList.Add(_command);

            _targetObject = new GameObject("Target");
        }

        [TearDown]
        public void Teardown()
        {
            if (_blackboardObject != null)
            {
                Object.DestroyImmediate(_blackboardObject);
            }
            if (_targetObject != null)
            {
                Object.DestroyImmediate(_targetObject);
            }
        }

        [UnityTest]
        public IEnumerator MoveTo_ChangesPosition_AfterDuration()
        {
            _targetObject.transform.position = Vector3.zero;

            MoveTo action = new MoveTo();
            TestReflectionUtils.SetupMoveToAction(action, _targetObject, new Vector3(10, 0, 0), 0.1f, true);

            _command.actions.Add(new InvokeActionCommand.ActionWrapper(action));

            _blackboard.ExecuteBlock(_block);

            // Wait enough time for tween to finish (0.1s duration + safety buffer)
            yield return new WaitForSeconds(0.2f);

            Assert.That(_targetObject.transform.position.x, Is.EqualTo(10f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator ScaleTo_ChangesScale_AfterDuration()
        {
            _targetObject.transform.localScale = Vector3.one;

            ScaleTo action = new ScaleTo();
            TestReflectionUtils.SetupScaleToAction(action, _targetObject, new Vector3(2, 2, 2), 0.1f, true);

            _command.actions.Add(new InvokeActionCommand.ActionWrapper(action));

            _blackboard.ExecuteBlock(_block);

            // Wait enough time for tween to finish
            yield return new WaitForSeconds(0.2f);

            Assert.That(_targetObject.transform.localScale.x, Is.EqualTo(2f).Within(0.01f));
            Assert.That(_targetObject.transform.localScale.y, Is.EqualTo(2f).Within(0.01f));
            Assert.That(_targetObject.transform.localScale.z, Is.EqualTo(2f).Within(0.01f));
        }
    }
}
