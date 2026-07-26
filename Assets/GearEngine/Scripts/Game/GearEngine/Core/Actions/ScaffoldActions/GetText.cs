using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Gets the text property from a UI Text object and stores it in a string variable.
    /// </summary>
    [CommandInfo("UI",
                 "Get Text",
                 "Gets the text property from a UI Text object and stores it in a string variable.")]
    [Serializable]
    public class GetText : ActionBase
    {
        [Tooltip("Text object to get text value from")]
        [SerializeField] protected GameObject targetTextObject;

        [Tooltip("String variable to store the text value in")]
        [VariableProperty(typeof(StringVariable))]
        [SerializeField] protected StringVariable stringVariable;

        #region Public members

        public override void OnEnter()
        {
            if (stringVariable == null)
            {
                Continue();
                return;
            }

            TextAdapter textAdapter = new TextAdapter();
            textAdapter.InitFromGameObject(targetTextObject);

            if (textAdapter.HasTextObject())
            {
                stringVariable.Value = textAdapter.Text;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (targetTextObject == null)
            {
                return "Error: No text object selected";
            }

            if (stringVariable == null)
            {
                return "Error: No variable selected";
            }

            return targetTextObject.name + " : " + stringVariable.name;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return stringVariable == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}