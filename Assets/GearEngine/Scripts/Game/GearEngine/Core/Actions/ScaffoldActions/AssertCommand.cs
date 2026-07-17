using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Assertions;

namespace Scaffold
{
    /// <summary>
    /// Assert on 2 Scaffold variable values.
    /// </summary>
    [CommandInfo("Scripting",
                 "Assert",
                 "Assert based on compared values.")]
    [Serializable]
    public class AssertCommand : ActionBase
    {
        [SerializeField]
        protected StringData message;

        [SerializeField]
        [VariableProperty(AllVariableTypes.VariableAny.Any)]
        protected Variable a, b;

        public enum Method
        {
            AreEqual,
            AreNotEqual,
        }

        [SerializeField]
        protected Method method;

        public override void OnEnter()
        {
            switch (method)
            {
                case Method.AreEqual:
                    Assert.AreEqual(a.GetValue(), b.GetValue());
                    break;

                case Method.AreNotEqual:
                    Assert.AreNotEqual(a.GetValue(), b.GetValue());
                    break;

                default:
                break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (a == null)
                return "Error: No A variable";
            if (b == null)
                return "Error: No B variable";

            return a.Key + " " + method.ToString() + " " + b.Key;
        }

        public override bool HasReference(Variable variable)
        {
            return variable == message.stringRef ||
                variable == a || variable == b ||
                base.HasReference(variable);
        }
    }
}