import { createRpcClient, RpcClient, RpcClientPort, Transport } from '@dcl/rpc'

export async function createRendererRpcClient(
  transport: Transport
): Promise<{ rpcClient: RpcClient; rendererInterfacePort: RpcClientPort }> {
  const rpcClient = await createRpcClient(transport)
  const rendererInterfacePort = await rpcClient.createPort('renderer-protocol')

  return { rendererInterfacePort, rpcClient }
}
