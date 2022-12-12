﻿using System.Threading;
using Cysharp.Threading.Tasks;
using Decentraland.Renderer.RendererServices;
using rpc_csharp;

namespace RPC.Services
{
    public class EmotesRendererServiceImpl : IEmotesRendererService<RPCContext>
    {
        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            EmotesRendererServiceCodeGen.RegisterService(port, new EmotesRendererServiceImpl());
        }

        public UniTask<TriggerSelfUserExpressionResponse> TriggerSelfUserExpression(TriggerSelfUserExpressionRequest request, RPCContext context, CancellationToken ct)
        {
            UserProfile.GetOwnUserProfile().SetAvatarExpression(request.Id, UserProfile.EmoteSource.Command);
            return default;
        }
    }
}
