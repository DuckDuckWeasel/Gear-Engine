using NUnit.Framework;
using UnityEngine;

namespace Scaffold.Tests.Editor
{
    public class VariableValueReferenceTests
    {
        [Test]
        public void FloatData_ResolvesTheSelectedBlackboardDirectAndScriptableObjectSources()
        {
            var gameObject = new GameObject("VariableValueReferenceTests");
            gameObject.AddComponent<Blackboard>();
            var blackboardVariable = gameObject.AddComponent<FloatVariable>();
            var valueAsset = ScriptableObject.CreateInstance<FloatValueSO>();

            try
            {
                blackboardVariable.Value = 12f;
                valueAsset.Value = 24f;

                var value = new FloatData(6f)
                {
                    floatRef = blackboardVariable,
                    floatSO = valueAsset,
                };

                value.source = VariableDataSource.BlackboardVariable;
                Assert.That(value.Value, Is.EqualTo(12f));

                value.source = VariableDataSource.Direct;
                Assert.That(value.Value, Is.EqualTo(6f));

                value.source = VariableDataSource.ScriptableObject;
                Assert.That(value.Value, Is.EqualTo(24f));
            }
            finally
            {
                Object.DestroyImmediate(valueAsset);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void FloatData_UnspecifiedSource_PreservesTheLegacyBlackboardThenDirectResolutionOrder()
        {
            var gameObject = new GameObject("VariableValueReferenceTests");
            gameObject.AddComponent<Blackboard>();
            var blackboardVariable = gameObject.AddComponent<FloatVariable>();

            try
            {
                blackboardVariable.Value = 9f;

                var referencedValue = new FloatData(4f)
                {
                    floatRef = blackboardVariable,
                };
                var directValue = new FloatData(4f);

                Assert.That(referencedValue.Value, Is.EqualTo(9f));
                Assert.That(directValue.Value, Is.EqualTo(4f));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void FloatData_AssignsToTheSelectedSource()
        {
            var gameObject = new GameObject("VariableValueReferenceTests");
            gameObject.AddComponent<Blackboard>();
            var blackboardVariable = gameObject.AddComponent<FloatVariable>();
            var valueAsset = ScriptableObject.CreateInstance<FloatValueSO>();

            try
            {
                var value = new FloatData(2f)
                {
                    floatRef = blackboardVariable,
                    floatSO = valueAsset,
                };

                value.source = VariableDataSource.BlackboardVariable;
                value.Value = 4f;
                Assert.That(blackboardVariable.Value, Is.EqualTo(4f));

                value.source = VariableDataSource.Direct;
                value.Value = 6f;
                Assert.That(value.floatVal, Is.EqualTo(6f));

                value.source = VariableDataSource.ScriptableObject;
                value.Value = 8f;
                Assert.That(valueAsset.Value, Is.EqualTo(8f));
            }
            finally
            {
                Object.DestroyImmediate(valueAsset);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void CharacterData_ResolvesTheSelectedBlackboardDirectAndScriptableObjectSources()
        {
            var variableGameObject = new GameObject("CharacterVariable");
            variableGameObject.AddComponent<Blackboard>();
            var blackboardVariable = variableGameObject.AddComponent<CharacterVariable>();
            var directCharacter = new GameObject("DirectCharacter").AddComponent<Character>();
            var blackboardCharacter = new GameObject("BlackboardCharacter").AddComponent<Character>();
            var scriptableObjectCharacter = new GameObject("ScriptableObjectCharacter").AddComponent<Character>();
            var valueAsset = ScriptableObject.CreateInstance<CharacterValueSO>();

            try
            {
                blackboardVariable.Value = blackboardCharacter;
                valueAsset.Value = scriptableObjectCharacter;

                var value = new CharacterData(directCharacter)
                {
                    characterRef = blackboardVariable,
                    characterSO = valueAsset,
                };

                value.source = VariableDataSource.BlackboardVariable;
                Assert.That(value.Value, Is.EqualTo(blackboardCharacter));

                value.source = VariableDataSource.Direct;
                Assert.That(value.Value, Is.EqualTo(directCharacter));

                value.source = VariableDataSource.ScriptableObject;
                Assert.That(value.Value, Is.EqualTo(scriptableObjectCharacter));
            }
            finally
            {
                Object.DestroyImmediate(valueAsset);
                Object.DestroyImmediate(variableGameObject);
                Object.DestroyImmediate(directCharacter.gameObject);
                Object.DestroyImmediate(blackboardCharacter.gameObject);
                Object.DestroyImmediate(scriptableObjectCharacter.gameObject);
            }
        }
    }
}
