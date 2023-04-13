import type {
  ConvertMessageToObjectRequest,
  ConvertMessageToObjectResponse,
  GetUserAccountRequest,
  GetUserAccountResponse,
  RequirePaymentRequest,
  RequirePaymentResponse,
  SendAsyncRequest,
  SendAsyncResponse,
  SignMessageRequest,
  SignMessageResponse
} from 'shared/protocol/decentraland/kernel/apis/ethereum_controller.gen'
import { EthereumControllerServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/ethereum_controller.gen'
import { PermissionItem } from 'shared/protocol/decentraland/kernel/apis/permissions.gen'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { RpcServerPort } from '@dcl/rpc/dist/types'
import * as EthService from 'lib/web3/EthereumService'
import * as EthProvider from 'lib/web3/provider'
import type { RPCSendableMessage } from 'shared/types'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import type { PortContext } from './context'
import { assertHasPermission } from './Permissions'

async function requirePayment(req: RequirePaymentRequest, ctx: PortContext): Promise<RequirePaymentResponse> {
  assertHasPermission(PermissionItem.PI_USE_WEB3_API, ctx)

  await getUnityInstance().RequestWeb3ApiUse('requirePayment', {
    ...req,
    sceneId: ctx.sceneData.id,
    sceneNumber: ctx.sceneData.sceneNumber
  })

  const response = EthService.requirePayment(req.toAddress, req.amount, req.currency)
  return {
    jsonAnyResponse: JSON.stringify(response)
  }
}

async function signMessage(req: SignMessageRequest, ctx: PortContext): Promise<SignMessageResponse> {
  assertHasPermission(PermissionItem.PI_USE_WEB3_API, ctx)

  await getUnityInstance().RequestWeb3ApiUse('signMessage', {
    message: await EthService.messageToString(req.message),
    sceneId: ctx.sceneData.id,
    sceneNumber: ctx.sceneData.sceneNumber
  })
  const response = await EthService.signMessage(req.message)
  return response
}

async function convertMessageToObject(
  req: ConvertMessageToObjectRequest,
  ctx: PortContext
): Promise<ConvertMessageToObjectResponse> {
  assertHasPermission(PermissionItem.PI_USE_WEB3_API, ctx)
  return { dict: await EthService.convertMessageToObject(req.message) }
}

async function sendAsync(req: SendAsyncRequest, ctx: PortContext): Promise<SendAsyncResponse> {
  const message: RPCSendableMessage = {
    jsonrpc: '2.0',
    id: req.id,
    method: req.method,
    params: JSON.parse(req.jsonParams) as any[]
  }

  assertHasPermission(PermissionItem.PI_USE_WEB3_API, ctx)
  if (EthService.rpcRequireSign(message)) {
    await getUnityInstance().RequestWeb3ApiUse('sendAsync', {
      message: `${message.method}(${message.params.join(',')})`,
      sceneId: ctx.sceneData.id,
      sceneNumber: ctx.sceneData.sceneNumber
    })
  }

  const response = await EthService.sendAsync(message)
  return {
    jsonAnyResponse: JSON.stringify(response)
  }
}

async function getUserAccount(_req: GetUserAccountRequest, ctx: PortContext): Promise<GetUserAccountResponse> {
  assertHasPermission(PermissionItem.PI_USE_WEB3_API, ctx)
  return { address: await EthProvider.getUserAccount(EthProvider.requestManager) }
}

export function registerEthereumControllerServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, EthereumControllerServiceDefinition, async () => ({
    requirePayment,
    signMessage,
    convertMessageToObject,
    sendAsync,
    getUserAccount
  }))
}
