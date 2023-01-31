import { ILogger, createLogger } from 'shared/logger'
import { Authenticator } from '@dcl/crypto'
import { createRpcClient, RpcClientPort } from '@dcl/rpc'
import { WebSocketTransport } from '@dcl/rpc/dist/transports/WebSocket'
import { loadService } from '@dcl/rpc/dist/codegen'
import {
  BffAuthenticationServiceDefinition,
  WelcomePeerInformation
} from '@dcl/protocol/out-ts/decentraland/bff/authentication_service.gen'
import { CommsServiceDefinition } from '@dcl/protocol/out-ts/decentraland/bff/comms_service.gen'
import { trackEvent } from 'shared/analytics'
import { ExplorerIdentity } from 'shared/session/types'
import { RealmConnectionEvents, BffServices, IRealmAdapter } from '../types'
import mitt from 'mitt'
import { legacyServices } from '../local-services/legacy'
import { AboutResponse } from '@dcl/protocol/out-ts/decentraland/bff/http_endpoints.gen'

export type TopicData = {
  peerId: string
  data: Uint8Array
}

export type TopicListener = {
  subscriptionId: number
}

async function authenticatePort(port: RpcClientPort, identity: ExplorerIdentity): Promise<string> {
  const address = identity.address

  const auth = loadService(port, BffAuthenticationServiceDefinition)

  const getChallengeResponse = await auth.getChallenge({ address })
  if (getChallengeResponse.alreadyConnected) {
    trackEvent('bff_auth_already_connected', {
      address
    })
  }

  const authChainJson = JSON.stringify(Authenticator.signPayload(identity, getChallengeResponse.challengeToSign))
  const authResponse: WelcomePeerInformation = await auth.authenticate({ authChainJson })
  return authResponse.peerId
}

function resolveBffUrl(baseUrl: string, bffPublicUrl?: string): string {
  let url: string
  if (bffPublicUrl && bffPublicUrl.startsWith('http')) {
    url = bffPublicUrl
    if (!url.endsWith('/')) {
      url += '/'
    }
    url += 'rpc'
  } else {
    const relativeUrl = ((bffPublicUrl || '/bff') + '/rpc').replace(/(\/+)/g, '/')

    url = new URL(relativeUrl, baseUrl).toString()
  }

  return url.replace(/^http/, 'ws')
}

export async function createBffRpcConnection(
  baseUrl: string,
  about: AboutResponse,
  identity: ExplorerIdentity
): Promise<IRealmAdapter> {
  const wsUrl = resolveBffUrl(baseUrl, about.bff?.publicUrl)
  const bffTransport = WebSocketTransport(new WebSocket(wsUrl, 'bff'))

  const rpcClient = await createRpcClient(bffTransport)
  const port = await rpcClient.createPort('kernel')

  const peerId = await authenticatePort(port, identity)

  // close the WS when the port is closed
  port.on('close', () => bffTransport.close())

  return new BffRpcConnection(baseUrl, about, port, peerId)
}

export class BffRpcConnection implements IRealmAdapter<any> {
  public events = mitt<RealmConnectionEvents>()
  public services: BffServices

  private logger: ILogger = createLogger('BFF: ')
  private disposed = false

  constructor(
    public baseUrl: string,
    public readonly about: AboutResponse,
    public port: RpcClientPort,
    public peerId: string
  ) {
    port.on('close', async () => {
      this.logger.log('BFF transport closed')
      this.disconnect().catch(this.logger.error)
    })

    this.services = {
      comms: loadService(port, CommsServiceDefinition),
      legacy: legacyServices(baseUrl, about)
    }
  }

  async disconnect(error?: Error) {
    if (this.disposed) {
      return
    }

    this.disposed = true

    if (this.port) {
      this.port.close()
    }

    this.events.emit('DISCONNECTION', { error })
  }
}
