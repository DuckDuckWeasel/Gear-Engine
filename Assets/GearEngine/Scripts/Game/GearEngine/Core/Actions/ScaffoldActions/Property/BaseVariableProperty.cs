using System;
using GearEngine.Core.Actions;

namespace Scaffold
{
    [Serializable]
    public abstract class BaseVariableProperty : ActionBase
    {
        public enum GetSet
        {
            Get,
            Set,
        }

        public GetSet getOrSet = GetSet.Get;
    }
}
