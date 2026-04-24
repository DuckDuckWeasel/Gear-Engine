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

        [Header("Motor Cog")]
        [Tooltip("Authoring reference: cell where CoreGear is auto-placed when missing from loadout. Keep aligned with Loadout remote config motor start.")]
        public Vector2Int MotorCogStartCell = new Vector2Int(2, 2);

        public int MaxBoardGears => GridWidth * GridHeight;

        private void OnValidate()
        {
            GridWidth = Mathf.Max(1, GridWidth);
            GridHeight = Mathf.Max(1, GridHeight);
        }
    }
}
