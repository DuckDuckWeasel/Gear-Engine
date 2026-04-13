using System;
using System.Collections.Generic;
using GearEngine.GearEngine.Bootstrap;
using GearEngine.GearEngine.Config;
using GearEngine.GearEngine.Nodes;
using GearEngine.GearEngine.Visuals;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation.UI
{
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private GearBoardDragHandler dragHandler;

        public event Action<GearConfigData, Vector3> OnGearDroppedOverUI;
        public event Action<GearConfigData> OnDragStarted;
        public event Action OnDragEnded;
        public event Action<IGridNode> OnTrashDropRequested;

        private BoardViewModel viewModel;
        private GearViewFactory localFactory;
        private readonly Dictionary<IGridNode, GearView> viewsByNode = new Dictionary<IGridNode, GearView>();

        public void Bind(BoardViewModel vm, bool interactable = false)
        {
            Unbind();
            viewModel = vm ?? throw new ArgumentNullException(nameof(vm));
            localFactory = new GearViewFactory();

            vm.OnGearPlaced += HandleGearPlaced;
            vm.OnGearRemoved += HandleGearRemoved;
            vm.OnBoardDragStarted += HandleDragStarted;
            vm.OnBoardDragEnded += HandleDragEnded;

            foreach (IGridNode node in vm.GetCurrentNodes())
            {
                SpawnView(node);
            }

            if (dragHandler != null)
            {
                dragHandler.enabled = interactable;
            }
        }

        public void Unbind()
        {
            if (viewModel == null)
            {
                return;
            }

            viewModel.OnGearPlaced -= HandleGearPlaced;
            viewModel.OnGearRemoved -= HandleGearRemoved;
            viewModel.OnBoardDragStarted -= HandleDragStarted;
            viewModel.OnBoardDragEnded -= HandleDragEnded;
            DestroyAllViews();
            localFactory = null;
            viewModel = null;

            if (dragHandler != null)
            {
                dragHandler.enabled = false;
            }
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

        internal void NotifyTrashDrop(IGridNode node)
        {
            OnTrashDropRequested?.Invoke(node);
        }

        private void HandleDragStarted(GearConfigData data) => OnDragStarted?.Invoke(data);
        private void HandleDragEnded() => OnDragEnded?.Invoke();

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
                Debug.Log($"<color=#ff9900>[BoardView]</color> HandleGearRemoved: no view found for node '{node.ConfigData?.Id}' at {node.Position}.");
                return;
            }

            Debug.Log($"<color=#ff5555>[BoardView]</color> Destroying GearView for '{node.ConfigData?.Id}' at {node.Position}.");
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

        private static void DestroyViewGameObject(GameObject go)
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
