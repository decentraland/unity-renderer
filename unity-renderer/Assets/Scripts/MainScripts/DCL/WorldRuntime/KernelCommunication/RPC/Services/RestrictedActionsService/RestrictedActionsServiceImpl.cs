using Cysharp.Threading.Tasks;
using DCL.Helpers.NFT;
using Decentraland.Renderer.RendererServices;
using rpc_csharp;
using RPC.Context;
using System;
using System.Threading;
using UnityEngine;

namespace RPC.Services
{
    public class RestrictedActionsServiceImpl : IRestrictedActionsService<RPCContext>
    {
        public const int MAX_ELAPSED_FRAMES_SINCE_INPUT = 30;

        private static readonly OpenModalResponse OPEN_MODAL_SUCCESS_RESPONSE = new OpenModalResponse() { Success = true };
        private static readonly OpenModalResponse OPEN_MODAL_FAIL_RESPONSE = new OpenModalResponse() { Success = false };
        private static readonly TeleportToResponse TELEPORT_TO_RESPONSE = new TeleportToResponse() {};

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            RestrictedActionsServiceCodeGen.RegisterService(port, new RestrictedActionsServiceImpl());
        }

        public async UniTask<TeleportToResponse> TeleportTo(TeleportToRequest request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);
            RestrictedActionsContext restrictedActions = context.restrictedActions;

            int currentFrameCount = restrictedActions.GetCurrentFrameCount?.Invoke() ?? GetCurrentFrameCount();

            try
            {
                ct.ThrowIfCancellationRequested();
                if ((currentFrameCount - restrictedActions.LastFrameWithInput) <= MAX_ELAPSED_FRAMES_SINCE_INPUT)
                    restrictedActions.TeleportToPrompt?.Invoke((int)request.WorldCoordinates.X, (int)request.WorldCoordinates.Y);
            }
            catch (OperationCanceledException _)
            { // ignored
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return TELEPORT_TO_RESPONSE;
        }

        public async UniTask<OpenModalResponse> OpenExternalUrl(OpenExternalUrlRequest request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);
            RestrictedActionsContext restrictedActions = context.restrictedActions;

            bool success = false;
            int currentFrameCount = restrictedActions.GetCurrentFrameCount?.Invoke() ?? GetCurrentFrameCount();

            try
            {
                ct.ThrowIfCancellationRequested();

                if ((currentFrameCount - restrictedActions.LastFrameWithInput) <= MAX_ELAPSED_FRAMES_SINCE_INPUT)
                    success = restrictedActions.OpenExternalUrlPrompt?.Invoke(request.Url, request.SceneNumber) ?? false;
            }
            catch (OperationCanceledException _)
            { // ignored
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return success ? OPEN_MODAL_SUCCESS_RESPONSE : OPEN_MODAL_FAIL_RESPONSE;
        }

        public async UniTask<OpenModalResponse> OpenNftDialog(OpenNftDialogRequest request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);
            RestrictedActionsContext restrictedActions = context.restrictedActions;

            bool success = false;
            int currentFrameCount = restrictedActions.GetCurrentFrameCount?.Invoke() ?? GetCurrentFrameCount();

            try
            {
                ct.ThrowIfCancellationRequested();

                if ((currentFrameCount - restrictedActions.LastFrameWithInput) <= MAX_ELAPSED_FRAMES_SINCE_INPUT)
                {
                    if (NFTUtils.TryParseUrn(request.Urn, out string contractAddress, out string tokenId))
                    {
                        restrictedActions.OpenNftPrompt?.Invoke(contractAddress, tokenId);
                        success = true;
                    }
                }
            }
            catch (OperationCanceledException _)
            { // ignored
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return success ? OPEN_MODAL_SUCCESS_RESPONSE : OPEN_MODAL_FAIL_RESPONSE;
        }

        // TODO: use scene tick instead of renderer frame count
        private static int GetCurrentFrameCount()
        {
            return Time.frameCount;
        }
    }
}
