using System;
using Scaffold.VisualScripting.Unity;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    public sealed class BlackboardEditorExecutionController
    {
        public bool CanControl(BlackboardBehaviour behaviour, out string reason)
        {
            if (!EditorApplication.isPlaying)
            {
                reason = "Enter Play Mode to control execution.";
                return false;
            }

            if (behaviour == null)
            {
                reason = "Select a BlackboardBehaviour to control execution.";
                return false;
            }

            if (!behaviour.IsRuntimeAvailable)
            {
                reason = "The selected Blackboard runtime is unavailable.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public bool Execute(BlackboardBehaviour behaviour, DefinitionId blockId)
        {
            return TryControl(behaviour, runtime => runtime.ExecuteBlock(blockId), "execute");
        }

        public bool ExecuteFromAction(BlackboardBehaviour behaviour, DefinitionId blockId, int actionIndex)
        {
            return TryControl(behaviour, runtime => ExecuteFromAction(runtime, blockId, actionIndex), "execute from action");
        }

        public static bool TryResolveSelectedActionStart(
            BlackboardAuthoringController controller,
            out DefinitionId blockId,
            out int taskIndex)
        {
            blockId = DefinitionId.Empty;
            taskIndex = -1;
            if (controller == null ||
                controller.Metadata.SelectedActionIds.Count != 1)
            {
                return false;
            }

            return TryResolveActionStart(
                controller,
                controller.Metadata.SelectedActionIds[0],
                out blockId,
                out taskIndex);
        }

        public static bool TryResolveActionStart(
            BlackboardAuthoringController controller,
            DefinitionId actionId,
            out DefinitionId blockId,
            out int taskIndex)
        {
            blockId = DefinitionId.Empty;
            taskIndex = -1;
            if (controller == null || actionId == DefinitionId.Empty)
            {
                return false;
            }

            for (int blockIndex = 0;
                 blockIndex < controller.Definition.Blocks.Count;
                 blockIndex++)
            {
                BlockDefinition block = controller.Definition.Blocks[blockIndex];
                if (TryResolveActionStart(
                    block,
                    actionId,
                    out taskIndex))
                {
                    blockId = block.DefinitionId;
                    return true;
                }
            }

            return false;
        }

        public bool Stop(BlackboardBehaviour behaviour, DefinitionId blockId)
        {
            return TryControl(behaviour, runtime => runtime.StopBlock(blockId), "stop");
        }

        public void StopAll(BlackboardBehaviour behaviour)
        {
            if (!CanControl(behaviour, out string reason))
            {
                Debug.LogWarning($"[BlackboardEditor] {reason}");
                return;
            }

            try
            {
                behaviour.Runtime.StopAll();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[BlackboardEditor] Failed to stop all Blocks: {exception.Message}");
            }
        }

        private bool TryControl(BlackboardBehaviour behaviour, Func<Blackboard, bool> control, string label)
        {
            if (!CanControl(behaviour, out string reason))
            {
                Debug.LogWarning($"[BlackboardEditor] {reason}");
                return false;
            }

            return InvokeControl(behaviour.Runtime, control, label);
        }

        private bool InvokeControl(Blackboard runtime, Func<Blackboard, bool> control, string label)
        {
            try
            {
                return control.Invoke(runtime);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[BlackboardEditor] Failed to {label} the Block: {exception.Message}");
                return false;
            }
        }

        private bool ExecuteFromAction(Blackboard runtime, DefinitionId blockId, int actionIndex)
        {
            Block block = null;
            for (int index = 0; index < runtime.Blocks.Count; index++)
            {
                if (runtime.Blocks[index].Definition.DefinitionId == blockId)
                {
                    block = runtime.Blocks[index];
                    break;
                }
            }

            return block != null && runtime.ExecuteBlock(block, actionIndex);
        }

        private static bool TryResolveActionStart(
            BlockDefinition block,
            DefinitionId actionId,
            out int taskIndex)
        {
            taskIndex = 0;
            if (block == null)
            {
                taskIndex = -1;
                return false;
            }

            for (int trackIndex = 0;
                 trackIndex < block.Tracks.Count;
                 trackIndex++)
            {
                ActionTrackDefinition track = block.Tracks[trackIndex];
                if (track == null)
                {
                    continue;
                }

                for (int actionIndex = 0;
                     actionIndex < track.ActionList.Actions.Count;
                     actionIndex++)
                {
                    IAction action = track.ActionList.Actions[actionIndex];
                    if (action != null &&
                        action.DefinitionId == actionId)
                    {
                        return true;
                    }

                    taskIndex++;
                }
            }

            taskIndex = -1;
            return false;
        }
    }
}
