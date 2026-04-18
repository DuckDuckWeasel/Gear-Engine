using System;
using UnityEngine;

namespace GearEngine.GearEngine
{
    /// <summary>
    /// Serializable board startup seed: optional initial layout.
    /// </summary>
    [Serializable]
    public sealed class GearBoardLoadoutData
    {
        [SerializeField] private BoardLayoutData boardLayout;

        public BoardLayoutData BoardLayout
        {
            get => boardLayout;
            set => boardLayout = value;
        }
    }
}
