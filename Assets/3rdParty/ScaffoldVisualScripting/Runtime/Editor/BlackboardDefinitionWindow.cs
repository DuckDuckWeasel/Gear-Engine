using System;
using System.Collections.Generic;
using Scaffold.VisualScripting.Authoring;
using Scaffold.VisualScripting.Unity;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scaffold.VisualScripting.Editor
{
    public sealed class BlackboardDefinitionWindow : EditorWindow
    {
        [SerializeField] private Object sourceObject;
        [SerializeField] private string search = string.Empty;
        [NonSerialized] private BlackboardAuthoringTarget target;
        [NonSerialized] private BlackboardAuthoringController controller;
        [NonSerialized] private BlackboardAuthoringClipboard clipboard;
        [NonSerialized] private string resolutionError;
        private readonly BlackboardAuthoringTargetResolver resolver = new BlackboardAuthoringTargetResolver();
        private readonly BlackboardExecutionFeedback feedback = new BlackboardExecutionFeedback();

        public void SetSource(Object source)
        {
            sourceObject = source;
            ResolveTarget();
            Repaint();
        }

        private void OnEnable()
        {
            CreateClipboard();
            Undo.undoRedoPerformed += HandleUndoRedo;
            Selection.selectionChanged += HandleSelectionChanged;
            EditorApplication.update += HandleEditorUpdate;
            ResolveTarget();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= HandleUndoRedo;
            Selection.selectionChanged -= HandleSelectionChanged;
            EditorApplication.update -= HandleEditorUpdate;
        }

        private void OnGUI()
        {
            DrawToolbar();
            if (!EnsureTarget())
            {
                return;
            }

            DrawValidation();
            target.Metadata.ScrollPosition = EditorGUILayout.BeginScrollView(target.Metadata.ScrollPosition);
            DrawDefinitionHeader();
            DrawBlocks();
            DrawVariables();
            DrawSerializedDetails();
            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            Object selected = EditorGUILayout.ObjectField(sourceObject, typeof(Object), true);
            if (selected != sourceObject)
            {
                SetSource(selected);
            }

            search = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.MinWidth(140f));
            EditorGUILayout.EndHorizontal();
        }

        private bool EnsureTarget()
        {
            if (target != null && controller != null)
            {
                return true;
            }

            EditorGUILayout.HelpBox(resolutionError ?? "Select a BlackboardBehaviour or BlackboardDefinitionAsset.", MessageType.Info);
            return false;
        }

        private void DrawValidation()
        {
            IReadOnlyList<BlackboardValidationIssue> issues = controller.Validate();
            for (int index = 0; index < issues.Count; index++)
            {
                EditorGUILayout.HelpBox(issues[index].ToString(), MessageType.Error);
            }
        }

        private void DrawDefinitionHeader()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(target.DisplayName, EditorStyles.boldLabel);
            string name = EditorGUILayout.TextField("Definition Name", controller.Definition.Name);
            if (!string.Equals(name, controller.Definition.Name, StringComparison.Ordinal))
            {
                ApplyChange("Rename Blackboard Definition", () => controller.Definition.Name = name);
            }

            DrawDefinitionActions();
        }

        private void DrawDefinitionActions()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Block"))
            {
                controller.AddBlock();
            }

            if (GUILayout.Button("Auto Layout"))
            {
                controller.AutoLayout();
            }

            DrawPasteBlockButton();
            DrawGroupButton();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPasteBlockButton()
        {
            using (new EditorGUI.DisabledScope(!clipboard.HasBlock))
            {
                if (GUILayout.Button("Paste Block"))
                {
                    controller.PasteBlock();
                }
            }
        }

        private void DrawGroupButton()
        {
            bool canGroup = !controller.Metadata.SelectedTrackId.IsEmpty && controller.Metadata.SelectedActionIds.Count > 0;
            using (new EditorGUI.DisabledScope(!canGroup))
            {
                if (GUILayout.Button("Group Selected"))
                {
                    controller.GroupActions(controller.Metadata.SelectedTrackId, controller.Metadata.SelectedActionIds, "Group");
                }
            }
        }

        private void DrawBlocks()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Blocks", EditorStyles.boldLabel);
            for (int index = 0; index < controller.Definition.Blocks.Count; index++)
            {
                BlockDefinition block = controller.Definition.Blocks[index];
                if (block != null && MatchesSearch(block.Name))
                {
                    DrawBlock(block, index);
                }
            }
        }

        private void DrawBlock(BlockDefinition block, int index)
        {
            BlockAuthoringMetadata layout = FindLayout(block.DefinitionId);
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = layout != null && layout.UseCustomTint ? layout.Tint : originalColor;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = originalColor;
            DrawBlockHeader(block, index);
            DrawBlockTrigger(block);
            DrawTracks(block);
            DrawBlockGroups(block);
            EditorGUILayout.EndVertical();
        }

        private void DrawBlockHeader(BlockDefinition block, int index)
        {
            EditorGUILayout.BeginHorizontal();
            string name = EditorGUILayout.TextField(block.Name);
            if (!string.Equals(name, block.Name, StringComparison.Ordinal))
            {
                ApplyChange("Rename Blackboard Block", () => block.Name = name);
            }

            DrawBlockStatus(block);
            DrawBlockButtons(block, index);
            EditorGUILayout.EndHorizontal();
            DrawBlockLayout(block);
        }

        private void DrawBlockStatus(BlockDefinition block)
        {
            if (sourceObject is BlackboardBehaviour behaviour && feedback.TryGetBlockState(behaviour, block.DefinitionId, out BlockExecutionState state))
            {
                GUILayout.Label(state.ToString(), EditorStyles.miniLabel, GUILayout.Width(70f));
            }
        }

        private void DrawBlockButtons(BlockDefinition block, int index)
        {
            if (GUILayout.Button("Copy", GUILayout.Width(48f)))
            {
                controller.CopyBlock(block.DefinitionId);
            }

            if (GUILayout.Button("Dup", GUILayout.Width(40f)))
            {
                controller.DuplicateBlock(block.DefinitionId);
            }

            DrawMoveBlockButtons(block, index);
            if (GUILayout.Button("X", GUILayout.Width(24f)))
            {
                controller.RemoveBlock(block.DefinitionId);
                GUIUtility.ExitGUI();
            }
        }

        private void DrawMoveBlockButtons(BlockDefinition block, int index)
        {
            using (new EditorGUI.DisabledScope(index == 0))
            {
                if (GUILayout.Button("↑", GUILayout.Width(24f)))
                {
                    controller.MoveBlock(block.DefinitionId, index - 1);
                }
            }

            using (new EditorGUI.DisabledScope(index + 1 >= controller.Definition.Blocks.Count))
            {
                if (GUILayout.Button("↓", GUILayout.Width(24f)))
                {
                    controller.MoveBlock(block.DefinitionId, index + 1);
                }
            }
        }

        private void DrawBlockLayout(BlockDefinition block)
        {
            BlockAuthoringMetadata layout = FindLayout(block.DefinitionId);
            if (layout == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();
            bool customTint = EditorGUILayout.ToggleLeft("Tint", layout.UseCustomTint, GUILayout.Width(52f));
            Color tint = EditorGUILayout.ColorField(layout.Tint);
            GUILayout.Label($"Graph: {layout.Position.position}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            if (customTint != layout.UseCustomTint || tint != layout.Tint)
            {
                controller.SetBlockTint(block.DefinitionId, customTint, tint);
            }
        }

        private void DrawBlockTrigger(BlockDefinition block)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(block.Trigger?.GetType().Name ?? "No Trigger", GUILayout.MinWidth(120f));
            if (GUILayout.Button("Set Trigger", GUILayout.Width(90f)))
            {
                ShowTypeMenu(BlackboardManagedTypeCatalog.GetTriggerTypes(search), type => controller.SetTrigger(block.DefinitionId, type));
            }

            using (new EditorGUI.DisabledScope(block.Trigger == null))
            {
                if (GUILayout.Button("Clear", GUILayout.Width(50f)))
                {
                    controller.ClearTrigger(block.DefinitionId);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawTracks(BlockDefinition block)
        {
            for (int index = 0; index < block.Tracks.Count; index++)
            {
                ActionTrackDefinition track = block.Tracks[index];
                if (track != null)
                {
                    DrawTrack(block, track, index);
                }
            }

            if (GUILayout.Button("Add Track"))
            {
                controller.AddTrack(block.DefinitionId);
            }
        }

        private void DrawTrack(BlockDefinition block, ActionTrackDefinition track, int index)
        {
            EditorGUILayout.BeginVertical("box");
            DrawTrackHeader(block, track, index);
            for (int actionIndex = 0; actionIndex < track.ActionList.Actions.Count; actionIndex++)
            {
                IAction action = track.ActionList.Actions[actionIndex];
                if (action != null && MatchesSearch(action.GetType().Name))
                {
                    DrawAction(track, action, actionIndex);
                }
            }

            DrawTrackActions(track);
            EditorGUILayout.EndVertical();
        }

        private void DrawTrackHeader(BlockDefinition block, ActionTrackDefinition track, int index)
        {
            EditorGUILayout.BeginHorizontal();
            string name = EditorGUILayout.TextField(track.Name);
            if (!string.Equals(name, track.Name, StringComparison.Ordinal))
            {
                ApplyChange("Rename Blackboard Track", () => track.Name = name);
            }

            DrawMoveTrackButtons(block, track, index);
            if (GUILayout.Button("X", GUILayout.Width(24f)))
            {
                controller.RemoveTrack(track.DefinitionId);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawMoveTrackButtons(BlockDefinition block, ActionTrackDefinition track, int index)
        {
            using (new EditorGUI.DisabledScope(index == 0))
            {
                if (GUILayout.Button("↑", GUILayout.Width(24f)))
                {
                    controller.MoveTrack(track.DefinitionId, index - 1);
                }
            }

            using (new EditorGUI.DisabledScope(index + 1 >= block.Tracks.Count))
            {
                if (GUILayout.Button("↓", GUILayout.Width(24f)))
                {
                    controller.MoveTrack(track.DefinitionId, index + 1);
                }
            }
        }

        private void DrawAction(ActionTrackDefinition track, IAction action, int index)
        {
            EditorGUILayout.BeginHorizontal();
            bool selected = controller.Metadata.SelectedActionIds.Contains(action.DefinitionId);
            bool requested = GUILayout.Toggle(selected, GUIContent.none, GUILayout.Width(18f));
            if (selected != requested)
            {
                ToggleActionSelection(track.DefinitionId, action.DefinitionId);
            }

            GUILayout.Label(action.GetType().Name, GUILayout.MinWidth(130f));
            DrawActionStatus(action);
            DrawActionButtons(track, action, index);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawActionStatus(IAction action)
        {
            if (sourceObject is BlackboardBehaviour behaviour && feedback.TryGetActionStatus(behaviour, action.DefinitionId, out ActionExecutionStatus status))
            {
                GUILayout.Label(status.ToString(), EditorStyles.miniLabel, GUILayout.Width(80f));
            }
        }

        private void DrawActionButtons(ActionTrackDefinition track, IAction action, int index)
        {
            if (GUILayout.Button("Copy", GUILayout.Width(42f)))
            {
                controller.CopyAction(action.DefinitionId);
            }

            if (GUILayout.Button("Dup", GUILayout.Width(38f)))
            {
                controller.DuplicateAction(action.DefinitionId);
            }

            DrawMoveActionButtons(track, action, index);
            if (GUILayout.Button("X", GUILayout.Width(24f)))
            {
                controller.RemoveAction(action.DefinitionId);
                GUIUtility.ExitGUI();
            }
        }

        private void DrawMoveActionButtons(ActionTrackDefinition track, IAction action, int index)
        {
            using (new EditorGUI.DisabledScope(index == 0))
            {
                if (GUILayout.Button("↑", GUILayout.Width(24f)))
                {
                    controller.MoveAction(action.DefinitionId, index - 1);
                }
            }

            using (new EditorGUI.DisabledScope(index + 1 >= track.ActionList.Actions.Count))
            {
                if (GUILayout.Button("↓", GUILayout.Width(24f)))
                {
                    controller.MoveAction(action.DefinitionId, index + 1);
                }
            }
        }

        private void DrawTrackActions(ActionTrackDefinition track)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Action"))
            {
                ShowTypeMenu(BlackboardManagedTypeCatalog.GetActionTypes(search), type => controller.AddAction(track.DefinitionId, type));
            }

            using (new EditorGUI.DisabledScope(!clipboard.HasAction))
            {
                if (GUILayout.Button("Paste Action"))
                {
                    controller.PasteAction(track.DefinitionId);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawBlockGroups(BlockDefinition block)
        {
            foreach (ActionGroupAuthoringMetadata group in controller.Metadata.ActionGroups.ToArray())
            {
                if (block.Tracks.Exists(track => track.DefinitionId == group.TrackId))
                {
                    DrawGroup(group);
                }
            }
        }

        private void DrawGroup(ActionGroupAuthoringMetadata group)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Group: {group.Name} ({group.ActionIds.Count})", EditorStyles.miniLabel);
            if (GUILayout.Button("Ungroup", GUILayout.Width(70f)))
            {
                controller.UngroupActions(group.GroupId);
                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawVariables()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Variables", EditorStyles.boldLabel);
            for (int index = 0; index < controller.Definition.Variables.Count; index++)
            {
                VariableDefinitionBase variable = controller.Definition.Variables[index];
                if (variable != null && MatchesSearch(variable.Key))
                {
                    DrawVariable(variable, index);
                }
            }

            if (GUILayout.Button("Add Variable"))
            {
                ShowTypeMenu(BlackboardManagedTypeCatalog.GetVariableTypes(search), type => controller.AddVariable(type));
            }
        }

        private void DrawVariable(VariableDefinitionBase variable, int index)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(variable.GetType().Name, EditorStyles.miniLabel, GUILayout.Width(150f));
            string key = EditorGUILayout.TextField(variable.Key);
            if (!string.Equals(key, variable.Key, StringComparison.Ordinal))
            {
                ApplyChange("Rename Blackboard Variable", () => variable.Key = key);
            }

            VariableScope scope = (VariableScope)EditorGUILayout.EnumPopup(variable.Scope, GUILayout.Width(90f));
            if (scope != variable.Scope)
            {
                ApplyChange("Set Blackboard Variable Scope", () => variable.Scope = scope);
            }

            DrawVariableButtons(variable, index);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawVariableButtons(VariableDefinitionBase variable, int index)
        {
            using (new EditorGUI.DisabledScope(index == 0))
            {
                if (GUILayout.Button("↑", GUILayout.Width(24f)))
                {
                    controller.MoveVariable(variable.DefinitionId, index - 1);
                }
            }

            using (new EditorGUI.DisabledScope(index + 1 >= controller.Definition.Variables.Count))
            {
                if (GUILayout.Button("↓", GUILayout.Width(24f)))
                {
                    controller.MoveVariable(variable.DefinitionId, index + 1);
                }
            }

            if (GUILayout.Button("X", GUILayout.Width(24f)))
            {
                controller.RemoveVariable(variable.DefinitionId);
                GUIUtility.ExitGUI();
            }
        }

        private void DrawSerializedDetails()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Serialized Details", EditorStyles.boldLabel);
            SerializedObject serialized = new SerializedObject(target.Owner);
            serialized.Update();
            DrawOwnerProperties(serialized);
            serialized.ApplyModifiedProperties();
        }

        private void DrawOwnerProperties(SerializedObject serialized)
        {
            if (target.Owner is BlackboardDefinitionAsset)
            {
                EditorGUILayout.PropertyField(serialized.FindProperty("definition"), true);
                EditorGUILayout.PropertyField(serialized.FindProperty("authoringMetadata"), true);
                return;
            }

            EditorGUILayout.PropertyField(serialized.FindProperty("definitionReference"), true);
            EditorGUILayout.PropertyField(serialized.FindProperty("sourceBehaviour"));
            EditorGUILayout.PropertyField(serialized.FindProperty("authoringMetadata"), true);
        }

        private void ToggleActionSelection(DefinitionId trackId, DefinitionId actionId)
        {
            ApplyChange("Select Blackboard Action", () =>
            {
                controller.Metadata.SelectedTrackId = trackId;
                if (!controller.Metadata.SelectedActionIds.Remove(actionId))
                {
                    controller.Metadata.SelectedActionIds.Add(actionId);
                }
            });
        }

        private void ShowTypeMenu(IReadOnlyList<Type> types, Action<Type> onSelected)
        {
            GenericMenu menu = new GenericMenu();
            for (int index = 0; index < types.Count; index++)
            {
                Type type = types[index];
                menu.AddItem(new GUIContent(type.FullName), false, () => onSelected(type));
            }

            if (types.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No matching types"));
            }

            menu.ShowAsContext();
        }

        private BlockAuthoringMetadata FindLayout(DefinitionId blockId)
        {
            return controller.Metadata.BlockLayouts.Find(layout => layout.BlockId == blockId);
        }

        private bool MatchesSearch(string value)
        {
            return string.IsNullOrWhiteSpace(search) || (value ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void ApplyChange(string label, Action change)
        {
            Undo.RegisterCompleteObjectUndo(target.Owner, label);
            change.Invoke();
            EditorUtility.SetDirty(target.Owner);
            if (target.Owner is Component component)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            }
        }

        private void CreateClipboard()
        {
            clipboard = new BlackboardAuthoringClipboard(new SerializedGraphCloner(), new DefinitionIdRegenerator());
        }

        private void ResolveTarget()
        {
            target = null;
            controller = null;
            resolutionError = null;
            if (sourceObject == null)
            {
                return;
            }

            TryResolveTarget();
        }

        private void TryResolveTarget()
        {
            try
            {
                target = resolver.Resolve(sourceObject);
                controller = new BlackboardAuthoringController(target, clipboard);
            }
            catch (Exception exception)
            {
                resolutionError = exception.Message;
            }
        }

        private void HandleUndoRedo()
        {
            ResolveTarget();
            Repaint();
        }

        private void HandleSelectionChanged()
        {
            if (Selection.activeObject is BlackboardDefinitionAsset || Selection.activeObject is BlackboardBehaviour)
            {
                SetSource(Selection.activeObject);
            }
        }

        private void HandleEditorUpdate()
        {
            if (EditorApplication.isPlaying)
            {
                Repaint();
            }
        }
    }
}
