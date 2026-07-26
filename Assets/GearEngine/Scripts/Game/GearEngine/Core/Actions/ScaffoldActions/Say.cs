using System;
using GearEngine.Core.Actions;

using UnityEngine;
using Ami.BroAudio;

namespace Scaffold
{
    /// <summary>
    /// Writes text in a dialog box.
    /// </summary>
    [CommandInfo("Narrative",
                 "Say",
                 "Writes text in a dialog box.")]
    [Serializable]
    public class Say : ActionBase, ILocalizable
    {
        // Removed this tooltip as users's reported it obscures the text box
        [TextArea(5, 10)]
        [Tooltip("The Story text")]
        [SerializeField] public string storyText = "";

        [Tooltip("Notes about this story text for other authors, localization, etc.")]
        [SerializeField] protected string description = "";

        [HideInInspector]
        [Tooltip("Legacy direct character reference retained for existing commands.")]
        [SerializeField] protected Character character;

        [InspectorName("Character")]
        [Tooltip("Character that is speaking. Choose a direct reference, Blackboard variable, or ScriptableObject value.")]
        [SerializeField] protected CharacterData characterData;

        [HideInInspector]
        [Tooltip("Legacy portrait override retained for existing commands.")]
        [SerializeField] protected Sprite portrait;

        [Tooltip("Legacy: standard AudioClip variable or object to play when writing the text")]
        [SerializeField] protected AudioClipData voiceOverClipData;

        [Tooltip("BroAudio SoundID to play when writing the text. (Takes priority over AudioClip)")]
        [SerializeField] protected SoundID voiceOverSound;

        [Tooltip("Always show this Say text when the command is executed multiple times")]
        [SerializeField] protected bool showAlways = true;

        [Tooltip("Number of times to show this Say text when the command is executed multiple times")]
        [SerializeField] protected int showCount = 1;

        [Tooltip("Type this text in the previous dialog box.")]
        [SerializeField] protected bool extendPrevious = false;

        [Tooltip("Fade out the dialog box when writing has finished and not waiting for input.")]
        [SerializeField] protected bool fadeWhenDone = true;

        [Tooltip("Wait for player to click before continuing.")]
        [SerializeField] protected bool waitForClick = true;

        [Tooltip("Wait for complete dialogue before continuing.")]
        [SerializeField] protected bool waitForComplete = true;

        [Tooltip("Stop playing voiceover when text finishes writing.")]
        [SerializeField] protected bool stopVoiceover = true;

        [Tooltip("Wait for the Voice Over to complete before continuing")]
        [SerializeField] protected bool waitForVO = false;

        //add wait for vo that overrides stopvo

        [Tooltip("Sets the active Say dialog with a reference to a Say Dialog object in the scene. All story text will now display using this Say Dialog.")]
        [SerializeField] protected SayDialog setSayDialog;

        protected int executionCount;

        #region Public members

        /// <summary>
        /// Character that is speaking.
        /// </summary>
        public virtual Character _Character { get { return ResolveCharacter(); } }

        /// <summary>
        /// Portrait that represents speaking character.
        /// </summary>
        public virtual Sprite Portrait { get { return ResolvePortrait(ResolveCharacter()); } set { portrait = value; } }

        /// <summary>
        /// Type this text in the previous dialog box.
        /// </summary>
        public virtual bool ExtendPrevious { get { return extendPrevious; } }

        public override void OnEnter()
        {
            if (!showAlways && executionCount >= showCount)
            {
                Continue();
                return;
            }

            executionCount++;

            Character resolvedCharacter = ResolveCharacter();

            // Override the active say dialog if needed
            if (resolvedCharacter != null && resolvedCharacter.SetSayDialog != null)
            {
                SayDialog.ActiveSayDialog = resolvedCharacter.SetSayDialog;
            }

            if (setSayDialog != null)
            {
                SayDialog.ActiveSayDialog = setSayDialog;
            }

            SayDialog sayDialog = SayDialog.GetSayDialog();
            if (sayDialog == null)
            {
                Continue();
                return;
            }

            Blackboard blackboard = GetBlackboard();

            sayDialog.SetActive(true);

            sayDialog.SetCharacter(resolvedCharacter);
            sayDialog.SetCharacterImage(ResolvePortrait(resolvedCharacter));

            string displayText = storyText;

            System.Collections.Generic.List<CustomTag> activeCustomTags = CustomTag.activeCustomTags;
            for (int i = 0; i < activeCustomTags.Count; i++)
            {
                CustomTag ct = activeCustomTags[i];
                displayText = displayText.Replace(ct.TagStartSymbol, ct.ReplaceTagStartWith);
                if (ct.TagEndSymbol != "" && ct.ReplaceTagEndWith != "")
                {
                    displayText = displayText.Replace(ct.TagEndSymbol, ct.ReplaceTagEndWith);
                }
            }

            string subbedText = blackboard.SubstituteVariables(displayText);

            if (voiceOverSound.IsValid())
            {
                BroAudio.Play(voiceOverSound);
            }

            if (waitForComplete)
            {
                sayDialog.Say(subbedText, !extendPrevious, waitForClick, fadeWhenDone, stopVoiceover, waitForVO, voiceOverClipData.Value, delegate
                {
                    if (stopVoiceover && voiceOverSound.IsValid())
                    {
                        BroAudio.Stop(voiceOverSound);
                    }

                    Continue();
                });
            }
            else
            {
                sayDialog.Say(subbedText, !extendPrevious, waitForClick, fadeWhenDone, stopVoiceover, waitForVO, voiceOverClipData.Value, delegate
                {
                    if (stopVoiceover && voiceOverSound.IsValid())
                    {
                        BroAudio.Stop(voiceOverSound);
                    }
                });
                Continue();
            }
        }

        public override string GetSummary()
        {
            string namePrefix = "";
            Character resolvedCharacter = ResolveCharacter();
            if (resolvedCharacter != null)
            {
                namePrefix = resolvedCharacter.NameText + ": ";
            }
            if (extendPrevious)
            {
                namePrefix = "EXTEND" + ": ";
            }
            return namePrefix + "\"" + storyText + "\"";
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255);
        }

        public override void OnReset()
        {
            executionCount = 0;
        }

        public override void OnStopExecuting()
        {
            if (voiceOverSound.IsValid())
            {
                BroAudio.Stop(voiceOverSound);
            }

            SayDialog sayDialog = SayDialog.GetSayDialog();
            if (sayDialog == null)
            {
                return;
            }

            sayDialog.Stop();
        }

        #endregion

        #region ILocalizable implementation

        public virtual string GetStandardText()
        {
            return storyText;
        }

        public virtual void SetStandardText(string standardText)
        {
            storyText = standardText;
        }

        public virtual string GetDescription()
        {
            return description;
        }

        public virtual string GetStringId()
        {
            // String id for Say commands is SAY.<Localization Id>.<Command id>.[Character Name]
            string stringId = "SAY." + GetBlackboard().LocalizationId + "." + ItemId + ".";
            Character resolvedCharacter = ResolveCharacter();
            if (resolvedCharacter != null)
            {
                stringId += resolvedCharacter.NameText;
            }

            return stringId;
        }

        #endregion

        private Character ResolveCharacter()
        {
            return characterData.IsConfigured ? characterData.Value : character;
        }

        private Sprite ResolvePortrait(Character resolvedCharacter)
        {
            return resolvedCharacter != null ? resolvedCharacter.DefaultPortrait : portrait;
        }
    }
}
