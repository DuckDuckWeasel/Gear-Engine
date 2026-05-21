using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiveOps.Modules.DTO.Roguelike;
using LiveOps.Modules.DTO.ModuleRequests;
using Scaffold.LiveOps;
using UnityEngine;
using VContainer;

namespace GearEngine.Campaign.Bootstrap.LiveOps
{
    public sealed class RoguelikeClientModule : GameClientModuleBase<RoguelikeGameData>
    {
        public RoguelikeClientModule(ILiveOpsService liveOps) : base(liveOps)
        {
        }

        public IReadOnlyList<string> CurrentRollIds => data?.CurrentRollIds;

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

        public async Task<bool> SkipAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await SendSkipAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeClientModule] SkipAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public async Task<IReadOnlyList<string>> RerollAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await FetchAndApplyRerollAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RoguelikeClientModule] RerollAsync failed: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private async Task<IReadOnlyList<string>> FetchAndApplyDrawAsync(CancellationToken cancellationToken)
        {
            DrawRoguelikeRollResponse resp = await liveOps.CallAsync(new DrawRoguelikeRollRequest(), cancellationToken);
            if (resp != null && data != null)
            {
                data.CurrentRollIds = new List<string>(resp.CurrentRollIds);
            }

            return data?.CurrentRollIds ?? (IReadOnlyList<string>)Array.Empty<string>();
        }

        private async Task<bool> SendClaimAsync(string pickedGearId, CancellationToken cancellationToken)
        {
            ClaimRoguelikePickResponse resp = await liveOps.CallAsync(new ClaimRoguelikePickRequest(pickedGearId), cancellationToken);
            if (resp != null && resp.Success && data != null)
            {
                data.CurrentRollIds = new List<string>();
            }

            return resp != null && resp.Success;
        }

        private async Task<bool> SendSkipAsync(CancellationToken cancellationToken)
        {
            SkipRoguelikePickResponse resp = await liveOps.CallAsync(new SkipRoguelikePickRequest(), cancellationToken);
            if (resp != null && resp.Success && data != null)
            {
                data.CurrentRollIds = new List<string>();
            }

            return resp != null && resp.Success;
        }

        private async Task<IReadOnlyList<string>> FetchAndApplyRerollAsync(CancellationToken cancellationToken)
        {
            RerollRoguelikeRollResponse resp = await liveOps.CallAsync(new RerollRoguelikeRollRequest(), cancellationToken);
            if (resp != null && data != null)
            {
                data.CurrentRollIds = new List<string>(resp.CurrentRollIds);
            }

            return data?.CurrentRollIds ?? (IReadOnlyList<string>)Array.Empty<string>();
        }
    }
}
