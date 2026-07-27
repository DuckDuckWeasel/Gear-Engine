using System;
using UnityEngine;

namespace Scaffold.Tutorial.Variables
{
    /// <summary>
    /// Odin-free base for ScriptableObject references.
    /// Uses [SerializeField] so Unity serializes the data natively.
    /// </summary>
    [Serializable]
    public abstract class TutorialScriptableObjectReference<T> : ScriptableObject
    {
        [SerializeField, HideInInspector]
        protected T data;

        public virtual T Data
        {
            get => data;
            set => data = value;
        }
    }
}
