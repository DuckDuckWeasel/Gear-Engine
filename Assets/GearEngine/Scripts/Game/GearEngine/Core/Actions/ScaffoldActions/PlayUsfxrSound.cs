using GearEngine.Core.Actions;

using System;
using UnityEngine;
using UnityEngine.Serialization;

﻿namespace Scaffold 
{
    /// <summary>
    /// Plays a usfxr synth sound. Use the usfxr editor [Tools > Scaffold > Utilities > Generate usfxr Sound Effects] to create the SettingsString. Set a ParentTransform if using positional sound. See https://github.com/zeh/usfxr for more information about usfxr.
    /// </summary>
    [CommandInfo("Audio", 
                 "Play Usfxr Sound", 
                 "Plays a usfxr synth sound. Use the usfxr editor [Tools > Scaffold > Utilities > Generate usfxr Sound Effects] to create the SettingsString. Set a ParentTransform if using positional sound. See https://github.com/zeh/usfxr for more information about usfxr.")]
    [AddComponentMenu("")]
    //[ExecuteInEditMode]
    [Serializable]
    public class PlayUsfxrSound : ActionBase
    {
        [Tooltip("Transform to use for positional audio")]
        [SerializeField] protected Transform ParentTransform = null;

        [Tooltip("Settings string which describes the audio")]
        [SerializeField] protected StringDataMulti settingsString = new StringDataMulti("");

        [Tooltip("Time to wait before executing the next command")]
        [SerializeField] protected float waitDuration = 0;

        protected SfxrSynth synth = new SfxrSynth();

        //Call this if the settings have changed
        protected virtual void UpdateCache()  
        {
            if (!string.IsNullOrEmpty(settingsString.Value)) 
            {
                synth.parameters.SetSettingsString(settingsString.Value);
                synth.CacheSound();
            }
        }

        protected virtual void Awake() 
        {
            //Always build the cache on awake
            UpdateCache();
        }

        protected void DoWait()
        {
            Continue();
        }

        #region Public members

        public override void OnEnter() 
        {
            synth.SetParentTransform(ParentTransform);
            synth.Play();
            if (Mathf.Approximately(waitDuration, 0f))
            {
                Continue();
            }
            else
            {
                Invoke ("DoWait", waitDuration);
            }
        }

        public override string GetSummary() 
        {
            if (String.IsNullOrEmpty(settingsString.Value)) 
            {
                return "Settings String hasn't been set!";
            }
            if (ParentTransform != null) 
            {
                return "" + ParentTransform.name + ": " + settingsString.Value;
            }
            return "Camera.main: " + settingsString.Value;
        }

        public override Color GetButtonColor() 
        {
            return new Color32(128, 200, 200, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return variable == settingsString.stringRef;
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("SettingsString")] public String SettingsStringOLD = "";

        protected virtual void OnEnable()
        {
            if (SettingsStringOLD != "")
            {
                settingsString.Value = SettingsStringOLD;
                SettingsStringOLD = "";
            }
        }

        public override void OnValidate()
        {
            OnEnable();
            base.OnValidate();
        }

        #endregion
    }
}
