using System;
using System.Reflection;
using Scaffold;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Game.GearEngine.RuntimeTests
{
    public static class TestReflectionUtils
    {
        public static void SetProtectedField(object obj, string fieldName, object value)
        {
            Type type = obj.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (field != null)
                {
                    field.SetValue(obj, value);
                    return;
                }
                type = type.BaseType;
            }
            throw new Exception($"Field {fieldName} not found on {obj.GetType()}");
        }

        public static void SetupSetVariableAction(SetVariable action, Variable targetVariable, Scaffold.SetOperator setOp, string stringValue)
        {
            AnyVariableAndDataPair pair = new AnyVariableAndDataPair();
            pair.variable = targetVariable;
            pair.data = new AnyVariableData { stringData = new StringData { Value = stringValue } };
            SetProtectedField(action, "anyVar", pair);
            SetProtectedField(action, "setOperator", setOp);
        }

        public static void SetupSetVariableActionInt(SetVariable action, Variable targetVariable, Scaffold.SetOperator setOp, int intValue)
        {
            AnyVariableAndDataPair pair = new AnyVariableAndDataPair();
            pair.variable = targetVariable;
            pair.data = new AnyVariableData { integerData = new IntegerData { Value = intValue } };
            SetProtectedField(action, "anyVar", pair);
            SetProtectedField(action, "setOperator", setOp);
        }

        public static void SetupIfAction(If action, Variable targetVariable, Scaffold.CompareOperator compareOp, bool boolValue)
        {
            AnyVariableAndDataPair pair = new AnyVariableAndDataPair();
            pair.variable = targetVariable;
            pair.data = new AnyVariableData { booleanData = new BooleanData { Value = boolValue } };
            ConditionExpression expr = new ConditionExpression(compareOp, pair);
            System.Collections.Generic.List<ConditionExpression> conditions = new System.Collections.Generic.List<ConditionExpression>();
            conditions.Add(expr);
            SetProtectedField(action, "conditions", conditions);
        }

        public static void SetupMoveToAction(MoveTo action, GameObject targetObj, Vector3 toPos, float duration, bool waitUntilFinished)
        {
            SetProtectedField(action, "targetObject", new Scaffold.GameObjectData { Value = targetObj });
            SetProtectedField(action, "toPosition", new Scaffold.Vector3Data { Value = toPos });
            SetProtectedField(action, "duration", new Scaffold.FloatData { Value = duration });
            SetProtectedField(action, "waitUntilFinished", waitUntilFinished);
        }

        public static void SetupCallAction(Call action, Block targetBlock)
        {
            SetProtectedField(action, "targetBlock", targetBlock);
        }

        public static void SetupScaleToAction(ScaleTo action, GameObject targetObj, Vector3 toScale, float duration, bool waitUntilFinished)
        {
            SetProtectedField(action, "targetObject", new Scaffold.GameObjectData { Value = targetObj });
            SetProtectedField(action, "toScale", new Scaffold.Vector3Data { Value = toScale });
            SetProtectedField(action, "duration", new Scaffold.FloatData { Value = duration });
            SetProtectedField(action, "waitUntilFinished", waitUntilFinished);
        }
    }
}
