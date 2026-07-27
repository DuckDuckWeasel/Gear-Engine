using UnityEngine;
using System;

namespace Scaffold
{
    [CommandInfo("Flow", "Sub Block", "Executes another block as a sub-block.")]
    public class SubBlockCommand : Command
    {
        [Tooltip("The block to execute as a sub-block")]
        [SerializeField] protected Block targetBlock;

        public Block TargetBlock { get { return targetBlock; } set { targetBlock = value; } }

        public override void OnEnter()
        {
            if (targetBlock == null)
            {
                Continue();
                return;
            }

            // Inicia a execução do sub-bloco e continua este comando apenas quando ele finalizar
            StartCoroutine(targetBlock.Execute(0, () => Continue()));
        }

        public override string GetSummary()
        {
            if (targetBlock == null)
            {
                return "Error: No block selected";
            }
            return targetBlock.BlockName;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255); // Uma cor levemente diferente para distinguir sub-blocos
        }
    }
}
