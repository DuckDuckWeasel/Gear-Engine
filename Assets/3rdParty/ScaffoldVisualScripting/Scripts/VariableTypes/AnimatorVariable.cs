
using UnityEngine;

namespace Scaffold
{
    /// <summary>
    /// Animator variable type.
    /// </summary>
    [VariableInfo("Other", "Animator")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class AnimatorVariable : VariableBase<Animator>
    {
    }

    /// <summary>
    /// Container for an Animator variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct AnimatorData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(AnimatorVariable))]
        public AnimatorVariable animatorRef;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public AnimatorValueSO animatorSO;
        [SerializeField]
        public Animator animatorVal;

        public static implicit operator Animator(AnimatorData animatorData)
        {
            return animatorData.Value;
        }

        public AnimatorData(Animator v)
        {
            animatorVal = v;
            animatorRef = null;
            source = VariableDataSource.Unspecified;
            animatorSO = null;
        }

        public Animator Value
        {
            get { return VariableValueReference.Resolve(animatorRef, animatorVal, animatorSO, source); }
            set { VariableValueReference.Assign(animatorRef, ref animatorVal, animatorSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(animatorRef, animatorVal, animatorSO, source);
        }
    }
}