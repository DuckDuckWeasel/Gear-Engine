using System;
using System.Collections.Generic;
using System.Linq;
using Scaffold.VisualScripting.Authoring;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    public sealed partial class BlackboardAuthoringController
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
            Metadata.SelectedBlockIds.Remove(blockId);
            if (Metadata.SelectedBlockId == blockId)
            {
                Metadata.SelectedBlockId = Metadata.SelectedBlockIds.Count == 0
                    ? DefinitionId.Empty
                    : Metadata.SelectedBlockIds[Metadata.SelectedBlockIds.Count - 1];
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
            Metadata.SelectedBlockIds.Add(block.DefinitionId);
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
        public UnityEngine.Object Owner => target.Owner;

        private bool blockMoveActive;

        public BlockDefinition GetBlock(DefinitionId blockId)
        {
            return Definition.Blocks.Find(block => block != null && block.DefinitionId == blockId);
        }

        public ActionTrackDefinition GetTrack(DefinitionId trackId)
        {
            return FindTrack(trackId);
        }

        public IAction GetAction(DefinitionId actionId)
        {
            ActionTrackDefinition track = FindOwningTrack(actionId);
            return track?.ActionList.Actions.Find(action => action != null && action.DefinitionId == actionId);
        }

        public ActionTrackDefinition GetOwningTrack(DefinitionId actionId)
        {
            return FindOwningTrack(actionId);
        }

        public BlockDefinition GetOwningBlockForAction(DefinitionId actionId)
        {
            ActionTrackDefinition track = FindOwningTrack(actionId);
            return track == null ? null : FindOwningBlock(track.DefinitionId);
        }

        public BlockAuthoringMetadata GetLayout(DefinitionId blockId)
        {
            return GetOrCreateLayout(blockId);
        }

        public void SynchronizeBlockSelection()
        {
            Metadata.SelectedBlockIds.RemoveAll(id => GetBlock(id) == null);
            if (!Metadata.SelectedBlockId.IsEmpty && !Metadata.SelectedBlockIds.Contains(Metadata.SelectedBlockId))
            {
                Metadata.SelectedBlockIds.Add(Metadata.SelectedBlockId);
            }

            SetSelectionFallback();
        }

        public void SelectOnlyBlock(DefinitionId blockId)
        {
            RequireBlock(blockId);
            BeginChange("Select Blackboard Block");
            Metadata.ClearSelection();
            Metadata.SelectedBlockId = blockId;
            Metadata.SelectedBlockIds.Add(blockId);
            GetOrCreateLayout(blockId);
            CompleteChange();
        }

        public void ToggleBlockSelection(DefinitionId blockId)
        {
            RequireBlock(blockId);
            BeginChange("Select Blackboard Blocks");
            ToggleSelectedBlock(blockId);
            CompleteChange();
        }

        public void SelectBlocks(IReadOnlyList<DefinitionId> blockIds)
        {
            BeginChange("Select Blackboard Blocks");
            Metadata.ClearSelection();
            AddSelectedBlocks(blockIds);
            SetSelectionFallback();
            CompleteChange();
        }

        public void ClearBlockSelection()
        {
            if (Metadata.SelectedBlockIds.Count == 0 && Metadata.SelectedBlockId.IsEmpty)
            {
                return;
            }

            BeginChange("Clear Blackboard Selection");
            Metadata.ClearSelection();
            CompleteChange();
        }

        public bool IsBlockSelected(DefinitionId blockId)
        {
            return Metadata.SelectedBlockIds.Contains(blockId) || Metadata.SelectedBlockId == blockId;
        }

        public void BeginBlockMove()
        {
            if (blockMoveActive)
            {
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.RegisterCompleteObjectUndo(target.Owner, "Move Blackboard Blocks");
            blockMoveActive = true;
        }

        public void MoveSelectedBlocks(Vector2 graphDelta)
        {
            if (!blockMoveActive)
            {
                throw new InvalidOperationException("Begin the Blackboard Block move before applying a delta.");
            }

            for (int index = 0; index < Metadata.SelectedBlockIds.Count; index++)
            {
                BlockAuthoringMetadata layout = GetOrCreateLayout(Metadata.SelectedBlockIds[index]);
                layout.Position = MoveRect(layout.Position, graphDelta);
            }
        }

        public void EndBlockMove()
        {
            if (!blockMoveActive)
            {
                return;
            }

            blockMoveActive = false;
            CompleteChange();
        }

        public void SetViewport(Vector2 scrollPosition, float zoom)
        {
            Metadata.ScrollPosition = scrollPosition;
            Metadata.Zoom = zoom;
            CompleteChange();
        }

        public void RenameBlock(DefinitionId blockId, string name)
        {
            BeginChange("Rename Blackboard Block");
            RequireBlock(blockId).Name = CreateUniqueBlockNameExcept(name, blockId);
            CompleteChange();
        }

        public void SetBlockDescription(DefinitionId blockId, string description)
        {
            BeginChange("Edit Blackboard Block Description");
            GetOrCreateLayout(blockId).Description = description;
            CompleteChange();
        }

        public void SetBlockPosition(DefinitionId blockId, Vector2 graphPosition)
        {
            BeginChange("Position Blackboard Block");
            BlockAuthoringMetadata layout = GetOrCreateLayout(blockId);
            Rect position = layout.Position;
            position.position = graphPosition;
            layout.Position = position;
            CompleteChange();
        }

        public void SetBlockExpanded(DefinitionId blockId, bool expanded)
        {
            BeginChange("Expand Blackboard Block");
            GetOrCreateLayout(blockId).Expanded = expanded;
            CompleteChange();
        }

        public void CopySelectedBlocks()
        {
            List<BlockDefinition> selected = GetSelectedBlocks();
            if (selected.Count > 0)
            {
                clipboard.Copy(selected);
            }
        }

        public IReadOnlyList<BlockDefinition> PasteBlocks(Vector2 graphPosition)
        {
            IReadOnlyList<BlockDefinition> pasted = clipboard.PasteBlocks();
            BeginChange("Paste Blackboard Blocks");
            AddPastedBlocks(pasted, graphPosition);
            CompleteChange();
            return pasted;
        }

        public IReadOnlyList<BlockDefinition> DuplicateSelectedBlocks()
        {
            List<BlockDefinition> selected = GetSelectedBlocks();
            BeginChange("Duplicate Blackboard Blocks");
            List<BlockDefinition> duplicates = CloneSelectedBlocks(selected);
            SelectDuplicateBlocks(duplicates);
            CompleteChange();
            return duplicates;
        }

        public void RemoveSelectedBlocks()
        {
            List<DefinitionId> selected = new List<DefinitionId>(Metadata.SelectedBlockIds);
            BeginChange("Remove Blackboard Blocks");
            RemoveBlocks(selected);
            Metadata.ClearSelection();
            CompleteChange();
        }

        public void CutSelectedBlocks()
        {
            CopySelectedBlocks();
            RemoveSelectedBlocks();
        }

        public bool MoveActionToTrack(DefinitionId actionId, DefinitionId destinationTrackId, int destinationIndex)
        {
            ActionTrackDefinition source = RequireOwningTrack(actionId);
            ActionTrackDefinition destination = RequireTrack(destinationTrackId);
            IAction action = RequireAction(source, actionId);
            BeginChange("Move Blackboard Action");
            MoveActionBetweenTracks(source, destination, action, destinationIndex);
            CompleteChange();
            return true;
        }

        public void SelectOnlyAction(DefinitionId trackId, DefinitionId actionId)
        {
            ActionTrackDefinition track = RequireTrack(trackId);
            RequireAction(track, actionId);
            BeginChange("Select Blackboard Action");
            Metadata.SelectedTrackId = trackId;
            Metadata.SelectedActionIds.Clear();
            Metadata.SelectedActionIds.Add(actionId);
            CompleteChange();
        }

        public void ToggleActionSelection(DefinitionId trackId, DefinitionId actionId)
        {
            ActionTrackDefinition track = RequireTrack(trackId);
            RequireAction(track, actionId);
            BeginChange("Select Blackboard Actions");
            Metadata.SelectedTrackId = trackId;
            if (!Metadata.SelectedActionIds.Remove(actionId))
            {
                Metadata.SelectedActionIds.Add(actionId);
            }

            CompleteChange();
        }

        public void SelectActionRange(DefinitionId trackId, DefinitionId actionId)
        {
            ActionTrackDefinition track = RequireTrack(trackId);
            int destinationIndex = FindActionIndex(track, actionId);
            int anchorIndex = Metadata.SelectedTrackId == trackId && Metadata.SelectedActionIds.Count > 0
                ? FindActionIndex(track, Metadata.SelectedActionIds[Metadata.SelectedActionIds.Count - 1])
                : -1;
            if (anchorIndex < 0 || destinationIndex < 0)
            {
                SelectOnlyAction(trackId, actionId);
                return;
            }

            BeginChange("Select Blackboard Action Range");
            Metadata.SelectedTrackId = trackId;
            Metadata.SelectedActionIds.Clear();
            int first = Math.Min(anchorIndex, destinationIndex);
            int last = Math.Max(anchorIndex, destinationIndex);
            for (int index = first; index <= last; index++)
            {
                IAction action = track.ActionList.Actions[index];
                if (action != null)
                {
                    Metadata.SelectedActionIds.Add(action.DefinitionId);
                }
            }

            CompleteChange();
        }

        public void SelectAllActions(DefinitionId trackId)
        {
            ActionTrackDefinition track = RequireTrack(trackId);
            BeginChange("Select All Blackboard Actions");
            Metadata.SelectedTrackId = trackId;
            Metadata.SelectedActionIds.Clear();
            for (int index = 0; index < track.ActionList.Actions.Count; index++)
            {
                IAction action = track.ActionList.Actions[index];
                if (action != null)
                {
                    Metadata.SelectedActionIds.Add(action.DefinitionId);
                }
            }

            CompleteChange();
        }

        public void ClearActionSelection()
        {
            BeginChange("Clear Blackboard Action Selection");
            Metadata.SelectedTrackId = DefinitionId.Empty;
            Metadata.SelectedActionIds.Clear();
            CompleteChange();
        }

        public void RemoveSelectedActions()
        {
            List<DefinitionId> selected = new List<DefinitionId>(Metadata.SelectedActionIds);
            BeginChange("Remove Blackboard Actions");
            for (int index = 0; index < selected.Count; index++)
            {
                RemoveActionWithoutUndo(selected[index]);
            }

            Metadata.SelectedActionIds.Clear();
            CompleteChange();
        }

        public VariableDefinitionBase DuplicateVariable(DefinitionId variableId)
        {
            VariableDefinitionBase source = RequireVariable(variableId);
            BeginChange("Duplicate Blackboard Variable");
            VariableDefinitionBase clone = cloner.CloneGraph(source);
            idRegenerator.Regenerate(clone);
            clone.Key = CreateUniqueVariableKey($"{source.Key} Copy");
            Definition.Variables.Insert(FindVariableIndex(variableId) + 1, clone);
            CompleteChange();
            return clone;
        }

        public void SortVariablesByName()
        {
            BeginChange("Sort Blackboard Variables By Name");
            Definition.Variables.Sort(CompareVariablesByName);
            CompleteChange();
        }

        public void SortVariablesByType()
        {
            BeginChange("Sort Blackboard Variables By Type");
            Definition.Variables.Sort(CompareVariablesByType);
            CompleteChange();
        }

        public void RecordSerializedChange(string label)
        {
            Undo.RegisterCompleteObjectUndo(target.Owner, label);
        }

        public void CompleteSerializedChange()
        {
            CompleteChange();
        }

        private void ToggleSelectedBlock(DefinitionId blockId)
        {
            if (Metadata.SelectedBlockIds.Remove(blockId))
            {
                SetSelectionFallback();
                return;
            }

            Metadata.SelectedBlockIds.Add(blockId);
            Metadata.SelectedBlockId = blockId;
            GetOrCreateLayout(blockId);
        }

        private void AddSelectedBlocks(IReadOnlyList<DefinitionId> blockIds)
        {
            for (int index = 0; blockIds != null && index < blockIds.Count; index++)
            {
                if (GetBlock(blockIds[index]) != null && !Metadata.SelectedBlockIds.Contains(blockIds[index]))
                {
                    Metadata.SelectedBlockIds.Add(blockIds[index]);
                }
            }
        }

        private void SetSelectionFallback()
        {
            Metadata.SelectedBlockId = Metadata.SelectedBlockIds.Count == 0
                ? DefinitionId.Empty
                : Metadata.SelectedBlockIds[Metadata.SelectedBlockIds.Count - 1];
        }

        private Rect MoveRect(Rect source, Vector2 delta)
        {
            source.position += delta;
            return source;
        }

        private string CreateUniqueBlockNameExcept(string requested, DefinitionId excludedId)
        {
            string baseName = string.IsNullOrWhiteSpace(requested) ? "Block" : requested;
            string candidate = baseName;
            for (int suffix = 2; BlockNameExists(candidate, excludedId); suffix++)
            {
                candidate = $"{baseName} {suffix}";
            }

            return candidate;
        }

        private bool BlockNameExists(string candidate, DefinitionId excludedId)
        {
            return Definition.Blocks.Exists(block =>
                block != null &&
                block.DefinitionId != excludedId &&
                string.Equals(block.Name, candidate, StringComparison.Ordinal));
        }

        private List<BlockDefinition> GetSelectedBlocks()
        {
            SynchronizeBlockSelection();
            List<BlockDefinition> selected = new List<BlockDefinition>();
            for (int index = 0; index < Definition.Blocks.Count; index++)
            {
                BlockDefinition block = Definition.Blocks[index];
                if (block != null && Metadata.SelectedBlockIds.Contains(block.DefinitionId))
                {
                    selected.Add(block);
                }
            }

            return selected;
        }

        private void AddPastedBlocks(IReadOnlyList<BlockDefinition> pasted, Vector2 position)
        {
            Metadata.ClearSelection();
            for (int index = 0; index < pasted.Count; index++)
            {
                BlockDefinition block = pasted[index];
                block.Name = CreateUniqueBlockName(block.Name);
                Definition.Blocks.Add(block);
                Rect rect = new Rect(position + new Vector2(index * 24f, index * 24f), new Vector2(260f, 100f));
                Metadata.BlockLayouts.Add(new BlockAuthoringMetadata(block.DefinitionId, rect));
                Metadata.SelectedBlockIds.Add(block.DefinitionId);
            }

            SetSelectionFallback();
        }

        private List<BlockDefinition> CloneSelectedBlocks(IReadOnlyList<BlockDefinition> selected)
        {
            List<BlockDefinition> duplicates = new List<BlockDefinition>();
            for (int index = 0; index < selected.Count; index++)
            {
                BlockDefinition source = selected[index];
                BlockDefinition clone = cloner.CloneGraph(source);
                idRegenerator.Regenerate(clone);
                clone.Name = CreateUniqueBlockName($"{source.Name} Copy");
                Definition.Blocks.Add(clone);
                DuplicateLayout(source.DefinitionId, clone.DefinitionId);
                duplicates.Add(clone);
            }

            return duplicates;
        }

        private void DuplicateLayout(DefinitionId sourceId, DefinitionId destinationId)
        {
            BlockAuthoringMetadata source = GetOrCreateLayout(sourceId);
            Rect rect = MoveRect(source.Position, new Vector2(24f, 24f));
            BlockAuthoringMetadata destination = new BlockAuthoringMetadata(destinationId, rect);
            destination.Description = source.Description;
            destination.UseCustomTint = source.UseCustomTint;
            destination.Tint = source.Tint;
            Metadata.BlockLayouts.Add(destination);
        }

        private void SelectDuplicateBlocks(IReadOnlyList<BlockDefinition> duplicates)
        {
            Metadata.ClearSelection();
            for (int index = 0; index < duplicates.Count; index++)
            {
                Metadata.SelectedBlockIds.Add(duplicates[index].DefinitionId);
            }

            SetSelectionFallback();
        }

        private void RemoveBlocks(IReadOnlyList<DefinitionId> selected)
        {
            for (int index = Definition.Blocks.Count - 1; index >= 0; index--)
            {
                if (selected.Contains(Definition.Blocks[index].DefinitionId))
                {
                    RemoveBlockAt(index);
                }
            }
        }

        private void MoveActionBetweenTracks(ActionTrackDefinition source, ActionTrackDefinition destination, IAction action, int destinationIndex)
        {
            int sourceIndex = source.ActionList.Actions.IndexOf(action);
            source.ActionList.Actions.RemoveAt(sourceIndex);
            if (source.DefinitionId == destination.DefinitionId &&
                destinationIndex > sourceIndex)
            {
                destinationIndex--;
            }

            int insertIndex = Mathf.Clamp(destinationIndex, 0, destination.ActionList.Actions.Count);
            destination.ActionList.Actions.Insert(insertIndex, action);
            if (source.DefinitionId != destination.DefinitionId)
            {
                RemoveActionFromGroups(action.DefinitionId);
            }

            Metadata.SelectedTrackId = destination.DefinitionId;
        }

        private void RemoveActionFromGroups(DefinitionId actionId)
        {
            for (int index = 0; index < Metadata.ActionGroups.Count; index++)
            {
                Metadata.ActionGroups[index].ActionIds.Remove(actionId);
            }

            Metadata.ActionGroups.RemoveAll(group => group.ActionIds.Count == 0);
        }

        private void RemoveActionWithoutUndo(DefinitionId actionId)
        {
            ActionTrackDefinition track = FindOwningTrack(actionId);
            int index = FindActionIndex(track, actionId);
            if (index >= 0)
            {
                track.ActionList.Actions.RemoveAt(index);
                RemoveActionMetadata(actionId);
            }
        }

        private VariableDefinitionBase RequireVariable(DefinitionId variableId)
        {
            VariableDefinitionBase variable = Definition.Variables.Find(item => item != null && item.DefinitionId == variableId);
            return variable ?? throw new InvalidOperationException($"Blackboard variable '{variableId}' was not found.");
        }

        private int CompareVariablesByName(VariableDefinitionBase left, VariableDefinitionBase right)
        {
            return string.Compare(left?.Key, right?.Key, StringComparison.OrdinalIgnoreCase);
        }

        private int CompareVariablesByType(VariableDefinitionBase left, VariableDefinitionBase right)
        {
            return string.Compare(left?.GetType().Name, right?.GetType().Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
