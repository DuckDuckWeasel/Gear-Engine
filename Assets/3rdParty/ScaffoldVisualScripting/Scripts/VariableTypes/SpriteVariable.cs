
using UnityEngine;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Sprite variable type.
    /// </summary>
    [VariableInfo("Other", "Sprite")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class SpriteVariable : VariableBase<Sprite>
    {
    }

    /// <summary>
    /// Container for a Sprite variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct SpriteData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(SpriteVariable))]
        public SpriteVariable spriteRef;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public SpriteValueSO spriteSO;
        [SerializeField]
        public Sprite spriteVal;

        public SpriteData(Sprite v)
        {
            spriteVal = v;
            spriteRef = null;
            source = VariableDataSource.Unspecified;
            spriteSO = null;
        }

        public static implicit operator Sprite(SpriteData spriteData)
        {
            return spriteData.Value;
        }

        public Sprite Value
        {
            get { return VariableValueReference.Resolve(spriteRef, spriteVal, spriteSO, source); }
            set { VariableValueReference.Assign(spriteRef, ref spriteVal, spriteSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(spriteRef, spriteVal, spriteSO, source);
        }
    }
}