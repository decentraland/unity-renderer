// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: 
// file: RendererProtocol.proto
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using rpc_csharp.protocol;
using rpc_csharp;

public abstract class CRDTService<Context>
{
    public const string ServiceName = "CRDTService";

    public delegate UniTask<CRDTResponse> SendCRDT(CRDTManyMessages request, Context context);

    public delegate IEnumerator<CRDTManyMessages> CRDTNotificationStream(CRDTStreamRequest request, Context context);

    public static void RegisterService(RpcServerPort<Context> port, SendCRDT sendCRDT, CRDTNotificationStream cRDTNotificationStream)
    {
        var result = new ServerModuleDefinition<Context>();

        result.definition.Add("SendCRDT", async (payload, context) =>
        {
            var res = await sendCRDT(CRDTManyMessages.Parser.ParseFrom(payload), context);
            return res?.ToByteString();
        });
        result.streamDefinition.Add("CRDTNotificationStream", (payload, context) => { return ProtocolHelpers.SerializeMessageEnumerator(cRDTNotificationStream(CRDTStreamRequest.Parser.ParseFrom(payload), context)); });

        port.RegisterModule(ServiceName, (port) => UniTask.FromResult(result));
    }
}

public abstract class PingPongService<Context>
{
    public const string ServiceName = "PingPongService";

    public delegate UniTask<PongResponse> Ping(PingRequest request, Context context);

    public static void RegisterService(RpcServerPort<Context> port, Ping ping)
    {
        var result = new ServerModuleDefinition<Context>();

        result.definition.Add("Ping", async (payload, context) =>
        {
            var res = await ping(PingRequest.Parser.ParseFrom(payload), context);
            return res?.ToByteString();
        });

        port.RegisterModule(ServiceName, (port) => UniTask.FromResult(result));
    }
}
