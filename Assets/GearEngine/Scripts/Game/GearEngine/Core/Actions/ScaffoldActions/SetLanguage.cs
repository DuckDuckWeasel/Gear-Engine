using System;
using GearEngine.Core.Actions;

﻿using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Set the active language for the scene. A Localization object with a localization file must be present in the scene.
    /// </summary>
    [CommandInfo("Narrative", 
                 "Set Language", 
                 "Set the active language for the scene. A Localization object with a localization file must be present in the scene.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class SetLanguage : ActionBase
    {
        [Tooltip("Code of the language to set. e.g. ES, DE, JA")]
        [SerializeField] protected StringData languageCode = new StringData(); 

        #region Public members

        public static string mostRecentLanguage = "";

        public override void OnEnter()
        {
            Localization localization = GameObject.FindObjectOfType<Localization>();
            if (localization != null)
            {
                localization.SetActiveLanguage(languageCode.Value, true);

                // Cache the most recently set language code so we can continue to 
                // use the same language in subsequent scenes.
                mostRecentLanguage = languageCode.Value;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return languageCode.Value;
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return languageCode.stringRef == variable || base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("languageCode")] public string languageCodeOLD = "";

        protected virtual void OnEnable()
        {
            if (languageCodeOLD != "")
            {
                languageCode.Value = languageCodeOLD;
                languageCodeOLD = "";
            }
        }

        #endregion
    }
}