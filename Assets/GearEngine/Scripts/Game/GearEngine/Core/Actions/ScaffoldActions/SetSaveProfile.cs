using GearEngine.Core.Actions;

using UnityEngine;
using System;

namespace Scaffold
{
    /// <summary>
    /// Sets the active profile that the Save Variable and Load Variable commands will use. This is useful to crete multiple player save games. Once set, the profile applies across all Blackboards and will also persist across scene loads.
    /// </summary>
    [CommandInfo("Variable",
                 "Set Save Profile",
                 "Sets the active profile that the Save Variable and Load Variable commands will use. This is useful to crete multiple player save games. Once set, the profile applies across all Blackboards and will also persist across scene loads.")]
    [Serializable]
    public class SetSaveProfile : ActionBase
    {
        [Tooltip("Name of save profile to make active.")]
        [SerializeField] protected string saveProfileName = "";

        #region Public members

        public override void OnEnter()
        {
            GetBlackboard().SaveProfile = saveProfileName ?? string.Empty;

            Continue();
        }

        public override string GetSummary()
        {
            return saveProfileName;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        #endregion
    }
}
