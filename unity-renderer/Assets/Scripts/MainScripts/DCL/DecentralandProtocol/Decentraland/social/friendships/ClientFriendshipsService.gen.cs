
// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: decentraland.social.friendships
// file: decentraland/social/friendships/friendships.proto
using Cysharp.Threading.Tasks;
using rpc_csharp;

namespace Decentraland.Social.Friendships {
public interface IClientFriendshipsService
{
  IUniTaskAsyncEnumerable<Users> GetFriends(Payload request);

  UniTask<RequestEvents> GetRequestEvents(Payload request);

  UniTask<UpdateFriendshipResponse> UpdateFriendshipEvent(UpdateFriendshipPayload request);

  IUniTaskAsyncEnumerable<SubscribeFriendshipEventsUpdatesResponse> SubscribeFriendshipEventsUpdates(Payload request);
}

public class ClientFriendshipsService : IClientFriendshipsService
{
  private readonly RpcClientModule module;

  public ClientFriendshipsService(RpcClientModule module)
  {
      this.module = module;
  }

  
  public IUniTaskAsyncEnumerable<Users> GetFriends(Payload request)
  {
      return module.CallServerStream<Users>("GetFriends", request);
  }

  public UniTask<RequestEvents> GetRequestEvents(Payload request)
  {
      return module.CallUnaryProcedure<RequestEvents>("GetRequestEvents", request);
  }

  public UniTask<UpdateFriendshipResponse> UpdateFriendshipEvent(UpdateFriendshipPayload request)
  {
      return module.CallUnaryProcedure<UpdateFriendshipResponse>("UpdateFriendshipEvent", request);
  }

  public IUniTaskAsyncEnumerable<SubscribeFriendshipEventsUpdatesResponse> SubscribeFriendshipEventsUpdates(Payload request)
  {
      return module.CallServerStream<SubscribeFriendshipEventsUpdatesResponse>("SubscribeFriendshipEventsUpdates", request);
  }

}
}
