using System;
using System.Collections;
using System.Collections.Generic;
using GearEngine.GearEngine.Abilities;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Events;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using NUnit.Framework;
using Scaffold.Events.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

namespace GearEngine.GearEngine.Tests.Runtime
{
    [TestFixture]
    public sealed class BoardCapacityPlayModeTests
    {
        [UnityTest]
        public IEnumerator SeventhGear_RejectsAndPunchesBoundCapacityChip()
        {
            StubBoardService board = new StubBoardService
            {
                CurrentCount = 6,
                MaximumCount = 6,
            };
            BoardViewModel viewModel = CreateViewModel(board);
            BoardCapacityChipView chip = CreateChip(out TextMeshProUGUI label);
            chip.Bind(viewModel);

            bool accepted = viewModel.HandleInventoryDrop(Vector2Int.zero, new GearItemData());
            yield return new WaitForSecondsRealtime(0.05f);

            Assert.IsFalse(accepted);
            Assert.That(board.TryPlaceCalls, Is.Zero);
            Assert.That(label.text, Is.EqualTo("6/6"));
            Assert.That(chip.transform.localScale.x, Is.GreaterThan(1f));

            chip.Unbind();
            Assert.That(chip.transform.localScale, Is.EqualTo(Vector3.one));
            yield return new WaitForSecondsRealtime(0.35f);
            Assert.That(chip.transform.localScale, Is.EqualTo(Vector3.one));

            UnityEngine.Object.Destroy(chip.gameObject);
        }

        [UnityTest]
        public IEnumerator FullBoardMerge_StillDelegatesToBoardService()
        {
            StubBoardService board = new StubBoardService
            {
                CurrentCount = 6,
                MaximumCount = 6,
                Occupant = new StubNode(),
                TryPlaceResult = true,
            };
            BoardViewModel viewModel = CreateViewModel(board);

            bool accepted = viewModel.HandleInventoryDrop(Vector2Int.zero, new GearItemData());
            yield return null;

            Assert.IsTrue(accepted);
            Assert.That(board.TryPlaceCalls, Is.EqualTo(1));
        }

        private static BoardCapacityChipView CreateChip(out TextMeshProUGUI label)
        {
            GameObject root = new GameObject("BoardCapacityChip", typeof(RectTransform));
            GameObject labelObject = new GameObject("CapacityLabel", typeof(RectTransform));
            labelObject.transform.SetParent(root.transform, false);
            label = labelObject.AddComponent<TextMeshProUGUI>();
            return root.AddComponent<BoardCapacityChipView>();
        }

        private static BoardViewModel CreateViewModel(StubBoardService board)
        {
            return new BoardViewModel(
                board,
                new StubEngineService(),
                new StubInventoryService(),
                new StubEventBus());
        }

        private sealed class StubBoardService : IBoardService
        {
            public event Action<IGridNode> GearPlaced
            {
                add { }
                remove { }
            }

            public event Action<IGridNode> GearRemoved
            {
                add { }
                remove { }
            }

            public event Action BoardLayoutChanged
            {
                add { }
                remove { }
            }

            public int CurrentCount { get; set; }
            public int MaximumCount { get; set; }
            public IGridNode Occupant { get; set; }
            public bool TryPlaceResult { get; set; }
            public int TryPlaceCalls { get; private set; }

            public BoardModel GetBoard() => null;
            public BoardRulesSO BoardRules => null;
            public bool IsSimulationRunning => false;
            public int CurrentBoardGearCount => CurrentCount;
            public int MaxAllowedBoardGears => MaximumCount;
            public bool ContainsMotorCog => true;
            public IGridNode GetNode(Vector2Int coord) => Occupant;
            public IEnumerable<IGridNode> GetAllNodes() => Array.Empty<IGridNode>();
            public void ToggleSimulation() { }
            public void LoadLayout(BoardLayoutData layout) { }
            public bool TryMoveBoardGear(IGridNode node, Vector2Int toPos, Vector2Int fromPos) => false;
            public bool TryRemoveBoardGear(IGridNode node) => false;
            public bool TryDeleteBoardGear(IGridNode node) => false;
            public void SnapNodeBackToOriginal(IGridNode node, Vector2Int originalPos) { }

            public bool TryPlace(Vector2Int targetDropPos, GearItemData gearData)
            {
                TryPlaceCalls++;
                return TryPlaceResult;
            }
        }

        private sealed class StubEngineService : IGearEngineService
        {
            public bool IsRunning => false;
            public void Play() { }
            public void Stop() { }
            public void ResetGridSimulationState() { }
            public IEnumerable<IGridNode> GetAllNodes() => Array.Empty<IGridNode>();
        }

        private sealed class StubInventoryService : IInventoryService
        {
            public string MotorCogGearId => string.Empty;
            public bool HasSavedInventory => false;
            public IReadOnlyList<OwnedGear> Owned => Array.Empty<OwnedGear>();
            public event Action InventoryChanged
            {
                add { }
                remove { }
            }
            public OwnedGear Add(GearItem gear) => null;
            public bool Remove(OwnedGear gear) => false;
            public void Clear() { }
        }

        private sealed class StubEventBus : IEventBus
        {
            public void AddListener<T>(Action<T> evt) where T : ContextEvent { }
            public void RemoveListener<T>(Action<T> evt) where T : ContextEvent { }
            public void AddListener(Type type, Action<ContextEvent> evt) { }
            public void RemoveListener(Type type, Action<ContextEvent> evt) { }
            public void Raise(ContextEvent evt) { }
            public void Clear() { }
        }

        private sealed class StubNode : IGridNode
        {
            public Vector2Int Position => Vector2Int.zero;
            public float CurrentRotation => 0f;
            public GearItemData ConfigData => null;
            public float LocalSpeedMultiplier { get; set; }
            public bool IsActive { get; set; }
            public bool IsInteractable => true;
            public IEventBus EventBus => null;
            public void SetPosition(Vector2Int position) { }
            public void AddAbility(GearAbilitySO ability, float duration = -1f) { }
            public void RemoveAbility(GearAbilitySO ability) { }
            public void Initialize(Vector2Int position, GearItemData configData) { }
            public void NodeUpdate(float deltaTime, float speedModifier) { }
            public void WindDownUpdate(float deltaTime, float speedModifier) { }
            public IEnumerable<GearAbilitySO> GetAbilities() => Array.Empty<GearAbilitySO>();
            public void ResetSimulationState() { }
            public void Dispose() { }
        }
    }
}
