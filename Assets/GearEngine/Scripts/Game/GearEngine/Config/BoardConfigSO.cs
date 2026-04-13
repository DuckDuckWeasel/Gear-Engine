using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "GearEngine/BoardConfig")]
    public class BoardConfigSO : ScriptableObject
    {
        [Header("Grid Layout")]
        [Min(1)]
        public int GridWidth = 5;
        
        [Min(1)]
        public int GridHeight = 5;
        
        [Min(0.1f)]
        public float Spacing = 0.75f;

        [Header("Interaction Mechanics")]
        [Min(0.1f)]
        public float MaxDragGrabDistance = 0.75f;
        
        [Header("Visuals")]
        public float StaggeredRotationOffset = 22.5f;

        public Vector3 GetWorldPosition(Vector2Int gridPos, float zOffset = 0f)
        {
            float offsetX = (GridWidth - 1) * Spacing / 2.0f;
            float offsetY = (GridHeight - 1) * Spacing / 2.0f;
            
            float worldX = (gridPos.x * Spacing) - offsetX;
            float worldY = (gridPos.y * Spacing) - offsetY;
            
            return new Vector3(worldX, worldY, zOffset);
        }

        public Vector2Int GetGridPosition(Vector3 worldPos)
        {
            float offsetX = (GridWidth - 1) * Spacing / 2.0f;
            float offsetY = (GridHeight - 1) * Spacing / 2.0f;
            
            float gridX = (worldPos.x + offsetX) / Spacing;
            float gridY = (worldPos.y + offsetY) / Spacing;
            
            int x = Mathf.Clamp(Mathf.RoundToInt(gridX), 0, GridWidth - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(gridY), 0, GridHeight - 1);
            
            return new Vector2Int(x, y);
        }
    }
}
