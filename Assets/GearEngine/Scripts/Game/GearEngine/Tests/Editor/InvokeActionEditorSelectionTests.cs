using System;
using System.Reflection;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Presentation.UI.Input;
using NUnit.Framework;
using Scaffold;
using Scaffold.EditorUtils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    public class InvokeActionEditorSelectionTests
    {
        private GameObject hostObject;
        private InvokeActionCommand command;

        [SetUp]
        public void SetUp()
        {
            hostObject = new GameObject("InvokeActionEditorSelectionTests");
            command = hostObject.AddComponent<InvokeActionCommand>();
            command.actions.Add(new InvokeActionCommand.ActionWrapper(new CameraZoom()));
            command.actions.Add(new InvokeActionCommand.ActionWrapper(new SendAnalyticsEvent()));
        }

        [TearDown]
        public void TearDown()
        {
            InvokeActionEditorSelection.Clear(command);
            UnityEngine.Object.DestroyImmediate(hostObject);
        }

        [Test]
        public void Select_StoresTheNestedActionSelectedFromTheBlockList()
        {
            InvokeActionEditorSelection.Select(command, 1);

            Assert.That(InvokeActionEditorSelection.GetSelectedIndex(command), Is.EqualTo(1));
        }

        [Test]
        public void GetDisplayName_UsesTheActionCommandInfoName()
        {
            Assert.That(InvokeActionEditorUtility.GetDisplayName(command.actions[1].action), Is.EqualTo("Send Event"));
        }

        [Test]
        public void GetDisplayName_AppendsTheActionSummaryWhenItIsAvailable()
        {
            IAction action = new HeaderSummaryAction();

            Assert.That(
                InvokeActionEditorUtility.GetDisplayName(action),
                Is.EqualTo("Header Summary Action: Player joined the lobby"));
        }

        [Test]
        public void GetDisplayName_UsesTheCommandNameWhenTheSummaryReportsAnIssue()
        {
            IAction action = new WarningSummaryAction();

            Assert.That(InvokeActionEditorUtility.GetDisplayName(action), Is.EqualTo("Warning Summary Action"));
        }

        [Test]
        public void ExecutionFeedback_TreatsOnlyOrderedSequentialModesAsDeterministic()
        {
            Assert.That(
                InvokeActionEditorUtility.IsDeterministicExecution(
                    CompositeExecutionMethod.Sequence,
                    CompositeOrderMode.Ordered),
                Is.True);
            Assert.That(
                InvokeActionEditorUtility.IsDeterministicExecution(
                    CompositeExecutionMethod.Sequence,
                    CompositeOrderMode.Random),
                Is.False);
            Assert.That(
                InvokeActionEditorUtility.IsDeterministicExecution(
                    CompositeExecutionMethod.Parallel,
                    CompositeOrderMode.Ordered),
                Is.False);
        }

        [Test]
        public void ExecutionFeedback_ExplainsNonDeterministicWaitSemantics()
        {
            Assert.That(
                InvokeActionEditorUtility.GetExecutionWaitingMessage(
                    CompositeExecutionMethod.Sequence,
                    CompositeAwaitMode.WaitAll,
                    CompositeOrderMode.Random),
                Is.EqualTo("Waiting for the weighted random order."));
            Assert.That(
                InvokeActionEditorUtility.GetExecutionWaitingMessage(
                    CompositeExecutionMethod.Parallel,
                    CompositeAwaitMode.WaitAny,
                    CompositeOrderMode.Ordered),
                Is.EqualTo("Waiting for the first action to complete."));
        }

        [Test]
        public void PercentageField_FormatsAtMostTwoDecimalPlaces()
        {
            Assert.That(InvokeActionEditorUtility.FormatPercentage(33.333333f), Is.EqualTo("33.33"));
            Assert.That(InvokeActionEditorUtility.FormatPercentage(50f), Is.EqualTo("50"));
        }

        [Test]
        public void ActionIssue_UsesSummarySeverityForErrorAndWarningBadges()
        {
            ActionIssue missingAction = InvokeActionEditorUtility.GetActionIssue(null);
            ActionIssue warningAction = InvokeActionEditorUtility.GetActionIssue(
                new WarningSummaryAction());

            Assert.That(missingAction.Severity, Is.EqualTo(ActionIssueSeverity.Error));
            Assert.That(warningAction.Severity, Is.EqualTo(ActionIssueSeverity.Warning));
            Assert.That(warningAction.Message, Is.EqualTo("Optional reference is not set."));
        }

        [Test]
        public void GetActionRowContentRect_ReservesTheReorderHandleBeforeTheFoldout()
        {
            Rect rowRect = new Rect(10f, 20f, 200f, 120f);

            Rect contentRect = InvokeActionEditorUtility.GetActionRowContentRect(
                rowRect,
                18f,
                30f,
                EditorGUIUtility.singleLineHeight);

            Assert.That(contentRect.x, Is.EqualTo(28f));
            Assert.That(contentRect.y, Is.EqualTo(20f));
            Assert.That(contentRect.width, Is.EqualTo(152f));
            Assert.That(contentRect.xMax, Is.EqualTo(180f));
            Assert.That(contentRect.height, Is.EqualTo(EditorGUIUtility.singleLineHeight));
        }

        [Test]
        public void ChildActionInspector_DoesNotExposeCompositeControlHelpers()
        {
            Assert.That(
                typeof(InvokeActionEditorUtility).GetMethod("ShouldShowCompositeControls"),
                Is.Null);
            Assert.That(
                typeof(CommandListAdaptor).GetMethod(
                    "DrawInvokeActionExecutionPopup",
                    BindingFlags.Static | BindingFlags.NonPublic),
                Is.Null);
            Assert.That(
                typeof(CommandListAdaptor).GetMethod(
                    "DrawInvokeActionSecondaryPopup",
                    BindingFlags.Static | BindingFlags.NonPublic),
                Is.Null);
        }

        [Test]
        public void ActionInvoker_UsesItsOwnDisplayName()
        {
            CommandInfoAttribute commandInfo = CommandEditor.GetCommandInfo(typeof(InvokeActionCommand));

            Assert.That(commandInfo.CommandName, Is.EqualTo("Action Invoker"));
        }

        [Test]
        public void StandaloneActionInvoker_DoesNotShowAnActionsList()
        {
            command.actions.RemoveAt(1);

            Assert.That(ShouldShowActionsList(command), Is.False);
        }

        [Test]
        public void StandaloneActionInvoker_UsesTheActionNameAsItsInspectorTitle()
        {
            command.actions.RemoveAt(1);

            Assert.That(GetInspectorTitle(command), Is.EqualTo("Camera Zoom"));
        }

        [Test]
        public void ExplicitActionInvoker_ShowsAnActionsList()
        {
            command.DisplayAsGroup = true;

            Assert.That(ShouldShowActionsList(command), Is.True);
        }

        [Test]
        public void ExplicitActionInvoker_ProvidesExecutionSettings()
        {
            command.DisplayAsGroup = true;
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(command);
            try
            {
                MethodInfo method = editor.GetType().GetMethod(
                    "DrawExecutionSettings",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.That(method, Is.Not.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }

        [Test]
        public void ActionInvokerInspector_SeparatesActionPropertiesFromTheHeader()
        {
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(command);
            try
            {
                MethodInfo drawPropertiesMethod = editor.GetType().GetMethod(
                    "DrawActionProperties",
                    BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo propertiesHeightMethod = editor.GetType().GetMethod(
                    "GetActionPropertiesHeight",
                    BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo elementHeightMethod = editor.GetType().GetMethod(
                    "GetActionElementHeight",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.That(drawPropertiesMethod, Is.Not.Null);
                Assert.That(propertiesHeightMethod, Is.Not.Null);
                Assert.That(elementHeightMethod, Is.Not.Null);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }

        [Test]
        public void ActionInvokerListItem_CollapsesToItsHeaderHeight()
        {
            SerializedObject serializedCommand = new SerializedObject(command);
            SerializedProperty actionProperty = serializedCommand
                .FindProperty("actions")
                .GetArrayElementAtIndex(0);
            actionProperty.isExpanded = false;

            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(command);
            try
            {
                MethodInfo method = editor.GetType().GetMethod(
                    "GetActionElementHeight",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.That(method, Is.Not.Null);
                float height = (float)method.Invoke(editor, new object[] { 0 });
                Assert.That(height, Is.EqualTo(EditorGUIUtility.singleLineHeight));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }

        [Test]
        public void ExpandedActionListItem_IncludesVisibleActionPropertyHeight()
        {
            command.actions[0] = new InvokeActionCommand.ActionWrapper(new PlayAnimState());
            SerializedObject serializedCommand = new SerializedObject(command);
            SerializedProperty actionProperty = serializedCommand
                .FindProperty("actions")
                .GetArrayElementAtIndex(0);
            actionProperty.isExpanded = true;

            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(command);
            try
            {
                MethodInfo propertiesHeightMethod = editor.GetType().GetMethod(
                    "GetActionPropertiesHeight",
                    BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo elementHeightMethod = editor.GetType().GetMethod(
                    "GetActionElementHeight",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.That(propertiesHeightMethod, Is.Not.Null);
                Assert.That(elementHeightMethod, Is.Not.Null);
                float propertiesHeight = (float)propertiesHeightMethod.Invoke(
                    null,
                    new object[] { actionProperty });
                float height = (float)elementHeightMethod.Invoke(editor, new object[] { 0 });

                Assert.That(propertiesHeight, Is.GreaterThan(0f));
                Assert.That(
                    height,
                    Is.EqualTo(
                        EditorGUIUtility.singleLineHeight +
                        EditorGUIUtility.standardVerticalSpacing +
                        propertiesHeight));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }

        [Test]
        public void ActionChildPropertyCheck_ExcludesTheNextActionInTheList()
        {
            SerializedObject serializedCommand = new SerializedObject(command);
            SerializedProperty firstActionProperty = serializedCommand
                .FindProperty("actions")
                .GetArrayElementAtIndex(0);
            SerializedProperty secondActionProperty = serializedCommand
                .FindProperty("actions")
                .GetArrayElementAtIndex(1);
            SerializedProperty firstChildProperty = firstActionProperty.Copy();
            bool movedToFirstChild = firstChildProperty.NextVisible(true);
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(command);
            try
            {
                MethodInfo method = editor.GetType().GetMethod(
                    "IsActionChildProperty",
                    BindingFlags.Static | BindingFlags.NonPublic);

                Assert.That(movedToFirstChild, Is.True);
                Assert.That(method, Is.Not.Null);
                Assert.That(
                    (bool)method.Invoke(null, new object[] { firstActionProperty, firstChildProperty }),
                    Is.True);
                Assert.That(
                    (bool)method.Invoke(null, new object[] { firstActionProperty, secondActionProperty }),
                    Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }

        [Test]
        public void SelectedActionListItem_RemainsCollapsedAfterSelectionSynchronization()
        {
            InvokeActionEditorSelection.Select(command, 0);
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(command);
            try
            {
                MethodInfo method = editor.GetType().GetMethod(
                    "SynchronizeSelectedAction",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.That(method, Is.Not.Null);
                method.Invoke(editor, null);
                SerializedObject serializedCommand = new SerializedObject(command);
                SerializedProperty actionProperty = serializedCommand
                    .FindProperty("actions")
                    .GetArrayElementAtIndex(0);
                actionProperty.isExpanded = false;

                method.Invoke(editor, null);

                Assert.That(actionProperty.isExpanded, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }

        [Test]
        public void ActionInvokerListSelection_SelectAllIncludesEveryAction()
        {
            InvokeActionEditorSelection.SelectAll(command);

            CollectionAssert.AreEquivalent(
                new[] { 0, 1 },
                InvokeActionEditorSelection.GetSelectedIndices(command));
        }

        [Test]
        public void ActionInvokerHeaderMenu_ShowsListSelectionOnlyForActionLists()
        {
            command.actions.RemoveAt(1);

            Assert.That(ShouldShowListSelectionItems(command), Is.False);

            command.DisplayAsGroup = true;

            Assert.That(ShouldShowListSelectionItems(command), Is.True);
        }

        [Test]
        public void GetMenuPath_UsesTheFlowCategoryForPerformInterruption()
        {
            string menuPath = InvokeActionEditorUtility.GetMenuPath(typeof(PerformInterruption));

            Assert.That(menuPath, Is.EqualTo("Flow/Perform Interruption"));
        }

        [Test]
        public void ConvertToBlock_CreatesTheExtractedBlockBesideTheSourceBlock()
        {
            UnityEngine.Object.DestroyImmediate(command);
            Blackboard blackboard = hostObject.AddComponent<Blackboard>();
            Block sourceBlock = hostObject.AddComponent<Block>();
            sourceBlock._NodeRect = new Rect(100f, 200f, 320f, 100f);
            command = hostObject.AddComponent<InvokeActionCommand>();
            command.ParentBlock = sourceBlock;
            sourceBlock.CommandList.Add(command);

            SerializedObject serializedBlock = new SerializedObject(sourceBlock);
            CommandListAdaptor adaptor = new CommandListAdaptor(
                sourceBlock,
                serializedBlock.FindProperty("commandList"));
            MethodInfo convertMethod = typeof(CommandListAdaptor).GetMethod(
                "ConvertInvokeActionToBlock",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(convertMethod, Is.Not.Null);
            convertMethod.Invoke(adaptor, new object[] { command, blackboard });

            Block extractedBlock = blackboard.SelectedBlock;
            Assert.That(extractedBlock, Is.Not.Null);
            Assert.That(extractedBlock, Is.Not.SameAs(sourceBlock));
            Assert.That(extractedBlock._NodeRect.position, Is.EqualTo(new Vector2(444f, 200f)));
            Assert.That(extractedBlock.CommandList, Contains.Item(command));
            CollectionAssert.DoesNotContain(sourceBlock.CommandList, command);
        }

        private static bool ShouldShowActionsList(InvokeActionCommand invokeAction)
        {
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(invokeAction);
            try
            {
                MethodInfo method = editor.GetType().GetMethod(
                    "ShouldShowActionsList",
                    BindingFlags.Static | BindingFlags.NonPublic);

                Assert.That(method, Is.Not.Null);
                return (bool)method.Invoke(null, new object[] { invokeAction });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }

        private static string GetInspectorTitle(InvokeActionCommand invokeAction)
        {
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(invokeAction);
            try
            {
                MethodInfo method = editor.GetType().GetMethod(
                    "GetCommandDisplayName",
                    BindingFlags.Instance | BindingFlags.NonPublic);

                Assert.That(method, Is.Not.Null);
                CommandInfoAttribute commandInfo =
                    CommandEditor.GetCommandInfo(typeof(InvokeActionCommand));
                return (string)method.Invoke(editor, new object[] { invokeAction, commandInfo });
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(editor);
            }
        }

        private static bool ShouldShowListSelectionItems(InvokeActionCommand invokeAction)
        {
            MethodInfo method = typeof(CommandEditor).GetMethod(
                "ShouldShowListSelectionItems",
                BindingFlags.Static | BindingFlags.NonPublic);

            Assert.That(method, Is.Not.Null);
            return (bool)method.Invoke(null, new object[] { invokeAction });
        }

        [Test]
        public void CompositeOptions_ShowAwaitOnlyForParallelMethods()
        {
            Assert.That(
                CompositeExecutionDescription.SupportsAwait(CompositeExecutionMethod.Parallel),
                Is.True);
            Assert.That(
                CompositeExecutionDescription.SupportsAwait(CompositeExecutionMethod.ParallelSelector),
                Is.True);
            Assert.That(
                CompositeExecutionDescription.SupportsAwait(CompositeExecutionMethod.Sequence),
                Is.False);
        }

        [Test]
        public void CompositeOptions_ShowOrderAndWeightOnlyWhenApplicable()
        {
            Assert.That(
                CompositeExecutionDescription.SupportsOrder(CompositeExecutionMethod.Sequence),
                Is.True);
            Assert.That(
                CompositeExecutionDescription.SupportsOrder(CompositeExecutionMethod.Selector),
                Is.True);
            Assert.That(
                CompositeExecutionDescription.SupportsWeight(
                    CompositeExecutionMethod.Sequence,
                    CompositeOrderMode.Random),
                Is.True);
            Assert.That(
                CompositeExecutionDescription.SupportsWeight(
                    CompositeExecutionMethod.Sequence,
                    CompositeOrderMode.Shuffle),
                Is.False);
        }

        [Test]
        public void CompositeTooltips_ReflectTheExecutionAndSecondarySelections()
        {
            string waitAnyTooltip = CompositeExecutionDescription.GetExecutionTooltip(
                CompositeExecutionMethod.Parallel,
                CompositeAwaitMode.WaitAny,
                CompositeOrderMode.Ordered);
            string shuffleTooltip = CompositeExecutionDescription.GetExecutionTooltip(
                CompositeExecutionMethod.Selector,
                CompositeAwaitMode.WaitAll,
                CompositeOrderMode.Shuffle);

            StringAssert.Contains("all tasks must succeed", waitAnyTooltip);
            StringAssert.Contains("Wait Any", waitAnyTooltip);
            StringAssert.Contains("until one succeeds", shuffleTooltip);
            StringAssert.Contains("Shuffle", shuffleTooltip);
        }

        [Test]
        public void IsMergeDrop_AcceptsTheCenterOfAnEmptyInvokeAction()
        {
            Rect targetRect = new Rect(0f, 0f, 240f, 26f);

            bool isMergeDrop = InvokeActionEditorUtility.IsMergeDrop(targetRect, targetRect.center);

            Assert.That(isMergeDrop, Is.True);
        }

        [Test]
        public void IsMergeDrop_LeavesTheRowEdgesAvailableForReordering()
        {
            Rect targetRect = new Rect(0f, 0f, 240f, 26f);
            Vector2 reorderPosition = new Vector2(targetRect.center.x, targetRect.y + 1f);

            bool isMergeDrop = InvokeActionEditorUtility.IsMergeDrop(targetRect, reorderPosition);

            Assert.That(isMergeDrop, Is.False);
        }

        [Test]
        public void GetReorderDragYWithHysteresis_KeepsSmallMovementsAtTheDragOrigin()
        {
            float adjustedMouseY = InvokeActionEditorUtility.GetReorderDragYWithHysteresis(100f, 108f, 10f);

            Assert.That(adjustedMouseY, Is.EqualTo(100f));
        }

        [Test]
        public void GetReorderDragYWithHysteresis_RequiresExtraTravelInBothDirections()
        {
            float adjustedDownwardY = InvokeActionEditorUtility.GetReorderDragYWithHysteresis(100f, 126f, 10f);
            float adjustedUpwardY = InvokeActionEditorUtility.GetReorderDragYWithHysteresis(100f, 74f, 10f);

            Assert.That(adjustedDownwardY, Is.EqualTo(116f));
            Assert.That(adjustedUpwardY, Is.EqualTo(84f));
        }

        [Test]
        public void ActionRowDragRect_AllowsDraggingFromTheLabelAndExcludesTheToggle()
        {
            Rect actionRect = new Rect(36f, 20f, 240f, 22f);
            Rect dragRect = InvokeActionEditorUtility.GetActionRowDragRect(actionRect, 22f);
            Vector2 labelPosition = new Vector2(actionRect.center.x, actionRect.center.y);
            Vector2 togglePosition = new Vector2(actionRect.xMax - 10f, actionRect.center.y);

            Assert.That(dragRect.Contains(labelPosition), Is.True);
            Assert.That(dragRect.Contains(togglePosition), Is.False);
        }

        [Test]
        public void HasDragStarted_RequiresMovementBeyondTheSubListThreshold()
        {
            Vector2 startPosition = new Vector2(100f, 100f);

            bool smallMovementStartedDrag = InvokeActionEditorUtility.HasDragStarted(
                startPosition,
                new Vector2(105f, 100f),
                8f);
            bool largeMovementStartedDrag = InvokeActionEditorUtility.HasDragStarted(
                startPosition,
                new Vector2(109f, 100f),
                8f);

            Assert.That(smallMovementStartedDrag, Is.False);
            Assert.That(largeMovementStartedDrag, Is.True);
        }

        [Test]
        public void ExtractNestedAction_PreservesTheEmptySourceGroupAndTheExtractedAction()
        {
            UnityEngine.Object.DestroyImmediate(command);
            Blackboard blackboard = hostObject.AddComponent<Blackboard>();
            Block block = hostObject.AddComponent<Block>();
            block.EnsureTracksInitialized();
            command = hostObject.AddComponent<InvokeActionCommand>();
            IAction extractedAction = new CameraZoom();
            command.actions.Add(new InvokeActionCommand.ActionWrapper(extractedAction));
            command.DisplayAsGroup = true;
            command.ParentBlock = block;
            command.ItemId = blackboard.NextItemId();
            block.CommandList.Add(command);

            SerializedObject serializedBlock = new SerializedObject(block);
            SerializedProperty tracksProperty = serializedBlock.FindProperty("tracks");
            SerializedProperty commandsProperty = tracksProperty
                .GetArrayElementAtIndex(0)
                .FindPropertyRelative("commands");
            CommandListAdaptor adaptor = new CommandListAdaptor(block, commandsProperty);
            Type dragType = typeof(CommandListAdaptor).GetNestedType(
                "NestedActionDrag",
                BindingFlags.NonPublic);
            object drag = Activator.CreateInstance(
                dragType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new object[] { command, 0, Vector2.zero },
                null);
            MethodInfo extractMethod = typeof(CommandListAdaptor).GetMethod(
                "MoveNestedActionToStandaloneGroup",
                BindingFlags.Instance | BindingFlags.NonPublic);

            extractMethod.Invoke(adaptor, new object[] { drag, 0 });
            serializedBlock.ApplyModifiedProperties();

            Assert.That(block.CommandList, Has.Count.EqualTo(2));
            InvokeActionCommand extractedCommand = block.CommandList[0] as InvokeActionCommand;
            Assert.That(extractedCommand, Is.Not.Null);
            Assert.That(extractedCommand.actions, Has.Count.EqualTo(1));
            Assert.That(extractedCommand.actions[0], Is.SameAs(extractedAction));
            Assert.That(block.CommandList[1], Is.SameAs(command));
            Assert.That(command.actions, Is.Empty);
            Assert.That(command.DisplayAsGroup, Is.True);
        }

        [Test]
        public void ParentDragSuppression_KeepsTheDragGutterVisibleDuringRepaint()
        {
            bool suppressDuringMouseDrag = InvokeActionEditorUtility.ShouldTemporarilySuppressParentDrag(
                true,
                EventType.MouseDrag);
            bool suppressDuringRepaint = InvokeActionEditorUtility.ShouldTemporarilySuppressParentDrag(
                true,
                EventType.Repaint);

            Assert.That(suppressDuringMouseDrag, Is.True);
            Assert.That(suppressDuringRepaint, Is.False);
        }

        [Test]
        public void ParentCommandDrag_IgnoresAPendingNestedActionDrag()
        {
            UnityEngine.Object.DestroyImmediate(command);
            Blackboard blackboard = hostObject.AddComponent<Blackboard>();
            Block block = hostObject.AddComponent<Block>();
            block.EnsureTracksInitialized();
            command = hostObject.AddComponent<InvokeActionCommand>();
            command.actions.Add(new InvokeActionCommand.ActionWrapper(new CameraZoom()));
            command.DisplayAsGroup = true;
            command.ParentBlock = block;
            command.ItemId = blackboard.NextItemId();
            block.CommandList.Add(command);

            SerializedObject serializedBlock = new SerializedObject(block);
            SerializedProperty commandsProperty = serializedBlock.FindProperty("tracks")
                .GetArrayElementAtIndex(0)
                .FindPropertyRelative("commands");
            CommandListAdaptor adaptor = new CommandListAdaptor(block, commandsProperty);
            Type dragType = typeof(CommandListAdaptor).GetNestedType(
                "NestedActionDrag",
                BindingFlags.NonPublic);
            object pendingDrag = Activator.CreateInstance(
                dragType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new object[] { command, 0, Vector2.zero },
                null);
            typeof(CommandListAdaptor).GetField(
                "_pendingNestedActionDrag",
                BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(adaptor, pendingDrag);
            ReorderableList commandList = (ReorderableList)typeof(CommandListAdaptor).GetField(
                "list",
                BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(adaptor);
            commandList.index = 0;
            MethodInfo handleCommandMouseDrag = typeof(CommandListAdaptor).GetMethod(
                "HandleCommandMouseDrag",
                BindingFlags.Instance | BindingFlags.NonPublic);

            handleCommandMouseDrag.Invoke(adaptor, new object[] { commandList });

            bool commandDragIsActive = (bool)typeof(CommandListAdaptor).GetField(
                "_commandReorderDragActive",
                BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(adaptor);
            Assert.That(commandDragIsActive, Is.False);
        }

        [Test]
        public void CommandDropRects_UseTheFullRowToSelectAnExactStandaloneIndex()
        {
            Rect commandRect = new Rect(12f, 20f, 240f, 26f);

            Rect beforeRect = InvokeActionEditorUtility.GetCommandBeforeDropRect(commandRect, 13f);
            Rect afterRect = InvokeActionEditorUtility.GetCommandAfterDropRect(commandRect, 13f);

            Assert.That(beforeRect.Contains(new Vector2(commandRect.center.x, commandRect.y + 2f)), Is.True);
            Assert.That(beforeRect.Contains(new Vector2(commandRect.center.x, commandRect.yMax - 2f)), Is.False);
            Assert.That(afterRect.Contains(new Vector2(commandRect.center.x, commandRect.yMax - 2f)), Is.True);
            Assert.That(afterRect.Contains(new Vector2(commandRect.center.x, commandRect.y + 2f)), Is.False);
        }

        [Test]
        public void CommandDropRects_LeaveAnInvokeGroupCenterAvailableForNestedInsertion()
        {
            Rect groupRect = new Rect(12f, 20f, 240f, 92f);

            Rect beforeRect = InvokeActionEditorUtility.GetCommandBeforeDropRect(groupRect, 8f);
            Rect afterRect = InvokeActionEditorUtility.GetCommandAfterDropRect(groupRect, 8f);

            Assert.That(beforeRect.Contains(groupRect.center), Is.False);
            Assert.That(afterRect.Contains(groupRect.center), Is.False);
            Assert.That(beforeRect.Contains(new Vector2(groupRect.center.x, groupRect.y + 2f)), Is.True);
            Assert.That(afterRect.Contains(new Vector2(groupRect.center.x, groupRect.yMax - 2f)), Is.True);
        }

        [Test]
        public void CanAcceptActionDrop_RejectsAStandaloneActionWrapper()
        {
            InvokeActionCommand standaloneAction = hostObject.AddComponent<InvokeActionCommand>();
            standaloneAction.actions.Add(new InvokeActionCommand.ActionWrapper(new CameraZoom()));

            bool canAcceptDrop = InvokeActionEditorUtility.CanAcceptActionDrop(standaloneAction);

            Assert.That(canAcceptDrop, Is.False);
        }

        [Test]
        public void CanAcceptActionDrop_AcceptsAnEmptyInvokeContainer()
        {
            InvokeActionCommand emptyContainer = hostObject.AddComponent<InvokeActionCommand>();

            bool canAcceptDrop = InvokeActionEditorUtility.CanAcceptActionDrop(emptyContainer);

            Assert.That(canAcceptDrop, Is.True);
        }

        [Test]
        public void ResolveReorderSource_PreservesTheCommandCapturedBeforeUnityReordersTheList()
        {
            InvokeActionCommand capturedSource = hostObject.AddComponent<InvokeActionCommand>();
            InvokeActionCommand destination = hostObject.AddComponent<InvokeActionCommand>();

            InvokeActionCommand resolvedSource = InvokeActionEditorUtility.ResolveReorderSource(
                capturedSource,
                destination);

            Assert.That(resolvedSource, Is.SameAs(capturedSource));
        }

        [Test]
        public void CallStringMethod_TargetObjectUsesTheUnifiedValueReference()
        {
            FieldInfo targetObject = typeof(CallStringMethod).GetField(
                "targetObjectData",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(targetObject, Is.Not.Null);
            Assert.That(targetObject.FieldType, Is.EqualTo(typeof(GameObjectData)));
        }



        [Serializable]
        private sealed class WarningSummaryAction : ActionBase
        {
            public override string GetSummary()
            {
                return "Warning: Optional reference is not set.";
            }
        }

        [Serializable]
        private sealed class HeaderSummaryAction : ActionBase
        {
            public override string GetSummary()
            {
                return "Player joined the lobby";
            }
        }
    }
}
