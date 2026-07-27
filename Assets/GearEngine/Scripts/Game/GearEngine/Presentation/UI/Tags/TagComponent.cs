using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI.Tags
{
    public class TagComponent : MonoBehaviour
    {
        public IReadOnlyList<TagSO> Tags => tags;

        [Tooltip("The conceptual tags assigned to this object.")]
        [SerializeField] private List<TagSO> tags = new List<TagSO>();

        [Tooltip("Native Unity tags assigned to this object (Bitmask mode).")]
        [SerializeField] private UnityNativeTagMask nativeTags;

        public static readonly HashSet<TagComponent> Instances = new HashSet<TagComponent>();

        private void OnEnable()
        {
            Instances.Add(this);
        }

        private void OnDisable()
        {
            Instances.Remove(this);
        }

        public bool HasAnyTag(IEnumerable<TagSO> tagsToCheck)
        {
            if (tagsToCheck == null)
            {
                return false;
            }

            foreach (TagSO t in tagsToCheck)
            {
                if (HasTag(t))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasTag(TagSO tagToCheck)
        {
            if (tagToCheck == null)
            {
                return false;
            }

            return tags.Contains(tagToCheck);
        }

        public bool HasNativeTag(string unityTag)
        {
            if (string.IsNullOrEmpty(unityTag))
            {
                return false;
            }

            return nativeTags.HasTag(unityTag);
        }

        public void AddTag(TagSO tagToAdd)
        {
            if (tagToAdd != null && !tags.Contains(tagToAdd))
            {
                tags.Add(tagToAdd);
            }
        }
    }
}
