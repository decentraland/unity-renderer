// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: decentraland.renderer.kernel_services
// file: decentraland/renderer/kernel_services/mutual_friends_kernel.proto
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using rpc_csharp.protocol;
using rpc_csharp;
namespace Decentraland.Renderer.KernelServices {
public interface IMutualFriendsKernelService<Context>
{

  UniTask<GetMutualFriendsResponse> GetMutualFriends(GetMutualFriendsRequest request, Context context, CancellationToken ct);

}

public static class MutualFriendsKernelServiceCodeGen
{
  public const string ServiceName = "MutualFriendsKernelService";

  public static void RegisterService<Context>(RpcServerPort<Context> port, IMutualFriendsKernelService<Context> service)
  {
    var result = new ServerModuleDefinition<Context>();
      
    result.definition.Add("GetMutualFriends", async (payload, context, ct) => { var res = await service.GetMutualFriends(GetMutualFriendsRequest.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });

    port.RegisterModule(ServiceName, (port) => UniTask.FromResult(result));
  }
}
}
