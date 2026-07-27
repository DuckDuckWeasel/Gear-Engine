using System;
using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI.Tags
{
    /// <summary>
    /// Represents a collection of Unity native tags (strings) that are drawn in the Editor
    /// as a multi-select Bitmask dropdown, making it fast and intuitive to select multiple tags.
    /// At runtime, it relies on fast string comparisons (CompareTag) and never suffers from index shifting.
    /// </summary>
    [Serializable]
    public struct UnityNativeTagMask
    {
        [SerializeField]
        private List<string> tags;

        public IReadOnlyList<string> Tags => tags ?? (tags = new List<string>());

        public UnityNativeTagMask(List<string> tags)
        {
            this.tags = tags;
        }

        public bool HasTag(string tag)
        {
            if (tags == null || tags.Count == 0) return false;
            return tags.Contains(tag);
        }

        public bool Matches(GameObject target)
        {
            if (target == null || tags == null || tags.Count == 0)
                return false;

            for (int i = 0; i < tags.Count; i++)
            {
                if (target.CompareTag(tags[i]))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the target GameObject OR its TagComponent possesses any of the native tags in this mask.
        /// </summary>
        public bool IsMatch(GameObject target)
        {
            if (target == null || tags == null || tags.Count == 0)
                return false;

            // Fast native check on the object itself
            for (int i = 0; i < tags.Count; i++)
            {
                if (target.CompareTag(tags[i]))
                    return true;
            }

            // Check if the object has a TagComponent that holds these native tags
            if (target.TryGetComponent<TagComponent>(out var tagComp))
            {
                for (int i = 0; i < tags.Count; i++)
                {
                    if (tagComp.HasNativeTag(tags[i]))
                        return true;
                }
            }

            return false;
        }
    }
}
