using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Sets the text property on a UI Text object and/or an Input Field object.
    /// </summary>
    [CommandInfo("UI",
                 "Set Text",
                 "Sets the text property on a UI Text object and/or an Input Field object.")]
    [Serializable]
    public class SetText : ActionBase, ILocalizable
    {
        [Tooltip("Text object to set text on. Can be a UI Text, Text Field or Text Mesh object.")]
        [SerializeField] protected GameObject targetTextObject;

        [Tooltip("String value to assign to the text object")]
        [FormerlySerializedAs("stringData")]
        [SerializeField] protected StringDataMulti text;

        [Tooltip("Notes about this story text for other authors, localization, etc.")]
        [SerializeField] protected string description;

        #region Public members

        public override void OnEnter()
        {
            Scaffold.VisualScripting.Blackboard blackboard = GetBlackboard();
            string newText = blackboard.Substitute(text.Value);

            if (targetTextObject == null)
            {
                Continue();
                return;
            }

            TextAdapter textAdapter = new TextAdapter();
            textAdapter.InitFromGameObject(targetTextObject);

            if (textAdapter.HasTextObject())
            {
                textAdapter.Text = newText;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (targetTextObject != null)
            {
                return targetTextObject.name + " : " + text.Value;
            }

            return "Error: No text object selected";
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return text.stringRef == variable || base.HasReference(variable);
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
            // String id for Set Text commands is SETTEXT.<Localization Id>.<Command id>
            return "SETTEXT." + GetBlackboard().LocalizationId + "." + ItemId;
        }

        #endregion

    }
}
