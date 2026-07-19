
using UnityEditor;

namespace Scaffold.EditorUtils
{
    public class SaveMenuItems 
    {
        [MenuItem("Tools/Scaffold/Create/Save Menu", false, 1100)]
        static void CreateSaveMenu()
        {
            BlackboardMenuItems.SpawnPrefab("SaveMenu");
        }

        [MenuItem("Tools/Scaffold/Create/Save Data", false, 1101)]
        static void CreateSaveData()
        {
            BlackboardMenuItems.SpawnPrefab("SaveData");
        }
    }
}