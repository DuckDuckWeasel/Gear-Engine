using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GearEngine.Core.Actions;
using Scaffold;
using Scaffold.VisualScripting;
using Scaffold.VisualScripting.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GearEngine.GearEngine.Editor
{
    [InitializeOnLoad]
    internal static class ExecutionMatrixSceneBuilder
    {
        internal const string k_scenePath = "Assets/GearEngine/Scenes/Test/TestTutorialScene.unity";
        private const string k_containerName = "Execution Matrix";
        private const string k_requestPath = "Temp/GenerateExecutionMatrixScene.request";

        private static readonly FieldInfo s_debugLogMessageField = typeof(DebugLog).GetField(
            "logMessage",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly IReadOnlyList<ExecutionCase> s_cases = new[]
        {
            new ExecutionCase("Sequence - Ordered", ActionListExecutionMethod.Sequence, ActionListOrderMode.Ordered),
            new ExecutionCase("Sequence - Random", ActionListExecutionMethod.Sequence, ActionListOrderMode.Random),
            new ExecutionCase("Sequence - Shuffle", ActionListExecutionMethod.Sequence, ActionListOrderMode.Shuffle),
            new ExecutionCase("Selector - Ordered", ActionListExecutionMethod.Selector, ActionListOrderMode.Ordered),
            new ExecutionCase("Selector - Random", ActionListExecutionMethod.Selector, ActionListOrderMode.Random),
            new ExecutionCase("Selector - Shuffle", ActionListExecutionMethod.Selector, ActionListOrderMode.Shuffle),
            new ExecutionCase("Parallel - Wait All", ActionListExecutionMethod.Parallel, ActionListAwaitMode.WaitAll),
            new ExecutionCase("Parallel - Wait Any", ActionListExecutionMethod.Parallel, ActionListAwaitMode.WaitAny),
            new ExecutionCase("Parallel - Wait None", ActionListExecutionMethod.Parallel, ActionListAwaitMode.WaitNone),
            new ExecutionCase("Parallel Selector - Wait All", ActionListExecutionMethod.ParallelSelector, ActionListAwaitMode.WaitAll),
            new ExecutionCase("Parallel Selector - Wait Any", ActionListExecutionMethod.ParallelSelector, ActionListAwaitMode.WaitAny),
            new ExecutionCase("Parallel Selector - Wait None", ActionListExecutionMethod.ParallelSelector, ActionListAwaitMode.WaitNone),
            new ExecutionCase("Utility Selector - Utility", ActionListExecutionMethod.UtilitySelector),
        };

        static ExecutionMatrixSceneBuilder()
        {
            EditorApplication.delayCall += TryProcessPendingRequest;
        }

        [MenuItem("GearEngine/Testing/Generate Execution Matrix Scene")]
        private static void GenerateFromMenu()
        {
            try
            {
                GenerateAndSave();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[ExecutionMatrix] Failed to generate the scene matrix: {exception.Message}\n{exception.StackTrace}");
            }
        }

        private static void TryProcessPendingRequest()
        {
            if (!File.Exists(k_requestPath))
            {
                return;
            }

            try
            {
                File.Delete(k_requestPath);
                GenerateAndSave();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[ExecutionMatrix] Failed to process the pending generation request: {exception.Message}\n{exception.StackTrace}");
            }
        }

        internal static void GenerateAndSave()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!string.Equals(scene.path, k_scenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Open '{k_scenePath}' before generating the execution matrix.");
            }

            GameObject existingContainer = GameObject.Find(k_containerName);
            if (existingContainer != null && existingContainer.scene == scene)
            {
                UnityEngine.Object.DestroyImmediate(existingContainer);
            }

            GameObject container = new GameObject(k_containerName);
            SceneManager.MoveGameObjectToScene(container, scene);
            BlackboardLifetimeScope lifetimeScope = container.AddComponent<BlackboardLifetimeScope>();
            BlackboardBehaviour behaviour = container.AddComponent<BlackboardBehaviour>();
            BlackboardDefinition definition = new BlackboardDefinition
            {
                Name = "Execution Matrix",
            };
            foreach (ExecutionCase executionCase in s_cases)
            {
                BlockDefinition block = CreateExecutionCase(executionCase);
                definition.Blocks.Add(block);
                int blockIndex = definition.Blocks.Count - 1;
                behaviour.AuthoringMetadata.BlockLayouts.Add(
                    new Scaffold.VisualScripting.Authoring.BlockAuthoringMetadata(
                        block.DefinitionId,
                        new Rect(
                            40f + blockIndex % 3 * 340f,
                            40f + blockIndex / 3 * 220f,
                            300f,
                            180f)));
            }

            behaviour.DefinitionReference.SetDirect(definition);
            EditorUtility.SetDirty(lifetimeScope);
            EditorUtility.SetDirty(behaviour);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException($"Unity could not save '{k_scenePath}'.");
            }

            Selection.activeGameObject = container;
            Debug.Log($"[ExecutionMatrix] Generated {s_cases.Count} execution cases in '{k_scenePath}'.");
        }

        private static BlockDefinition CreateExecutionCase(ExecutionCase executionCase)
        {
            BlockDefinition block = new BlockDefinition
            {
                Name = executionCase.Name,
                Trigger = new GameStartedTriggerDefinition(),
                ExecutionMethod = executionCase.ExecutionMethod,
                OrderMode = executionCase.OrderMode,
                AwaitMode = executionCase.AwaitMode,
            };
            ActionTrackDefinition track = new ActionTrackDefinition
            {
                Name = "Primary",
            };
            track.ActionList.Actions.Add(CreateDebugLog($"[{executionCase.Name}] Start", 10f));
            track.ActionList.Actions.Add(new Wait { Utility = 20f });
            track.ActionList.Actions.Add(CreateDebugLog($"[{executionCase.Name}] End", 30f));
            block.Tracks.Add(track);
            return block;
        }

        private static DebugLog CreateDebugLog(string message, float utility)
        {
            if (s_debugLogMessageField == null)
            {
                throw new MissingFieldException(typeof(DebugLog).FullName, "logMessage");
            }

            DebugLog debugLog = new DebugLog();
            s_debugLogMessageField.SetValue(debugLog, new StringDataMulti(message));
            debugLog.Utility = utility;
            return debugLog;
        }

        private readonly struct ExecutionCase
        {
            public ExecutionCase(
                string name,
                ActionListExecutionMethod executionMethod,
                ActionListOrderMode orderMode = ActionListOrderMode.Ordered)
            {
                Name = name;
                ExecutionMethod = executionMethod;
                OrderMode = orderMode;
                AwaitMode = ActionListAwaitMode.WaitAll;
            }

            public ExecutionCase(
                string name,
                ActionListExecutionMethod executionMethod,
                ActionListAwaitMode awaitMode)
            {
                Name = name;
                ExecutionMethod = executionMethod;
                OrderMode = ActionListOrderMode.Ordered;
                AwaitMode = awaitMode;
            }

            public string Name { get; }

            public ActionListExecutionMethod ExecutionMethod { get; }

            public ActionListOrderMode OrderMode { get; }

            public ActionListAwaitMode AwaitMode { get; }
        }
    }
}
