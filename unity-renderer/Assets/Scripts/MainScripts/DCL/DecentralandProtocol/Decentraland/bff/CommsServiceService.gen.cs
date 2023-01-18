// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: decentraland.bff
// file: decentraland/bff/comms_service.proto
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using rpc_csharp.protocol;
using rpc_csharp;
namespace Decentraland.Bff {
public interface ICommsService<Context>
{

  UniTask<Subscription> SubscribeToPeerMessages(SubscriptionRequest request, Context context, CancellationToken ct);

  IUniTaskAsyncEnumerable<PeerTopicSubscriptionResultElem> GetPeerMessages(Subscription request, Context context);

  UniTask<UnsubscriptionResult> UnsubscribeToPeerMessages(Subscription request, Context context, CancellationToken ct);

  UniTask<Subscription> SubscribeToSystemMessages(SubscriptionRequest request, Context context, CancellationToken ct);

  IUniTaskAsyncEnumerable<SystemTopicSubscriptionResultElem> GetSystemMessages(Subscription request, Context context);

  UniTask<UnsubscriptionResult> UnsubscribeToSystemMessages(Subscription request, Context context, CancellationToken ct);

  UniTask<PublishToTopicResult> PublishToTopic(PublishToTopicRequest request, Context context, CancellationToken ct);

}

public static class CommsServiceCodeGen
{
  public const string ServiceName = "CommsService";

  public static void RegisterService<Context>(RpcServerPort<Context> port, ICommsService<Context> service)
  {
    var result = new ServerModuleDefinition<Context>();
      
    result.definition.Add("SubscribeToPeerMessages", async (payload, context, ct) => { var res = await service.SubscribeToPeerMessages(SubscriptionRequest.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });
    result.serverStreamDefinition.Add("GetPeerMessages", (payload, context) => { return ProtocolHelpers.SerializeMessageEnumerator<PeerTopicSubscriptionResultElem>(service.GetPeerMessages(Subscription.Parser.ParseFrom(payload), context)); });
    result.definition.Add("UnsubscribeToPeerMessages", async (payload, context, ct) => { var res = await service.UnsubscribeToPeerMessages(Subscription.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });
    result.definition.Add("SubscribeToSystemMessages", async (payload, context, ct) => { var res = await service.SubscribeToSystemMessages(SubscriptionRequest.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });
    result.serverStreamDefinition.Add("GetSystemMessages", (payload, context) => { return ProtocolHelpers.SerializeMessageEnumerator<SystemTopicSubscriptionResultElem>(service.GetSystemMessages(Subscription.Parser.ParseFrom(payload), context)); });
    result.definition.Add("UnsubscribeToSystemMessages", async (payload, context, ct) => { var res = await service.UnsubscribeToSystemMessages(Subscription.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });
    result.definition.Add("PublishToTopic", async (payload, context, ct) => { var res = await service.PublishToTopic(PublishToTopicRequest.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });

    port.RegisterModule(ServiceName, (port) => UniTask.FromResult(result));
  }
}
}
