using System;
using System.Linq;
using System.Reflection;
using Scaffold;
using Scaffold.VisualScripting;
using Scaffold.VisualScripting.Unity;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GearEngine.GearEngine.Editor
{
    internal static class UIEffectsForEachDemoSceneBuilder
    {
        internal const string k_scenePath =
            "Assets/GearEngine/Scenes/Test/UIEffectsForEachDemo.unity";

        private const string k_buttonName = "CycleUIEffectButton";
        private const string k_labelName = "EffectDescription";

        private static readonly FieldInfo s_targetGameObjectField =
            typeof(CycleUIEffectPreset).GetField(
                "targetGameObject",
                BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo s_targetLabelField =
            typeof(CycleUIEffectPreset).GetField(
                "targetLabel",
                BindingFlags.Instance | BindingFlags.NonPublic);

        [MenuItem("GearEngine/Testing/Rebuild UI Effects Demo Blackboard")]
        public static void RebindAndSave()
        {
            try
            {
                RebindAndSaveInternal();
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[UIEffectsDemo] Failed to rebuild the scene: {exception.Message}\n{exception.StackTrace}");
                throw;
            }
        }

        private static void RebindAndSaveInternal()
        {
            Scene scene = EditorSceneManager.OpenScene(
                k_scenePath,
                OpenSceneMode.Single);
            Button button = FindRequiredComponent<Button>(k_buttonName);
            Text label = FindRequiredComponent<Text>(k_labelName);
            BlackboardBehaviour behaviour =
                UnityEngine.Object.FindObjectsByType<BlackboardBehaviour>()
                    .Single(candidate => candidate.gameObject.scene == scene);
            BlackboardDefinition definition =
                behaviour.DefinitionReference.ResolveDefinition();
            BlockDefinition block = definition.Blocks.Single();
            BindableTriggerDefinition trigger =
                block.Trigger as BindableTriggerDefinition ??
                throw new InvalidOperationException(
                    "The UI effects demo requires a bindable Button trigger.");
            ButtonTriggerSignalSource source =
                trigger.Source as ButtonTriggerSignalSource ??
                throw new InvalidOperationException(
                    "The UI effects demo trigger requires a Button signal source.");
            CycleUIEffectPreset action = block.Tracks
                .SelectMany(track => track.ActionList.Actions)
                .OfType<CycleUIEffectPreset>()
                .Single();

            source.Target = button;
            SetRequiredField(
                s_targetGameObjectField,
                action,
                new GameObjectData(button.gameObject));
            SetRequiredField(s_targetLabelField, action, label);

            EditorUtility.SetDirty(behaviour);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException(
                    $"Unity could not save '{k_scenePath}'.");
            }

            Debug.Log(
                $"[UIEffectsDemo] Rebuilt managed Blackboard references in '{k_scenePath}'.");
        }

        private static T FindRequiredComponent<T>(string objectName)
            where T : Component
        {
            GameObject gameObject = GameObject.Find(objectName);
            if (gameObject == null ||
                !gameObject.TryGetComponent(out T component))
            {
                throw new InvalidOperationException(
                    $"Scene object '{objectName}' requires {typeof(T).Name}.");
            }

            return component;
        }

        private static void SetRequiredField(
            FieldInfo field,
            object target,
            object value)
        {
            if (field == null)
            {
                throw new MissingFieldException(
                    typeof(CycleUIEffectPreset).FullName,
                    "managed action reference");
            }

            field.SetValue(target, value);
        }
    }
}
