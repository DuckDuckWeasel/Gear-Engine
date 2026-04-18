using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "GearEngine/BoardConfig")]
    public class BoardConfigSO : ScriptableObject
    {
        public int MaxBoardGears
        {
            get
            {
                return GridWidth * GridHeight;
            }
        }

        [Header("Grid Layout")]
        [Min(1)]
        public int GridWidth = 5;
        
        [Min(1)]
        public int GridHeight = 5;
        
        [Min(0.1f)]
        public float Spacing = 0.75f;

        [Header("Interaction Mechanics")]
        [Min(0.1f)]
        public float MaxDragGrabDistance = 0.35f;
        
        [Header("Visuals")]
        public float StaggeredRotationOffset = 22.5f;

        [Tooltip("Vertical pixel offset of the trash zone above the grid's top edge.")]
        public float TrashZoneYOffset = 80f;

        [Tooltip("Centralized global multiplier applied to all gear SpriteRenderers to seamlessly align World-Space sprites with Canvas pixel dimensions (e.g. 115).")]
        [Min(0.1f)]
        public float GlobalGearScale = 115f;

        [Header("Limits")]
        [Tooltip("Gameplay limit for dynamic board control. Must be ≤ MaxBoardGears.")]
        [Min(1)]
        public int MaxAllowedBoardGears = 5;

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

        private void OnValidate()
        {
            GridWidth = Mathf.Max(1, GridWidth);
            GridHeight = Mathf.Max(1, GridHeight);
            MaxAllowedBoardGears = Mathf.Clamp(MaxAllowedBoardGears, 1, MaxBoardGears);
        }
    }
}

