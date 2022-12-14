
// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: decentraland.bff
// file: decentraland/bff/comms_director_service.proto
using Cysharp.Threading.Tasks;
using rpc_csharp;
using Google.Protobuf.WellKnownTypes;

namespace Decentraland.Bff {
public class ClientCommsDirectorService
{
  private readonly RpcClientModule module;

  public ClientCommsDirectorService(RpcClientModule module)
  {
      this.module = module;
  }

  public UniTask<Empty> SendHeartbeat(Heartbeat request)
  {
      return module.CallUnaryProcedure<Empty>("SendHeartbeat", request);
  }

  public IUniTaskAsyncEnumerable<WorldCommand> GetCommsCommands(Empty request)
  {
      return module.CallServerStream<WorldCommand>("GetCommsCommands", request);
  }
}
}
