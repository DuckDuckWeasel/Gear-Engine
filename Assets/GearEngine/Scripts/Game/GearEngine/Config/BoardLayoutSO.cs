using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    [CreateAssetMenu(fileName = "BoardLayout", menuName = "GearEngine/Config/Board Layout")]
    public sealed class BoardLayoutSO : ScriptableObject
    {
        [Min(0.1f)]
        public float Spacing = 0.75f;

        [Header("Interaction")]
        [Min(0.1f)]
        public float MaxDragGrabDistance = 0.35f;

        [Header("Visuals")]
        public float StaggeredRotationOffset = 22.5f;

        [Tooltip("Vertical pixel offset of the trash zone above the grid's top edge.")]
        public float TrashZoneYOffset = 80f;

        public Vector3 GetCellLocalPosition(Vector2Int gridPos, BoardRulesSO rules, float zOffset = 0f)
        {
            if (rules == null)
            {
                return Vector3.zero;
            }

            int gw = rules.GridWidth;
            int gh = rules.GridHeight;
            float offsetX = (gw - 1) * Spacing / 2.0f;
            float offsetY = (gh - 1) * Spacing / 2.0f;

            float worldX = (gridPos.x * Spacing) - offsetX;
            float worldY = (gridPos.y * Spacing) - offsetY;

            return new Vector3(worldX, worldY, zOffset);
        }

        public Vector2Int GetGridPosition(Vector3 localBoardPos, BoardRulesSO rules)
        {
            if (rules == null)
            {
                return Vector2Int.zero;
            }

            int gw = rules.GridWidth;
            int gh = rules.GridHeight;
            float offsetX = (gw - 1) * Spacing / 2.0f;
            float offsetY = (gh - 1) * Spacing / 2.0f;

            float gridX = (localBoardPos.x + offsetX) / Spacing;
            float gridY = (localBoardPos.y + offsetY) / Spacing;

            int x = Mathf.Clamp(Mathf.RoundToInt(gridX), 0, gw - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(gridY), 0, gh - 1);

            return new Vector2Int(x, y);
        }
    }
}
