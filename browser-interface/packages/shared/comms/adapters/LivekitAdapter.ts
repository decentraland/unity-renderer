import * as proto from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { MAXIMUM_NETWORK_MSG_LENGTH } from 'config'
import {
  ConnectionState,
  DataPacket_Kind,
  DisconnectReason,
  Participant,
  RemoteParticipant,
  Room,
  RoomEvent
} from 'livekit-client'
import mitt from 'mitt'
import { trackEvent } from 'shared/analytics/trackEvent'
import type { ILogger } from 'lib/logger'
import { incrementCommsMessageSent } from 'shared/session/getPerformanceInfo'
import type { VoiceHandler } from 'shared/voiceChat/VoiceHandler'
import { commsLogger } from '../logger'
import type { ActiveVideoStreams, CommsAdapterEvents, MinimumCommunicationsAdapter, SendHints } from './types'
import { createLiveKitVoiceHandler } from './voice/liveKitVoiceHandler'
import { GlobalAudioStream } from './voice/loopback'

export type LivekitConfig = {
  url: string
  token: string
  logger: ILogger
  globalAudioStream: GlobalAudioStream
}

export class LivekitAdapter implements MinimumCommunicationsAdapter {
  public readonly events = mitt<CommsAdapterEvents>()

  private disposed = false
  private readonly room: Room
  private voiceHandler: VoiceHandler

  constructor(private config: LivekitConfig) {
    this.room = new Room()

    this.voiceHandler = createLiveKitVoiceHandler(this.room, this.config.globalAudioStream)

    this.room
      .on(RoomEvent.ParticipantConnected, (_: RemoteParticipant) => {
        this.config.logger.log(this.room.name, 'remote participant joined', _.identity)
      })
      .on(RoomEvent.ParticipantDisconnected, (_: RemoteParticipant) => {
        this.events.emit('PEER_DISCONNECTED', {
          address: _.identity
        })
        this.config.logger.log(this.room.name, 'remote participant left', _.identity)
      })
      .on(RoomEvent.ConnectionStateChanged, (state: ConnectionState) => {
        this.config.logger.log(this.room.name, 'connection state changed', state)
      })
      .on(RoomEvent.Disconnected, (reason: DisconnectReason | undefined) => {
        if (this.disposed) {
          return
        }

        this.config.logger.log(this.room.name, 'disconnected from room', reason, {
          liveKitParticipantSid: this.room.localParticipant.sid,
          liveKitRoomSid: this.room.sid
        })
        trackEvent('disconnection_cause', {
          context: 'livekit-adapter',
          message: `Got RoomEvent.Disconnected. Reason: ${reason}`,
          liveKitParticipantSid: this.room.localParticipant.sid,
          liveKitRoomSid: this.room.sid
        })
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

  async createVoiceHandler(): Promise<VoiceHandler> {
    return this.voiceHandler
  }

  async connect(): Promise<void> {
    await this.room.connect(this.config.url, this.config.token, { autoSubscribe: true })
    await this.room.engine.waitForPCInitialConnection()
    this.config.logger.log(this.room.name, `Connected to livekit room ${this.room.name}`)
  }

  async send(data: Uint8Array, { reliable }: SendHints): Promise<void> {
    if (this.disposed) {
      return
    }

    incrementCommsMessageSent(data.length)
    const state = this.room.state

    if (data.length > MAXIMUM_NETWORK_MSG_LENGTH) {
      const message = proto.Packet.decode(data)
      this.config.logger.error('Skipping big message over comms', message)
      trackEvent('invalid_comms_message_too_big', { message: JSON.stringify(message) })
      return
    }

    if (state !== ConnectionState.Connected) {
      this.config.logger.log(`Skip sending message because connection state is ${state}`)
      return
    }

    try {
      await this.room.localParticipant.publishData(data, reliable ? DataPacket_Kind.RELIABLE : DataPacket_Kind.LOSSY)
    } catch (err: any) {
      // NOTE: for tracking purposes only, this is not a "code" error, this is a failed connection or a problem with the livekit instance
      trackEvent('error', {
        context: 'livekit-adapter',
        message: `Error trying to send data. Reason: ${err.message}`,
        stack: err.stack,
        saga_stack: `room session id: ${this.room.sid}, participant id: ${this.room.localParticipant.sid}, state: ${state}`
      })
      await this.disconnect()
    }
  }

  async disconnect() {
    return this.do_disconnect(false)
  }

  async do_disconnect(kicked: boolean) {
    if (this.disposed) {
      return
    }

    this.disposed = true
    await this.room.disconnect().catch(commsLogger.error)
    this.events.emit('DISCONNECTION', { kicked })
  }

  handleMessage(address: string, data: Uint8Array) {
    this.events.emit('message', {
      address,
      data
    })
  }

  getActiveVideoStreams(): Map<string, ActiveVideoStreams> {
    const result = new Map<string, ActiveVideoStreams>()
    const participants = this.room.participants

    for (const [sid, participant] of participants) {
      if (participant.videoTracks.size > 0) {
        const participantTracks = new Map<string, MediaStream>()
        for (const [videoSid, track] of participant.videoTracks) {
          if (track.videoTrack?.mediaStream) {
            participantTracks.set(videoSid, track.videoTrack.mediaStream)
          }
        }
        result.set(sid, { identity: participant.identity, videoTracks: participantTracks })
      }
    }

    return result
  }
}
