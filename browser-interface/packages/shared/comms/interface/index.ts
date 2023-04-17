import { Package } from './types'
import * as proto from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'
import { Emitter } from 'mitt'
import { CommsAdapterEvents } from '../adapters/types'
import { VoiceHandler } from 'shared/voiceChat/VoiceHandler'

export type CommsEvents = CommsAdapterEvents & {
  // RFC4 messages
  sceneMessageBus: Package<proto.Scene>
  chatMessage: Package<proto.Chat>
  profileMessage: Package<proto.AnnounceProfileVersion>
  position: Package<proto.Position>
  voiceMessage: Package<proto.Voice>
  profileResponse: Package<proto.ProfileResponse>
  profileRequest: Package<proto.ProfileRequest>
}

export interface RoomConnection {
  // this operation is non-reversible
  disconnect(): Promise<void>
  // @once
  connect(): Promise<void>

  events: Emitter<CommsEvents>

  sendProfileMessage(profile: proto.AnnounceProfileVersion): Promise<void>
  sendProfileRequest(request: proto.ProfileRequest): Promise<void>
  sendProfileResponse(response: proto.ProfileResponse): Promise<void>
  sendPositionMessage(position: Omit<proto.Position, 'index'>): Promise<void>
  sendParcelSceneMessage(message: proto.Scene): Promise<void>
  sendChatMessage(message: proto.Chat): Promise<void>
  sendVoiceMessage(message: proto.Voice): Promise<void>

  createVoiceHandler(): Promise<VoiceHandler>
}
