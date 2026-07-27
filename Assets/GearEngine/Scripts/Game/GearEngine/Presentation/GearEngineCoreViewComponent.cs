using GearEngine.GearEngine.Presentation.UI;
using Scaffold.MVVM;
using UnityEngine;

namespace GearEngine.GearEngine.Presentation
{
    public sealed class GearEngineCoreViewComponent : ViewComponent<GearEngineViewModel>
    {
        [SerializeField] private GearWorkspaceView workspace;

        protected override void OnBind()
        {
            base.OnBind();
            workspace ??= GetComponentInChildren<GearWorkspaceView>(includeInactive: true);
            if (workspace == null)
            {
                Debug.LogError("[GearEngineCoreViewComponent] GearWorkspaceView is missing.");
                return;
            }

            workspace.BindInteractive(
                viewModel.Board,
                viewModel.Inventory,
                viewModel.TrashZone,
                viewModel.DragService);
        }
    }
}
