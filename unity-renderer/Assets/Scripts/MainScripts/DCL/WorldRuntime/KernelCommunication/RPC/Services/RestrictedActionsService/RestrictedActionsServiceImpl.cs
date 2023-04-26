using Cysharp.Threading.Tasks;
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

        private static readonly OpenModalResponse SUCCESS_RESPONSE = new OpenModalResponse() { Success = true };
        private static readonly OpenModalResponse FAIL_RESPONSE = new OpenModalResponse() { Success = false };

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            RestrictedActionsServiceCodeGen.RegisterService(port, new RestrictedActionsServiceImpl());
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
                {
                    success = restrictedActions.OpenExternalUrlPrompt?.Invoke(request.Url, request.SceneNumber) ?? false;
                }
            }
            catch (OperationCanceledException _)
            { // ignored
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return success ? SUCCESS_RESPONSE : FAIL_RESPONSE;
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
                    if (TryParseUrn(request.Urn, out string contractAddress, out string tokenId))
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

            return success ? SUCCESS_RESPONSE : FAIL_RESPONSE;
        }

        // TODO: use scene tick instead of renderer frame count
        private static int GetCurrentFrameCount()
        {
            return Time.frameCount;
        }

        // TODO: change this when support for wearables/emotes/etc urn is added
        // urn format urn:<CHAIN>:<CONTRACT_STANDARD>:<CONTRACT>:<TOKEN_ID> i.e: urn:ethereum:erc721:0x00...000:123
        internal static bool TryParseUrn(string urn, out string contractAddress, out string tokenId)
        {
            const char SEPARATOR = ':';
            const string CHAIN_ETHEREUM = "ethereum";

            contractAddress = string.Empty;
            tokenId = string.Empty;

            try
            {
                var urnSpan = urn.AsSpan();

                if (!urnSpan.Slice(0, 3).Equals("urn", StringComparison.Ordinal))
                    return false;

                urnSpan = urnSpan.Slice(4);

                var chainSpan = urnSpan.Slice(0, CHAIN_ETHEREUM.Length);
                urnSpan = urnSpan.Slice(chainSpan.Length + 1);

                if (!chainSpan.Equals(CHAIN_ETHEREUM, StringComparison.Ordinal))
                    return false;

                var contractStandardSpan = urnSpan.Slice(0, urnSpan.IndexOf(SEPARATOR));
                urnSpan = urnSpan.Slice(contractStandardSpan.Length + 1);

                var contractAddressSpan = urnSpan.Slice(0, urnSpan.IndexOf(SEPARATOR));
                urnSpan = urnSpan.Slice(contractAddressSpan.Length + 1);

                var tokenIdSpan = urnSpan;

                contractAddress = contractAddressSpan.ToString();
                tokenId = tokenIdSpan.ToString();
                return true;
            }
            catch (Exception _)
            { // ignored
            }

            return false;
        }
    }
}
