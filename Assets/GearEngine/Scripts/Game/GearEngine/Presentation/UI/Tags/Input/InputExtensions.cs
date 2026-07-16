using System.Collections.Generic;
using Scaffold.Input.Contracts;
using GearEngine.GearEngine.Presentation.UI.Tags;
using GearEngine.Core.Architecture.References;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI.Input
{
    public static class InputExtensions
    {
        public static void FilterForTag(this IInputFilterService input, TagSO tagSO)
        {
            input.SetButtonUpRaycastFilter((rh) =>
            {
                TagComponent tagComponent = rh.transform.GetComponentInParent<TagComponent>();
                return tagComponent && tagComponent.HasTag(tagSO);
            }, true);
        }

        public static void FilterForPointerEnterTags(this IInputFilterService input, bool matchAll, params TagSO[] tagSOArray)
        {
            input.SetPointerEnterRaycastFilter((rh) =>
            {
                TagComponent tagComponent = rh.transform.GetComponentInParent<TagComponent>();

                if (!tagComponent)
                {
                    return false;
                }

                return tagComponent.ContainsTag(tagSOArray, matchAll);
            }, true);
        }

        public static void FilterForButtonDownTags(this IInputFilterService input, bool matchAll, params TagSO[] tagSOArray)
        {
            input.SetButtonDownRaycastFilter((rh) =>
            {
                TagComponent tagComponent = rh.transform.GetComponentInParent<TagComponent>();

                if (!tagComponent)
                {
                    return false;
                }

                return tagComponent.ContainsTag(tagSOArray, matchAll);
            }, true);
        }

        public static void FilterForDropEnterTags(this IInputFilterService input, bool matchAll, bool checkDroppedGameObject, params TagSO[] tagSOArray)
        {
            input.SetDropRaycastFilter((rh) =>
            {
                TagComponent tagComponent = rh.transform.GetComponentInParent<TagComponent>();

                if (!tagComponent)
                {
                    return false;
                }

                return tagComponent.ContainsTag(tagSOArray, matchAll);
            }, checkDroppedGameObject, true);
        }

        public static void FilterForButtonUpTags(this IInputFilterService input, bool matchAll, params TagSO[] tagSOArray)
        {
            input.SetButtonUpRaycastFilter((rh) =>
            {
                TagComponent tagComponent = rh.transform.GetComponentInParent<TagComponent>();
                if (!tagComponent) return false;
                return tagComponent.ContainsTag(tagSOArray, matchAll);
            }, true);
        }

        public static bool ContainsTag(this TagComponent tagComponent, TagSO[] tags, bool matchAll)
        {
            if (tags == null || tags.Length == 0) return false;

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

        // --- NEW TARGET REFERENCE EXTENSIONS ---

        public static void FilterForPointerEnterTarget(this IInputFilterService input, TargetReference targetReference)
        {
            input.SetPointerEnterRaycastFilter((rh) =>
            {
                return targetReference != null && targetReference.IsMatch(rh.transform.gameObject);
            }, true);
        }

        public static void FilterForButtonDownTarget(this IInputFilterService input, TargetReference targetReference)
        {
            input.SetButtonDownRaycastFilter((rh) =>
            {
                return targetReference != null && targetReference.IsMatch(rh.transform.gameObject);
            }, true);
        }

        public static void FilterForDropEnterTarget(this IInputFilterService input, bool checkDroppedGameObject, TargetReference targetReference)
        {
            input.SetDropRaycastFilter((rh) =>
            {
                if (targetReference == null) return false;

                // Check exact hit object
                if (targetReference.IsMatch(rh.transform.gameObject)) return true;

                // Fallback to parents
                TagComponent tagComponent = rh.transform.GetComponentInParent<TagComponent>();
                return tagComponent != null && targetReference.IsMatch(tagComponent.gameObject);
            }, checkDroppedGameObject, true);
        }

        public static void FilterForButtonUpTarget(this IInputFilterService input, TargetReference targetReference)
        {
            input.SetButtonUpRaycastFilter((rh) =>
            {
                return targetReference != null && targetReference.IsMatch(rh.transform.gameObject);
            }, true);
        }
        
        public static void FilterForDropEnterTargets(this IInputFilterService input, bool checkDroppedGameObject, List<TargetReference> targetReferences)
        {
            input.SetDropRaycastFilter((rh) =>
            {
                if (targetReferences == null || targetReferences.Count == 0) return false;

                foreach (var target in targetReferences)
                {
                    if (target == null) continue;
                    if (target.IsMatch(rh.transform.gameObject)) return true;

                    TagComponent tagComponent = rh.transform.GetComponentInParent<TagComponent>();
                    if (tagComponent != null && target.IsMatch(tagComponent.gameObject)) return true;
                }

                return false;
            }, checkDroppedGameObject, true);
        }
    }
}
