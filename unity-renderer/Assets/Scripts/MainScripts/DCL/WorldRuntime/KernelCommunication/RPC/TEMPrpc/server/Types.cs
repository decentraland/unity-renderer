using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using rpc_csharp.transport;

namespace rpc_csharp.server
{
    public delegate void RpcServerHandler<TContext>
    (
        RpcServerPort<TContext> serverPort,
        ITransport transport,
        TContext context
    );

    public delegate UniTask<ByteString> UnaryCallback<in TContext>(ByteString payload, TContext context);

    public delegate IEnumerator<ByteString> StreamCallback<in TContext>(ByteString payload,
        TContext context);

    public delegate UniTask<ServerModuleDefinition<TContext>> ModuleGeneratorFunction<TContext>(
        RpcServerPort<TContext> port);

    public class ServerModuleDefinition<TContext>
    {
        public readonly Dictionary<string, UnaryCallback<TContext>> definition =
            new Dictionary<string, UnaryCallback<TContext>>();

        public readonly Dictionary<string, StreamCallback<TContext>> streamDefinition =
            new Dictionary<string, StreamCallback<TContext>>();
    }

    public readonly struct ServerModuleProcedureInfo
    {
        public readonly string procedureName;
        public readonly uint procedureId;

        public ServerModuleProcedureInfo(uint procedureId, string procedureName)
        {
            this.procedureId = procedureId;
            this.procedureName = procedureName;
        }
    }

    public class ServerModuleDeclaration
    {
        public List<ServerModuleProcedureInfo> procedures;
    }
}