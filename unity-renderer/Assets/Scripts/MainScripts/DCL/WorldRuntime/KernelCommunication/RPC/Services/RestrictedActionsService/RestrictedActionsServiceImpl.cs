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
        private static readonly OpenModalResponse OPEN_MODAL_SUCCESS_RESPONSE = new OpenModalResponse() { Success = true };
        private static readonly OpenModalResponse OPEN_MODAL_FAIL_RESPONSE = new OpenModalResponse() { Success = false };
        private static readonly TeleportToResponse TELEPORT_TO_RESPONSE = new TeleportToResponse() { };

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            RestrictedActionsServiceCodeGen.RegisterService(port, new RestrictedActionsServiceImpl());
        }

        public async UniTask<TeleportToResponse> TeleportTo(TeleportToRequest request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);
            RestrictedActionsContext restrictedActions = context.restrictedActions;

            int sceneNumber = request.SceneNumber;

            try
            {
                ct.ThrowIfCancellationRequested();

                if (restrictedActions.IsSceneRestrictedActionEnabled(sceneNumber))
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
            int sceneNumber = request.SceneNumber;

            try
            {
                ct.ThrowIfCancellationRequested();

                if (restrictedActions.IsSceneRestrictedActionEnabled(sceneNumber))
                    success = restrictedActions.OpenExternalUrlPrompt?.Invoke(request.Url, sceneNumber) ?? false;
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
            int sceneNumber = request.SceneNumber;

            try
            {
                ct.ThrowIfCancellationRequested();

                if (restrictedActions.IsSceneRestrictedActionEnabled(sceneNumber))
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
    }
}
