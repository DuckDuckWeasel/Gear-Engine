using System;
using System.Globalization;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Presentation.UI.Input;
using UnityEditor;
using UnityEngine;

namespace Scaffold.EditorUtils
{
    public enum ActionIssueSeverity
    {
        None,
        Warning,
        Error,
    }

    public readonly struct ActionIssue
    {
        public ActionIssue(ActionIssueSeverity severity, string message)
        {
            Severity = severity;
            Message = message ?? string.Empty;
        }

        public ActionIssueSeverity Severity { get; }

        public string Message { get; }

        public bool HasIssue => Severity != ActionIssueSeverity.None;
    }

    public static class InvokeActionEditorUtility
    {
        private static readonly Color s_ExecutionFillColor =
            new Color(0.12f, 0.48f, 0.9f, 0.42f);
        private static readonly Color s_ExecutionSuccessColor =
            new Color(0.16f, 0.65f, 0.3f, 0.42f);
        private static readonly Color s_ExecutionFailureColor =
            new Color(0.82f, 0.18f, 0.18f, 0.42f);

        public static string GetDisplayName(IAction action)
        {
            if (action == null)
            {
                return "Empty Action";
            }

            string actionName = GetDisplayName(action.GetType());
            if (action is ActionBase actionBase)
            {
                try
                {
                    string summary = GetHeaderSummary(actionBase.GetSummary());
                    if (!string.IsNullOrEmpty(summary))
                    {
                        return string.Equals(summary, actionName, StringComparison.OrdinalIgnoreCase)
                            ? actionName
                            : $"{actionName}: {summary}";
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError($"[InvokeActionEditor] Failed to get the action header: {exception.Message}");
                }
            }

            return actionName;
        }

        public static string GetDisplayName(Type actionType)
        {
            if (actionType == null)
            {
                return "Empty Action";
            }

            CommandInfoAttribute commandInfo = CommandEditor.GetCommandInfo(actionType);
            return commandInfo != null
                ? commandInfo.CommandName
                : ObjectNames.NicifyVariableName(actionType.Name);
        }

        private static string GetHeaderSummary(string summary)
        {
            if (string.IsNullOrWhiteSpace(summary))
            {
                return string.Empty;
            }

            string singleLineSummary = summary
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();
            if (singleLineSummary.StartsWith("Error:", StringComparison.OrdinalIgnoreCase) ||
                singleLineSummary.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return singleLineSummary;
        }

        public static string GetMenuPath(Type actionType)
        {
            if (actionType == null)
            {
                return "Empty Action";
            }

            CommandInfoAttribute commandInfo = CommandEditor.GetCommandInfo(actionType);
            if (commandInfo == null)
            {
                return ObjectNames.NicifyVariableName(actionType.Name);
            }

            return string.IsNullOrEmpty(commandInfo.Category)
                ? commandInfo.CommandName
                : commandInfo.Category + "/" + commandInfo.CommandName;
        }

        public static bool IsMergeDrop(Rect targetRect, Vector2 mousePosition)
        {
            const float k_MergeVerticalInset = 6f;
            float verticalInset = Mathf.Min(k_MergeVerticalInset, targetRect.height * 0.25f);
            Rect mergeRect = new Rect(
                targetRect.x,
                targetRect.y + verticalInset,
                targetRect.width,
                targetRect.height - (verticalInset * 2f));
            return mergeRect.Contains(mousePosition);
        }

        public static int GetInsertionIndex(Rect targetRect, Vector2 mousePosition, int actionCount)
        {
            return mousePosition.y < targetRect.center.y ? 0 : actionCount;
        }

        public static Rect GetCommandBeforeDropRect(Rect commandRect, float targetHeight)
        {
            float clampedHeight = Mathf.Clamp(targetHeight, 0f, commandRect.height * 0.5f);
            return new Rect(commandRect.x, commandRect.y, commandRect.width, clampedHeight);
        }

        public static Rect GetCommandAfterDropRect(Rect commandRect, float targetHeight)
        {
            float clampedHeight = Mathf.Clamp(targetHeight, 0f, commandRect.height * 0.5f);
            return new Rect(
                commandRect.x,
                commandRect.yMax - clampedHeight,
                commandRect.width,
                clampedHeight);
        }

        public static Rect GetActionRowDragRect(Rect actionRect, float excludedRightWidth)
        {
            float clampedExcludedWidth = Mathf.Clamp(excludedRightWidth, 0f, actionRect.width);
            return new Rect(
                actionRect.x,
                actionRect.y,
                actionRect.width - clampedExcludedWidth,
                actionRect.height);
        }

        public static Rect GetActionRowContentRect(
            Rect actionRect,
            float excludedLeftWidth,
            float excludedRightWidth,
            float contentHeight)
        {
            float clampedExcludedLeftWidth = Mathf.Clamp(excludedLeftWidth, 0f, actionRect.width);
            float remainingWidth = actionRect.width - clampedExcludedLeftWidth;
            float clampedExcludedRightWidth = Mathf.Clamp(excludedRightWidth, 0f, remainingWidth);
            float clampedContentHeight = Mathf.Clamp(contentHeight, 0f, actionRect.height);
            return new Rect(
                actionRect.x + clampedExcludedLeftWidth,
                actionRect.y,
                remainingWidth - clampedExcludedRightWidth,
                clampedContentHeight);
        }

        public static bool HasDragStarted(Vector2 startPosition, Vector2 currentPosition, float threshold)
        {
            float clampedThreshold = Mathf.Max(0f, threshold);
            Vector2 delta = currentPosition - startPosition;
            return delta.sqrMagnitude >= clampedThreshold * clampedThreshold;
        }

        public static bool ShouldTemporarilySuppressParentDrag(bool hasActionDrag, EventType rawEventType)
        {
            return hasActionDrag &&
                   (rawEventType == EventType.MouseDrag || rawEventType == EventType.MouseUp);
        }

        public static float GetReorderDragYWithHysteresis(float dragStartY, float mouseY, float hysteresis)
        {
            float clampedHysteresis = Mathf.Max(0f, hysteresis);
            return Mathf.MoveTowards(mouseY, dragStartY, clampedHysteresis);
        }

        public static bool CanAcceptActionDrop(InvokeActionCommand invokeAction)
        {
            return invokeAction != null &&
                   (invokeAction.actions == null ||
                    invokeAction.actions.Count == 0 ||
                    invokeAction.DisplayAsGroup ||
                    invokeAction.actions.Count > 1);
        }

        public static InvokeActionCommand ResolveReorderSource(
            InvokeActionCommand capturedSource,
            InvokeActionCommand commandAtOldIndex)
        {
            return capturedSource ?? commandAtOldIndex;
        }

        public static bool IsDeterministicExecution(
            CompositeExecutionMethod executionMethod,
            CompositeOrderMode orderMode)
        {
            return CompositeExecutionDescription.SupportsOrder(executionMethod) &&
                   orderMode == CompositeOrderMode.Ordered;
        }

        public static string GetExecutionWaitingMessage(
            CompositeExecutionMethod executionMethod,
            CompositeAwaitMode awaitMode,
            CompositeOrderMode orderMode)
        {
            if (executionMethod == CompositeExecutionMethod.UtilitySelector)
            {
                return "Waiting for the highest-utility action.";
            }

            if (CompositeExecutionDescription.SupportsOrder(executionMethod))
            {
                switch (orderMode)
                {
                    case CompositeOrderMode.Random:
                        return "Waiting for the weighted random order.";
                    case CompositeOrderMode.Shuffle:
                        return "Waiting for the shuffled order.";
                    default:
                        return string.Empty;
                }
            }

            if (!CompositeExecutionDescription.SupportsAwait(executionMethod) ||
                awaitMode == CompositeAwaitMode.WaitNone)
            {
                return string.Empty;
            }

            if (awaitMode == CompositeAwaitMode.WaitAny)
            {
                return "Waiting for the first action to complete.";
            }

            return executionMethod == CompositeExecutionMethod.ParallelSelector
                ? "Waiting for all actions; any success is enough."
                : "Waiting for all actions to complete.";
        }

        public static ActionIssue GetActionIssue(IAction action)
        {
            if (action == null)
            {
                return new ActionIssue(ActionIssueSeverity.Error, "Action reference is missing.");
            }

            if (action is not ActionBase actionBase)
            {
                return default;
            }

            string summary;
            try
            {
                summary = actionBase.GetSummary() ?? string.Empty;
            }
            catch (Exception exception)
            {
                return new ActionIssue(
                    ActionIssueSeverity.Error,
                    $"Validation failed: {exception.Message}");
            }

            if (summary.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            {
                return new ActionIssue(
                    ActionIssueSeverity.Error,
                    summary.Substring("Error:".Length).Trim());
            }

            if (summary.StartsWith("Warning:", StringComparison.OrdinalIgnoreCase))
            {
                return new ActionIssue(
                    ActionIssueSeverity.Warning,
                    summary.Substring("Warning:".Length).Trim());
            }

            return default;
        }

        public static void DrawExecutionProgress(Rect rect, float progress)
        {
            Rect fillRect = rect;
            fillRect.width *= Mathf.Clamp01(progress);
            EditorGUI.DrawRect(fillRect, s_ExecutionFillColor);
        }

        public static void DrawExecutingHighlight(Rect rect)
        {
            EditorGUI.DrawRect(rect, s_ExecutionFillColor);
        }

        public static void DrawExecutionResult(
            Rect rect,
            CompositeExecutionStatus status)
        {
            Color color = status == CompositeExecutionStatus.Success
                ? s_ExecutionSuccessColor
                : s_ExecutionFailureColor;
            EditorGUI.DrawRect(rect, color);
        }

        public static void DrawWaitingMessage(Rect rect, string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            EditorGUI.LabelField(
                rect,
                new GUIContent(message, message),
                EditorStyles.centeredGreyMiniLabel);
        }

        public static bool DrawActionIssueBadge(Rect rect, IAction action)
        {
            ActionIssue issue = GetActionIssue(action);
            if (!issue.HasIssue)
            {
                return false;
            }

            string iconName = issue.Severity == ActionIssueSeverity.Error
                ? "console.erroricon.sml"
                : "console.warnicon.sml";
            GUIContent sourceIcon = EditorGUIUtility.IconContent(iconName);
            GUIContent icon = new GUIContent(sourceIcon.image, issue.Message);
            GUI.Label(rect, icon);
            return true;
        }

        public static float DelayedPercentageField(
            Rect rect,
            GUIContent label,
            float value)
        {
            string formattedValue = FormatPercentage(value);
            string requestedValue = EditorGUI.DelayedTextField(rect, label, formattedValue);
            string normalizedValue = requestedValue.Replace(',', '.');
            return float.TryParse(
                normalizedValue,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float parsedValue)
                ? parsedValue
                : value;
        }

        public static string FormatPercentage(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
