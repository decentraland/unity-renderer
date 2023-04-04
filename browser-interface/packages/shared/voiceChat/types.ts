import { VoiceHandler } from './VoiceHandler'

export type VoiceChatState = {
  joined: boolean
  recording: boolean
  requestRecording: boolean
  policy: VoicePolicy
  voiceHandler: VoiceHandler | null // TODO: Replace any by VoiceHandler
  error: string | null
  media?: any
  volume: number
  mute: boolean
  outputDeviceId: string | null
  inputDeviceId: string | null
}

export enum VoicePolicy {
  ALLOW_ALL,
  ALLOW_VERIFIED_ONLY,
  ALLOW_FRIENDS_ONLY
}

export type RootVoiceChatState = {
  voiceChat: VoiceChatState
}
