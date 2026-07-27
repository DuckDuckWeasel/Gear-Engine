using System;
using System.Collections.Generic;
using Scaffold.VisualScripting.Authoring;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    public sealed class BlackboardAuthoringController
    {
        public BlackboardAuthoringController(BlackboardAuthoringTarget target, BlackboardAuthoringClipboard clipboard)
        {
            this.target = target ?? throw new ArgumentNullException(nameof(target));
            this.clipboard = clipboard ?? throw new ArgumentNullException(nameof(clipboard));
        }

        public BlackboardDefinition Definition => target.Definition;

        public BlackboardAuthoringMetadata Metadata => target.Metadata;

        public BlackboardAuthoringClipboard Clipboard => clipboard;

        private readonly BlackboardAuthoringTarget target;
        private readonly BlackboardAuthoringClipboard clipboard;
        private readonly SerializedGraphCloner cloner = new SerializedGraphCloner();
        private readonly DefinitionIdRegenerator idRegenerator = new DefinitionIdRegenerator();
        private readonly BlackboardDefinitionValidator validator = new BlackboardDefinitionValidator();

        public BlockDefinition AddBlock(string name = "Block")
        {
            BeginChange("Add Blackboard Block");
            BlockDefinition block = CreateBlock(CreateUniqueBlockName(name));
            Definition.Blocks.Add(block);
            SelectBlock(block);
            CompleteChange();
            return block;
        }

        public bool RemoveBlock(DefinitionId blockId)
        {
            int index = FindBlockIndex(blockId);
            if (index < 0)
            {
                return false;
            }

            BeginChange("Remove Blackboard Block");
            RemoveBlockAt(index);
            CompleteChange();
            return true;
        }

        public bool MoveBlock(DefinitionId blockId, int destinationIndex)
        {
            int sourceIndex = FindBlockIndex(blockId);
            if (!CanMove(sourceIndex, destinationIndex, Definition.Blocks.Count))
            {
                return false;
            }

            BeginChange("Reorder Blackboard Block");
            MoveBlockAt(sourceIndex, destinationIndex);
            CompleteChange();
            return true;
        }

        public BlockDefinition DuplicateBlock(DefinitionId blockId)
        {
            BlockDefinition source = RequireBlock(blockId);
            BeginChange("Duplicate Blackboard Block");
            BlockDefinition clone = cloner.CloneGraph(source);
            idRegenerator.Regenerate(clone);
            clone.Name = CreateUniqueBlockName($"{source.Name} Copy");
            Definition.Blocks.Add(clone);
            SelectBlock(clone);
            CompleteChange();
            return clone;
        }

        public void CopyBlock(DefinitionId blockId)
        {
            clipboard.Copy(RequireBlock(blockId));
        }

        public BlockDefinition PasteBlock()
        {
            BeginChange("Paste Blackboard Block");
            BlockDefinition block = clipboard.PasteBlock();
            block.Name = CreateUniqueBlockName(block.Name);
            Definition.Blocks.Add(block);
            SelectBlock(block);
            CompleteChange();
            return block;
        }

        public ActionTrackDefinition AddTrack(DefinitionId blockId, string name = "Track")
        {
            BlockDefinition block = RequireBlock(blockId);
            BeginChange("Add Blackboard Action Track");
            ActionTrackDefinition track = new ActionTrackDefinition { Name = CreateUniqueTrackName(block, name) };
            block.Tracks.Add(track);
            Metadata.SelectedTrackId = track.DefinitionId;
            CompleteChange();
            return track;
        }

        public bool RemoveTrack(DefinitionId trackId)
        {
            BlockDefinition block = FindOwningBlock(trackId);
            int index = FindTrackIndex(block, trackId);
            if (index < 0)
            {
                return false;
            }

            BeginChange("Remove Blackboard Action Track");
            block.Tracks.RemoveAt(index);
            RemoveTrackMetadata(trackId);
            CompleteChange();
            return true;
        }

        public bool MoveTrack(DefinitionId trackId, int destinationIndex)
        {
            BlockDefinition block = FindOwningBlock(trackId);
            int sourceIndex = FindTrackIndex(block, trackId);
            if (!CanMove(sourceIndex, destinationIndex, block?.Tracks.Count ?? 0))
            {
                return false;
            }

            BeginChange("Reorder Blackboard Action Track");
            ActionTrackDefinition track = block.Tracks[sourceIndex];
            block.Tracks.RemoveAt(sourceIndex);
            block.Tracks.Insert(destinationIndex, track);
            CompleteChange();
            return true;
        }

        public IAction AddAction(DefinitionId trackId, Type actionType)
        {
            ActionTrackDefinition track = RequireTrack(trackId);
            IAction action = CreateManagedInstance<IAction>(actionType, "action");
            BeginChange("Add Blackboard Action");
            track.ActionList.Actions.Add(action);
            SelectAction(track, action);
            CompleteChange();
            return action;
        }

        public bool RemoveAction(DefinitionId actionId)
        {
            ActionTrackDefinition track = FindOwningTrack(actionId);
            int index = FindActionIndex(track, actionId);
            if (index < 0)
            {
                return false;
            }

            BeginChange("Remove Blackboard Action");
            track.ActionList.Actions.RemoveAt(index);
            RemoveActionMetadata(actionId);
            CompleteChange();
            return true;
        }

        public bool MoveAction(DefinitionId actionId, int destinationIndex)
        {
            ActionTrackDefinition track = FindOwningTrack(actionId);
            int sourceIndex = FindActionIndex(track, actionId);
            if (!CanMove(sourceIndex, destinationIndex, track?.ActionList.Actions.Count ?? 0))
            {
                return false;
            }

            BeginChange("Reorder Blackboard Action");
            IAction action = track.ActionList.Actions[sourceIndex];
            track.ActionList.Actions.RemoveAt(sourceIndex);
            track.ActionList.Actions.Insert(destinationIndex, action);
            CompleteChange();
            return true;
        }

        public IAction DuplicateAction(DefinitionId actionId)
        {
            ActionTrackDefinition track = RequireOwningTrack(actionId);
            IAction source = RequireAction(track, actionId);
            BeginChange("Duplicate Blackboard Action");
            IAction clone = cloner.CloneGraph(source);
            idRegenerator.Regenerate(clone);
            track.ActionList.Actions.Insert(FindActionIndex(track, actionId) + 1, clone);
            SelectAction(track, clone);
            CompleteChange();
            return clone;
        }

        public void CopyAction(DefinitionId actionId)
        {
            ActionTrackDefinition track = RequireOwningTrack(actionId);
            clipboard.Copy(RequireAction(track, actionId));
        }

        public IAction PasteAction(DefinitionId trackId)
        {
            ActionTrackDefinition track = RequireTrack(trackId);
            BeginChange("Paste Blackboard Action");
            IAction action = clipboard.PasteAction();
            track.ActionList.Actions.Add(action);
            SelectAction(track, action);
            CompleteChange();
            return action;
        }

        public TriggerDefinition SetTrigger(DefinitionId blockId, Type triggerType)
        {
            BlockDefinition block = RequireBlock(blockId);
            TriggerDefinition trigger = CreateManagedInstance<TriggerDefinition>(triggerType, "trigger");
            BeginChange("Set Blackboard Trigger");
            block.Trigger = trigger;
            CompleteChange();
            return trigger;
        }

        public void ClearTrigger(DefinitionId blockId)
        {
            BeginChange("Clear Blackboard Trigger");
            RequireBlock(blockId).Trigger = null;
            CompleteChange();
        }

        public VariableDefinitionBase AddVariable(Type variableType, string key = "Variable")
        {
            VariableDefinitionBase variable = CreateManagedInstance<VariableDefinitionBase>(variableType, "variable");
            BeginChange("Add Blackboard Variable");
            variable.Key = CreateUniqueVariableKey(key);
            Definition.Variables.Add(variable);
            CompleteChange();
            return variable;
        }

        public bool RemoveVariable(DefinitionId variableId)
        {
            int index = FindVariableIndex(variableId);
            if (index < 0)
            {
                return false;
            }

            BeginChange("Remove Blackboard Variable");
            Definition.Variables.RemoveAt(index);
            CompleteChange();
            return true;
        }

        public bool MoveVariable(DefinitionId variableId, int destinationIndex)
        {
            int sourceIndex = FindVariableIndex(variableId);
            if (!CanMove(sourceIndex, destinationIndex, Definition.Variables.Count))
            {
                return false;
            }

            BeginChange("Reorder Blackboard Variable");
            VariableDefinitionBase variable = Definition.Variables[sourceIndex];
            Definition.Variables.RemoveAt(sourceIndex);
            Definition.Variables.Insert(destinationIndex, variable);
            CompleteChange();
            return true;
        }

        public ActionGroupAuthoringMetadata GroupActions(DefinitionId trackId, IReadOnlyList<DefinitionId> actionIds, string name)
        {
            ActionTrackDefinition track = RequireTrack(trackId);
            EnsureActionsBelongToTrack(track, actionIds);
            BeginChange("Group Blackboard Actions");
            ActionGroupAuthoringMetadata group = new ActionGroupAuthoringMetadata(trackId, name);
            group.ActionIds.AddRange(actionIds);
            Metadata.ActionGroups.Add(group);
            CompleteChange();
            return group;
        }

        public bool UngroupActions(string groupId)
        {
            int index = FindGroupIndex(groupId);
            if (index < 0)
            {
                return false;
            }

            BeginChange("Ungroup Blackboard Actions");
            Metadata.ActionGroups.RemoveAt(index);
            CompleteChange();
            return true;
        }

        public void SetBlockTint(DefinitionId blockId, bool useCustomTint, Color tint)
        {
            BeginChange("Set Blackboard Block Tint");
            BlockAuthoringMetadata layout = GetOrCreateLayout(blockId);
            layout.UseCustomTint = useCustomTint;
            layout.Tint = tint;
            CompleteChange();
        }

        public void AutoLayout(float horizontalSpacing = 360f, float verticalSpacing = 220f)
        {
            BeginChange("Layout Blackboard Graph");
            for (int index = 0; index < Definition.Blocks.Count; index++)
            {
                PositionBlock(index, horizontalSpacing, verticalSpacing);
            }

            CompleteChange();
        }

        public IReadOnlyList<BlackboardValidationIssue> Validate()
        {
            return validator.Validate(Definition);
        }

        private BlockDefinition CreateBlock(string name)
        {
            BlockDefinition block = new BlockDefinition { Name = name };
            block.Tracks.Add(new ActionTrackDefinition { Name = "Main" });
            return block;
        }

        private string CreateUniqueBlockName(string requested)
        {
            return CreateUniqueName(requested, candidate => Definition.Blocks.Exists(block => block != null && block.Name == candidate));
        }

        private string CreateUniqueTrackName(BlockDefinition block, string requested)
        {
            return CreateUniqueName(requested, candidate => block.Tracks.Exists(track => track != null && track.Name == candidate));
        }

        private string CreateUniqueVariableKey(string requested)
        {
            return CreateUniqueName(requested, candidate => Definition.Variables.Exists(variable => variable != null && variable.Key == candidate));
        }

        private string CreateUniqueName(string requested, Predicate<string> exists)
        {
            string baseName = string.IsNullOrWhiteSpace(requested) ? "Item" : requested;
            string candidate = baseName;
            for (int suffix = 2; exists(candidate); suffix++)
            {
                candidate = $"{baseName} {suffix}";
            }

            return candidate;
        }

        private void RemoveBlockAt(int index)
        {
            DefinitionId blockId = Definition.Blocks[index].DefinitionId;
            foreach (ActionTrackDefinition track in Definition.Blocks[index].Tracks)
            {
                RemoveTrackMetadata(track.DefinitionId);
            }

            Definition.Blocks.RemoveAt(index);
            Metadata.BlockLayouts.RemoveAll(layout => layout.BlockId == blockId);
            if (Metadata.SelectedBlockId == blockId)
            {
                Metadata.ClearSelection();
            }
        }

        private void MoveBlockAt(int sourceIndex, int destinationIndex)
        {
            BlockDefinition block = Definition.Blocks[sourceIndex];
            Definition.Blocks.RemoveAt(sourceIndex);
            Definition.Blocks.Insert(destinationIndex, block);
        }

        private void SelectBlock(BlockDefinition block)
        {
            Metadata.ClearSelection();
            Metadata.SelectedBlockId = block.DefinitionId;
            GetOrCreateLayout(block.DefinitionId);
        }

        private void SelectAction(ActionTrackDefinition track, IAction action)
        {
            Metadata.SelectedTrackId = track.DefinitionId;
            Metadata.SelectedActionIds.Clear();
            Metadata.SelectedActionIds.Add(action.DefinitionId);
        }

        private void RemoveTrackMetadata(DefinitionId trackId)
        {
            Metadata.ActionGroups.RemoveAll(group => group.TrackId == trackId);
            if (Metadata.SelectedTrackId == trackId)
            {
                Metadata.SelectedTrackId = DefinitionId.Empty;
                Metadata.SelectedActionIds.Clear();
            }
        }

        private void RemoveActionMetadata(DefinitionId actionId)
        {
            Metadata.SelectedActionIds.Remove(actionId);
            foreach (ActionGroupAuthoringMetadata group in Metadata.ActionGroups)
            {
                group.ActionIds.Remove(actionId);
            }

            Metadata.ActionGroups.RemoveAll(group => group.ActionIds.Count == 0);
        }

        private void EnsureActionsBelongToTrack(ActionTrackDefinition track, IReadOnlyList<DefinitionId> actionIds)
        {
            if (actionIds == null || actionIds.Count == 0)
            {
                throw new ArgumentException("Select at least one action to create a group.", nameof(actionIds));
            }

            for (int index = 0; index < actionIds.Count; index++)
            {
                RequireAction(track, actionIds[index]);
            }
        }

        private int FindGroupIndex(string groupId)
        {
            return Metadata.ActionGroups.FindIndex(group => string.Equals(group.GroupId, groupId, StringComparison.Ordinal));
        }

        private void PositionBlock(int index, float horizontalSpacing, float verticalSpacing)
        {
            BlockDefinition block = Definition.Blocks[index];
            BlockAuthoringMetadata layout = GetOrCreateLayout(block.DefinitionId);
            int column = index % 3;
            int row = index / 3;
            layout.Position = new Rect(column * horizontalSpacing, row * verticalSpacing, 320f, 180f);
        }

        private BlockAuthoringMetadata GetOrCreateLayout(DefinitionId blockId)
        {
            BlockAuthoringMetadata layout = Metadata.BlockLayouts.Find(item => item.BlockId == blockId);
            if (layout != null)
            {
                return layout;
            }

            layout = new BlockAuthoringMetadata(blockId, new Rect(0f, 0f, 320f, 180f));
            Metadata.BlockLayouts.Add(layout);
            return layout;
        }

        private BlockDefinition RequireBlock(DefinitionId blockId)
        {
            BlockDefinition block = Definition.Blocks.Find(item => item != null && item.DefinitionId == blockId);
            return block ?? throw new InvalidOperationException($"Blackboard block '{blockId}' was not found.");
        }

        private ActionTrackDefinition RequireTrack(DefinitionId trackId)
        {
            ActionTrackDefinition track = FindTrack(trackId);
            return track ?? throw new InvalidOperationException($"Blackboard action track '{trackId}' was not found.");
        }

        private ActionTrackDefinition RequireOwningTrack(DefinitionId actionId)
        {
            ActionTrackDefinition track = FindOwningTrack(actionId);
            return track ?? throw new InvalidOperationException($"Blackboard action '{actionId}' was not found.");
        }

        private IAction RequireAction(ActionTrackDefinition track, DefinitionId actionId)
        {
            IAction action = track.ActionList.Actions.Find(item => item != null && item.DefinitionId == actionId);
            return action ?? throw new InvalidOperationException($"Blackboard action '{actionId}' was not found.");
        }

        private int FindBlockIndex(DefinitionId blockId)
        {
            return Definition.Blocks.FindIndex(block => block != null && block.DefinitionId == blockId);
        }

        private int FindTrackIndex(BlockDefinition block, DefinitionId trackId)
        {
            return block == null ? -1 : block.Tracks.FindIndex(track => track != null && track.DefinitionId == trackId);
        }

        private int FindActionIndex(ActionTrackDefinition track, DefinitionId actionId)
        {
            return track == null ? -1 : track.ActionList.Actions.FindIndex(action => action != null && action.DefinitionId == actionId);
        }

        private int FindVariableIndex(DefinitionId variableId)
        {
            return Definition.Variables.FindIndex(variable => variable != null && variable.DefinitionId == variableId);
        }

        private BlockDefinition FindOwningBlock(DefinitionId trackId)
        {
            return Definition.Blocks.Find(block => FindTrackIndex(block, trackId) >= 0);
        }

        private ActionTrackDefinition FindOwningTrack(DefinitionId actionId)
        {
            foreach (BlockDefinition block in Definition.Blocks)
            {
                ActionTrackDefinition track = block.Tracks.Find(item => FindActionIndex(item, actionId) >= 0);
                if (track != null)
                {
                    return track;
                }
            }

            return null;
        }

        private ActionTrackDefinition FindTrack(DefinitionId trackId)
        {
            BlockDefinition block = FindOwningBlock(trackId);
            return block?.Tracks.Find(track => track != null && track.DefinitionId == trackId);
        }

        private T CreateManagedInstance<T>(Type type, string label) where T : class
        {
            if (type == null || type.IsAbstract || type.IsGenericTypeDefinition || !typeof(T).IsAssignableFrom(type))
            {
                throw new ArgumentException($"The selected {label} type is invalid.", nameof(type));
            }

            return Activator.CreateInstance(type, true) as T ?? throw new InvalidOperationException($"The {label} type '{type.FullName}' could not be created.");
        }

        private bool CanMove(int sourceIndex, int destinationIndex, int count)
        {
            return sourceIndex >= 0 && destinationIndex >= 0 && destinationIndex < count && sourceIndex != destinationIndex;
        }

        private void BeginChange(string label)
        {
            Undo.RegisterCompleteObjectUndo(target.Owner, label);
        }

        private void CompleteChange()
        {
            EditorUtility.SetDirty(target.Owner);
            if (target.Owner is Component component)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            }
        }
    }
}
