using System;
using GearEngine.Core.Actions;

using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Displays a button in a multiple choice menu.
    /// </summary>
    [CommandInfo("Narrative",
                 "Menu",
                 "Displays a button in a multiple choice menu")]
    [Serializable]
    public class Menu : ActionBase, ILocalizable
    {
        [Tooltip("Text to display on the menu button")]
        [TextArea()]
        [SerializeField] protected string text = "Option Text";

        [Tooltip("Notes about the option text for other authors, localization, etc.")]
        [SerializeField] protected string description = "";

        [Tooltip("Name of the Block to execute when this option is selected")]
        [SerializeField] private StringData targetBlockName = new StringData();

        [Tooltip("Hide this option if the target block has been executed previously")]
        [SerializeField] protected bool hideIfVisited;

        [Tooltip("If false, the menu option will be displayed but will not be selectable")]
        [SerializeField] protected BooleanData interactable = new BooleanData(true);

        [Tooltip("Menu Dialog used to display this option")]
        [SerializeField] protected MenuDialog menuDialog;

        [Tooltip("If true, this option will be passed to the Menu Dialogue but marked as hidden, this can be used to hide options while maintaining a Menu Shuffle.")]
        [SerializeField] protected BooleanData hideThisOption = new BooleanData(false);

        #region Public members

        public MenuDialog SetMenuDialog
        {
            get => menuDialog;
            set => menuDialog = value;
        }

        public override void OnEnter()
        {
            if (menuDialog == null)
            {
                Debug.LogError("[Menu] A Menu Dialog reference is required.");
                Fail();
                return;
            }

            VisualScripting.Block targetBlock = GetBlackboard().FindBlock(targetBlockName.Value);
            bool hideOption =
                (hideIfVisited &&
                    targetBlock != null &&
                    targetBlock.ExecutionCount > 0) ||
                hideThisOption.Value;

            menuDialog.SetActive(true);

            VisualScripting.Blackboard blackboard = GetBlackboard();
            string displayText = blackboard.Substitute(text);

            menuDialog.AddOption(
                displayText,
                interactable,
                hideOption,
                () => blackboard.ExecuteBlock(targetBlockName.Value));

            Continue();
        }

        public override string GetSummary()
        {
            if (string.IsNullOrWhiteSpace(targetBlockName.Value))
            {
                return "Error: No target block selected";
            }

            if (text == "")
            {
                return "Error: No button text selected";
            }

            return text + " : " + targetBlockName.Value;
        }

        public override Color GetButtonColor()
        {
            return new Color32(184, 210, 235, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return targetBlockName.stringRef == variable ||
                interactable.booleanRef == variable ||
                hideThisOption.booleanRef == variable ||
                base.HasReference(variable);
        }

        #endregion

        #region ILocalizable implementation

        public virtual string GetStandardText()
        {
            return text;
        }

        public virtual void SetStandardText(string standardText)
        {
            text = standardText;
        }

        public virtual string GetDescription()
        {
            return description;
        }

        public virtual string GetStringId()
        {
            // String id for Menu commands is MENU.<Localization Id>.<Command id>
            return "MENU." + GetBlackboard().LocalizationId + "." + ItemId;
        }

        #endregion

    }
}
