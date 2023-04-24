import { ILogger, createLogger } from 'lib/logger'
import { Authenticator } from '@dcl/crypto'
import { future } from 'fp-future'
import { Writer } from 'protobufjs/minimal'
import { ExplorerIdentity } from 'shared/session/types'
import { RealmConnectionEvents, IRealmAdapter, LegacyServices } from '../types'
import mitt from 'mitt'
import { legacyServices } from '../local-services/legacy'
import { ClientPacket, ServerPacket } from 'shared/protocol/decentraland/kernel/comms/v3/archipelago.gen'
import { wsAsAsyncChannel } from '../../comms/logic/ws-async-channel'
import { Vector3 } from 'lib/math/Vector3'
import { BringDownClientAndShowError } from 'shared/loading/ReportFatalError'
import { AboutResponse } from 'shared/protocol/decentraland/realm/about.gen'

// shared writer to leverage pools
const writer = new Writer()

function craftMessage(packet: ClientPacket): Uint8Array {
  writer.reset()
  ClientPacket.encode(packet as any, writer)
  return writer.finish()
}

export async function createArchipelagoConnection(
  baseUrl: string,
  about: AboutResponse,
  identity: ExplorerIdentity
): Promise<IRealmAdapter> {
  const logger = createLogger('Archipelago handshake: ')
  const address = identity.address
  const url = new URL('/archipelago/ws', baseUrl).toString()
  const wsUrl = url.replace(/^http/, 'ws')

  const connected = future<void>()
  const ws = new WebSocket(wsUrl, 'archipelago')
  ws.binaryType = 'arraybuffer'
  ws.onopen = () => connected.resolve()

  ws.onerror = (event) => {
    logger.error('socket error', event)
    connected.reject(event as any)
  }

  ws.onclose = () => {
    logger.error('socket closed')
    connected.reject(new Error('Socket closed'))
  }

  const channel = wsAsAsyncChannel<ServerPacket>(ws, ServerPacket.decode)
  try {
    await connected

    {
      // phase 0, identify ourselves
      const identificationMessage = craftMessage({
        message: {
          $case: 'challengeRequest',
          challengeRequest: {
            address
          }
        }
      })
      ws.send(identificationMessage)
    }

    {
      // phase 1, respond to challenge
      const { message } = await channel.yield(1000, 'Error waiting for remote challenge')

      if (!message) {
        throw new Error('Protocol error: empty message')
      }

      switch (message.$case) {
        case 'challengeResponse': {
          const authChainJson = JSON.stringify(
            Authenticator.signPayload(identity, message.challengeResponse.challengeToSign)
          )
          ws.send(
            craftMessage({
              message: {
                $case: 'signedChallenge',
                signedChallenge: { authChainJson }
              }
            })
          )
          break
        }
        default: {
          throw new Error(`Protocol error: server did not provide a valid handshake message ${message.$case}`)
        }
      }
    }

    {
      // phase 2, we are in
      const { message } = await channel.yield(1000, 'Error waiting for welcome message')
      if (!message || message.$case !== 'welcome') {
        throw new Error('Protocol error: server did not send a welcomeMessage')
      }

      return new ArchipelagoConnection(baseUrl, about, ws, address)
    }
  } catch (err: any) {
    connected.reject(err)
    if (ws.readyState === ws.OPEN) {
      ws.close()
    }
    throw err
  } finally {
    channel.close()
  }
}

export class ArchipelagoConnection implements IRealmAdapter {
  public events = mitt<RealmConnectionEvents>()
  public services: LegacyServices

  private logger: ILogger = createLogger('Archipelago: ')
  private disposed = false

  constructor(
    public baseUrl: string,
    public readonly about: AboutResponse,
    public ws: WebSocket,
    public peerId: string
  ) {
    ws.onclose = () => {
      this.disconnect().catch(this.logger.error)
    }

    ws.onerror = (event) => {
      this.logger.error('socket error', event)
    }

    ws.addEventListener('message', ({ data }) => {
      const { message } = ServerPacket.decode(new Uint8Array(data))
      if (!message) {
        return
      }

      switch (message.$case) {
        case 'islandChanged': {
          this.events.emit('setIsland', message.islandChanged)
          break
        }
        case 'kicked': {
          const error =
            'Disconnected from realm as the user id is already taken. Please make sure you are not logged into the world through another tab'
          BringDownClientAndShowError(error)
          break
        }
      }
    })

    this.services = legacyServices(baseUrl, about)
  }

  sendHeartbeat(p: Vector3) {
    if (this.ws.readyState === WebSocket.OPEN) {
      this.ws.send(
        craftMessage({
          message: {
            $case: 'heartbeat',
            heartbeat: {
              position: {
                x: p.x,
                y: p.y,
                z: p.z
              }
            }
          }
        })
      )
    }
  }

  async disconnect(error?: Error) {
    if (this.disposed) {
      return
    }
    this.ws.close()
    this.logger.log('Archipelago adapter closed')
    this.disposed = true
    this.events.emit('DISCONNECTION', { error })
  }
}
