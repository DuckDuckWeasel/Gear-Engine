using System;
using System.Collections.Generic;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI.Tags
{
    /// <summary>
    /// A consolidated filter that can check multiple types of tags (SO, Native, Enum) at once.
    /// Replaces the old List&lt;TagSO&gt; in commands, allowing them to support Native bitmasks out of the box.
    /// </summary>
    [Serializable]
    public class TagFilter : ITagFilter
    {
        [Tooltip("Custom ScriptableObject tags to look for.")]
        public List<TagSO> soTags = new List<TagSO>();

        [Tooltip("Native Unity tags to look for (Bitmask).")]
        public UnityNativeTagMask nativeTags;

        [Tooltip("If true, the target must have ALL specified tags (SO and Native). If false, having ANY of them is enough.")]
        public bool matchAll = false;

        public bool IsMatch(GameObject target)
        {
            if (target == null)
                return false;

            bool hasSOTags = soTags != null && soTags.Count > 0;
            bool hasNativeTags = nativeTags.Tags.Count > 0;

            // If no tags are specified, it matches nothing (or could match everything, but usually implies nothing to find)
            if (!hasSOTags && !hasNativeTags)
                return false;

            TagComponent tagComp = target.GetComponent<TagComponent>();

            if (matchAll)
            {
                // Must match ALL SO tags
                if (hasSOTags)
                {
                    if (tagComp == null) return false;
                    foreach (var tagSO in soTags)
                    {
                        if (!tagComp.HasTag(tagSO))
                            return false;
                    }
                }

                // Must match ALL Native tags
                if (hasNativeTags)
                {
                    foreach (var nTag in nativeTags.Tags)
                    {
                        // Check if the native tag is exactly on the GameObject OR in its TagComponent
                        bool matchesNative = target.CompareTag(nTag) || (tagComp != null && tagComp.HasNativeTag(nTag));
                        if (!matchesNative)
                            return false;
                    }
                }

                return true;
            }
            else
            {
                // Must match ANY tag
                
                // Check SO tags
                if (hasSOTags && tagComp != null)
                {
                    foreach (var tagSO in soTags)
                    {
                        if (tagComp.HasTag(tagSO))
                            return true;
                    }
                }

                // Check Native tags
                if (hasNativeTags)
                {
                    foreach (var nTag in nativeTags.Tags)
                    {
                        if (target.CompareTag(nTag) || (tagComp != null && tagComp.HasNativeTag(nTag)))
                            return true;
                    }
                }

                return false;
            }
        }
    }
}
