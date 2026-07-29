using Scaffold.EditorUtils;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    public static class BlackboardEditorStyles
    {
        private static readonly Color s_connection = new Color(0.65f, 0.65f, 0.65f, 1f);
        private static readonly Color s_selection = new Color(0.28f, 0.63f, 1f, 0.35f);
        private static readonly Color s_canvasDark = new Color(0.18f, 0.18f, 0.18f, 1f);
        private static readonly Color s_canvasLight = new Color(0.72f, 0.72f, 0.72f, 1f);

        public static Color Connection => s_connection;

        public static Color Selection => s_selection;

        public static Texture2D Add => ScaffoldEditorResources.AddSmall;

        public static Texture2D Delete => ScaffoldEditorResources.Delete;

        public static Texture2D Duplicate => ScaffoldEditorResources.Duplicate;

        public static Texture2D FlowGraph => ScaffoldEditorResources.FlowGraph;

        public static Texture2D Play => ScaffoldEditorResources.PlaySmall;

        public static Texture2D ConnectionPoint => ScaffoldEditorResources.ConnectionPoint;

        public static Texture2D Node(bool selected, bool hasTrigger, bool isChoice)
        {
            if (hasTrigger)
            {
                return selected ? ScaffoldEditorResources.EventNodeOn : ScaffoldEditorResources.EventNodeOff;
            }

            if (isChoice)
            {
                return selected ? ScaffoldEditorResources.ChoiceNodeOn : ScaffoldEditorResources.ChoiceNodeOff;
            }

            return selected ? ScaffoldEditorResources.ProcessNodeOn : ScaffoldEditorResources.ProcessNodeOff;
        }

        public static GUIStyle NodeLabel()
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = true;
            style.padding = new RectOffset(14, 14, 5, 5);
            return style;
        }

        public static GUIStyle TriggerLabel()
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Italic;
            return style;
        }

        public static Color CanvasColor()
        {
            return EditorGUIUtility.isProSkin ? s_canvasDark : s_canvasLight;
        }
    }
}
