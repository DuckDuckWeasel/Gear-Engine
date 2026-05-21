using System;
using System.Collections.Generic;
using GearEngine.GearEngine;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Presentation;
using GearEngine.GearEngine.Services.Board;
using NUnit.Framework;
using Scaffold.Navigation.Contracts;
using UnityEngine;

namespace GearEngine.GearEngine.Tests.Editor
{
    public sealed class GearTestSceneBootstrapTests
    {
        [Test]
        public void GearTestSceneBootstrap_Initialize_OpensGearEngineViewModel()
        {
            var go = new GameObject("GearBootstrapTest");
            try
            {
                GearTestSceneBootstrap bootstrap = go.AddComponent<GearTestSceneBootstrap>();
                var nav = new CapturingNavigation();
                var board = new StubBoardService();
                bootstrap.Construct(nav, board);
                bootstrap.Initialize();
                Assert.That(nav.LastOpened, Is.InstanceOf<GearEngineViewModel>());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        private sealed class StubBoardService : IBoardService
        {
            public event Action<IGridNode> GearPlaced;
            public event Action<IGridNode> GearRemoved;
            public event Action BoardLayoutChanged;

            public BoardModel GetBoard() => new BoardModel();

            public BoardRulesSO BoardRules => null;

            public bool IsSimulationRunning => false;

            public int CurrentBoardGearCount => 0;

            public int MaxAllowedBoardGears => 99;

            public bool ContainsMotorCog => true;

            public IGridNode GetNode(Vector2Int coord) => null;

            public IEnumerable<IGridNode> GetAllNodes() => Array.Empty<IGridNode>();

            public void ToggleSimulation()
            {
            }

            public void LoadLayout(BoardLayoutData layout)
            {
            }

            public bool TryMoveBoardGear(IGridNode node, Vector2Int toPos, Vector2Int fromPos) => false;

            public bool TryPlace(Vector2Int targetDropPos, GearItemData gearData) => false;

            public bool TryRemoveBoardGear(IGridNode node) => false;

            public bool TryDeleteBoardGear(IGridNode node) => false;

            public void SnapNodeBackToOriginal(IGridNode node, Vector2Int originalPos)
            {
            }
        }

        private sealed class CapturingNavigation : INavigation
        {
            public IViewController LastOpened { get; private set; }

            public IViewController CurrentController => LastOpened;

            public void Open<TViewController>(TViewController controller, bool closeCurrent = false, NavigationOptions options = null) where TViewController : IViewController
            {
                LastOpened = controller;
            }

            public void Open<TViewController>(TViewController controller, NavigationOptions options) where TViewController : IViewController
            {
                LastOpened = controller;
            }

            public void PrepareDependencies(IViewController controller)
            {
            }

            public void Close<TViewController>(TViewController controller) where TViewController : IViewController
            {
            }

            public IViewController Return()
            {
                return null;
            }
        }
    }
}
