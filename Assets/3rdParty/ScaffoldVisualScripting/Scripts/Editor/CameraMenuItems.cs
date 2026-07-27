
using UnityEngine;
using UnityEditor;

namespace Scaffold.EditorUtils
{
    public class CameraMenuItems 
    {
        [MenuItem("Tools/Scaffold/Create/View", false, 100)]
        static void CreateView()
        {
            BlackboardMenuItems.SpawnPrefab("View");
        }
    }
}