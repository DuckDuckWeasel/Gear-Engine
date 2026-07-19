
using UnityEngine;
using UnityEditor;

namespace Scaffold.EditorUtils
{
    // The prefab names are prefixed with Scaffold to avoid clashes with any other prefabs in the project
    public class NarrativeMenuItems 
    {

        [MenuItem("Tools/Scaffold/Create/Character", false, 50)]
        static void CreateCharacter()
        {
            GameObject go = BlackboardMenuItems.SpawnPrefab("Character");
            go.transform.position = Vector3.zero;
        }

        [MenuItem("Tools/Scaffold/Create/Say Dialog", false, 51)]
        static void CreateSayDialog()
        {
            GameObject go = BlackboardMenuItems.SpawnPrefab("SayDialog");
            go.transform.position = Vector3.zero;
        }

        [MenuItem("Tools/Scaffold/Create/Menu Dialog", false, 52)]
        static void CreateMenuDialog()
        {
            GameObject go = BlackboardMenuItems.SpawnPrefab("MenuDialog");
            go.transform.position = Vector3.zero;
        }

        [MenuItem("Tools/Scaffold/Create/Tag", false, 53)]
        static void CreateTag()
        {
            GameObject go = BlackboardMenuItems.SpawnPrefab("Tag");
            go.transform.position = Vector3.zero;
        }

        [MenuItem("Tools/Scaffold/Create/Audio Tag", false, 54)]
        static void CreateAudioTag()
        {
            GameObject go = BlackboardMenuItems.SpawnPrefab("AudioTag");
            go.transform.position = Vector3.zero;
        }

        [MenuItem("Tools/Scaffold/Create/Stage", false, 55)]
        static void CreateStage()
        {
            GameObject go = BlackboardMenuItems.SpawnPrefab("Stage");
            go.transform.position = Vector3.zero;
        }
        
        [MenuItem("Tools/Scaffold/Create/Stage Position", false, 56)]
        static void CreateStagePosition()
        {
            BlackboardMenuItems.SpawnPrefab("StagePosition");
        }

        [MenuItem("Tools/Scaffold/Create/Localization", false, 57)]
        static void CreateLocalization()
        {
            GameObject go = BlackboardMenuItems.SpawnPrefab("Localization");
            go.transform.position = Vector3.zero;
        }
    }
}