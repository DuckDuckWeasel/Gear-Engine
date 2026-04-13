using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI.Tags
{
    [CreateAssetMenu(menuName = "GearEngine/Tags/TagSO", fileName = "NewTag")]
    public class TagSO : ScriptableObject
    {
        [Tooltip("Optional description for what this tag represents.")]
        public string Description;

        /// <summary>Sample: Implicit conversion for logs or legacy Unity tag comparisons.</summary>
        public static implicit operator string(TagSO tag)
        {
            return tag != null ? tag.name : "null_tag";
        }
    }
}
