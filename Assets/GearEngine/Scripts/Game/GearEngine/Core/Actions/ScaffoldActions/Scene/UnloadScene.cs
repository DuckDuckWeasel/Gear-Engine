using GearEngine.Core.Actions;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scaffold
{
    [CommandInfo("Scene", "Unload Scene", "Unloads an Additive Scene asynchronously.")]
    [Serializable]
    [AddComponentMenu("")]
    public class UnloadScene : ActionBase
    {
        [Tooltip("Name of the scene to unload")]
        [SerializeField] protected StringData sceneName;

        public override void OnEnter()
        {
            if (!string.IsNullOrEmpty(sceneName.Value))
            {
                SceneManager.UnloadSceneAsync(sceneName.Value);
            }
            Continue();
        }

        public override string GetSummary()
        {
            return $"Unload '{sceneName.Value}'";
        }
        
        public override Color GetButtonColor() { return new Color32(199, 204, 219, 255); }
    }
}
