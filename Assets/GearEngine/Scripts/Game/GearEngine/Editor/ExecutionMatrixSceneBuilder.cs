using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GearEngine.Core.Actions;
using GearEngine.GearEngine.Presentation.UI.Input;
using Scaffold;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GearEngine.GearEngine.Editor
{
    [InitializeOnLoad]
    internal static class ExecutionMatrixSceneBuilder
    {
        private const string ScenePath = "Assets/GearEngine/Scenes/Test/Test Tutorial Scene.unity";
        private const string ContainerName = "Execution Matrix";
        private const string RequestPath = "Temp/GenerateExecutionMatrixScene.request";

        private static readonly FieldInfo DebugLogMessageField = typeof(DebugLog).GetField(
            "logMessage",
            BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly IReadOnlyList<ExecutionCase> Cases = new[]
        {
            new ExecutionCase("Sequence - Ordered", CompositeExecutionMethod.Sequence, CompositeOrderMode.Ordered),
            new ExecutionCase("Sequence - Random", CompositeExecutionMethod.Sequence, CompositeOrderMode.Random),
            new ExecutionCase("Sequence - Shuffle", CompositeExecutionMethod.Sequence, CompositeOrderMode.Shuffle),
            new ExecutionCase("Selector - Ordered", CompositeExecutionMethod.Selector, CompositeOrderMode.Ordered),
            new ExecutionCase("Selector - Random", CompositeExecutionMethod.Selector, CompositeOrderMode.Random),
            new ExecutionCase("Selector - Shuffle", CompositeExecutionMethod.Selector, CompositeOrderMode.Shuffle),
            new ExecutionCase("Parallel - Wait All", CompositeExecutionMethod.Parallel, CompositeAwaitMode.WaitAll),
            new ExecutionCase("Parallel - Wait Any", CompositeExecutionMethod.Parallel, CompositeAwaitMode.WaitAny),
            new ExecutionCase("Parallel - Wait None", CompositeExecutionMethod.Parallel, CompositeAwaitMode.WaitNone),
            new ExecutionCase("Parallel Selector - Wait All", CompositeExecutionMethod.ParallelSelector, CompositeAwaitMode.WaitAll),
            new ExecutionCase("Parallel Selector - Wait Any", CompositeExecutionMethod.ParallelSelector, CompositeAwaitMode.WaitAny),
            new ExecutionCase("Parallel Selector - Wait None", CompositeExecutionMethod.ParallelSelector, CompositeAwaitMode.WaitNone),
            new ExecutionCase("Utility Selector - Utility", CompositeExecutionMethod.UtilitySelector),
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
            if (!File.Exists(RequestPath))
            {
                return;
            }

            try
            {
                File.Delete(RequestPath);
                GenerateAndSave();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[ExecutionMatrix] Failed to process the pending generation request: {exception.Message}\n{exception.StackTrace}");
            }
        }

        private static void GenerateAndSave()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!string.Equals(scene.path, ScenePath, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Open '{ScenePath}' before generating the execution matrix.");
            }

            GameObject existingContainer = GameObject.Find(ContainerName);
            if (existingContainer != null && existingContainer.scene == scene)
            {
                UnityEngine.Object.DestroyImmediate(existingContainer);
            }

            GameObject container = new GameObject(ContainerName);
            SceneManager.MoveGameObjectToScene(container, scene);
            foreach (ExecutionCase executionCase in Cases)
            {
                CreateExecutionCase(container.transform, executionCase);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException($"Unity could not save '{ScenePath}'.");
            }

            Selection.activeGameObject = container;
            Debug.Log($"[ExecutionMatrix] Generated {Cases.Count} execution cases in '{ScenePath}'.");
        }

        private static void CreateExecutionCase(
            Transform container,
            ExecutionCase executionCase)
        {
            GameObject host = new GameObject(executionCase.Name);
            host.transform.SetParent(container, false);

            Blackboard blackboard = host.AddComponent<Blackboard>();
            Block block = blackboard.CreateBlock(Vector2.zero);
            block.BlockName = executionCase.Name;
            block.ExecutionMethod = executionCase.ExecutionMethod;
            block.OrderMode = executionCase.OrderMode;
            block.AwaitMode = executionCase.AwaitMode;

            GameStarted gameStarted = host.AddComponent<GameStarted>();
            gameStarted.ParentBlock = block;
            block._EventHandler = gameStarted;

            InvokeActionCommand firstCommand = block.CommandList.Count > 0
                ? block.CommandList[0] as InvokeActionCommand
                : null;
            if (firstCommand == null)
            {
                firstCommand = host.AddComponent<InvokeActionCommand>();
                block.CommandList.Add(firstCommand);
            }

            ConfigureCommand(
                firstCommand,
                blackboard,
                block,
                CreateDebugLog($"[{executionCase.Name}] Start"),
                10f);
            AddCommand(host, blackboard, block, new Wait(), 20f);
            AddCommand(
                host,
                blackboard,
                block,
                CreateDebugLog($"[{executionCase.Name}] End"),
                30f);

            CommandTrack primaryTrack = block.Tracks[0];
            for (int commandIndex = 0; commandIndex < primaryTrack.Commands.Count; commandIndex++)
            {
                Command command = primaryTrack.Commands[commandIndex];
                command.ParentBlock = block;
                command.ParentTrack = primaryTrack;
                command.CommandIndex = commandIndex;
            }
        }

        private static void AddCommand(
            GameObject host,
            Blackboard blackboard,
            Block block,
            IAction action,
            float utility)
        {
            InvokeActionCommand command = host.AddComponent<InvokeActionCommand>();
            block.CommandList.Add(command);
            ConfigureCommand(command, blackboard, block, action, utility);
        }

        private static void ConfigureCommand(
            InvokeActionCommand command,
            Blackboard blackboard,
            Block block,
            IAction action,
            float utility)
        {
            command.ItemId = blackboard.NextItemId();
            command.ParentBlock = block;
            command.CompositeUtility = utility;
            command.InsertAction(0, action, true);
        }

        private static DebugLog CreateDebugLog(string message)
        {
            if (DebugLogMessageField == null)
            {
                throw new MissingFieldException(typeof(DebugLog).FullName, "logMessage");
            }

            DebugLog debugLog = new DebugLog();
            DebugLogMessageField.SetValue(debugLog, new StringDataMulti(message));
            return debugLog;
        }

        private readonly struct ExecutionCase
        {
            public ExecutionCase(
                string name,
                CompositeExecutionMethod executionMethod,
                CompositeOrderMode orderMode = CompositeOrderMode.Ordered)
            {
                Name = name;
                ExecutionMethod = executionMethod;
                OrderMode = orderMode;
                AwaitMode = CompositeAwaitMode.WaitAll;
            }

            public ExecutionCase(
                string name,
                CompositeExecutionMethod executionMethod,
                CompositeAwaitMode awaitMode)
            {
                Name = name;
                ExecutionMethod = executionMethod;
                OrderMode = CompositeOrderMode.Ordered;
                AwaitMode = awaitMode;
            }

            public string Name { get; }

            public CompositeExecutionMethod ExecutionMethod { get; }

            public CompositeOrderMode OrderMode { get; }

            public CompositeAwaitMode AwaitMode { get; }
        }
    }
}
