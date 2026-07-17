using System;
using GearEngine.Core.Actions;

﻿using UnityEngine;
using Scaffold;

namespace Scaffold
{
    /// <summary>
    /// Opens the specified URL in the browser.
    /// </summary>
    [CommandInfo("Scripting",
                 "Open URL",
                 "Opens the specified URL in the browser.")]
    [Serializable]
    public class OpenURL : ActionBase
    {
        [Tooltip("URL to open in the browser")]
        [SerializeField] protected StringData url = new StringData();

        #region Public members

        public override void OnEnter()
        {
            Application.OpenURL(url.Value);

            Continue();
        }

        public override string GetSummary()
        {
            return url.Value;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return url.stringRef == variable || base.HasReference(variable);
        }

        #endregion
    }
}