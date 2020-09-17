export type CommsState = {
  initialized: boolean
  voiceChatRecording: boolean
}

export type RootCommsState = {
  comms: CommsState
}
