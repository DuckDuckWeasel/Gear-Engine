
#if UNITY_5_3_OR_NEWER

using UnityEngine;
using System.Collections.Generic;

namespace Scaffold
{
    /// <summary>
    /// This component encodes and decodes a list of game objects to be saved for each Save Point.
    /// It knows how to encode / decode concrete game classes like Blackboard and BlackboardData.
    /// To extend the save system to handle other data types, just modify or subclass this component.
    /// </summary>
    public class SaveData : MonoBehaviour
    {
        protected const string BlackboardDataKey = "BlackboardData";

        protected const string NarrativeLogKey = "NarrativeLogData";

        [Tooltip("A list of Blackboard objects whose variables will be encoded in the save data. Boolean, Integer, Float and String variables are supported.")]
        [SerializeField] protected List<Blackboard> blackboards = new List<Blackboard>();

        public static SaveData Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        #region Public methods

        /// <summary>
        /// Encodes the objects to be saved as a list of SaveDataItems.
        /// </summary
        public virtual void Encode(List<SaveDataItem> saveDataItems)
        {
            for (int i = 0; i < blackboards.Count; i++)
            {
                Blackboard blackboard = blackboards[i];
                BlackboardData blackboardData = BlackboardData.Encode(blackboard);

                SaveDataItem saveDataItem = SaveDataItem.Create(BlackboardDataKey, JsonUtility.ToJson(blackboardData));
                saveDataItems.Add(saveDataItem);

                SaveDataItem narrativeLogItem = SaveDataItem.Create(NarrativeLogKey, ScaffoldManager.Instance.NarrativeLog.GetJsonHistory());
                saveDataItems.Add(narrativeLogItem);
            }
        }

        /// <summary>
        /// Decodes the loaded list of SaveDataItems to restore the saved game state.
        /// </summary>
        public virtual void Decode(List<SaveDataItem> saveDataItems)
        {
            for (int i = 0; i < saveDataItems.Count; i++)
            {
                SaveDataItem saveDataItem = saveDataItems[i];
                if (saveDataItem == null)
                {
                    continue;
                }

                if (saveDataItem.DataType == BlackboardDataKey)
                {
                    BlackboardData blackboardData = JsonUtility.FromJson<BlackboardData>(saveDataItem.Data);
                    if (blackboardData == null)
                    {
                        Debug.LogError("Failed to decode Blackboard save data item");
                        return;
                    }

                    BlackboardData.Decode(blackboardData);
                }

                if (saveDataItem.DataType == NarrativeLogKey)
                {
                    ScaffoldManager.Instance.NarrativeLog.LoadHistory(saveDataItem.Data);
                }
            }
        }

        #endregion
    }
}

#endif