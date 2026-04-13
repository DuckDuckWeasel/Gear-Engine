using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI.Tags
{
    public class TagComponent : MonoBehaviour
    {
        public IReadOnlyList<TagSO> Tags => tags;

        [Tooltip("The conceptual tags assigned to this object.")]
        [SerializeField] private List<TagSO> tags = new List<TagSO>();

        public bool HasAnyTag(IEnumerable<TagSO> tagsToCheck)
        {
            if (tagsToCheck == null)
            {
                return false;
            }

            foreach (var t in tagsToCheck)
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

        public void AddTag(TagSO tagToAdd)
        {
            if (tagToAdd != null && !tags.Contains(tagToAdd))
            {
                tags.Add(tagToAdd);
            }
        }
    }
}
