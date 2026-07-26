using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// AudioClip variable type.
    /// </summary>
    [VariableInfo("Other", "AudioClip")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class AudioClipVariable : VariableBase<AudioClip>
    {
    }

    /// <summary>
    /// Container for an AudioClip variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct AudioClipData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(AudioClipVariable))]
        public AudioClipVariable audioClipRef;

        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public AudioClipValueSO audioClipSO;

        [SerializeField]
        public AudioClip audioClipVal;

        public static implicit operator AudioClip(AudioClipData audioClipData)
        {
            return audioClipData.Value;
        }

        public AudioClipData(AudioClip v)
        {
            audioClipVal = v;
            audioClipRef = null;
            source = VariableDataSource.Unspecified;
            audioClipSO = null;
        }

        public AudioClip Value
        {
            get { return VariableValueReference.Resolve(audioClipRef, audioClipVal, audioClipSO, source); }
            set { VariableValueReference.Assign(audioClipRef, ref audioClipVal, audioClipSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(audioClipRef, audioClipVal, audioClipSO, source);
        }
    }
}
