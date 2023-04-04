import * as proto from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { MAXIMUM_NETWORK_MSG_LENGTH } from 'config'
import future from 'fp-future'
import { DataPacket_Kind, DisconnectReason, Participant, RemoteParticipant, Room, RoomEvent } from 'livekit-client'
import mitt from 'mitt'
import { trackEvent } from 'shared/analytics/trackEvent'
import type { ILogger } from 'lib/logger'
import defaultLogger from 'lib/logger'
import { incrementCommsMessageSent } from 'shared/session/getPerformanceInfo'
import type { VoiceHandler } from 'shared/voiceChat/VoiceHandler'
import { commsLogger } from '../logger'
import type { CommsAdapterEvents, MinimumCommunicationsAdapter, SendHints } from './types'
import { createLiveKitVoiceHandler } from './voice/liveKitVoiceHandler'

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
      .on(RoomEvent.ParticipantConnected, (_: RemoteParticipant) => {
        this.config.logger.log('remote participant joined', _.identity)
      })
      .on(RoomEvent.ParticipantDisconnected, (_: RemoteParticipant) => {
        this.events.emit('PEER_DISCONNECTED', {
          address: _.identity
        })
        this.config.logger.log('remote participant left', _.identity)
      })
      .on(RoomEvent.Disconnected, (reason: DisconnectReason | undefined) => {
        this.config.logger.log('disconnected from room', reason, {
          liveKitParticipantSid: this.room.localParticipant.sid,
          liveKitRoomSid: this.room.sid
        })
        if (!this.disconnected) {
          trackEvent('disconnection_cause', {
            context: 'livekit-adapter',
            message: `Got RoomEvent.Disconnected. Reason: ${reason}`,
            liveKitParticipantSid: this.room.localParticipant.sid,
            liveKitRoomSid: this.room.sid
          })
        }
        const kicked = reason === DisconnectReason.DUPLICATE_IDENTITY
        this.do_disconnect(kicked).catch((err) => {
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
        if (data.length > MAXIMUM_NETWORK_MSG_LENGTH) {
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
      trackEvent('error', {
        context: 'livekit-adapter',
        message: `Error trying to send data. Reason: ${err.message}`,
        stack: err.stack,
        saga_stack: `room session id: ${this.room.sid}, participant id: ${this.room.localParticipant.sid}`
      })
      // this fails in some cases, catch is needed
      this.config.logger.error(err)
    }
  }

  async disconnect() {
    return this.do_disconnect(false)
  }

  async do_disconnect(kicked: boolean) {
    if (this.disconnected) {
      return
    }

    this.connectedFuture.resolve()

    this.disconnected = true
    this.room.disconnect().catch(commsLogger.error)
    this.events.emit('DISCONNECTION', { kicked })
  }

  handleMessage(address: string, data: Uint8Array) {
    this.events.emit('message', {
      address,
      data
    })
  }
}
