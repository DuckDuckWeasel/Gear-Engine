using UnityEngine;

namespace GearEngine.GearEngine.Config
{
    [CreateAssetMenu(fileName = "BoardRules", menuName = "GearEngine/BoardRules")]
    public sealed class BoardRulesSO : ScriptableObject
    {
        [Header("Grid")]
        [Min(1)]
        public int GridWidth = 5;

        [Min(1)]
        public int GridHeight = 5;

        [Header("Limits")]
        [Tooltip("Gameplay limit for dynamic board control. Must be ≤ MaxBoardGears.")]
        [Min(1)]
        public int MaxAllowedBoardGears = 5;

        public int MaxBoardGears => GridWidth * GridHeight;

        private void OnValidate()
        {
            GridWidth = Mathf.Max(1, GridWidth);
            GridHeight = Mathf.Max(1, GridHeight);
            MaxAllowedBoardGears = Mathf.Clamp(MaxAllowedBoardGears, 1, MaxBoardGears);
        }
    }
}
