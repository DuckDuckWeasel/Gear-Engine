#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;
using GearEngine.GearEngine.Visuals;
using GearEngine.GearEngine.Presentation.UI;

namespace GearEngine.Editor
{
    public static class FixFloatingText
    {
        [MenuItem("Tools/Fix Floating Text Prefab")]
        public static void Fix()
        {
            // 1. Create the GameObject and setup components
            var go = new GameObject("FloatingText");
            var rect = go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            var textMesh = go.AddComponent<TextMeshProUGUI>();
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.fontSize = 36f;
            
            var ft = go.AddComponent<FloatingText>();
            
            // 2. Save it as a Prefab
            string path = "Assets/GearEngine/Prefabs/Gears/FloatingText.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            GameObject.DestroyImmediate(go);
            
            // 3. Assign to BoardViewComponent
            string boardPath = "Assets/GearEngine/Prefabs/Gears/Grid/GridBoardViewComponent.prefab";
            var boardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(boardPath);
            if (boardPrefab != null)
            {
                var comp = boardPrefab.GetComponent<BoardViewComponent>();
                if (comp != null)
                {
                    var so = new SerializedObject(comp);
                    so.FindProperty("floatingTextPrefab").objectReferenceValue = prefab.GetComponent<FloatingText>();
                    so.ApplyModifiedProperties();
                    PrefabUtility.SavePrefabAsset(boardPrefab);
                    Debug.Log("<color=green>Successfully created FloatingText prefab and assigned it to GridBoardViewComponent!</color>");
                }
                else
                {
                    Debug.LogError("Could not find BoardViewComponent on " + boardPath);
                }
            }
            else
            {
                Debug.LogError("Could not find GridBoardViewComponent prefab at " + boardPath);
            }
        }
    }
}
#endif
