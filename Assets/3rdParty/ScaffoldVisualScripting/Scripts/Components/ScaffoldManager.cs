
using TriInspector;
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Scaffold manager singleton. Manages access to all Scaffold singletons in a consistent manner.
    /// </summary>
    [RequireComponent(typeof(CameraManager))]
    [RequireComponent(typeof(EventDispatcher))]
    [RequireComponent(typeof(GlobalVariables))]
#if UNITY_5_3_OR_NEWER
    [RequireComponent(typeof(SaveManager))]
    [RequireComponent(typeof(NarrativeLog))]
#endif
    public sealed class ScaffoldManager : MonoBehaviour
    {
        [ShowInInspector]
        private static volatile ScaffoldManager instance;  // The keyword "volatile" is friendly to the multi-thread.
        private static readonly object _lock = new object();  // The keyword "readonly" is friendly to the multi-thread.

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
        }

        private void Awake()
        {
            if (instance != this && instance != null)
            {
                Destroy(instance.gameObject);
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            CameraManager = GetComponent<CameraManager>();
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
        /// Gets the ScaffoldManager singleton instance.
        /// </summary>
        public static ScaffoldManager Instance
        {
            get
            {
                if (!Application.isPlaying)
                {
                    Debug.LogWarning("ScaffoldManager.Instance() was called while application is quitting. Returning null instead.");
                    return null;
                }

                if (instance == null)
                {
                    // If no instance exists, create a new one
                    GameObject go = new GameObject("ScaffoldManager");
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<ScaffoldManager>();
                    Debug.Log("ScaffoldManager instance was created automatically.");
                }

                return instance;
            }
        }

        #endregion
    }
}