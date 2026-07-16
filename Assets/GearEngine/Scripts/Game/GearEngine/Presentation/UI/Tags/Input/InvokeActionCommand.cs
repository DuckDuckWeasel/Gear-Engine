using Fungus;
using UnityEngine;
using GearEngine.Core.Actions;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    /// <summary>
    /// A generic wrapper command for Fungus that executes a decoupled pure C# IAction.
    /// This removes the need to create a new MonoBehaviour Command for every new behavior.
    /// </summary>
    [CommandInfo("Generic", "Invoke Action", "Executes a pure C# action independent of MonoBehaviours.")]
    [AddComponentMenu("")]
    public class InvokeActionCommand : Command
    {
        [Tooltip("The pure C# action to execute.")]
        [SerializeReference]
        public IAction action;

        public override void OnEnter()
        {
            if (action != null)
            {
                action.Execute(Continue);
            }
            else
            {
                Debug.LogWarning("[InvokeActionCommand] No action assigned to execute.");
                Continue();
            }
        }

        public override string GetSummary()
        {
            if (action == null)
                return "None";
            return action.GetType().Name;
        }
    }
}
