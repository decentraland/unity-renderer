using System.Threading;
using Cysharp.Threading.Tasks;

namespace RPC.Services
{
    public class EmotesServiceImpl : IEmotesRendererService<RPCContext>
    {
        public UniTask<EmotesResponse> TriggerSelfUserExpression(TriggerSelfUserExpressionRequest request, RPCContext context, CancellationToken ct)
        {
            UserProfile.GetOwnUserProfile().SetAvatarExpression(request.Id, UserProfile.EmoteSource.Command);
            return default;
        }
    }
}