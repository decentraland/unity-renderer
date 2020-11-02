export type CommsState = {
  initialized: boolean
  voiceChatRecording: boolean
  voicePolicy: VoicePolicy
}

export type RootCommsState = {
  comms: CommsState
}

export enum VoicePolicy {
  ALLOW_ALL,
  ALLOW_VERIFIED_ONLY,
  ALLOW_FRIENDS_ONLY
}
