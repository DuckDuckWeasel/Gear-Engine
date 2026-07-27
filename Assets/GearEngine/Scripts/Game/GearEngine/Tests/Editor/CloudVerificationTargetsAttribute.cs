using System;
using NUnit.Framework;

namespace GearEngine.GearEngine.Tests.Editor
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class CloudVerificationTargetsAttribute : PropertyAttribute
    {
        public CloudVerificationTargetsAttribute(params CloudVerificationTarget[] targets)
            : base("CloudVerificationTargets", JoinTargets(targets))
        {
        }

        private static string JoinTargets(CloudVerificationTarget[] targets)
        {
            if (targets == null || targets.Length == 0)
            {
                throw new ArgumentException("At least one cloud verification target is required.", nameof(targets));
            }

            return string.Join(",", targets);
        }
    }
}
