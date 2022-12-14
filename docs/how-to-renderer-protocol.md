# Renderer Protocol

## What?

Renderer Protocol is the messages that are sent between the [Kernel](http://github.com/decentraland/kernel) and the [Renderer](http://github.com/decentraland/unity-renderer).

Those messages are defined in the [Decentraland Protocol](https://github.com/decentraland/protocol/tree/main/renderer-protocol) in proto3 format.

## Types of messages

The messages exchange are defined by RPC calls. There are defined as Services. Those Services are bidirectional.
You can have `RendererServices` where `Kernel` calls `Renderer`. Or `KernelService` where `Renderer` calls `Kernel`
## How to add a message

To add a message in the Renderer Protocol, you must change the [Decentraland Protocol](https://github.com/decentraland/protocol/tree/main/renderer-protocol), and then update the package in the `protocol-gen` of the `unity-renderer` repository [here](https://github.com/decentraland/unity-renderer/tree/dev/protocol-gen).

Example of `RendererService`: https://github.com/decentraland/protocol/blob/9fcad98380eb95544e50490cc1213b55e0df1f17/proto/decentraland/renderer/renderer_services/emotes_renderer.proto

Example of `KernelService`: https://github.com/decentraland/protocol/blob/9fcad98380eb95544e50490cc1213b55e0df1f17/proto/decentraland/renderer/kernel_services/analytics.proto

After adding a `KernelService` or `RendererService`
You must install the package in the `protocol-gen` just run `npm run build` and the Renderer Protocol will be re-generated.

## RPC

The Renderer works as a `RPC Server` that is connected by the Kernel, the `RPC Client`.
And the Renderer implements a service called `TransportService` which is used to create a RPC Transport which is used as a InverseRPC where we can use the Kernel as a `RPC Server` and the Renderer as `RPC Client`.

So the services can be implemented both ways. We have Kernel services an Renderer service for the Renderer protocol.

> **_NOTE:_**  You can read the following articles to understand RPC [article 1](https://www.techtarget.com/searchapparchitecture/definition/Remote-Procedure-Call-RPC); [article 2](https://grpc.io/docs/what-is-grpc/introduction/)

### Implement Renderer Service

In the example, we're going to implement the following service from the protobuf above:
```
// Service implemented in Renderer and used in Kernel
service EmotesRendererService {
  // Triggers an expression in our own avatar (use example: SDK triggers a expression)
  rpc TriggerSelfUserExpression(TriggerSelfUserExpressionRequest) returns (EmotesResponse) {}
}
```

After we generated the code, need to create a folder named `EmotesService` in the following path:
`Assets\Scripts\MainScripts\DCL\WorldRuntime\KernelCommunication\RPC\Services\EmotesService`

Inside it, we create the following files:
```
RPC.Service.Emotes.asmdef
EmotesServiceImpl.cs
```

And the `EmotesRendererServiceImpl.cs` we add the following code:

```csharp
using System.Threading;
using Cysharp.Threading.Tasks;
using rpc_csharp;

namespace RPC.Services
{
    public class EmotesRendererServiceImpl : IEmotesRendererService<RPCContext>
    {
        public static void RegisterService(RpcServerPort<RPCContext> port)
        {
            EmotesRendererServiceCodeGen.RegisterService(port, new EmotesRendererServiceImpl());
        }

        public UniTask<EmotesResponse> TriggerSelfUserExpression(TriggerSelfUserExpressionRequest request, RPCContext context, CancellationToken ct)
        {
            DataStore.i.myInventedDataStoreWithAGoodName.avatarExpression new AvatarExpression()
            {
                Id = request.Id,
                Command = UserProfile.EmoteSource.Command,
                Timestamp = UTC.Now
            }
            return default;
        }
    }
}
```

To execute this code in the Kernel, we need to go to the Kernel, create the following file:
`packages/renderer-protocol/services/emotesRendererService.ts`
with this code:
```ts
import { RpcClientPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import { EmotesRendererServiceDefinition } from '@dcl/protocol/out-ts/decentraland/renderer/emotes.gen'

export function registerEmotesRendererService<Context>(
  clientPort: RpcClientPort
): codegen.RpcClientModule<EmotesRendererServiceDefinition, Context> {
  return codegen.loadService<Context, EmotesRendererServiceDefinition>(clientPort, EmotesRendererServiceDefinition)
}
```

in `packages/renderer-protocol/rpcClient.ts`

we add the following lines:
```ts
  ...
  const emotesService = registerEmotesService(clientPort) // add this line

  rendererProtocol.resolve({
    ...,
    emotesService // and this line
  })
```

and finally in `packages/renderer-protocol/types.ts`
we add the service in the RendererProtocol type:
```ts
export type RendererProtocol = {
  ...
  emotesService: codegen.RpcClientModule<EmotesRendererServiceDefinition, any> // here
}
```

and to use it, we call it as:
```ts
void rendererProtocol.then(async (protocol) => {
  await protocol.emotesService.triggerSelfUserExpression({ id: req.predefinedEmote })
})
```
> Note: When you're migrating messages, remember that the Kernel must send the message with the renderer protocol and must maintain for a good period of time the old way (with JSONs) to avoid compatibility issues.

### Implement Kernel Services

#### Example with PR

protocol: https://github.com/decentraland/protocol/pull/81
unity-renderer: https://github.com/decentraland/unity-renderer/pull/3605
kernel: https://github.com/decentraland/kernel/pull/728

#### Step by step
Create the service:

in: `packages/renderer-protocol/inverseRpc/services/emotesService.ts`
```ts
import { RpcServerPort } from '@dcl/rpc'
import { RendererProtocolContext } from '../context'
import * as codegen from '@dcl/rpc/dist/codegen'
import { EmotesKernelServiceDefinition } from '@dcl/protocol/out-ts/decentraland/renderer/kernel_services/emotes_kernel.gen'
import { allScenesEvent } from '../../../shared/world/parcelSceneManager'
import { sendPublicChatMessage } from '../../../shared/comms'

export function registerEmotesKernelService(port: RpcServerPort<RendererProtocolContext>) {
  codegen.registerService(port, EmotesKernelServiceDefinition, async () => ({
    async triggerExpression(req, _) {
      allScenesEvent({
        eventType: 'playerExpression',
        payload: {
          expressionId: req.id
        }
      })

      const body = `␐${req.id} ${req.timestamp}`

      sendPublicChatMessage(body)
      return {}
    }
  }))
}
```

Add the service to the registering list:

in: `packages/renderer-protocol/inverseRpc/rpcServer.ts`
```ts
async function registerKernelServices(serverPort: RpcServerPort<RendererProtocolContext>) {
  ...
  registerEmotesKernelService(serverPort)
}
```

And done! We implemented the Kernel Service.

To use it from the Renderer we add the Client Service to the `IRPC`:

in `Assets/Scripts/MainScripts/DCL/WorldRuntime/KernelCommunication/RPC/Interfaces/IRPC.cs`
```csharp
public interface IRPC : IService
{
    ...

    public ClientEmotesKernelService Emotes();
}
```

And we load the module:

in: `Assets/Scripts/MainScripts/DCL/WorldRuntime/KernelCommunication/RPC/RPC.cs`
```csharp
private ClientEmotesKernelService emotes;

public ClientEmotesKernelService Emotes() =>
    emotes;

private async UniTaskVoid LoadRpcModulesAsync(RpcClientPort port)
{
    emotes = new ClientEmotesKernelService(await port.LoadModule(EmotesKernelServiceCodeGen.ServiceName));
    ...
}
```

And finally we can use it with the following code:
```csharp
ClientEmotesKernelService emotes = DCL.Environment.i.serviceLocator.Get<IRPC>().emotes;
emotes?.TriggerExpression(new TriggerExpressionRequest()
{
    Id = id,
    Timestamp = timestamp
});
```
