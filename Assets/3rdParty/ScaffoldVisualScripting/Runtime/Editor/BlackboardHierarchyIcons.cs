using System.Collections.Generic;
using Scaffold.EditorUtils;
using Scaffold.VisualScripting.Unity;
using UnityEditor;
using UnityEngine;

namespace Scaffold.VisualScripting.Editor
{
    [InitializeOnLoad]
    public static class BlackboardHierarchyIcons
    {
        private static readonly Dictionary<EntityId, BlackboardBehaviour> s_Blackboards = new Dictionary<EntityId, BlackboardBehaviour>();

        static BlackboardHierarchyIcons()
        {
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += DrawHierarchyItem;
            EditorApplication.hierarchyChanged += Refresh;
            EditorApplication.projectChanged += Refresh;
            Refresh();
        }

        private static void Refresh()
        {
            s_Blackboards.Clear();
            if (ScaffoldEditorPreferences.hideBlackboardIconInHierarchy)
            {
                return;
            }

            BlackboardBehaviour[] behaviours = Resources.FindObjectsOfTypeAll<BlackboardBehaviour>();
            for (int index = 0; index < behaviours.Length; index++)
            {
                BlackboardBehaviour behaviour = behaviours[index];
                if (behaviour != null && behaviour.gameObject.scene.IsValid())
                {
                    s_Blackboards[behaviour.gameObject.GetEntityId()] = behaviour;
                }
            }
        }

        private static void DrawHierarchyItem(EntityId entityId, Rect selectionRect)
        {
            if (ScaffoldEditorPreferences.hideBlackboardIconInHierarchy || !s_Blackboards.TryGetValue(entityId, out BlackboardBehaviour behaviour))
            {
                return;
            }

            Rect iconRect = new Rect(selectionRect.x - 28f, selectionRect.y, selectionRect.height, selectionRect.height);
            GUIContent content = new GUIContent(BlackboardEditorStyles.FlowGraph, "Open Blackboard");
            if (GUI.Button(iconRect, content, GUIStyle.none))
            {
                BlackboardDefinitionWindowLauncher.Open(behaviour);
            }
        }
    }
}
