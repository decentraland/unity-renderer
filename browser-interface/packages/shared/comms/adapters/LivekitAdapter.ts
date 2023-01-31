import * as proto from '@dcl/protocol/out-ts/decentraland/kernel/comms/rfc4/comms.gen'
import { Room, RoomEvent, RemoteParticipant, Participant, DataPacket_Kind, DisconnectReason } from 'livekit-client'
import mitt from 'mitt'
import defaultLogger, { ILogger } from 'shared/logger'
import { incrementCommsMessageSent } from 'shared/session/getPerformanceInfo'
import { VoiceHandler } from 'shared/voiceChat/VoiceHandler'
import { commsLogger } from '../context'
import { createLiveKitVoiceHandler } from './voice/liveKitVoiceHandler'
import { CommsAdapterEvents, MinimumCommunicationsAdapter, SendHints } from './types'
import future from 'fp-future'
import { trackEvent } from 'shared/analytics'

export type LivekitConfig = {
  url: string
  token: string
  logger: ILogger
}

export class LivekitAdapter implements MinimumCommunicationsAdapter {
  public readonly events = mitt<CommsAdapterEvents>()

  private disconnected = false
  private room: Room
  private connectedFuture = future<void>()

  private voiceChatHandlerCache?: Promise<VoiceHandler>

  constructor(private config: LivekitConfig) {
    this.room = new Room()

    this.room
      .on(RoomEvent.ParticipantDisconnected, (_: RemoteParticipant) => {
        this.events.emit('PEER_DISCONNECTED', {
          address: _.identity
        })
        this.config.logger.log('remote participant left')
      })
      .on(RoomEvent.Disconnected, (_reason: DisconnectReason | undefined) => {
        this.config.logger.log('disconnected from room')
        this.disconnect().catch((err) => {
          this.config.logger.error(`error during disconnection ${err.toString()}`)
        })
      })
      .on(RoomEvent.DataReceived, (payload: Uint8Array, participant?: Participant, _?: DataPacket_Kind) => {
        if (participant) {
          this.handleMessage(participant.identity, payload)
        }
      })
  }

  async getVoiceHandler(): Promise<VoiceHandler> {
    if (this.voiceChatHandlerCache) {
      await (await this.voiceChatHandlerCache).destroy()
    }
    return (this.voiceChatHandlerCache = createLiveKitVoiceHandler(this.room))
  }

  async connect(): Promise<void> {
    await this.room.connect(this.config.url, this.config.token, { autoSubscribe: true })
    await this.room.engine.waitForPCConnected()
    this.config.logger.log(`Connected to livekit room ${this.room.name}`)
    this.connectedFuture.resolve()
  }

  async send(data: Uint8Array, { reliable }: SendHints): Promise<void> {
    incrementCommsMessageSent(data.length)
    try {
      await this.connectedFuture
      if (!this.disconnected) {
        if (data.length > 65000) {
          const message = proto.Packet.decode(data)
          defaultLogger.error('Skipping big message over comms', message)
          trackEvent('invalid_comms_message_too_big', { message: JSON.stringify(message) })
        } else {
          await this.room.localParticipant.publishData(
            data,
            reliable ? DataPacket_Kind.RELIABLE : DataPacket_Kind.LOSSY
          )
        }
      }
    } catch (err: any) {
      // this fails in some cases, catch is needed
      this.config.logger.error(err)
    }
  }

  async disconnect() {
    if (this.disconnected) {
      return
    }

    this.connectedFuture.resolve()

    this.disconnected = true
    this.room.disconnect().catch(commsLogger.error)
    this.events.emit('DISCONNECTION', { kicked: false })
  }

  handleMessage(address: string, data: Uint8Array) {
    this.events.emit('message', {
      address,
      data
    })
  }
}
