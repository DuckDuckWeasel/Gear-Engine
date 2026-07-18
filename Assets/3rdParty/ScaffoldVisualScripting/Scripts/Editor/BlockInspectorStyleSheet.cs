using UnityEditor;
using UnityEngine;

namespace Scaffold.EditorUtils
{
    /// <summary>
    /// Shared IMGUI styles and layout metrics for the Block Inspector.
    /// </summary>
    internal static class BlockInspectorStyleSheet
    {
        internal const float OuterSpacing = 4f;
        internal const float InnerSpacing = 2f;
        internal const float CompactSummaryBreakpoint = 480f;
        internal const int DescriptionMinimumLines = 1;
        internal const int DescriptionMaximumLines = 4;

        private static GUIStyle _title;
        private static GUIStyle _identityCard;
        private static GUIStyle _sectionCard;
        private static GUIStyle _sectionFoldout;
        private static GUIStyle _fieldHeader;
        private static GUIStyle _summaryHeader;
        private static GUIStyle _summaryPopup;
        private static GUIStyle _descriptionTextArea;

        internal static GUIStyle Title { get { return _title ?? (_title = new GUIStyle(EditorStyles.boldLabel)); } }

        internal static GUIStyle IdentityCard
        {
            get
            {
                if (_identityCard == null)
                {
                    _identityCard = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(10, 10, 8, 8)
                    };
                }

                return _identityCard;
            }
        }

        internal static GUIStyle SectionCard
        {
            get
            {
                if (_sectionCard == null)
                {
                    _sectionCard = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(8, 8, 5, 6)
                    };
                }

                return _sectionCard;
            }
        }

        internal static GUIStyle SectionFoldout
        {
            get
            {
                if (_sectionFoldout == null)
                {
                    _sectionFoldout = new GUIStyle(EditorStyles.foldoutHeader)
                    {
                        margin = new RectOffset(0, 0, 0, 2)
                    };
                }

                return _sectionFoldout;
            }
        }

        internal static GUIStyle FieldHeader { get { return _fieldHeader ?? (_fieldHeader = new GUIStyle(EditorStyles.miniLabel)); } }

        internal static GUIStyle SummaryHeader
        {
            get
            {
                if (_summaryHeader == null)
                {
                    _summaryHeader = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        margin = new RectOffset(2, 2, 0, 1)
                    };
                }

                return _summaryHeader;
            }
        }

        internal static GUIStyle SummaryPopup
        {
            get
            {
                if (_summaryPopup == null)
                {
                    _summaryPopup = new GUIStyle(EditorStyles.popup)
                    {
                        alignment = TextAnchor.MiddleLeft
                    };
                }

                return _summaryPopup;
            }
        }

        internal static GUIStyle DescriptionTextArea
        {
            get
            {
                if (_descriptionTextArea == null)
                {
                    _descriptionTextArea = new GUIStyle(EditorStyles.textArea)
                    {
                        wordWrap = true,
                        padding = new RectOffset(6, 6, 3, 3)
                    };
                }

                return _descriptionTextArea;
            }
        }

        internal static DescriptionLayout CalculateDescriptionLayout(float contentHeight, float lineHeight)
        {
            float minimumHeight = lineHeight * DescriptionMinimumLines;
            float maximumHeight = lineHeight * DescriptionMaximumLines;
            bool requiresScroll = contentHeight > maximumHeight;
            return new DescriptionLayout(Mathf.Clamp(contentHeight, minimumHeight, maximumHeight), requiresScroll);
        }

        internal static bool UsesCompactSummaryLayout(float availableWidth)
        {
            return availableWidth < CompactSummaryBreakpoint;
        }

        internal readonly struct DescriptionLayout
        {
            internal DescriptionLayout(float height, bool requiresScroll)
            {
                Height = height;
                RequiresScroll = requiresScroll;
            }

            internal float Height { get; }

            internal bool RequiresScroll { get; }
        }
    }
}
