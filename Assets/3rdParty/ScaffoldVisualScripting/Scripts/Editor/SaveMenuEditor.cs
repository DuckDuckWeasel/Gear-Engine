
#if UNITY_5_3_OR_NEWER

using UnityEngine;
using UnityEditor;

namespace Scaffold.EditorUtils
{
    [CustomEditor(typeof(SaveMenu), true)]
    public class SaveMenuEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button(new GUIContent("Delete Save Data", "Deletes the save data associated with the Save Data Key")))
            {
                SaveMenu saveMenu = target as SaveMenu;

                if (saveMenu != null)
                {
                    SaveManager.Delete(saveMenu.SaveDataKey);
                    BlackboardWindow.ShowNotification("Deleted Save Data");
                }
            }

            base.OnInspectorGUI();
        }
    }
}

#endif