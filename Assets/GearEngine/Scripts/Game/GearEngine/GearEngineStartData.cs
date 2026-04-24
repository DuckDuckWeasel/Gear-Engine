using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GearEngine.GearEngine
{
    [Serializable]
    public sealed class GearEngineStartData
    {
        [FormerlySerializedAs("boardLoadout")]
        [SerializeField]
        private BoardLayoutData boardLayout = new BoardLayoutData();

        public BoardLayoutData BoardLayout => boardLayout;

        public BoardLayoutData GetBoardLayout() => boardLayout;
    }
}
