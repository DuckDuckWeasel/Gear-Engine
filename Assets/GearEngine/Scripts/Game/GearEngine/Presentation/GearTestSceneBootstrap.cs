using GearEngine.GearEngine;
using GearEngine.GearEngine.Services.Board;
using Scaffold.Navigation.Contracts;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GearEngine.GearEngine.Presentation
{
    public sealed class GearTestSceneBootstrap : MonoBehaviour, IInitializable
    {
        [SerializeField]
        private BoardLayoutData boardSeed;

        private INavigation navigation;
        private IBoardService board;

        [Inject]
        public void Construct(INavigation navigation, IBoardService board)
        {
            this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            this.board = board ?? throw new ArgumentNullException(nameof(board));
        }

        public void Initialize()
        {
            try
            {
                if (boardSeed?.Placements != null && boardSeed.Placements.Count > 0)
                {
                    board.LoadLayout(boardSeed);
                }

                navigation.Open(new GearEngineViewModel());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GearTestSceneBootstrap] Initialize failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
