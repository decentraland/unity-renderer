# Renderer Protocol

## What?

The Renderer Protocol consists of messages that are sent between the [Browser Interface](http://github.com/decentraland/unity-renderer/tree/dev/browser-interface) and the [Renderer](http://github.com/decentraland/unity-renderer/tree/dev/unity-renderer). These messages are defined in the [Renderer Protocol] folder (http://github.com/decentraland/unity-renderer/tree/dev/renderer-protocol) using the proto3 format.

## Types of messages

The message exchange is defined by RPC calls, which are bi-directional Services. For example, the `Renderer` can call the `Browser Interface` using a `KernelService`, or the `Browser Interface` can call the `Renderer` using a `RendererService`.

## How to add a message

To add a message to the Renderer Protocol, you must first add it to the [protocol definition](https://github.com/decentraland/unity-renderer/tree/dev/renderer-protocol)

[Example](https://github.com/decentraland/protocol/blob/9fcad98380eb95544e50490cc1213b55e0df1f17/proto/decentraland/renderer/renderer_services/emotes_renderer.proto) of `RendererService`.

[Example](https://github.com/decentraland/protocol/blob/9fcad98380eb95544e50490cc1213b55e0df1f17/proto/decentraland/renderer/kernel_services/analytics.proto) of `KernelService`.

After adding a `KernelService` or `RendererService`, you must run `npm run build` to regenerate the Renderer Protocol.

## RPC

The Renderer acts as an `RPC Server`, while the Browser Interface is the `RPC Client`. The Renderer implements a service called TransportService which allows it to create an RPC Transport that functions as an InverseRPC. This allows the Browser Interface to act as an`RPC Server` and the Renderer to act as an `RPC Client`. As a result, services can be implemented in either direction. There are both Browser Interface services and Renderer services for the Renderer protocol.

> **_NOTE:_**  You can read the following articles to understand RPC [article 1](https://www.techtarget.com/searchapparchitecture/definition/Remote-Procedure-Call-RPC); [article 2](https://grpc.io/docs/what-is-grpc/introduction/)

## Implement Renderer Service
### **Renderer Side:**

In the next example, we will implement the service described in the protobuf below:
```protobuf
// Service implemented in Renderer and used in Browser Interface
service EmotesRendererService {
  // Triggers an expression in our own avatar (use example: SDK triggers a expression)
  rpc TriggerSelfUserExpression(TriggerSelfUserExpressionRequest) returns (EmotesResponse) {}
}
```

Once we've generated the code, we need first to create a folder named `EmotesService` in the path `Assets\Scripts\MainScripts\DCL\WorldRuntime\KernelCommunication\RPC\Services` and then create the following files:
```
RPC.Service.Emotes.asmdef
EmotesRendererServiceImpl.cs
```

In the `EmotesRendererServiceImpl.cs` file, we need to add the code:

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

### **Browser Interface Side:**
To run this code in the Browser Interface, we need first create the file `packages/renderer-protocol/services/emotesRendererService.ts` and then paste this code:

```ts
import { RpcClientPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import { EmotesRendererServiceDefinition } from 'shared/protocol/decentraland/renderer/renderer_services/emotes_renderer.gen'
import defaultLogger from 'shared/logger'

export function registerEmotesService<Context>(
  clientPort: RpcClientPort
): codegen.RpcClientModule<EmotesRendererServiceDefinition, Context> | undefined {
  try {
    return codegen.loadService<Context, EmotesRendererServiceDefinition>(clientPort, EmotesRendererServiceDefinition)
  } catch (e) {
    defaultLogger.error('EmotesService could not be loaded')
    return undefined
  }
}
```

Then, in `packages/shared/renderer/sagas.ts` we need to add the following line:

```ts
  ...
  const emotesService = registerEmotesService(clientPort) // add this line here.
  ...
```

Finally, in `packages/shared/renderer/types.ts` we need to add the service in the RendererModules type:
```ts
export type RendererModules = {
  ...
  emotesService: codegen.RpcClientModule<EmotesRendererServiceDefinition, any> | undefined // add this line here.
}
```

To use it, we call it as:
```ts
getRendererModules(store.getState())
  ?.emotesService?.triggerSelfUserExpression({ id: req.predefinedEmote })
  .catch(defaultLogger.error)
```
> Note of caution: When you're migrating messages, remember that the Kernel must send the message with the renderer protocol and must keep the old way (with JSON based mechanism) for a good period of time to avoid compatibility issues.

## Implement Kernel Services

#### Example with PR

- protocol: https://github.com/decentraland/protocol/pull/81
- unity-renderer: https://github.com/decentraland/unity-renderer/pull/3605

### **Browser Interface Side:**
#### Step by step
Create the service:

In: `packages/renderer-protocol/inverseRpc/services/emotesService.ts`
```ts
import { RpcServerPort } from '@dcl/rpc'
import { RendererProtocolContext } from '../context'
import * as codegen from '@dcl/rpc/dist/codegen'
import { EmotesKernelServiceDefinition } from 'shared/protocol/decentraland/renderer/kernel_services/emotes_kernel.gen'
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

Add the service to the registering list in: `packages/renderer-protocol/inverseRpc/rpcServer.ts`
```ts
async function registerKernelServices(serverPort: RpcServerPort<RendererProtocolContext>) {
  ...
  registerEmotesKernelService(serverPort)
}
```

And done! We've implemented the Kernel Service.

### **Renderer Side:**
To use it from the Renderer we need to add the Client Service to the `IRPC` in: `Assets/Scripts/MainScripts/DCL/WorldRuntime/KernelCommunication/RPC/Interfaces/IRPC.cs`
```csharp
public interface IRPC : IService
{
    ...

    public ClientEmotesKernelService Emotes();
}
```

Then we load the module in: `Assets/Scripts/MainScripts/DCL/WorldRuntime/KernelCommunication/RPC/RPC.cs`
```csharp
private ClientEmotesKernelService emotes;

public ClientEmotesKernelService Emotes() => emotes;

private async UniTaskVoid LoadRpcModulesAsync(RpcClientPort port)
{
    emotes = await SafeLoadModule(EmotesKernelServiceCodeGen.ServiceName, port,
                module => new ClientEmotesKernelService(module));
    ...
}
```

Finally, we can use it with the following code:
```csharp
ClientEmotesKernelService emotes = DCL.Environment.i.serviceLocator.Get<IRPC>().emotes;
emotes?.TriggerExpression(new TriggerExpressionRequest()
{
    Id = id,
    Timestamp = timestamp
});
```
