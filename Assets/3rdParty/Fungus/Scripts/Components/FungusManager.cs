// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using TriInspector;
using UnityEngine;

namespace Fungus
{
    /// <summary>
    /// Fungus manager singleton. Manages access to all Fungus singletons in a consistent manner.
    /// </summary>
    [RequireComponent(typeof(CameraManager))]
    [RequireComponent(typeof(MusicManager))]
    [RequireComponent(typeof(EventDispatcher))]
    [RequireComponent(typeof(GlobalVariables))]
#if UNITY_5_3_OR_NEWER
    [RequireComponent(typeof(SaveManager))]
    [RequireComponent(typeof(NarrativeLog))]
#endif
    public sealed class FungusManager : MonoBehaviour
    {
        [ShowInInspector]
        private static volatile FungusManager instance;  // The keyword "volatile" is friendly to the multi-thread.
        private static readonly object _lock = new object();  // The keyword "readonly" is friendly to the multi-thread.

        private void Awake()
        {
            if (instance != this && instance != null)
            {                    
                Destroy(instance.gameObject);
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            CameraManager = GetComponent<CameraManager>();
            MusicManager = GetComponent<MusicManager>();
            EventDispatcher = GetComponent<EventDispatcher>();
            GlobalVariables = GetComponent<GlobalVariables>();
#if UNITY_5_3_OR_NEWER
            SaveManager = GetComponent<SaveManager>();
            NarrativeLog = GetComponent<NarrativeLog>();
#endif
        }
        #region Public methods

        /// <summary>
        /// Gets the camera manager singleton instance.
        /// </summary>
        public CameraManager CameraManager { get; private set; }

        /// <summary>
        /// Gets the music manager singleton instance.
        /// </summary>
        public MusicManager MusicManager { get; private set; }

        /// <summary>
        /// Gets the event dispatcher singleton instance.
        /// </summary>
        public EventDispatcher EventDispatcher { get; private set; }

        /// <summary>
        /// Gets the global variable singleton instance.
        /// </summary>
        public GlobalVariables GlobalVariables { get; private set; }

#if UNITY_5_3_OR_NEWER
        /// <summary>
        /// Gets the save manager singleton instance.
        /// </summary>
        public SaveManager SaveManager { get; private set; }
        
        /// <summary>
        /// Gets the history manager singleton instance.
        /// </summary>
        public NarrativeLog NarrativeLog { get; private set; }
#endif

        /// <summary>
        /// Gets the FungusManager singleton instance.
        /// </summary>
        public static FungusManager Instance
        {
            get
            {
                if (!Application.isPlaying)
                {
                    Debug.LogWarning("FungusManager.Instance() was called while application is quitting. Returning null instead.");
                    return null;
                }

                if (instance == null)
                {
                    // Attempt to find an existing instance in the scene
                    instance = FindObjectOfType<FungusManager>();
                    if (instance == null)
                    {
                        // If no instance exists, create a new one
                        GameObject go = new GameObject("FungusManager");
                        DontDestroyOnLoad(go);
                        instance = go.AddComponent<FungusManager>();
                        Debug.Log("FungusManager instance was created automatically.");
                    }
                }

                return instance;
            }
        }

        #endregion
    }
}