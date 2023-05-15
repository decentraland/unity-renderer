// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: decentraland.social.friendships
// file: decentraland/social/friendships/friendships.proto
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using rpc_csharp.protocol;
using rpc_csharp;
namespace Decentraland.Social.Friendships {
public interface IFriendshipsService<Context>
{

  IUniTaskAsyncEnumerable<Users> GetFriends(Payload request, Context context);

  UniTask<RequestEvents> GetRequestEvents(Payload request, Context context, CancellationToken ct);

  UniTask<UpdateFriendshipResponse> UpdateFriendshipEvent(UpdateFriendshipPayload request, Context context, CancellationToken ct);

  IUniTaskAsyncEnumerable<SubscribeFriendshipEventsUpdatesResponse> SubscribeFriendshipEventsUpdates(Payload request, Context context);

}

public static class FriendshipsServiceCodeGen
{
  public const string ServiceName = "FriendshipsService";

  public static void RegisterService<Context>(RpcServerPort<Context> port, IFriendshipsService<Context> service)
  {
    var result = new ServerModuleDefinition<Context>();
      
    result.serverStreamDefinition.Add("GetFriends", (payload, context) => { return ProtocolHelpers.SerializeMessageEnumerator<Users>(service.GetFriends(Payload.Parser.ParseFrom(payload), context)); });
    result.definition.Add("GetRequestEvents", async (payload, context, ct) => { var res = await service.GetRequestEvents(Payload.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });
    result.definition.Add("UpdateFriendshipEvent", async (payload, context, ct) => { var res = await service.UpdateFriendshipEvent(UpdateFriendshipPayload.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });
    result.serverStreamDefinition.Add("SubscribeFriendshipEventsUpdates", (payload, context) => { return ProtocolHelpers.SerializeMessageEnumerator<SubscribeFriendshipEventsUpdatesResponse>(service.SubscribeFriendshipEventsUpdates(Payload.Parser.ParseFrom(payload), context)); });

    port.RegisterModule(ServiceName, (port) => UniTask.FromResult(result));
  }
}
}
