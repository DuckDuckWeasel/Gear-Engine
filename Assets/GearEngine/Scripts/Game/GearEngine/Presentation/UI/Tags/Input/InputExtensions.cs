using System.Collections.Generic;
using Scaffold.Input.Contracts;
using GearEngine.GearEngine.Presentation.UI.Tags;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    public static class InputExtensions
    {
        public static void FilterForTag(this IInputFilterService input, TagSO tagSO)
        {
            input.SetButtonUpRaycastFilter((rh) =>
            {
                TagComponent tagComponent = rh.transform.GetComponent<TagComponent>();
                return tagComponent && tagComponent.HasTag(tagSO);
            }, true);
        }

        public static void FilterForPointerEnterTags(this IInputFilterService input, bool matchAll, params TagSO[] tagSOArray)
        {
            input.SetPointerEnterRaycastFilter((rh) =>
            {
                TagComponent tagComponent = rh.transform.GetComponent<TagComponent>();

                if (!tagComponent)
                {
                    return false;
                }

                if (matchAll)
                {
                    foreach (TagSO tag in tagSOArray)
                    {
                        if (!tagComponent.HasTag(tag)) return false;
                    }
                    return true;
                }
                return tagComponent.HasAnyTag(tagSOArray);
            }, true);
        }

        public static void FilterForButtonDownTags(this IInputFilterService input, bool matchAll, params TagSO[] tagSOArray)
        {
            input.SetButtonDownRaycastFilter((rh) =>
            {
                TagComponent tagComponent = rh.transform.GetComponent<TagComponent>();

                if (!tagComponent)
                {
                    return false;
                }

                if (matchAll)
                {
                    foreach (TagSO tag in tagSOArray)
                    {
                        if (!tagComponent.HasTag(tag)) return false;
                    }
                    return true;
                }
                return tagComponent.HasAnyTag(tagSOArray);
            }, true);
        }

        public static void FilterForDropEnterTags(this IInputFilterService input, bool matchAll, bool checkDroppedGameObject, params TagSO[] tagSOArray)
        {
            input.SetDropRaycastFilter((rh) =>
            {
                TagComponent tagComponent = rh.transform.GetComponent<TagComponent>();

                if (!tagComponent)
                {
                    return false;
                }

                if (matchAll)
                {
                    foreach (TagSO tag in tagSOArray)
                    {
                        if (!tagComponent.HasTag(tag)) return false;
                    }
                    return true;
                }
                return tagComponent.HasAnyTag(tagSOArray);
            }, checkDroppedGameObject, true);
        }

        public static void FilterForButtonUpTags(this IInputFilterService input, bool matchAll, params TagSO[] tagSOArray)
        {
            input.SetButtonUpRaycastFilter((rh) =>
            {
                TagComponent tagComponent = rh.transform.GetComponent<TagComponent>();

                if (!tagComponent)
                {
                    return false;
                }

                if (matchAll)
                {
                    foreach (TagSO tag in tagSOArray)
                    {
                        if (!tagComponent.HasTag(tag)) return false;
                    }
                    return true;
                }
                return tagComponent.HasAnyTag(tagSOArray);
            }, true);
        }

        public static bool ContainsTag(this TagComponent tagComponent, TagSO[] tags, bool matchAll)
        {
            if (matchAll)
            {
                foreach (TagSO tag in tags)
                {
                    if (!tagComponent.HasTag(tag)) return false;
                }
                return true;
            }
            return tagComponent.HasAnyTag(tags);
        }
    }
}
