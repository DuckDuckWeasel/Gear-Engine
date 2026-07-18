
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Scaffold
{
    namespace EditorUtils
    {
        /// <summary>
        /// Shows Scaffold section in the Edit->Preferences in unity allows you to configure Scaffold behaviour
        /// 
        /// ref https://docs.unity3d.com/ScriptReference/PreferenceItem.html
        /// </summary>
        [InitializeOnLoad]
        public static class ScaffoldEditorPreferences
        {
            // Have we loaded the prefs yet
            private static bool prefsLoaded = false;
            private const string HIDE_FLOWCHART_ICON_KEY = "hideFlowchartIconInHierarchy";
            private const string LEGACY_HIDE_MUSHROOM_KEY = "hideMushroomInHierarchy";
            private const string USE_LEGACY_MENUS = "useLegacyMenus";
            private const string USE_GRID_SNAP = "useGridSnap";

            public static bool hideFlowchartIconInHierarchy;
            public static bool useLegacyMenus;
            public static bool useGridSnap;

            static ScaffoldEditorPreferences()
            {
                LoadOnScriptLoad();
            }

#if UNITY_2019_1_OR_NEWER
            [SettingsProvider]
            public static SettingsProvider CreateScaffoldSettingsProvider()
            {
                // First parameter is the path in the Settings window.
                // Second parameter is the scope of this setting: it only appears in the Project Settings window.
                var provider = new SettingsProvider("Project/Scaffold", SettingsScope.Project)
                {
                    // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                    guiHandler = (searchContext) => PreferencesGUI()

                    // // Populate the search keywords to enable smart search filtering and label highlighting:
                    // keywords = new HashSet<string>(new[] { "Number", "Some String" })
                };

                return provider;
            }

#else

            [PreferenceItem("Scaffold")]
#endif
            private static void PreferencesGUI()
            {
                // Load the preferences
                if (!prefsLoaded)
                {
                    LoadOnScriptLoad();
                }

                // Preferences GUI
                hideFlowchartIconInHierarchy = EditorGUILayout.Toggle("Hide Flowchart Icon in Hierarchy", hideFlowchartIconInHierarchy);
                useLegacyMenus = EditorGUILayout.Toggle(new GUIContent("Legacy Menus", "Force Legacy menus for Event, Add Variable and Add Command menus"), useLegacyMenus);
                useGridSnap = EditorGUILayout.Toggle(new GUIContent("Grid Snap", "Align and Snap block positions and widths in the flowchart window to the grid"), useGridSnap);

                EditorGUILayout.Space();
                //ideally if any are null, but typically it is all or nothing that have broken links due to version changes or moving files external to Unity
                if (ScaffoldEditorResources.Add == null)
                {
                    EditorGUILayout.HelpBox("ScaffoldEditorResources need to be regenerated!", MessageType.Error);
                }

                if (GUILayout.Button(new GUIContent("Select Scaffold Editor Resources SO", "If Scaffold icons are not showing correctly you may need to reassign the references in the ScaffoldEditorResources. Button below will locate it.")))
                {
                    var ids = AssetDatabase.FindAssets("t:ScaffoldEditorResources");
                    if (ids.Length > 0)
                    {
                        var p = AssetDatabase.GUIDToAssetPath(ids[0]);
                        var asset = AssetDatabase.LoadAssetAtPath<ScaffoldEditorResources>(p);
                        Selection.activeObject = asset;
                    }
                    else
                    {
                        Debug.LogError("No ScaffoldEditorResources found!");
                    }
                }

                if (GUILayout.Button("Open Changelog (version info)"))
                {
                    //From project path down, look for our Scaffold\Docs\ChangeLog.txt
                    var projectPath = System.IO.Directory.GetParent(Application.dataPath);
                    var fileMacthes = System.IO.Directory.GetFiles(projectPath.FullName, "CHANGELOG.txt", System.IO.SearchOption.AllDirectories);

                    fileMacthes = fileMacthes.Where((x) =>
                    {
                        var fileFolder = System.IO.Directory.GetParent(x);
                        return fileFolder.Name == "Docs" && fileFolder.Parent.Name == "Scaffold";
                    }).ToArray();

                    if (fileMacthes == null || fileMacthes.Length == 0)
                    {
                        Debug.LogWarning("Cannot locate Scaffold\\Docs\\CHANGELONG.txt");
                    }
                    else
                    {
                        Application.OpenURL(fileMacthes[0]);
                    }
                }

                // Save the preferences
                if (GUI.changed)
                {
                    EditorPrefs.SetBool(HIDE_FLOWCHART_ICON_KEY, hideFlowchartIconInHierarchy);
                    EditorPrefs.SetBool(USE_LEGACY_MENUS, useLegacyMenus);
                    EditorPrefs.SetBool(USE_GRID_SNAP, useGridSnap);
                }
            }

            public static void LoadOnScriptLoad()
            {
                hideFlowchartIconInHierarchy = EditorPrefs.HasKey(HIDE_FLOWCHART_ICON_KEY)
                    ? EditorPrefs.GetBool(HIDE_FLOWCHART_ICON_KEY)
                    : EditorPrefs.GetBool(LEGACY_HIDE_MUSHROOM_KEY, false);
                useLegacyMenus = EditorPrefs.GetBool(USE_LEGACY_MENUS, false);
                useGridSnap = EditorPrefs.GetBool(USE_GRID_SNAP, false);
                prefsLoaded = true;
            }
        }
    }
}
