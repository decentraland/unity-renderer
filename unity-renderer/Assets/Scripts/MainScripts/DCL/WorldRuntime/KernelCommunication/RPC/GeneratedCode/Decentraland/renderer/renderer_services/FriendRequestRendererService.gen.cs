// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: 
// file: decentraland/renderer/renderer_services/friend_request_renderer.proto
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using rpc_csharp.protocol;
using rpc_csharp;

public interface IFriendRequestRendererService<Context>
{

  UniTask<AddFriendRequestReply> AddFriendRequest(AddFriendRequestPayload request, Context context, CancellationToken ct);

}

public static class FriendRequestRendererServiceCodeGen
{
  public const string ServiceName = "FriendRequestRendererService";

  public static void RegisterService<Context>(RpcServerPort<Context> port, IFriendRequestRendererService<Context> service)
  {
    var result = new ServerModuleDefinition<Context>();
      
    result.definition.Add("AddFriendRequest", async (payload, context, ct) => { var res = await service.AddFriendRequest(AddFriendRequestPayload.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });

    port.RegisterModule(ServiceName, (port) => UniTask.FromResult(result));
  }
}
