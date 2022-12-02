// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: 
// file: decentraland/renderer/kernel_services/friend_request_kernel.proto
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using rpc_csharp.protocol;
using rpc_csharp;

public interface IFriendRequestKernelService<Context>
{

  UniTask<GetFriendRequestsReply> GetFriendRequests(GetFriendRequestsPayload request, Context context, CancellationToken ct);

}

public static class FriendRequestKernelServiceCodeGen
{
  public const string ServiceName = "FriendRequestKernelService";

  public static void RegisterService<Context>(RpcServerPort<Context> port, IFriendRequestKernelService<Context> service)
  {
    var result = new ServerModuleDefinition<Context>();
      
    result.definition.Add("GetFriendRequests", async (payload, context, ct) => { var res = await service.GetFriendRequests(GetFriendRequestsPayload.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });

    port.RegisterModule(ServiceName, (port) => UniTask.FromResult(result));
  }
}
