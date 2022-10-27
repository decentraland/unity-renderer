// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: 
// file: protocol.proto
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using rpc_csharp.protocol;
using rpc_csharp;

public interface ICRDTService<Context>
{

  UniTask<CRDTResponse> SendCrdt(CRDTManyMessages request, Context context, CancellationToken ct);

  UniTask<CRDTManyMessages> PullCrdt(PullCRDTRequest request, Context context, CancellationToken ct);

  IUniTaskAsyncEnumerable<CRDTManyMessages> CrdtNotificationStream(CRDTStreamRequest request, Context context);

}

public static class CRDTServiceCodeGen
{
  public const string ServiceName = "CRDTService";

  public static void RegisterService<Context>(RpcServerPort<Context> port, ICRDTService<Context> service)
  {
    var result = new ServerModuleDefinition<Context>();
      
    result.definition.Add("SendCrdt", async (payload, context, ct) => { var res = await service.SendCrdt(CRDTManyMessages.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });
    result.definition.Add("PullCrdt", async (payload, context, ct) => { var res = await service.PullCrdt(PullCRDTRequest.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });
    result.serverStreamDefinition.Add("CrdtNotificationStream", (payload, context) => { return ProtocolHelpers.SerializeMessageEnumerator<CRDTManyMessages>(service.CrdtNotificationStream(CRDTStreamRequest.Parser.ParseFrom(payload), context)); });

    port.RegisterModule(ServiceName, (port) => UniTask.FromResult(result));
  }
}
