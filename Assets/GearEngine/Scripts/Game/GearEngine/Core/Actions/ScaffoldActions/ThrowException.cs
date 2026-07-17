using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    [Serializable]
    public class ScaffoldException : System.Exception
    {
        public ScaffoldException()
        {
        }

        public ScaffoldException(string message) : base(message)
        {
        }

        public ScaffoldException(string message, System.Exception inner) : base(message, inner)
        {
        }

        protected ScaffoldException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    /// <summary>
    /// Throw a Scaffold.Exception
    /// </summary>
    [CommandInfo("Scripting",
                 "Throw Exception",
                 "Throw a scaffold exception")]
    [Serializable]
    public class ThrowException : ActionBase
    {
        [SerializeField]
        protected StringData message;

        public override void OnEnter()
        {
            throw new ScaffoldException(GetLocationIdentifier() + " " + message.Value);

#pragma warning disable CS0162 // Unreachable code detected
            Continue();
#pragma warning restore CS0162 // Unreachable code detected
        }

        public override string GetSummary()
        {
            return message.Value;
        }

        public override bool HasReference(Variable variable)
        {
            return variable == message.stringRef || base.HasReference(variable);
        }
    }
}