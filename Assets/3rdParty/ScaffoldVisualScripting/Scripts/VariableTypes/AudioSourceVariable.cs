
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// AudioSource variable type.
    /// </summary>
    [VariableInfo("Other", "AudioSource")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class AudioSourceVariable : VariableBase<AudioSource>
    {
    }

    /// <summary>
    /// Container for an AudioSource variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct AudioSourceData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(AudioSourceVariable))]
        public AudioSourceVariable audioSourceRef;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public AudioSourceValueSO audioSourceSO;
        [SerializeField]
        public AudioSource audioSourceVal;

        public static implicit operator AudioSource(AudioSourceData audioSourceData)
        {
            return audioSourceData.Value;
        }

        public AudioSourceData(AudioSource v)
        {
            audioSourceVal = v;
            audioSourceRef = null;
            source = VariableDataSource.Unspecified;
            audioSourceSO = null;
        }

        public AudioSource Value
        {
            get { return VariableValueReference.Resolve(audioSourceRef, audioSourceVal, audioSourceSO, source); }
            set { VariableValueReference.Assign(audioSourceRef, ref audioSourceVal, audioSourceSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(audioSourceRef, audioSourceVal, audioSourceSO, source);
        }
    }
}