import { future, IFuture } from 'fp-future'

import * as rfc5 from 'shared/protocol/decentraland/kernel/comms/rfc5/ws_comms.gen'
import { Writer } from 'protobufjs/minimal'
import { ILogger, createLogger } from 'lib/logger'
import { ExplorerIdentity } from 'shared/session/types'
import { Authenticator } from '@dcl/crypto'
import mitt from 'mitt'
import { CommsAdapterEvents, MinimumCommunicationsAdapter, SendHints } from './types'
import { createOpusVoiceHandler } from './voice/opusVoiceHandler'
import { VoiceHandler } from 'shared/voiceChat/VoiceHandler'
import { notifyStatusThroughChat } from 'shared/chat'
import { wsAsAsyncChannel } from '../logic/ws-async-channel'

// shared writer to leverage pools
const writer = new Writer()

function craftMessage(packet: rfc5.WsPacket): Uint8Array {
  writer.reset()
  rfc5.WsPacket.encode(packet as any, writer)
  return writer.finish()
}

export class WebSocketAdapter implements MinimumCommunicationsAdapter {
  public alias: number | null = null
  public events = mitt<CommsAdapterEvents>()

  public logger: ILogger = createLogger('WSComms: ')

  private connected = future<void>()
  private peersToAddress = new Map<number, string>()

  get connectedPromise(): IFuture<void> {
    return this.connected
  }

  private ws: WebSocket | null = null

  constructor(public url: string, private identity: ExplorerIdentity) {}

  async connect(): Promise<void> {
    if (this.ws) throw new Error('Cannot call connect twice per IBrokerTransport')

    const ws = new WebSocket(this.url, ['rfc5', 'rfc4'])
    const connected = future<void>()
    ws.binaryType = 'arraybuffer'
    ws.onopen = () => connected.resolve()

    ws.onerror = (event) => {
      this.logger.error('socket error', event)
      this.disconnect().catch(this.logger.error)
      connected.reject(event as any)
    }

    ws.onclose = () => {
      this.disconnect().catch(this.logger.error)
      connected.reject(new Error('Socket closed'))
    }

    const channel = wsAsAsyncChannel<rfc5.WsPacket>(ws, rfc5.WsPacket.decode)
    try {
      await connected

      {
        // phase 0, identify ourselves
        const identificationMessage = craftMessage({
          message: {
            $case: 'peerIdentification',
            peerIdentification: {
              address: this.identity.address
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
          case 'welcomeMessage': {
            return this.handleWelcomeMessage(message.welcomeMessage, ws)
          }
          case 'challengeMessage': {
            const authChainJson = JSON.stringify(
              Authenticator.signPayload(this.identity, message.challengeMessage.challengeToSign)
            )
            ws.send(
              craftMessage({
                message: {
                  $case: 'signedChallengeForServer',
                  signedChallengeForServer: { authChainJson }
                }
              })
            )
            break
          }
          case 'peerKicked': {
            const { peerKicked } = message
            notifyStatusThroughChat(peerKicked.reason)
            await this.disconnect(Error(message.peerKicked.reason))
          }
          default: {
            // only welcomeMessage and challengeMessage are valid options for this phase of the protocol
            throw new Error('Protocol error: server did not provide a challenge')
          }
        }
      }

      {
        // phase 2, we are in
        const { message } = await channel.yield(1000, 'Error waiting for welcome message')
        if (!message || message.$case !== 'welcomeMessage')
          throw new Error('Protocol error: server did not send a welcomeMessage')

        return this.handleWelcomeMessage(message.welcomeMessage, ws)
      }
    } catch (err: any) {
      this.connected.reject(err)
      // close the WebSocket on error
      if (ws.readyState === ws.OPEN) ws.close()
      // and bubble up the error
      throw err
    } finally {
      channel.close()
    }
  }

  async createVoiceHandler(): Promise<VoiceHandler> {
    return createOpusVoiceHandler()
  }

  handleWelcomeMessage(welcomeMessage: rfc5.WsWelcome, socket: WebSocket) {
    this.alias = welcomeMessage.alias
    for (const [alias, address] of Object.entries(welcomeMessage.peerIdentities)) {
      this.peersToAddress.set(+alias | 0, address)
    }
    this.ws = socket
    this.connected.resolve()
    socket.addEventListener('message', this.onWsMessage.bind(this))
  }

  send(body: Uint8Array, hints: SendHints) {
    this.internalSend(
      craftMessage({
        message: {
          $case: 'peerUpdateMessage',
          peerUpdateMessage: {
            body,
            fromAlias: this.alias || 0,
            unreliable: !hints.reliable
          }
        }
      })
    )
  }

  async disconnect(error?: Error) {
    this.internalDisconnect(false, error)
  }

  internalDisconnect(kicked: boolean, _error?: Error) {
    if (this.ws) {
      const ws = this.ws
      this.ws = null
      this.events.emit('DISCONNECTION', { kicked })

      ws.onmessage = null
      ws.onerror = null
      ws.onclose = null
      ws.close()
    }
  }

  private async onWsMessage(event: MessageEvent) {
    const data = event.data
    const msg = new Uint8Array(data)
    const { message } = rfc5.WsPacket.decode(msg)

    if (!message) return

    switch (message.$case) {
      case 'peerJoinMessage': {
        const { peerJoinMessage } = message
        this.peersToAddress.set(peerJoinMessage.alias, peerJoinMessage.address)
        break
      }
      case 'peerKicked': {
        this.internalDisconnect(true)
        break
      }
      case 'peerLeaveMessage': {
        const { peerLeaveMessage } = message
        const currentPeerAddress = this.peersToAddress.get(peerLeaveMessage.alias)
        if (currentPeerAddress) {
          this.peersToAddress.delete(peerLeaveMessage.alias)
          this.events.emit('PEER_DISCONNECTED', { address: currentPeerAddress })
        }
        break
      }
      case 'peerUpdateMessage': {
        const { peerUpdateMessage } = message
        const currentPeerAddress = this.peersToAddress.get(peerUpdateMessage.fromAlias)
        if (currentPeerAddress) {
          this.events.emit('message', {
            address: currentPeerAddress,
            data: peerUpdateMessage.body
          })
        } else {
          debugger
        }
        break
      }
    }
  }

  private internalSend(msg: Uint8Array) {
    if (!this.ws) {
      console.error(new Error('This transport is closed'))
      return
    }

    this.connected
      .then(() => {
        if (this.ws && this.ws.readyState === WebSocket.OPEN) this.ws.send(msg)
      })
      .catch(console.error)
  }
}
