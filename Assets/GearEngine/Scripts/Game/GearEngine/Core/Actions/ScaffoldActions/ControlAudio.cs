using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// The type of audio control to perform.
    /// </summary>
    public enum ControlAudioType
    {
        /// <summary> Play the audiosource once. </summary>
        PlayOnce,
        /// <summary> Play the audiosource in a loop. </summary>
        PlayLoop,
        /// <summary> Pause a looping audiosource. </summary>
        PauseLoop,
        /// <summary> Stop a looping audiosource. </summary>
        StopLoop,
        /// <summary> Change the volume level of an audiosource. </summary>
        ChangeVolume
    }

    /// <summary>
    /// Plays, loops, or stops an audiosource. Any AudioSources with the same tag as the target Audio Source will automatically be stoped.
    /// </summary>
    [CommandInfo("Audio", 
                 "Control Audio",
                 "Plays, loops, or stops an audiosource. Any AudioSources with the same tag as the target Audio Source will automatically be stoped.")]
    [ExecuteInEditMode]
    [Serializable]
    public class ControlAudio : ActionBase
    {
        [Tooltip("What to do to audio")]
        [SerializeField] protected ControlAudioType control;
        public virtual ControlAudioType Control { get { return control; } }

        [Tooltip("Audio clip to play")]
        [SerializeField] protected AudioSourceData audioSource;

        [Range(0,1)]
        [Tooltip("Start audio at this volume")]
        [SerializeField] protected float startVolume = 1;

        [Range(0,1)]
        [Tooltip("End audio at this volume")]
        [SerializeField] protected float endVolume = 1;
        
        [Tooltip("Time to fade between current volume level and target volume level.")]
        [SerializeField] protected float fadeDuration; 

        [Tooltip("Wait until this command has finished before executing the next command.")]
        [SerializeField] protected bool waitUntilFinished = false;

        // If there's other music playing in the scene, assign it the same tag as the new music you want to play and
        // the old music will be automatically stopped.
        protected virtual void StopAudioWithSameTag()
        {
            // Don't stop audio if there's no tag assigned
            if (audioSource.Value == null ||
                audioSource.Value.tag == "Untagged")
            {
                return;
            }

            var audioSources = GameObject.FindObjectsOfType<AudioSource>();
            for (int i = 0; i < audioSources.Length; i++)
            {
                var a = audioSources[i];
                if (a != audioSource.Value && a.tag == audioSource.Value.tag)
                {
                    StopLoop(a);
                }
            }
        }

        protected virtual void PlayOnce() 
        {
            if (fadeDuration > 0)
            {
                // Fade volume in
                LeanTween.value(audioSource.Value.gameObject.gameObject, 
                    audioSource.Value.volume, 
                    endVolume,
                    fadeDuration
                ).setOnUpdate(
                    (float updateVolume)=>{
                    audioSource.Value.volume = updateVolume;
                });
            }

            audioSource.Value.PlayOneShot(audioSource.Value.clip);

            if (waitUntilFinished)
            {
                host.StartCoroutine(WaitAndContinue());
            }
        }

        protected virtual IEnumerator WaitAndContinue()
        {
            // Poll the audiosource until playing has finished
            // This allows for things like effects added to the audiosource.
            while (audioSource.Value.isPlaying)
            {
                yield return null;
            }

            Continue();
        }

        protected virtual void PlayLoop()
        {
            if (fadeDuration > 0)
            {
                audioSource.Value.volume = 0;
                audioSource.Value.loop = true;
                audioSource.Value.gameObject.GetComponent<AudioSource>().Play();
                LeanTween.value(audioSource.Value.gameObject.gameObject,0,endVolume,fadeDuration
                ).setOnUpdate(
                    (float updateVolume)=>{
                    audioSource.Value.volume = updateVolume;
                }
                ).setOnComplete(
                    ()=>{
                    if (waitUntilFinished)
                    {
                        Continue();
                    }
                }
                );
            }
            else
            {
                audioSource.Value.volume = endVolume;
                audioSource.Value.loop = true;
                audioSource.Value.gameObject.GetComponent<AudioSource>().Play();
            }
        }

        protected virtual void PauseLoop()
        {
            if (fadeDuration > 0)
            {
                LeanTween.value(audioSource.Value.gameObject.gameObject,audioSource.Value.volume,0,fadeDuration
                ).setOnUpdate(
                    (float updateVolume)=>{
                    audioSource.Value.volume = updateVolume;
                }
                ).setOnComplete(
                    ()=>{

                    audioSource.Value.gameObject.GetComponent<AudioSource>().Pause();
                    if (waitUntilFinished)
                    {
                        Continue();
                    }
                }
                );
            }
            else
            {
                audioSource.Value.gameObject.GetComponent<AudioSource>().Pause();
            }
        }

        protected virtual void StopLoop(AudioSource source)
        {
            if (fadeDuration > 0)
            {
                LeanTween.value(source.gameObject.gameObject,audioSource.Value.volume,0,fadeDuration
                ).setOnUpdate(
                    (float updateVolume)=>{
                    source.volume = updateVolume;
                }
                ).setOnComplete(
                    ()=>{

                    source.gameObject.GetComponent<AudioSource>().Stop();
                    if (waitUntilFinished)
                    {
                        Continue();
                    }
                }
                );
            }
            else
            {
                source.gameObject.GetComponent<AudioSource>().Stop();
            }
        }

        protected virtual void ChangeVolume()
        {
            LeanTween.value(audioSource.Value.gameObject.gameObject,audioSource.Value.volume,endVolume,fadeDuration
            ).setOnUpdate(
                (float updateVolume)=>{
                audioSource.Value.volume = updateVolume;
            }).setOnComplete(
                ()=>{
                if (waitUntilFinished)
                {
                    Continue();
                }
            });
        }

        protected virtual void AudioFinished()
        {
            if (waitUntilFinished)
            {
                Continue();
            }
        }

        #region Public members

        public override void OnEnter()
        {
            if (audioSource.Value == null)
            {
                Continue();
                return;
            }

            if (control != ControlAudioType.ChangeVolume)
            {
                audioSource.Value.volume = endVolume;
            }

            switch(control)
            {
                case ControlAudioType.PlayOnce:
                    StopAudioWithSameTag();
                    PlayOnce();
                    break;
                case ControlAudioType.PlayLoop:
                    StopAudioWithSameTag();
                    PlayLoop();
                    break;
                case ControlAudioType.PauseLoop:
                    PauseLoop();
                    break;
                case ControlAudioType.StopLoop:
                    StopLoop(audioSource.Value);
                    break;
                case ControlAudioType.ChangeVolume:
                    ChangeVolume(); 
                    break;
            }
            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (audioSource.Value == null)
            {
                return "Error: No sound clip selected";
            }
            string fadeType = "";
            if (fadeDuration > 0)
            {
                fadeType = " Fade out";
                if (control != ControlAudioType.StopLoop)
                {
                    fadeType = " Fade in volume to " + endVolume;
                }
                if (control == ControlAudioType.ChangeVolume)
                {
                    fadeType = " to " + endVolume;
                }
                fadeType += " over " + fadeDuration + " seconds.";
            }
            return control.ToString() + " \"" + audioSource.Value.name + "\"" + fadeType;
        }
        
        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return audioSource.audioSourceRef == variable || base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("audioSource")] public AudioSource audioSourceOLD;

        protected virtual void OnEnable()
        {
            if (audioSourceOLD != null)
            {
                audioSource.Value = audioSourceOLD;
                audioSourceOLD = null;
            }
        }

        #endregion
    }    
}