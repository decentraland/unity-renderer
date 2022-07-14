// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: 
// file: RendererProtocol.proto
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using rpc_csharp.protocol;
using rpc_csharp;

public abstract class CRDTService<Context>
{
    public const string ServiceName = "CRDTService";

    public delegate UniTask<CRDTResponse> SendCrdt(CRDTManyMessages request, Context context , CancellationToken ct);

    public delegate UniTask<CRDTManyMessages> PullCrdt(PullCRDTRequest request, Context context , CancellationToken ct);

    public delegate IEnumerator<CRDTManyMessages> CrdtNotificationStream(CRDTStreamRequest request, Context context );

    public static void RegisterService(RpcServerPort<Context> port, SendCrdt sendCrdt, PullCrdt pullCrdt, CrdtNotificationStream crdtNotificationStream)
    {
        var result = new ServerModuleDefinition<Context>();
      
        result.definition.Add("SendCrdt", async (payload, context, ct) => { var res = await sendCrdt(CRDTManyMessages.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });
        result.definition.Add("PullCrdt", async (payload, context, ct) => { var res = await pullCrdt(PullCRDTRequest.Parser.ParseFrom(payload), context, ct); return res?.ToByteString(); });
        result.streamDefinition.Add("CrdtNotificationStream", (payload, context) => { return new ProtocolHelpers.StreamEnumerator<CRDTManyMessages>(crdtNotificationStream(CRDTStreamRequest.Parser.ParseFrom(payload), context)); });

        port.RegisterModule(ServiceName, (port) => UniTask.FromResult(result));
    }
}