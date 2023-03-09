
// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: decentraland.echo
// file: decentraland/renderer/renderer_services/social_client.proto
using Cysharp.Threading.Tasks;
using rpc_csharp;

namespace Decentraland.Echo {
public interface IClientFriendshipsService
{
  UniTask<Users> GetFriends(Empty request);

  UniTask<RequestEvents> GetRequestEvents(Empty request);

  UniTask<UpdateFriendshipResponse> UpdateFriendship(UpdateFriendshipPayload request);

  IUniTaskAsyncEnumerable<SubscribeFriendshipEventsUpdatesResponse> SubscribeFriendshipEventsUpdates(Empty request);
}

public class ClientFriendshipsService : IClientFriendshipsService
{
  private readonly RpcClientModule module;

  public ClientFriendshipsService(RpcClientModule module)
  {
      this.module = module;
  }

  
  public UniTask<Users> GetFriends(Empty request)
  {
      return module.CallUnaryProcedure<Users>("GetFriends", request);
  }

  public UniTask<RequestEvents> GetRequestEvents(Empty request)
  {
      return module.CallUnaryProcedure<RequestEvents>("GetRequestEvents", request);
  }

  public UniTask<UpdateFriendshipResponse> UpdateFriendship(UpdateFriendshipPayload request)
  {
      return module.CallUnaryProcedure<UpdateFriendshipResponse>("UpdateFriendship", request);
  }

  public IUniTaskAsyncEnumerable<SubscribeFriendshipEventsUpdatesResponse> SubscribeFriendshipEventsUpdates(Empty request)
  {
      return module.CallServerStream<SubscribeFriendshipEventsUpdatesResponse>("SubscribeFriendshipEventsUpdates", request);
  }

}
}
