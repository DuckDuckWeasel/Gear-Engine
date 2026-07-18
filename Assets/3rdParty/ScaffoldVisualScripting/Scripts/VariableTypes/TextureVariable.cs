
using UnityEngine;
using System.Collections;

namespace Scaffold
{
    /// <summary>
    /// Texture variable type.
    /// </summary>
    [VariableInfo("Other", "Texture")]
    [AddComponentMenu("")]
    [System.Serializable]
    public class TextureVariable : VariableBase<Texture>
    {
    }

    /// <summary>
    /// Container for a Texture variable reference or constant value.
    /// </summary>
    [System.Serializable]
    public struct TextureData
    {
        [SerializeField]
        [VariableProperty("<Value>", typeof(TextureVariable))]
        public TextureVariable textureRef;


        [SerializeField]
        public VariableDataSource source;

        [SerializeField]
        public TextureValueSO textureSO;
        [SerializeField]
        public Texture textureVal;

        public TextureData(Texture v)
        {
            textureVal = v;
            textureRef = null;
            source = VariableDataSource.Unspecified;
            textureSO = null;
        }

        public static implicit operator Texture(TextureData textureData)
        {
            return textureData.Value;
        }

        public Texture Value
        {
            get { return VariableValueReference.Resolve(textureRef, textureVal, textureSO, source); }
            set { VariableValueReference.Assign(textureRef, ref textureVal, textureSO, source, value); }
        }

        public string GetDescription()
        {
            return VariableValueReference.Describe(textureRef, textureVal, textureSO, source);
        }
    }
}