using UnityEngine;

namespace Game.GearEngine.Presentation
{
    [CreateAssetMenu(menuName = "GearEngine/Tags/TagSO", fileName = "NewTag")]
    public class TagSO : ScriptableObject
    {
        [Tooltip("Optional description for what this tag represents.")]
        public string Description;

        // Implicit conversion to string enables us to use it easily in logs or legacy Unity tag comparisons if needed
        public static implicit operator string(TagSO tag)
        {
            return tag != null ? tag.name : "null_tag";
        }
    }
}
