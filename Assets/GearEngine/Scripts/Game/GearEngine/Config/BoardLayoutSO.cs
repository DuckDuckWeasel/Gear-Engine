using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    [CreateAssetMenu(fileName = "BoardLayout", menuName = "GearEngine/Config/Board Layout")]
    public sealed class BoardLayoutSO : ScriptableObject
    {
        [Min(1f)]
        public float CellSpacing = 132f;

        [Header("Visuals")]
        public float StaggeredRotationOffset = 22.5f;

        public Vector3 GetCellLocalPosition(Vector2Int gridPos, BoardRulesSO rules, float zOffset = 0f)
        {
            if (rules == null)
            {
                return Vector3.zero;
            }

            int gw = rules.GridWidth;
            int gh = rules.GridHeight;
            float offsetX = (gw - 1) * CellSpacing / 2.0f;
            float offsetY = (gh - 1) * CellSpacing / 2.0f;

            float localX = (gridPos.x * CellSpacing) - offsetX;
            float localY = (gridPos.y * CellSpacing) - offsetY;

            return new Vector3(localX, localY, zOffset);
        }

        public bool TryGetGridPosition(Vector2 localBoardPosition, BoardRulesSO rules, out Vector2Int gridPosition)
        {
            gridPosition = Vector2Int.zero;
            if (rules == null || rules.GridWidth <= 0 || rules.GridHeight <= 0)
            {
                return false;
            }

            float halfWidth = rules.GridWidth * CellSpacing * 0.5f;
            float halfHeight = rules.GridHeight * CellSpacing * 0.5f;
            if (Mathf.Abs(localBoardPosition.x) > halfWidth ||
                Mathf.Abs(localBoardPosition.y) > halfHeight)
            {
                return false;
            }

            gridPosition = GetGridPosition(localBoardPosition, rules);
            return true;
        }

        public Vector2Int GetGridPosition(Vector3 localBoardPos, BoardRulesSO rules)
        {
            if (rules == null)
            {
                return Vector2Int.zero;
            }
            int gw = rules.GridWidth;
            int gh = rules.GridHeight;
            float offsetX = (gw - 1) * CellSpacing / 2.0f;
            float offsetY = (gh - 1) * CellSpacing / 2.0f;
            float gridX = (localBoardPos.x + offsetX) / CellSpacing;
            float gridY = (localBoardPos.y + offsetY) / CellSpacing;
            int x = Mathf.Clamp(Mathf.RoundToInt(gridX), 0, gw - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(gridY), 0, gh - 1);
            return new Vector2Int(x, y);
        }
    }
}
