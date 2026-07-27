using System;
using System.Collections.Generic;
using System.Linq;
using GearEngine.GearEngine.Abilities;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Events;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation.UI;
using GearEngine.GearEngine.Services;
using GearEngine.GearEngine.Services.Board;
using NUnit.Framework;
using Scaffold.Events.Contracts;
using UnityEditor;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    [TestFixture]
    public sealed class BoardViewModelCapacityTests
    {
        [Test]
        public void Constructor_FormatsAuthoritativeTotalCapacity()
        {
            StubBoardService board = new StubBoardService
            {
                CurrentCount = 6,
                MaximumCount = 6,
            };

            BoardViewModel viewModel = CreateViewModel(board);

            Assert.That(viewModel.BoardCapacityText, Is.EqualTo("6/6"));
            Assert.That(viewModel.BoardLimitText, Is.EqualTo("Board: 6/6"));
        }

        [Test]
        public void BoardCountEvents_RefreshCapacityText()
        {
            StubBoardService board = new StubBoardService
            {
                CurrentCount = 5,
                MaximumCount = 6,
            };
            BoardViewModel viewModel = CreateViewModel(board);

            board.CurrentCount = 6;
            board.RaiseGearPlaced();
            Assert.That(viewModel.BoardCapacityText, Is.EqualTo("6/6"));

            board.CurrentCount = 5;
            board.RaiseGearRemoved();
            Assert.That(viewModel.BoardCapacityText, Is.EqualTo("5/6"));

            board.CurrentCount = 4;
            board.RaiseBoardLayoutChanged();
            Assert.That(viewModel.BoardCapacityText, Is.EqualTo("4/6"));
        }

        [Test]
        public void HandleInventoryDrop_FullEmptyCell_RejectsWithoutCallingService()
        {
            StubBoardService board = new StubBoardService
            {
                CurrentCount = 6,
                MaximumCount = 6,
                Occupant = null,
                TryPlaceResult = true,
            };
            BoardViewModel viewModel = CreateViewModel(board);

            bool accepted = viewModel.HandleInventoryDrop(Vector2Int.zero, new GearItemData());

            Assert.IsFalse(accepted);
            Assert.That(board.TryPlaceCalls, Is.Zero);
            Assert.That(viewModel.CapacityFeedbackRevision, Is.EqualTo(1));
        }

        [Test]
        public void HandleInventoryDrop_FullOccupiedCell_DelegatesForMerge()
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

            Assert.IsTrue(accepted);
            Assert.That(board.TryPlaceCalls, Is.EqualTo(1));
            Assert.That(viewModel.CapacityFeedbackRevision, Is.Zero);
        }

        [Test]
        public void SetupPrefab_WiresCapacityChipToCogLabel()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GearEngine/Prefabs/Campaign/Setup View.prefab");

            Assert.IsNotNull(prefab);
            BoardCapacityChipView chip = prefab.GetComponent<BoardCapacityChipView>();
            Assert.IsNotNull(chip);
            Assert.IsNotNull(chip.CapacityLabel);
            Assert.That(chip.CapacityLabel.transform.parent.name, Is.EqualTo("chips_cogs"));

            MonoBehaviour setupView = prefab
                .GetComponents<MonoBehaviour>()
                .First(component => component != null && component.GetType().Name == "SetupView");
            SerializedProperty chipProperty =
                new SerializedObject(setupView).FindProperty("boardCapacityChip");
            Assert.IsNotNull(chipProperty);
            Assert.That(chipProperty.objectReferenceValue, Is.SameAs(chip));
        }

        [TestCase("Assets/GearEngine/Prefabs/Buttons/StartRace_Button.prefab")]
        [TestCase("Assets/GearEngine/Prefabs/UI/Chips/chips_tracks.prefab")]
        [TestCase("Assets/GearEngine/Prefabs/UI/Header/headerchips_group.prefab")]
        [TestCase("Assets/GearEngine/Prefabs/Campaign/PFB_BoardView.prefab")]
        [TestCase("Assets/GearEngine/Prefabs/Campaign/Setup View.prefab")]
        public void SetupPrefabs_ContainNoMissingScripts(string prefabPath)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            Assert.IsNotNull(prefab);

            string[] objectsWithMissingScripts = prefab
                .GetComponentsInChildren<Transform>(includeInactive: true)
                .Where(transform =>
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        transform.gameObject) > 0)
                .Select(transform => transform.name)
                .ToArray();

            Assert.That(
                objectsWithMissingScripts,
                Is.Empty,
                $"{prefabPath}: {string.Join(", ", objectsWithMissingScripts)}");
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
            public event Action<IGridNode> GearPlaced;
            public event Action<IGridNode> GearRemoved;
            public event Action BoardLayoutChanged;

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

            public void RaiseGearPlaced() => GearPlaced?.Invoke(Occupant);
            public void RaiseGearRemoved() => GearRemoved?.Invoke(Occupant);
            public void RaiseBoardLayoutChanged() => BoardLayoutChanged?.Invoke();
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
