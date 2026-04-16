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

        private BoardViewModel viewModel;
        private GearViewFactory localFactory;
        private readonly Dictionary<IGridNode, GearView> viewsByNode = new Dictionary<IGridNode, GearView>();

        public event Action<GearConfigData, Vector3> OnGearDroppedOverUI;
        public event Action<IGridNode> OnTrashDropRequested;

        public void Bind(BoardViewModel vm, bool interactable = false)
        {
            Unbind();
            InitializeBinding(vm);
            SubscribeToViewModel();
            SyncViewsFromModel();
            UpdateDragHandlerState(interactable);
        }

        private void OnDestroy()
        {
            Unbind();
        }

        public void Unbind()
        {
            if (viewModel == null)
            {
                return;
            }

            viewModel.OnGearPlaced -= HandleGearPlaced;
            viewModel.OnGearRemoved -= HandleGearRemoved;
            DestroyAllViews();
            localFactory = null;
            viewModel = null;
            UpdateDragHandlerState(false);
        }

        internal void NotifyPickedUp(IGridNode node, Vector2Int coord)
        {
            viewModel?.OnGearPickedUp(node, coord);
        }

        internal void NotifyDropped(IGridNode node, Vector2Int coord)
        {
            viewModel?.OnGearDropped(node, coord);
        }

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

        internal GearView DetachViewForDrag(IGridNode node)
        {
            if (node == null)
            {
                return null;
            }

            if (viewsByNode.TryGetValue(node, out GearView view))
            {
                viewsByNode.Remove(node);
                return view;
            }

            return null;
        }

        internal void DestroyDetachedView(GearView view)
        {
            if (view != null)
            {
                DestroyViewGameObject(view.gameObject);
            }
        }

        internal IEnumerable<GearView> GetViews()
        {
            return viewsByNode.Values;
        }

        internal bool IsRunning()
        {
            return viewModel?.EngineService?.IsRunning ?? false;
        }

        internal BoardConfigSO GetBoardConfig()
        {
            return viewModel?.BoardConfig;
        }

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

        private void InitializeBinding(BoardViewModel vm)
        {
            viewModel = vm ?? throw new ArgumentNullException(nameof(vm));
            localFactory = new GearViewFactory();
        }

        private void SubscribeToViewModel()
        {
            viewModel.OnGearPlaced += HandleGearPlaced;
            viewModel.OnGearRemoved += HandleGearRemoved;
        }

        private void SyncViewsFromModel()
        {
            foreach (IGridNode node in viewModel.GetCurrentNodes())
            {
                SpawnView(node);
            }
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

        private void UpdateDragHandlerState(bool interactable)
        {
            if (dragHandler != null)
            {
                dragHandler.enabled = interactable;
                return;
            }

            if (interactable)
            {
                Debug.LogError("<color=#ff0000>[BoardView]</color> CRITICAL ERROR: dragHandler is NULL in inspector! Board interactions will be silently disabled!");
            }
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
    }
}
