
// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: decentraland.renderer.renderer_services
// file: decentraland/renderer/renderer_services/friend_request_renderer.proto
using Cysharp.Threading.Tasks;
using rpc_csharp;

namespace Decentraland.Renderer.RendererServices {
public class ClientFriendRequestRendererService
{
  private readonly RpcClientModule module;

  public ClientFriendRequestRendererService(RpcClientModule module)
  {
      this.module = module;
  }

  public UniTask<ApproveFriendRequestReply> ApproveFriendRequest(ApproveFriendRequestPayload request)
  {
      return module.CallUnaryProcedure<ApproveFriendRequestReply>("ApproveFriendRequest", request);
  }

  public UniTask<RejectFriendRequestReply> RejectFriendRequest(RejectFriendRequestPayload request)
  {
      return module.CallUnaryProcedure<RejectFriendRequestReply>("RejectFriendRequest", request);
  }

  public UniTask<CancelFriendRequestReply> CancelFriendRequest(CancelFriendRequestPayload request)
  {
      return module.CallUnaryProcedure<CancelFriendRequestReply>("CancelFriendRequest", request);
  }

  public UniTask<ReceiveFriendRequestReply> ReceiveFriendRequest(ReceiveFriendRequestPayload request)
  {
      return module.CallUnaryProcedure<ReceiveFriendRequestReply>("ReceiveFriendRequest", request);
  }
}
}
