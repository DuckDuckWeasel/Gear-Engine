using UnityEditor;
using UnityEngine;

public static class CheckIcons
{
    [MenuItem("Tools/Check Icons")]
    public static void Check()
    {
        string[] icons = { "preAudioAutoPlayOn", "preAudioAutoPlayOn@2x", "preAudioLoopOff", "preAudioLoopOn", "preAudioLoopOff@2x", "preAudioLoopOn@2x", "d_preAudioLoopOff", "d_preAudioLoopOff@2x" };
        foreach (var i in icons)
        {
            var content = EditorGUIUtility.IconContent(i);
            if (content != null && content.image != null)
                Debug.Log($"Found: {i}");
            else
                Debug.Log($"Not found: {i}");
        }
    }
}
