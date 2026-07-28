using System;
using System.Collections.Generic;
using Scaffold;
using Scaffold.EditorUtils;
using Scaffold.VisualScripting.Authoring;
using Scaffold.VisualScripting.Unity;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    public sealed class BlackboardGraphCanvas
    {
        private const float k_gridSpacing = 120f;
        private const float k_gridSnap = 20f;
        private const float k_minNodeWidth = 60f;
        private const float k_maxNodeWidth = 240f;
        private const float k_nodeHeight = 40f;
        private const float k_contextTolerance = 5f;
        private static readonly Vector3[] s_arrowPoints = new Vector3[3];
        private static readonly Vector2[] s_sourceConnectionPoints = new Vector2[4];
        private static readonly Vector2[] s_destinationConnectionPoints = new Vector2[4];
        private static readonly int[] s_destinationPointIndices = { 3, 2, 1, 0 };

        private readonly BlackboardGraphConnectionResolver connectionResolver = new BlackboardGraphConnectionResolver();
        private readonly BlackboardEditorExecutionController execution = new BlackboardEditorExecutionController();
        private IReadOnlyList<BlackboardGraphConnection> connections = Array.Empty<BlackboardGraphConnection>();
        private GUIStyle nodeLabel;
        private GUIStyle triggerLabel;
        private BlackboardAuthoringController controller;
        private BlackboardBehaviour behaviour;
        private BlackboardExecutionFeedback feedback;
        private string search = string.Empty;
        private Rect viewRect;
        private Vector2 lastMousePosition;
        private Vector2 selectionStart;
        private Vector2 selectionEnd;
        private Vector2 rightClickStart;
        private bool movingBlocks;
        private bool selecting;
        private bool panning;
        private bool rightClickPending;

        public void Draw(Rect position, BlackboardAuthoringController authoringController, BlackboardBehaviour sourceBehaviour, BlackboardExecutionFeedback executionFeedback, string searchText)
        {
            controller = authoringController ?? throw new ArgumentNullException(nameof(authoringController));
            behaviour = sourceBehaviour;
            feedback = executionFeedback ?? throw new ArgumentNullException(nameof(executionFeedback));
            search = searchText ?? string.Empty;
            viewRect = new Rect(Vector2.zero, position.size);
            EnsureStyles();
            controller.SynchronizeBlockSelection();
            connections = connectionResolver.Resolve(controller.Definition);
            GUI.BeginGroup(position);
            DrawBackground();
            HandleEvent(Event.current);
            DrawGrid();
            DrawConnections();
            DrawNodes();
            DrawSelectionBox();
            GUI.EndGroup();
        }

        private void EnsureStyles()
        {
            nodeLabel ??= BlackboardEditorStyles.NodeLabel();
            triggerLabel ??= BlackboardEditorStyles.TriggerLabel();
        }

        public void FrameAll()
        {
            if (controller == null || controller.Definition.Blocks.Count == 0)
            {
                return;
            }

            Rect bounds = GetGraphBounds();
            Vector2 center = viewRect.center - (bounds.center * controller.Metadata.Zoom);
            controller.SetViewport(center, controller.Metadata.Zoom);
        }

        public void FocusBlock(DefinitionId blockId)
        {
            if (controller == null || controller.GetBlock(blockId) == null)
            {
                return;
            }

            Rect graphRect = GetGraphNodeRect(blockId);
            Vector2 center = viewRect.center - (graphRect.center * controller.Metadata.Zoom);
            controller.SetViewport(center, controller.Metadata.Zoom);
            controller.SelectOnlyBlock(blockId);
        }

        public Vector2 GetVisibleGraphCenter()
        {
            if (controller == null)
            {
                return Vector2.zero;
            }

            return ScreenToGraph(viewRect.center);
        }

        public bool FocusFirstMatch(string searchText)
        {
            if (controller == null || string.IsNullOrWhiteSpace(searchText))
            {
                return false;
            }

            string previousSearch = search;
            search = searchText;
            for (int index = 0; index < controller.Definition.Blocks.Count; index++)
            {
                BlockDefinition block = controller.Definition.Blocks[index];
                if (block != null && MatchesSearch(block))
                {
                    FocusBlock(block.DefinitionId);
                    search = previousSearch;
                    return true;
                }
            }

            search = previousSearch;
            return false;
        }

        private void DrawBackground()
        {
            EditorGUI.DrawRect(viewRect, BlackboardEditorStyles.CanvasColor());
        }

        private void HandleEvent(Event current)
        {
            if (!viewRect.Contains(current.mousePosition) && !HasActiveGesture())
            {
                return;
            }

            switch (current.type)
            {
                case EventType.MouseDown:
                    OnMouseDown(current);
                    break;
                case EventType.MouseDrag:
                    OnMouseDrag(current);
                    break;
                case EventType.MouseUp:
                    OnMouseUp(current);
                    break;
                case EventType.ScrollWheel:
                    OnScrollWheel(current);
                    break;
                case EventType.ValidateCommand:
                    OnValidateCommand(current);
                    break;
                case EventType.ExecuteCommand:
                    OnExecuteCommand(current);
                    break;
                case EventType.KeyDown:
                    OnKeyDown(current);
                    break;
            }
        }

        private void OnMouseDown(Event current)
        {
            lastMousePosition = current.mousePosition;
            if (ShouldPan(current))
            {
                panning = true;
                current.Use();
                return;
            }

            if (current.button == 1)
            {
                rightClickPending = true;
                rightClickStart = current.mousePosition;
                current.Use();
                return;
            }

            if (current.button != 0)
            {
                return;
            }

            BlockDefinition hit = GetBlockAt(current.mousePosition);
            if (hit != null)
            {
                BeginBlockGesture(current, hit);
            }
            else
            {
                BeginSelectionGesture(current);
            }

            current.Use();
        }

        private void BeginBlockGesture(Event current, BlockDefinition hit)
        {
            if (IsAdditive(current))
            {
                controller.ToggleBlockSelection(hit.DefinitionId);
            }
            else if (!controller.IsBlockSelected(hit.DefinitionId))
            {
                controller.SelectOnlyBlock(hit.DefinitionId);
            }

            if (controller.IsBlockSelected(hit.DefinitionId))
            {
                controller.BeginBlockMove();
                movingBlocks = true;
            }
        }

        private void BeginSelectionGesture(Event current)
        {
            selecting = true;
            selectionStart = current.mousePosition;
            selectionEnd = selectionStart;
            if (!IsAdditive(current))
            {
                controller.ClearBlockSelection();
            }
        }

        private void OnMouseDrag(Event current)
        {
            if (panning || rightClickPending)
            {
                Pan(current.delta);
                panning = true;
                rightClickPending = rightClickPending && Vector2.Distance(rightClickStart, current.mousePosition) <= k_contextTolerance;
                current.Use();
                return;
            }

            if (movingBlocks)
            {
                controller.MoveSelectedBlocks(current.delta / controller.Metadata.Zoom);
                current.Use();
                return;
            }

            if (selecting)
            {
                selectionEnd = current.mousePosition;
                current.Use();
            }
        }

        private void OnMouseUp(Event current)
        {
            if (movingBlocks)
            {
                movingBlocks = false;
                controller.EndBlockMove();
                current.Use();
            }

            if (selecting)
            {
                CompleteSelection(current);
                current.Use();
            }

            if (current.button == 1 && rightClickPending)
            {
                ShowContextMenu(current.mousePosition);
                current.Use();
            }

            panning = false;
            rightClickPending = false;
        }

        private void CompleteSelection(Event current)
        {
            selecting = false;
            selectionEnd = current.mousePosition;
            Rect selection = MakeRect(selectionStart, selectionEnd);
            List<DefinitionId> ids = IsAdditive(current)
                ? new List<DefinitionId>(controller.Metadata.SelectedBlockIds)
                : new List<DefinitionId>();
            AddOverlappingBlocks(selection, ids);
            controller.SelectBlocks(ids);
        }

        private void OnScrollWheel(Event current)
        {
            float previousZoom = controller.Metadata.Zoom;
            float requestedZoom = previousZoom * (1f - (current.delta.y * 0.04f));
            float nextZoom = Mathf.Clamp(requestedZoom, 0.25f, 1f);
            Vector2 graphPoint = ScreenToGraph(current.mousePosition, previousZoom);
            Vector2 scroll = current.mousePosition - (graphPoint * nextZoom);
            controller.SetViewport(scroll, nextZoom);
            current.Use();
        }

        private void OnValidateCommand(Event current)
        {
            if (IsSupportedCommand(current.commandName))
            {
                current.Use();
            }
        }

        private void OnExecuteCommand(Event current)
        {
            if (!IsSupportedCommand(current.commandName))
            {
                return;
            }

            ExecuteCommand(current.commandName, lastMousePosition);
            current.Use();
        }

        private void OnKeyDown(Event current)
        {
            if (current.keyCode == KeyCode.Delete || current.keyCode == KeyCode.Backspace)
            {
                DeleteSelection();
                current.Use();
            }
        }

        private void ExecuteCommand(string commandName, Vector2 screenPosition)
        {
            switch (commandName)
            {
                case "Copy":
                    controller.CopySelectedBlocks();
                    break;
                case "Cut":
                    controller.CutSelectedBlocks();
                    break;
                case "Paste":
                    PasteAt(screenPosition);
                    break;
                case "Duplicate":
                    controller.DuplicateSelectedBlocks();
                    break;
                case "Delete":
                case "SoftDelete":
                    DeleteSelection();
                    break;
                case "SelectAll":
                    SelectAllBlocks();
                    break;
            }
        }

        private void ShowContextMenu(Vector2 screenPosition)
        {
            BlockDefinition hit = GetBlockAt(screenPosition);
            if (hit != null && !controller.IsBlockSelected(hit.DefinitionId))
            {
                controller.SelectOnlyBlock(hit.DefinitionId);
            }

            GenericMenu menu = new GenericMenu();
            AddAuthoringMenuItems(menu, screenPosition, hit);
            AddExecutionMenuItems(menu, hit);
            menu.ShowAsContext();
        }

        private void AddAuthoringMenuItems(GenericMenu menu, Vector2 screenPosition, BlockDefinition hit)
        {
            menu.AddItem(new GUIContent("Add Block"), false, () => AddBlockAt(screenPosition));
            menu.AddSeparator(string.Empty);
            AddSelectionItem(menu, "Copy", controller.CopySelectedBlocks);
            AddSelectionItem(menu, "Cut", controller.CutSelectedBlocks);
            AddPasteItem(menu, screenPosition);
            AddSelectionItem(menu, "Duplicate", () => controller.DuplicateSelectedBlocks());
            AddSelectionItem(menu, "Delete", DeleteSelection);
            if (hit == null)
            {
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Select All"), false, SelectAllBlocks);
                menu.AddItem(new GUIContent("Frame All"), false, FrameAll);
            }
        }

        private void AddExecutionMenuItems(GenericMenu menu, BlockDefinition hit)
        {
            if (hit == null)
            {
                return;
            }

            menu.AddSeparator(string.Empty);
            bool canControl = execution.CanControl(behaviour, out _);
            if (!canControl)
            {
                menu.AddDisabledItem(new GUIContent("Execute"));
                menu.AddDisabledItem(new GUIContent("Stop"));
                menu.AddDisabledItem(new GUIContent("Stop All"));
                return;
            }

            menu.AddItem(new GUIContent("Execute"), false, () => execution.Execute(behaviour, hit.DefinitionId));
            menu.AddItem(new GUIContent("Stop"), false, () => execution.Stop(behaviour, hit.DefinitionId));
            menu.AddItem(new GUIContent("Stop All"), false, () => execution.StopAll(behaviour));
        }

        private void AddSelectionItem(GenericMenu menu, string label, GenericMenu.MenuFunction action)
        {
            if (controller.Metadata.SelectedBlockIds.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent(label));
                return;
            }

            menu.AddItem(new GUIContent(label), false, action);
        }

        private void AddPasteItem(GenericMenu menu, Vector2 screenPosition)
        {
            if (!controller.Clipboard.HasBlock)
            {
                menu.AddDisabledItem(new GUIContent("Paste"));
                return;
            }

            menu.AddItem(new GUIContent("Paste"), false, () => PasteAt(screenPosition));
        }

        private void AddBlockAt(Vector2 screenPosition)
        {
            BlockDefinition block = controller.AddBlock();
            controller.SetBlockPosition(block.DefinitionId, ScreenToGraph(screenPosition));
        }

        private void PasteAt(Vector2 screenPosition)
        {
            if (controller.Clipboard.HasBlock)
            {
                controller.PasteBlocks(ScreenToGraph(screenPosition));
            }
        }

        private void DeleteSelection()
        {
            if (controller.Metadata.SelectedBlockIds.Count > 0)
            {
                controller.RemoveSelectedBlocks();
            }
        }

        private void SelectAllBlocks()
        {
            List<DefinitionId> ids = new List<DefinitionId>();
            for (int index = 0; index < controller.Definition.Blocks.Count; index++)
            {
                if (controller.Definition.Blocks[index] != null)
                {
                    ids.Add(controller.Definition.Blocks[index].DefinitionId);
                }
            }

            controller.SelectBlocks(ids);
        }

        private void Pan(Vector2 delta)
        {
            controller.SetViewport(controller.Metadata.ScrollPosition + delta, controller.Metadata.Zoom);
            lastMousePosition += delta;
        }

        private void AddOverlappingBlocks(Rect selection, ICollection<DefinitionId> ids)
        {
            for (int index = 0; index < controller.Definition.Blocks.Count; index++)
            {
                BlockDefinition block = controller.Definition.Blocks[index];
                if (block != null && selection.Overlaps(GetScreenNodeRect(block.DefinitionId)) && !ids.Contains(block.DefinitionId))
                {
                    ids.Add(block.DefinitionId);
                }
            }
        }

        private void DrawGrid()
        {
            float spacing = k_gridSpacing * controller.Metadata.Zoom;
            Vector2 offset = controller.Metadata.ScrollPosition;
            Color color = EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.5f) : new Color(0f, 0f, 0f, 0.22f);
            Handles.BeginGUI();
            Handles.color = color;
            for (float x = offset.x % spacing; x < viewRect.width; x += spacing)
            {
                Handles.DrawLine(new Vector2(x, 0f), new Vector2(x, viewRect.height));
            }

            for (float y = offset.y % spacing; y < viewRect.height; y += spacing)
            {
                Handles.DrawLine(new Vector2(0f, y), new Vector2(viewRect.width, y));
            }

            Handles.EndGUI();
        }

        private void DrawConnections()
        {
            Handles.BeginGUI();
            for (int index = 0; index < connections.Count; index++)
            {
                DrawConnection(connections[index]);
            }

            Handles.EndGUI();
        }

        private void DrawConnection(BlackboardGraphConnection connection)
        {
            Rect source = GetScreenNodeRect(connection.Source.DefinitionId);
            Rect destination = GetScreenNodeRect(connection.Destination.DefinitionId);
            GetConnectionEndpoints(source, destination, out Vector2 start, out Vector2 end, out Vector2 sourceDirection, out Vector2 destinationDirection);
            float horizontal = Mathf.Abs(end.x - start.x);
            float vertical = Mathf.Abs(end.y - start.y);
            float controlLength = ((Mathf.Min(horizontal, vertical) * 0.75f) + (Mathf.Max(horizontal, vertical) * 0.25f)) * 0.67f;
            Vector2 startControl = start - (sourceDirection * controlLength);
            Vector2 endControl = end - (destinationDirection * controlLength);
            Handles.DrawBezier(start, end, startControl, endControl, BlackboardEditorStyles.Connection, null, 3f);
            DrawConnectionArrow(start, startControl, endControl, end);
            DrawConnectionPoint(start, sourceDirection);
            DrawConnectionPoint(end, destinationDirection);
        }

        private void DrawNodes()
        {
            for (int index = 0; index < controller.Definition.Blocks.Count; index++)
            {
                BlockDefinition block = controller.Definition.Blocks[index];
                if (block != null)
                {
                    DrawNode(block);
                }
            }
        }

        private void DrawNode(BlockDefinition block)
        {
            Rect nodeRect = GetScreenNodeRect(block.DefinitionId);
            if (!viewRect.Overlaps(nodeRect))
            {
                return;
            }

            bool selected = controller.IsBlockSelected(block.DefinitionId);
            Color tint = GetNodeTint(block);
            bool matches = MatchesSearch(block);
            tint.a = matches ? 1f : 0.22f;
            DrawNodeTexture(nodeRect, selected, block.Trigger != null, IsChoiceBlock(block), tint);
            DrawNodeLabel(nodeRect, block, tint);
            DrawTriggerLabel(nodeRect, block);
            DrawDescription(nodeRect, block);
            DrawExecutionState(nodeRect, block);
        }

        private void DrawNodeTexture(Rect nodeRect, bool selected, bool hasTrigger, bool isChoice, Color tint)
        {
            Texture2D texture = BlackboardEditorStyles.Node(selected, hasTrigger, isChoice);
            Color previous = GUI.color;
            GUI.color = tint;
            if (texture != null)
            {
                GUI.DrawTexture(nodeRect, texture, ScaleMode.StretchToFill, true);
            }
            else
            {
                EditorGUI.DrawRect(nodeRect, tint);
                GUI.Box(nodeRect, GUIContent.none);
            }

            GUI.color = previous;
        }

        private void DrawNodeLabel(Rect nodeRect, BlockDefinition block, Color tint)
        {
            Color previous = nodeLabel.normal.textColor;
            double brightness = (tint.r * 0.3d) + (tint.g * 0.59d) + (tint.b * 0.11d);
            nodeLabel.normal.textColor = brightness >= 0.5d ? Color.black : Color.white;
            GUI.Label(nodeRect, block.Name, nodeLabel);
            nodeLabel.normal.textColor = previous;
        }

        private void DrawTriggerLabel(Rect nodeRect, BlockDefinition block)
        {
            if (block.Trigger == null)
            {
                return;
            }

            Rect labelRect = new Rect(nodeRect.x, nodeRect.y - 18f, nodeRect.width, 18f);
            GUI.Label(labelRect, $"<{BlackboardEditorDisplay.GetName(block.Trigger.GetType())}>", triggerLabel);
        }

        private void DrawDescription(Rect nodeRect, BlockDefinition block)
        {
            BlockAuthoringMetadata layout = controller.GetLayout(block.DefinitionId);
            if (string.IsNullOrWhiteSpace(layout.Description))
            {
                return;
            }

            float height = EditorStyles.helpBox.CalcHeight(new GUIContent(layout.Description), nodeRect.width);
            Rect descriptionRect = new Rect(nodeRect.x, nodeRect.yMax, nodeRect.width, height);
            GUI.Label(descriptionRect, layout.Description, EditorStyles.helpBox);
        }

        private void DrawExecutionState(Rect nodeRect, BlockDefinition block)
        {
            if (behaviour == null || !feedback.TryGetBlockState(behaviour, block.DefinitionId, out BlockExecutionState state))
            {
                return;
            }

            Rect stateRect = new Rect(nodeRect.xMax - 18f, nodeRect.y + 2f, 16f, 16f);
            Texture2D play = BlackboardEditorStyles.Play;
            if (state == BlockExecutionState.Executing && play != null)
            {
                GUI.DrawTexture(stateRect, play, ScaleMode.ScaleToFit);
                return;
            }

            GUI.Label(stateRect, state == BlockExecutionState.Disposed ? "×" : "•", EditorStyles.miniBoldLabel);
        }

        private void DrawSelectionBox()
        {
            if (!selecting)
            {
                return;
            }

            Rect rect = MakeRect(selectionStart, selectionEnd);
            EditorGUI.DrawRect(rect, BlackboardEditorStyles.Selection);
            GUI.Box(rect, GUIContent.none);
        }

        private BlockDefinition GetBlockAt(Vector2 screenPosition)
        {
            for (int index = controller.Definition.Blocks.Count - 1; index >= 0; index--)
            {
                BlockDefinition block = controller.Definition.Blocks[index];
                if (block != null && GetScreenNodeRect(block.DefinitionId).Contains(screenPosition))
                {
                    return block;
                }
            }

            return null;
        }

        private Rect GetScreenNodeRect(DefinitionId blockId)
        {
            Rect graphRect = GetGraphNodeRect(blockId);
            Vector2 position = GraphToScreen(graphRect.position);
            return new Rect(position, graphRect.size * controller.Metadata.Zoom);
        }

        private Rect GetGraphNodeRect(DefinitionId blockId)
        {
            BlockDefinition block = controller.GetBlock(blockId);
            BlockAuthoringMetadata layout = controller.GetLayout(blockId);
            float labelWidth = nodeLabel.CalcSize(new GUIContent(block?.Name ?? "Block")).x + 24f;
            float width = Mathf.Clamp(labelWidth, k_minNodeWidth, k_maxNodeWidth);
            Vector2 position = layout.Position.position;
            if (ScaffoldEditorPreferences.useGridSnap)
            {
                position = new Vector2(Snap(position.x), Snap(position.y));
                width = Snap(width);
            }

            return new Rect(position, new Vector2(width, k_nodeHeight));
        }

        private Rect GetGraphBounds()
        {
            Rect bounds = GetGraphNodeRect(controller.Definition.Blocks[0].DefinitionId);
            for (int index = 1; index < controller.Definition.Blocks.Count; index++)
            {
                Rect node = GetGraphNodeRect(controller.Definition.Blocks[index].DefinitionId);
                bounds = Rect.MinMaxRect(Mathf.Min(bounds.xMin, node.xMin), Mathf.Min(bounds.yMin, node.yMin), Mathf.Max(bounds.xMax, node.xMax), Mathf.Max(bounds.yMax, node.yMax));
            }

            return bounds;
        }

        private Color GetNodeTint(BlockDefinition block)
        {
            BlockAuthoringMetadata layout = controller.GetLayout(block.DefinitionId);
            if (layout.UseCustomTint)
            {
                return layout.Tint;
            }

            if (block.Trigger != null)
            {
                return ScaffoldConstants.DefaultEventBlockTint;
            }

            return IsChoiceBlock(block)
                ? ScaffoldConstants.DefaultChoiceBlockTint
                : ScaffoldConstants.DefaultProcessBlockTint;
        }

        private bool IsChoiceBlock(BlockDefinition block)
        {
            BlockDefinition firstDestination = null;
            for (int index = 0; index < connections.Count; index++)
            {
                BlackboardGraphConnection connection = connections[index];
                if (connection.Source == block && connection.Destination != block)
                {
                    if (firstDestination == null)
                    {
                        firstDestination = connection.Destination;
                    }
                    else if (connection.Destination != firstDestination)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private float Snap(float value)
        {
            return Mathf.Round(value / k_gridSnap) * k_gridSnap;
        }

        private void GetConnectionEndpoints(Rect source, Rect destination, out Vector2 start, out Vector2 end, out Vector2 sourceDirection, out Vector2 destinationDirection)
        {
            s_sourceConnectionPoints[0] = new Vector2(source.xMin, source.center.y);
            s_sourceConnectionPoints[1] = new Vector2(source.center.x, source.yMin);
            s_sourceConnectionPoints[2] = new Vector2(source.center.x, source.yMax);
            s_sourceConnectionPoints[3] = new Vector2(source.xMax, source.center.y);
            s_destinationConnectionPoints[0] = new Vector2(destination.xMin, destination.center.y);
            s_destinationConnectionPoints[1] = new Vector2(destination.center.x, destination.yMin);
            s_destinationConnectionPoints[2] = new Vector2(destination.center.x, destination.yMax);
            s_destinationConnectionPoints[3] = new Vector2(destination.xMax, destination.center.y);
            int closest = 0;
            float distance = float.MaxValue;
            for (int index = 0; index < s_sourceConnectionPoints.Length; index++)
            {
                float candidate = Vector2.Distance(s_sourceConnectionPoints[index], s_destinationConnectionPoints[s_destinationPointIndices[index]]);
                if (candidate < distance)
                {
                    closest = index;
                    distance = candidate;
                }
            }

            start = s_sourceConnectionPoints[closest];
            end = s_destinationConnectionPoints[s_destinationPointIndices[closest]];
            sourceDirection = (source.center - start).normalized;
            destinationDirection = (destination.center - end).normalized;
        }

        private void DrawConnectionArrow(Vector2 start, Vector2 startControl, Vector2 endControl, Vector2 end)
        {
            Vector2 point = GetBezierPoint(start, startControl, endControl, end, 0.7f);
            Vector2 direction = (GetBezierPoint(start, startControl, endControl, end, 0.6f) - point).normalized;
            Vector2 perpendicular = new Vector2(direction.y, -direction.x);
            s_arrowPoints[0] = point;
            s_arrowPoints[1] = point + (direction * 10f) + (perpendicular * 5f);
            s_arrowPoints[2] = point + (direction * 10f) - (perpendicular * 5f);
            Handles.color = BlackboardEditorStyles.Connection;
            Handles.DrawAAConvexPolygon(s_arrowPoints);
        }

        private void DrawConnectionPoint(Vector2 point, Vector2 direction)
        {
            Texture2D texture = BlackboardEditorStyles.ConnectionPoint;
            if (texture == null)
            {
                return;
            }

            Vector2 center = point + (direction * 4f);
            GUI.DrawTexture(new Rect(center.x - 4f, center.y - 4f, 8f, 8f), texture, ScaleMode.ScaleToFit);
        }

        private Vector2 GetBezierPoint(Vector2 start, Vector2 startControl, Vector2 endControl, Vector2 end, float time)
        {
            float remaining = 1f - time;
            float mixed = remaining * time;
            return (remaining * remaining * remaining * start) +
                (3f * remaining * mixed * startControl) +
                (3f * mixed * time * endControl) +
                (time * time * time * end);
        }

        private bool MatchesSearch(BlockDefinition block)
        {
            if (string.IsNullOrWhiteSpace(search) || Contains(block.Name, search))
            {
                return true;
            }

            for (int trackIndex = 0; trackIndex < block.Tracks.Count; trackIndex++)
            {
                List<IAction> actions = block.Tracks[trackIndex].ActionList.Actions;
                for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
                {
                    IAction action = actions[actionIndex];
                    if (action != null && (Contains(BlackboardEditorDisplay.GetName(action.GetType()), search) || Contains(BlackboardEditorDisplay.GetSummary(action), search)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool Contains(string value, string expected)
        {
            return (value ?? string.Empty).IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool ShouldPan(Event current)
        {
            return current.button == 2 || (current.button == 0 && current.alt);
        }

        private bool IsAdditive(Event current)
        {
            return current.command || current.control || current.shift;
        }

        private bool HasActiveGesture()
        {
            return movingBlocks || selecting || panning || rightClickPending;
        }

        private bool IsSupportedCommand(string commandName)
        {
            return commandName == "Copy" || commandName == "Cut" || commandName == "Paste" || commandName == "Duplicate" || commandName == "Delete" || commandName == "SoftDelete" || commandName == "SelectAll";
        }

        private Vector2 ScreenToGraph(Vector2 screenPosition)
        {
            return ScreenToGraph(screenPosition, controller.Metadata.Zoom);
        }

        private Vector2 ScreenToGraph(Vector2 screenPosition, float zoom)
        {
            return (screenPosition - controller.Metadata.ScrollPosition) / zoom;
        }

        private Vector2 GraphToScreen(Vector2 graphPosition)
        {
            return (graphPosition * controller.Metadata.Zoom) + controller.Metadata.ScrollPosition;
        }

        private Rect MakeRect(Vector2 first, Vector2 second)
        {
            return Rect.MinMaxRect(Mathf.Min(first.x, second.x), Mathf.Min(first.y, second.y), Mathf.Max(first.x, second.x), Mathf.Max(first.y, second.y));
        }
    }
}
