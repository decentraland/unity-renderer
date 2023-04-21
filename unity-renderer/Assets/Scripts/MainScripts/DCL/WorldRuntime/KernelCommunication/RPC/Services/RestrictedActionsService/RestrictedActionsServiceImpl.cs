using Cysharp.Threading.Tasks;
using Decentraland.Renderer.RendererServices;
using rpc_csharp;
using System;
using System.Threading;

namespace RPC.Services
{
    public class RestrictedActionsServiceImpl : IRestrictedActionsService<RPCContext>
    {
        private static readonly SuccessResponse SUCCESS_RESPONSE = new SuccessResponse() { Success = true };
        private static readonly SuccessResponse FAIL_RESPONSE = new SuccessResponse() { Success = false };

        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            RestrictedActionsServiceCodeGen.RegisterService(port, new RestrictedActionsServiceImpl());
        }

        public async UniTask<SuccessResponse> OpenExternalUrl(OpenExternalUrlRequest request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);
            bool success = false;

            try
            {
                success = context.restrictedActions.OpenExternalUrlPrompt?.Invoke(request.Url, request.SceneNumber) ?? false;
            }
            catch (Exception _)
            { // ignored
            }

            return success ? SUCCESS_RESPONSE : FAIL_RESPONSE;
        }

        public async UniTask<SuccessResponse> OpenNftDialog(OpenNftDialogRequest request, RPCContext context, CancellationToken ct)
        {
            await UniTask.SwitchToMainThread(ct);
            bool success = false;

            try
            {
                context.restrictedActions.OpenNftPrompt?.Invoke("", "");
                success = true;
            }
            catch (Exception _)
            { // ignored
            }

            return success ? SUCCESS_RESPONSE : FAIL_RESPONSE;
        }
    }
}
