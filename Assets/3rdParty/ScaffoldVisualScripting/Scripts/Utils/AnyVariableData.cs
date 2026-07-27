using System;

namespace Scaffold
{
    /// <summary>
    /// Collection of every compatibility VariableData type.
    /// </summary>
    [Serializable]
    public partial struct AnyVariableData
    {
        public AnimatorData animatorData;
        public AudioSourceData audioSourceData;
        public BooleanData booleanData;
        public CollectionData collectionData;
        public Collider2DData collider2DData;
        public ColliderData colliderData;
        public ColorData colorData;
        public FloatData floatData;
        public GameObjectData gameObjectData;
        public IntegerData integerData;
        public MaterialData materialData;
        public Matrix4x4Data matrix4x4Data;
        public ObjectData objectData;
        public QuaternionData quaternionData;
        public Rigidbody2DData rigidbody2DData;
        public RigidbodyData rigidbodyData;
        public SpriteData spriteData;
        public StringData stringData;
        public TextureData textureData;
        public TransformData transformData;
        public Vector2Data vector2Data;
        public Vector3Data vector3Data;
        public Vector4Data vector4Data;

        public bool HasReference(Variable variable)
        {
            return animatorData.animatorRef == variable ||
                audioSourceData.audioSourceRef == variable ||
                booleanData.booleanRef == variable ||
                collectionData.collectionRef == variable ||
                collider2DData.collider2DRef == variable ||
                colliderData.colliderRef == variable ||
                colorData.colorRef == variable ||
                floatData.floatRef == variable ||
                gameObjectData.gameObjectRef == variable ||
                integerData.integerRef == variable ||
                materialData.materialRef == variable ||
                matrix4x4Data.matrix4x4Ref == variable ||
                objectData.objectRef == variable ||
                quaternionData.quaternionRef == variable ||
                rigidbody2DData.rigidbody2DRef == variable ||
                rigidbodyData.rigidbodyRef == variable ||
                spriteData.spriteRef == variable ||
                stringData.stringRef == variable ||
                textureData.textureRef == variable ||
                transformData.transformRef == variable ||
                vector2Data.vector2Ref == variable ||
                vector3Data.vector3Ref == variable ||
                vector4Data.vector4Ref == variable;
        }
    }
}
