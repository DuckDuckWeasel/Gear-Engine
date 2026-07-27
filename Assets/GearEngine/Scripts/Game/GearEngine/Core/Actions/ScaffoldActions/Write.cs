using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    [CommandInfo("UI",
                 "Write",
                 "Writes content to a UI Text or Text Mesh object.")]
    [Serializable]
    public class Write : ActionBase, ILocalizable
    {
        [Tooltip("Text object to set text on. Text, Input Field and Text Mesh objects are supported.")]
        [SerializeField] protected GameObject textObject;

        [Tooltip("String value to assign to the text object")]
        [SerializeField] protected StringDataMulti text;

        [Tooltip("Notes about this story text for other authors, localization, etc.")]
        [SerializeField] protected string description;

        [Tooltip("Clear existing text before writing new text")]
        [SerializeField] protected bool clearText = true;

        [Tooltip("Wait until this command finishes before executing the next command")]
        [SerializeField] protected bool waitUntilFinished = true;

        [Tooltip("Color mode to apply to the text.")]
        [SerializeField] protected TextColor textColor = TextColor.Default;

        [Tooltip("Alpha to apply to the text.")]
        [SerializeField] protected FloatData setAlpha = new FloatData(1f);

        [Tooltip("Color to apply to the text.")]
        [SerializeField] protected ColorData setColor = new ColorData(Color.white);

        #region Public members

        public override void OnEnter()
        {
            if (textObject == null)
            {
                Continue();
                return;
            }

            Writer writer = GetWriter();
            if (writer == null)
            {
                Continue();
                return;
            }

            ApplyTextColor(writer);
            Blackboard blackboard = GetBlackboard();
            string newText = blackboard.SubstituteVariables(text.Value);
            StartWrite(writer, newText);
        }

        private void ApplyTextColor(Writer writer)
        {
            switch (textColor)
            {
                case TextColor.SetAlpha: writer.SetTextAlpha(setAlpha); break;
                case TextColor.SetColor: writer.SetTextColor(setColor); break;
                case TextColor.SetVisible: writer.SetTextAlpha(1f); break;
            }
        }

        private void StartWrite(Writer writer, string newText)
        {
            Action completion = waitUntilFinished ? Continue : null;
            System.Collections.IEnumerator routine = writer.Write(newText, clearText, false, true, false, null, completion);
            RunRoutine(routine, !waitUntilFinished);
            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (textObject != null)
            {
                return textObject.name + " : " + text.Value;
            }

            return "Error: No text object selected";
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override void OnStopExecuting()
        {
            GetWriter().Stop();
        }

        protected Writer GetWriter()
        {
            Writer writer = textObject.gameObject.GetComponent<Writer>();
            if (writer == null)
            {
                writer = textObject.AddComponent<Writer>();
            }

            return writer;
        }

        #endregion

        #region ILocalizable implementation

        public virtual string GetStandardText()
        {
            return text;
        }

        public virtual void SetStandardText(string standardText)
        {
            text.Value = standardText;
        }

        public virtual string GetDescription()
        {
            return description;
        }

        public virtual string GetStringId()
        {
            // String id for Write commands is WRITE.<Localization Id>.<Command id>
            return "WRITE." + GetBlackboard().LocalizationId + "." + ItemId;
        }

        public override bool HasReference(Variable variable)
        {
            return text.stringRef == variable || setAlpha.floatRef == variable || setColor.colorRef == variable || base.HasReference(variable);
        }

        #endregion
    }
}
