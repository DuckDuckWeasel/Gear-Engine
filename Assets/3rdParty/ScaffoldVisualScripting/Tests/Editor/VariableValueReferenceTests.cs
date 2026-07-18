using NUnit.Framework;
using UnityEngine;

namespace Scaffold.Tests.Editor
{
    public class VariableValueReferenceTests
    {
        [Test]
        public void FloatData_ResolvesTheSelectedFlowchartDirectAndScriptableObjectSources()
        {
            var gameObject = new GameObject("VariableValueReferenceTests");
            gameObject.AddComponent<Flowchart>();
            var flowchartVariable = gameObject.AddComponent<FloatVariable>();
            var valueAsset = ScriptableObject.CreateInstance<FloatValueSO>();

            try
            {
                flowchartVariable.Value = 12f;
                valueAsset.Value = 24f;

                var value = new FloatData(6f)
                {
                    floatRef = flowchartVariable,
                    floatSO = valueAsset,
                };

                value.source = VariableDataSource.FlowchartVariable;
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
        public void FloatData_UnspecifiedSource_PreservesTheLegacyFlowchartThenDirectResolutionOrder()
        {
            var gameObject = new GameObject("VariableValueReferenceTests");
            gameObject.AddComponent<Flowchart>();
            var flowchartVariable = gameObject.AddComponent<FloatVariable>();

            try
            {
                flowchartVariable.Value = 9f;

                var referencedValue = new FloatData(4f)
                {
                    floatRef = flowchartVariable,
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
            gameObject.AddComponent<Flowchart>();
            var flowchartVariable = gameObject.AddComponent<FloatVariable>();
            var valueAsset = ScriptableObject.CreateInstance<FloatValueSO>();

            try
            {
                var value = new FloatData(2f)
                {
                    floatRef = flowchartVariable,
                    floatSO = valueAsset,
                };

                value.source = VariableDataSource.FlowchartVariable;
                value.Value = 4f;
                Assert.That(flowchartVariable.Value, Is.EqualTo(4f));

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
    }
}
