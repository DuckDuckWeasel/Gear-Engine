using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameModuleDTO.Modules.Roguelike;
using GameModuleDTO.ModuleRequests;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class RoguelikeClientModule : GameClientModuleBase<RoguelikeGameData>
    {
        public RoguelikeClientModule(IObjectResolver resolver, ILiveOpsService liveOps) : base(resolver)
        {
            liveOpsService = liveOps ?? throw new ArgumentNullException(nameof(liveOps));
        }

        public IReadOnlyList<string> CurrentRollIds => data?.CurrentRollIds;

        private readonly ILiveOpsService liveOpsService;

        public async Task<IReadOnlyList<string>> EnsureCurrentRollAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (data != null && data.CurrentRollIds.Count > 0)
                {
                    return data.CurrentRollIds;
                }

                return await FetchAndApplyDrawAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeClientModule] EnsureCurrentRollAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public async Task<bool> ClaimAsync(string pickedGearId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(pickedGearId))
            {
                throw new ArgumentException("pickedGearId is required.", nameof(pickedGearId));
            }

            try
            {
                return await SendClaimAsync(pickedGearId, cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeClientModule] ClaimAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private async Task<IReadOnlyList<string>> FetchAndApplyDrawAsync(CancellationToken cancellationToken)
        {
            DrawRoguelikeRollResponse resp = await liveOpsService.CallAsync(new DrawRoguelikeRollRequest(), cancellationToken);
            if (resp != null && data != null)
            {
                data.CurrentRollIds = new List<string>(resp.CurrentRollIds);
            }

            return data?.CurrentRollIds ?? (IReadOnlyList<string>)Array.Empty<string>();
        }

        private async Task<bool> SendClaimAsync(string pickedGearId, CancellationToken cancellationToken)
        {
            ClaimRoguelikePickResponse resp = await liveOpsService.CallAsync(new ClaimRoguelikePickRequest(pickedGearId), cancellationToken);
            if (resp != null && resp.Success && data != null)
            {
                data.CurrentRollIds = new List<string>();
            }

            return resp != null && resp.Success;
        }
    }
}
