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
        private const float k_defaultAuthoringWidth = 280f;
        private const float k_defaultInspectorWidth = 280f;
        private const float k_minSidePanelWidth = 240f;
        private const float k_minBoardWidth = 320f;
        private const float k_splitterWidth = 6f;
        private const int k_authoringSplitter = 1;
        private const int k_inspectorSplitter = 2;

        [SerializeField] private Object sourceObject;
        [SerializeField] private string search = string.Empty;
        [SerializeField] private float detailWidth = k_defaultAuthoringWidth;
        [SerializeField] private float inspectorWidth = k_defaultInspectorWidth;
        [SerializeField] private bool showAuthoringPanel = true;
        [SerializeField] private bool showInspectorPanel = true;
        [NonSerialized] private BlackboardAuthoringTarget target;
        [NonSerialized] private BlackboardAuthoringController controller;
        [NonSerialized] private BlackboardAuthoringClipboard clipboard;
        [NonSerialized] private BlackboardGraphCanvas canvas;
        [NonSerialized] private BlackboardDetailPanel detailPanel;
        [NonSerialized] private string resolutionError;

        private readonly BlackboardAuthoringTargetResolver resolver = new BlackboardAuthoringTargetResolver();
        private readonly BlackboardExecutionFeedback feedback = new BlackboardExecutionFeedback();
        private readonly BlackboardEditorExecutionController execution = new BlackboardEditorExecutionController();

        public void SetSource(Object source)
        {
            sourceObject = source;
            ResolveTarget();
            Repaint();
        }

        private void OnEnable()
        {
            minSize = new Vector2(840f, 420f);
            titleContent = new GUIContent("Blackboard", BlackboardEditorStyles.FlowGraph);
            wantsMouseMove = true;
            CreateEditorServices();
            Undo.undoRedoPerformed += HandleUndoRedo;
            Selection.selectionChanged += HandleSelectionChanged;
            EditorApplication.update += HandleEditorUpdate;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
            ResolveTarget();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= HandleUndoRedo;
            Selection.selectionChanged -= HandleSelectionChanged;
            EditorApplication.update -= HandleEditorUpdate;
            EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
            EditorApplication.delayCall -= RebindAfterPlayModeTransition;
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.MouseDown ||
                Event.current.type == EventType.MouseMove ||
                Event.current.type == EventType.MouseLeaveWindow)
            {
                Repaint();
            }

            EnsureEditorServices();
            DrawToolbar();
            if (!EnsureTarget())
            {
                return;
            }

            bool hasValidation = DrawValidation();
            DrawWorkspace(hasValidation);
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            DrawAddBlockButton();
            DrawCanvasButtons();
            DrawBackButton();
            DrawSourceField();
            GUILayout.FlexibleSpace();
            DrawSearchField();
            DrawRuntimeButtons();
            DrawPanelsButton();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAddBlockButton()
        {
            using (new EditorGUI.DisabledScope(controller == null))
            {
                GUIContent content = new GUIContent(BlackboardEditorStyles.Add, "Add Block");
                if (GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(28f)))
                {
                    BlockDefinition block = controller.AddBlock();
                    controller.SetBlockPosition(block.DefinitionId, canvas.GetVisibleGraphCenter());
                }
            }
        }

        private void DrawCanvasButtons()
        {
            using (new EditorGUI.DisabledScope(controller == null))
            {
                if (GUILayout.Button(new GUIContent("Frame", "Frame all Blocks"), EditorStyles.toolbarButton))
                {
                    canvas.FrameAll();
                }

                if (GUILayout.Button(new GUIContent("Layout", "Automatically arrange Blocks"), EditorStyles.toolbarButton))
                {
                    controller.AutoLayout();
                    canvas.FrameAll();
                }
            }
        }

        private void DrawSourceField()
        {
            GUILayout.Space(4f);
            Object selected = EditorGUILayout.ObjectField(sourceObject, typeof(Object), true, GUILayout.MinWidth(160f));
            if (selected != sourceObject)
            {
                SetSource(selected);
            }

            if (target != null)
            {
                GUILayout.Label(target.DisplayName, EditorStyles.miniLabel);
            }
        }

        private void DrawBackButton()
        {
            BlackboardBehaviour behaviour = sourceObject as BlackboardBehaviour;
            bool canGoBack = behaviour != null &&
                behaviour.DefinitionReference.Source == BlackboardDefinitionSource.BlackboardVariable &&
                behaviour.SourceBehaviour != null;
            using (new EditorGUI.DisabledScope(!canGoBack))
            {
                if (GUILayout.Button(new GUIContent("Back", "Open the source Blackboard"), EditorStyles.toolbarButton))
                {
                    SetSource(behaviour.SourceBehaviour);
                }
            }
        }

        private void DrawSearchField()
        {
            string nextSearch = GUILayout.TextField(search, EditorStyles.toolbarSearchField, GUILayout.Width(180f));
            if (!string.Equals(nextSearch, search, StringComparison.Ordinal))
            {
                search = nextSearch;
                Repaint();
            }

            using (new EditorGUI.DisabledScope(controller == null || string.IsNullOrWhiteSpace(search)))
            {
                if (GUILayout.Button(new GUIContent("Focus", "Focus the first matching Block or Action"), EditorStyles.toolbarButton))
                {
                    canvas.FocusFirstMatch(search);
                }
            }
        }

        private void DrawRuntimeButtons()
        {
            BlackboardBehaviour behaviour = GetSourceBehaviour();
            bool canControl = execution.CanControl(behaviour, out string reason);
            bool canPlayFromStart =
                controller != null &&
                controller.GetBlock(controller.Metadata.SelectedBlockId) != null;
            bool canPlayFromSelected =
                BlackboardEditorExecutionController
                    .TryResolveSelectedActionStart(
                        controller,
                        out _,
                        out _);
            using (new EditorGUI.DisabledScope(
                (!canPlayFromStart && !canPlayFromSelected)))
            {
                GUIContent play = new GUIContent(
                    BlackboardEditorStyles.Play,
                    "Choose where to start the selected Block");
                if (GUILayout.Button(
                    play,
                    EditorStyles.toolbarDropDown,
                    GUILayout.Width(34f)))
                {
                    ShowPlayMenu(
                        behaviour,
                        GUILayoutUtility.GetLastRect(),
                        canControl,
                        canPlayFromStart,
                        canPlayFromSelected);
                }
            }

            using (new EditorGUI.DisabledScope(!canControl || controller == null))
            {
                if (GUILayout.Button(new GUIContent("■", "Stop selected Block"), EditorStyles.toolbarButton, GUILayout.Width(28f)))
                {
                    execution.Stop(behaviour, controller.Metadata.SelectedBlockId);
                }

                if (GUILayout.Button(new GUIContent("Stop All", "Stop every running Block"), EditorStyles.toolbarButton))
                {
                    execution.StopAll(behaviour);
                }
            }

            if (!canControl && Event.current.type == EventType.Repaint)
            {
                GUIContent tooltip = new GUIContent(string.Empty, reason);
                GUI.Label(GUILayoutUtility.GetLastRect(), tooltip);
            }
        }

        private void ShowPlayMenu(
            BlackboardBehaviour behaviour,
            Rect anchor,
            bool canControl,
            bool canPlayFromStart,
            bool canPlayFromSelected)
        {
            GenericMenu menu = new GenericMenu();
            if (canControl && canPlayFromStart)
            {
                DefinitionId blockId = controller.Metadata.SelectedBlockId;
                menu.AddItem(
                    new GUIContent("Play From Start"),
                    false,
                    () => execution.Execute(behaviour, blockId));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Play From Start"));
            }

            if (canControl &&
                canPlayFromSelected &&
                BlackboardEditorExecutionController
                    .TryResolveSelectedActionStart(
                        controller,
                        out DefinitionId selectedBlockId,
                        out int selectedTaskIndex))
            {
                menu.AddItem(
                    new GUIContent("Play From Selected"),
                    false,
                    () => execution.ExecuteFromAction(
                        behaviour,
                        selectedBlockId,
                        selectedTaskIndex));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Play From Selected"));
            }

            menu.DropDown(anchor);
        }

        private void ShowVariablesPanel()
        {
            if (detailPanel == null)
            {
                return;
            }

            showAuthoringPanel = true;
            detailPanel.ShowVariables();
            Repaint();
        }

        private void DrawPanelsButton()
        {
            if (!GUILayout.Button(
                    "Panels",
                    EditorStyles.toolbarDropDown))
            {
                return;
            }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(
                new GUIContent("Authoring"),
                showAuthoringPanel,
                ToggleAuthoringPanel);
            menu.AddItem(
                new GUIContent("Action Inspector"),
                showInspectorPanel,
                ToggleInspectorPanel);
            menu.AddSeparator(string.Empty);
            menu.AddItem(
                new GUIContent("Show Variables"),
                false,
                ShowVariablesPanel);
            menu.AddItem(
                new GUIContent("Reset Layout"),
                false,
                ResetPanelLayout);
            menu.DropDown(GUILayoutUtility.GetLastRect());
        }

        private void ToggleAuthoringPanel()
        {
            showAuthoringPanel = !showAuthoringPanel;
            Repaint();
        }

        private void ToggleInspectorPanel()
        {
            showInspectorPanel = !showInspectorPanel;
            Repaint();
        }

        private void ResetPanelLayout()
        {
            detailWidth = k_defaultAuthoringWidth;
            inspectorWidth = k_defaultInspectorWidth;
            showAuthoringPanel = true;
            showInspectorPanel = true;
            Repaint();
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

        private bool DrawValidation()
        {
            IReadOnlyList<BlackboardValidationIssue> issues = controller.Validate();
            if (issues.Count == 0)
            {
                return false;
            }

            string message = issues.Count == 1
                ? issues[0].ToString()
                : $"{issues.Count} validation issues. {issues[0]}";
            EditorGUILayout.HelpBox(message, MessageType.Error);
            return true;
        }

        private void DrawWorkspace(bool hasValidation)
        {
            float toolbarHeight =
                EditorGUIUtility.singleLineHeight + 2f;
            float validationHeight = hasValidation
                ? (EditorGUIUtility.singleLineHeight * 2f) + 6f
                : 0f;
            Rect workspace = new Rect(
                0f,
                toolbarHeight + validationHeight,
                position.width,
                Mathf.Max(
                    0f,
                    position.height -
                    toolbarHeight -
                    validationHeight));
            CalculateWorkspaceRects(
                workspace,
                detailWidth,
                inspectorWidth,
                showAuthoringPanel,
                showInspectorPanel,
                out Rect authoring,
                out Rect board,
                out Rect inspector);
            UpdateVisiblePanelWidths(authoring, inspector);

            if (showAuthoringPanel)
            {
                DrawPanel(
                    authoring,
                    () => detailPanel.DrawAuthoring(
                        controller,
                        GetSourceBehaviour()));
            }

            if (showInspectorPanel)
            {
                DrawPanel(
                    inspector,
                    () => detailPanel.DrawInspector(controller));
            }

            canvas.Draw(
                board,
                controller,
                GetSourceBehaviour(),
                feedback,
                search);
            DrawWorkspaceSplitters(
                workspace,
                authoring,
                inspector);
        }

        private void UpdateVisiblePanelWidths(
            Rect authoring,
            Rect inspector)
        {
            if (showAuthoringPanel)
            {
                detailWidth = authoring.width;
            }

            if (showInspectorPanel)
            {
                inspectorWidth = inspector.width;
            }
        }

        private void DrawWorkspaceSplitters(
            Rect workspace,
            Rect authoring,
            Rect inspector)
        {
            if (showAuthoringPanel)
            {
                Rect splitter = CreateSplitterRect(
                    authoring.xMax,
                    workspace);
                DrawWorkspaceSplitter(
                    workspace,
                    splitter,
                    k_authoringSplitter);
            }

            if (showInspectorPanel)
            {
                Rect splitter = CreateSplitterRect(
                    inspector.xMin,
                    workspace);
                DrawWorkspaceSplitter(
                    workspace,
                    splitter,
                    k_inspectorSplitter);
            }
        }

        private Rect CreateSplitterRect(float x, Rect workspace)
        {
            return new Rect(
                x - (k_splitterWidth * 0.5f),
                workspace.y,
                k_splitterWidth,
                workspace.height);
        }

        private void DrawWorkspaceSplitter(
            Rect workspace,
            Rect splitter,
            int splitterId)
        {
            int controlId = GUIUtility.GetControlID(
                splitterId,
                FocusType.Passive,
                splitter);
            EditorGUIUtility.AddCursorRect(
                splitter,
                MouseCursor.ResizeHorizontal,
                controlId);
            HandleWorkspaceSplitterEvent(
                workspace,
                splitter,
                splitterId,
                controlId);
            DrawWorkspaceSplitterVisual(splitter, controlId);
        }

        private void HandleWorkspaceSplitterEvent(
            Rect workspace,
            Rect splitter,
            int splitterId,
            int controlId)
        {
            Event current = Event.current;
            EventType type = current.GetTypeForControl(controlId);
            if (type == EventType.MouseDown &&
                current.button == 0 &&
                splitter.Contains(current.mousePosition))
            {
                GUIUtility.hotControl = controlId;
                current.Use();
                return;
            }

            if (type == EventType.MouseDrag &&
                GUIUtility.hotControl == controlId)
            {
                ResizePanelFromPointer(
                    workspace,
                    current.mousePosition.x,
                    splitterId);
                current.Use();
                Repaint();
                return;
            }

            if (type == EventType.MouseUp &&
                GUIUtility.hotControl == controlId)
            {
                GUIUtility.hotControl = 0;
                current.Use();
            }
        }

        private void ResizePanelFromPointer(
            Rect workspace,
            float pointerX,
            int splitterId)
        {
            if (splitterId == k_authoringSplitter)
            {
                float requested = pointerX - workspace.x;
                detailWidth = ClampDraggedPanelWidth(
                    requested,
                    workspace.width,
                    showInspectorPanel ? inspectorWidth : 0f);
                return;
            }

            float inspectorRequested = workspace.xMax - pointerX;
            inspectorWidth = ClampDraggedPanelWidth(
                inspectorRequested,
                workspace.width,
                showAuthoringPanel ? detailWidth : 0f);
        }

        private static float ClampDraggedPanelWidth(
            float requestedWidth,
            float workspaceWidth,
            float otherPanelWidth)
        {
            float maximum = Mathf.Max(
                0f,
                workspaceWidth -
                k_minBoardWidth -
                otherPanelWidth);
            float minimum = Mathf.Min(
                k_minSidePanelWidth,
                maximum);
            return Mathf.Clamp(
                requestedWidth,
                minimum,
                maximum);
        }

        private void DrawWorkspaceSplitterVisual(
            Rect splitter,
            int controlId)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            bool active = GUIUtility.hotControl == controlId;
            bool hovered = splitter.Contains(Event.current.mousePosition);
            Color color = active || hovered
                ? new Color(0.20f, 0.55f, 0.90f, 0.85f)
                : new Color(0f, 0f, 0f, 0.28f);
            EditorGUI.DrawRect(splitter, color);
        }

        private static void DrawPanel(
            Rect panel,
            Action drawContent)
        {
            GUILayout.BeginArea(
                panel,
                EditorStyles.helpBox);
            try
            {
                drawContent();
            }
            finally
            {
                GUILayout.EndArea();
            }
        }

        public static void CalculateWorkspaceRects(
            Rect workspace,
            float requestedSideWidth,
            out Rect authoring,
            out Rect board,
            out Rect inspector)
        {
            CalculateWorkspaceRects(
                workspace,
                requestedSideWidth,
                requestedSideWidth,
                true,
                true,
                out authoring,
                out board,
                out inspector);
        }

        public static void CalculateWorkspaceRects(
            Rect workspace,
            float requestedAuthoringWidth,
            float requestedInspectorWidth,
            bool showAuthoring,
            bool showInspector,
            out Rect authoring,
            out Rect board,
            out Rect inspector)
        {
            CalculateSidePanelWidths(
                workspace.width,
                requestedAuthoringWidth,
                requestedInspectorWidth,
                showAuthoring,
                showInspector,
                out float authoringWidth,
                out float inspectorPanelWidth);
            authoring = new Rect(
                workspace.x,
                workspace.y,
                authoringWidth,
                workspace.height);
            inspector = new Rect(
                workspace.xMax - inspectorPanelWidth,
                workspace.y,
                inspectorPanelWidth,
                workspace.height);
            board = new Rect(
                authoring.xMax,
                workspace.y,
                Mathf.Max(
                    0f,
                    inspector.xMin - authoring.xMax),
                workspace.height);
        }

        private static void CalculateSidePanelWidths(
            float workspaceWidth,
            float requestedAuthoringWidth,
            float requestedInspectorWidth,
            bool showAuthoring,
            bool showInspector,
            out float authoringWidth,
            out float inspectorPanelWidth)
        {
            float available = Mathf.Max(
                0f,
                workspaceWidth - k_minBoardWidth);
            int visibleCount =
                (showAuthoring ? 1 : 0) +
                (showInspector ? 1 : 0);
            float minimum = visibleCount == 0
                ? 0f
                : Mathf.Min(
                    k_minSidePanelWidth,
                    available / visibleCount);
            authoringWidth = showAuthoring
                ? Mathf.Max(minimum, requestedAuthoringWidth)
                : 0f;
            inspectorPanelWidth = showInspector
                ? Mathf.Max(minimum, requestedInspectorWidth)
                : 0f;
            FitSidePanelWidths(
                available,
                minimum,
                ref authoringWidth,
                ref inspectorPanelWidth);
        }

        private static void FitSidePanelWidths(
            float available,
            float minimum,
            ref float authoringWidth,
            ref float inspectorPanelWidth)
        {
            float requestedTotal =
                authoringWidth + inspectorPanelWidth;
            if (requestedTotal <= available)
            {
                return;
            }

            float flexibleTotal =
                (authoringWidth - minimum) +
                (inspectorPanelWidth - minimum);
            float overflow = requestedTotal - available;
            if (flexibleTotal <= 0f)
            {
                authoringWidth = Mathf.Min(authoringWidth, available);
                inspectorPanelWidth = Mathf.Max(
                    0f,
                    available - authoringWidth);
                return;
            }

            authoringWidth -= overflow *
                ((authoringWidth - minimum) / flexibleTotal);
            inspectorPanelWidth = Mathf.Max(
                0f,
                available - authoringWidth);
        }

        public static float ClampSidePanelWidth(
            float requestedWidth,
            float windowWidth)
        {
            float maximum = Mathf.Max(
                0f,
                (windowWidth - k_minBoardWidth) * 0.5f);
            float minimum = Mathf.Min(
                k_minSidePanelWidth,
                maximum);
            return Mathf.Clamp(
                requestedWidth,
                minimum,
                maximum);
        }

        private BlackboardBehaviour GetSourceBehaviour()
        {
            return sourceObject as BlackboardBehaviour;
        }

        private void CreateEditorServices()
        {
            clipboard = new BlackboardAuthoringClipboard(new SerializedGraphCloner(), new DefinitionIdRegenerator());
            canvas = new BlackboardGraphCanvas();
            detailPanel = new BlackboardDetailPanel();
        }

        private void EnsureEditorServices()
        {
            if (clipboard == null || canvas == null || detailPanel == null)
            {
                CreateEditorServices();
                ResolveTarget();
            }
        }

        private void ResolveTarget()
        {
            target = null;
            controller = null;
            resolutionError = null;
            if (sourceObject == null || clipboard == null)
            {
                return;
            }

            try
            {
                target = resolver.Resolve(sourceObject);
                controller = new BlackboardAuthoringController(target, clipboard);
                controller.SynchronizeBlockSelection();
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

        public static bool RequiresTargetRebind(PlayModeStateChange state)
        {
            return state == PlayModeStateChange.EnteredPlayMode ||
                state == PlayModeStateChange.EnteredEditMode;
        }

        private void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            target = null;
            controller = null;
            resolutionError = null;
            EditorApplication.delayCall -= RebindAfterPlayModeTransition;
            if (RequiresTargetRebind(state))
            {
                EditorApplication.delayCall += RebindAfterPlayModeTransition;
            }

            Repaint();
        }

        private void RebindAfterPlayModeTransition()
        {
            EditorApplication.delayCall -= RebindAfterPlayModeTransition;
            if (sourceObject == null)
            {
                sourceObject = FindSelectedBlackboardSource();
            }

            ResolveTarget();
            Repaint();
        }

        private Object FindSelectedBlackboardSource()
        {
            if (Selection.activeObject is BlackboardDefinitionAsset ||
                Selection.activeObject is BlackboardBehaviour)
            {
                return Selection.activeObject;
            }

            return Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponent<BlackboardBehaviour>()
                : null;
        }
    }
}
