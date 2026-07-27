using System;
using GearEngine.Core.Actions;

using UnityEngine;
using System.Collections;

namespace Scaffold
{
    [CommandInfo("Narrative",
                 "Conversation",
                 "Do multiple say and portrait commands in a single block of text. Format is: [character] [portrait] [stage position] [hide] [<<< | >>>] [clear | noclear] [wait | nowait] [fade | nofade] [: Story text]")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class Conversation : ActionBase
    {
        [Tooltip("The Conversation text")]
        [SerializeField] protected StringDataMulti conversationText;

        [Tooltip("The Conversation manager")]
        protected ConversationManager conversationManager = new ConversationManager();

        [Tooltip("The Clear previous")]
        [SerializeField] protected BooleanData clearPrevious = new BooleanData(true);
        [Tooltip("The Wait for input")]
        [SerializeField] protected BooleanData waitForInput = new BooleanData(true);
        [Tooltip("a wait for seconds added to each item of the conversation.")]
        [SerializeField] protected FloatData waitForSeconds = new FloatData(0);
        [SerializeField] protected BooleanData fadeWhenDone = new BooleanData(true);

        protected virtual void Start()
        {
            conversationManager.PopulateCharacterCache();
        }

        public override void OnEnter()
        {
            RunRoutine(DoConversation());
        }

        protected virtual IEnumerator DoConversation()
        {
            Scaffold.VisualScripting.Blackboard blackboard = GetBlackboard();
            string subbedText = blackboard.Substitute(conversationText.Value);

            conversationManager.ClearPrev = clearPrevious;
            conversationManager.WaitForInput = waitForInput;
            conversationManager.FadeDone = fadeWhenDone;
            conversationManager.WaitForSeconds = waitForSeconds;

            yield return conversationManager.DoConversation(subbedText);

            Continue();
        }

        #region Public members

        public override string GetSummary()
        {
            return conversationText.Value;
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return clearPrevious.booleanRef == variable || waitForInput.booleanRef == variable ||
                waitForSeconds.floatRef == variable || fadeWhenDone.booleanRef == variable ||
                base.HasReference(variable);
        }

        #endregion


    }
}
