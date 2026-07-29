using GearEngine.Core.Actions;

using System;
using UnityEngine;

namespace Scaffold
{
    [Serializable]
    public abstract class ControlWithDisplay<TDisplayEnum> : ActionBase
    {
        [Tooltip("Display type")]
        [SerializeField] protected TDisplayEnum display;

        protected virtual bool IsDisplayNone<TEnum>(TEnum enumValue)
        {
            string displayTypeStr = Enum.GetName(typeof(TEnum), enumValue);
            return displayTypeStr == "None";
        }

        #region Public members

        public virtual TDisplayEnum Display { get { return display; } }

        #endregion
    }
}
