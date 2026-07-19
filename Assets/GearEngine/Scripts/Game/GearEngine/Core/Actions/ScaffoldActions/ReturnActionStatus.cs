using System;
using GearEngine.Core.Actions;
using UnityEngine;

namespace Scaffold
{
    [Serializable]
    [CommandInfo("Flow", "Return Status", "Completes with a success or failure status.")]
    [AddComponentMenu("")]
    public sealed class ReturnActionStatus : ActionBase
    {
        [SerializeField] private BooleanData success = new BooleanData(true);

        public bool Success
        {
            get => success.Value;
            set => success.Value = value;
        }

        public override void OnEnter()
        {
            if (success.Value)
            {
                Continue();
                return;
            }

            Fail();
        }

        public override string GetSummary()
        {
            return success.Value ? "Success" : "Failure";
        }
    }
}
