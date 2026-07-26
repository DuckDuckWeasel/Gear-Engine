using System;
using GearEngine.Core.Actions;

using UnityEngine;
using UnityEngine.Serialization;

namespace Scaffold
{
    /// <summary>
    /// Loads a new Unity scene and displays an optional loading image. This is useful
    /// for splitting a large game across multiple scene files to reduce peak memory
    /// usage. Previously loaded assets will be released before loading the scene to free up memory.
    /// The scene to be loaded must be added to the scene list in Build Settings.")]
    /// </summary>
    [CommandInfo("Flow",
                 "Load Scene",
                 "Loads a new Unity scene and displays an optional loading image. This is useful " +
                 "for splitting a large game across multiple scene files to reduce peak memory " +
                 "usage. Previously loaded assets will be released before loading the scene to free up memory." +
                 "The scene to be loaded must be added to the scene list in Build Settings.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [Serializable]
    public class LoadScene : ActionBase
    {
        [Tooltip("Name of the scene to load. The scene must also be added to the build settings.")]
        [SerializeField] protected StringData sceneName = new StringData("");

        [Tooltip("Image to display while loading the scene")]
        [SerializeField] protected Texture2D loadingImage;

        #region Public members

        public override void OnEnter()
        {
            SceneLoader.LoadScene(sceneName.Value, loadingImage);
        }

        public override string GetSummary()
        {
            if (sceneName.Value.Length == 0)
            {
                return "Error: No scene name selected";
            }

            return sceneName.Value;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return sceneName.stringRef == variable ||
                base.HasReference(variable);
        }

        #endregion

    }
}