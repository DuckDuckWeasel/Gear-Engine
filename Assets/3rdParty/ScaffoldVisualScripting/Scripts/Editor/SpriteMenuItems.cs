
using UnityEditor;

namespace Scaffold.EditorUtils
{
    public class SpriteMenuItems 
    {
        [MenuItem("Tools/Scaffold/Create/Clickable Sprite", false, 150)]
        static void CreateClickableSprite()
        {
            FlowchartMenuItems.SpawnPrefab("ClickableSprite");
        }

        [MenuItem("Tools/Scaffold/Create/Draggable Sprite", false, 151)]
        static void CreateDraggableSprite()
        {
            FlowchartMenuItems.SpawnPrefab("DraggableSprite");
        }

        [MenuItem("Tools/Scaffold/Create/Drag Target Sprite", false, 152)]
        static void CreateDragTargetSprite()
        {
            FlowchartMenuItems.SpawnPrefab("DragTargetSprite");
        }

        [MenuItem("Tools/Scaffold/Create/Parallax Sprite", false, 152)]
        static void CreateParallaxSprite()
        {
            FlowchartMenuItems.SpawnPrefab("ParallaxSprite");
        }
    }
}