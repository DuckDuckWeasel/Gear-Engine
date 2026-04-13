using System;
using System.Collections.Generic;
using Scaffold.GearEngine;
using Scaffold.MVVM;
using UnityEngine;

namespace Scaffold.GearEngine.Presentation.UI
{
    [DisallowMultipleComponent]
    public class BoardView : ViewComponent<BoardViewModel>
    {
        [SerializeField] private GearBoardDragHandler dragHandler;

        private GearViewFactory localFactory;
        private readonly Dictionary<IGridNode, GearView> viewsByNode = new Dictionary<IGridNode, GearView>();

        private bool dragInteractable;

        public event Action<GearConfigData, Vector3> OnGearDroppedOverUI;

        /// <summary>
        /// Binds the board view model and configures whether board drag is enabled for this binding.
        /// </summary>
        public void Bind(BoardViewModel vm, bool interactable)
        {
            dragInteractable = interactable;
            base.Bind(vm ?? throw new ArgumentNullException(nameof(vm)));
        }

        public new void Unbind()
        {
            base.Unbind();
        }

        protected override void OnBind()
        {
            localFactory = new GearViewFactory();

            viewModel.OnGearPlaced += HandleGearPlaced;
            viewModel.OnGearRemoved += HandleGearRemoved;

            foreach (IGridNode node in viewModel.GetCurrentNodes())
            {
                SpawnView(node);
            }

            if (dragHandler != null)
            {
                dragHandler.enabled = dragInteractable;
            }
        }

        protected override void OnUnbind()
        {
            if (viewModel != null)
            {
                viewModel.OnGearPlaced -= HandleGearPlaced;
                viewModel.OnGearRemoved -= HandleGearRemoved;
            }

            DestroyAllViews();
            localFactory = null;

            if (dragHandler != null)
            {
                dragHandler.enabled = false;
            }

            base.OnUnbind();
        }

        internal void NotifyPickedUp(IGridNode node, Vector2Int coord)
            => viewModel?.OnGearPickedUp(node, coord);

        internal void NotifyDropped(IGridNode node, Vector2Int coord)
            => viewModel?.OnGearDropped(node, coord);

        internal void NotifyBoardGearDroppedOverUI(IGridNode node, GearConfigData config, Vector3 worldPos)
        {
            viewModel?.HandleBoardGearReturnedOverUI(node);
            if (config != null)
            {
                OnGearDroppedOverUI?.Invoke(config, worldPos);
            }
        }

        internal IEnumerable<GearView> GetViews() => viewsByNode.Values;

        internal bool IsRunning() => viewModel?.EngineService?.IsRunning ?? false;

        internal BoardConfigSO GetBoardConfig() => viewModel?.BoardConfig;

        private void HandleGearPlaced(IGridNode node)
        {
            if (node == null)
            {
                return;
            }

            SpawnView(node);
        }

        private void HandleGearRemoved(IGridNode node)
        {
            if (node == null)
            {
                return;
            }

            if (!viewsByNode.TryGetValue(node, out GearView view))
            {
                return;
            }

            viewsByNode.Remove(node);
            DestroyViewGameObject(view.gameObject);
        }

        private void SpawnView(IGridNode node)
        {
            if (node == null)
            {
                return;
            }

            if (viewsByNode.ContainsKey(node))
            {
                return;
            }

            Vector3 localPosition = viewModel.BoardConfig.GetWorldPosition(node.Position);
            GearView view = localFactory.CreateView(node, node.ConfigData, transform, localPosition);
            view.Initialize(node, node.ConfigData, viewModel.BoardConfig, localFactory);
            viewsByNode[node] = view;
        }

        private void DestroyAllViews()
        {
            foreach (KeyValuePair<IGridNode, GearView> pair in viewsByNode)
            {
                if (pair.Value != null)
                {
                    DestroyViewGameObject(pair.Value.gameObject);
                }
            }

            viewsByNode.Clear();
        }

        private void DestroyViewGameObject(GameObject go)
        {
            if (go == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(go);
                return;
            }
#endif
            UnityEngine.Object.Destroy(go);
        }

        private void OnDestroy() => Unbind();
    }
}
