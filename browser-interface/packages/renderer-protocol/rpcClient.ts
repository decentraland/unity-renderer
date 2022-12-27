import { createRpcClient, RpcClientPort, Transport } from '@dcl/rpc'

export async function createRendererRpcClient(transport: Transport): Promise<RpcClientPort> {
  const rpcClient = await createRpcClient(transport)
  const clientPort = await rpcClient.createPort('renderer-protocol')

  return clientPort
}
