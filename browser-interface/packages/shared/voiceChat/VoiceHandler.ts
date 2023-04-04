import * as rfc4 from 'shared/protocol/decentraland/kernel/comms/rfc4/comms.gen'

export type VoiceHandler = {
  // UI Methods
  // setTalking is called from the UI or keyboard to broadcast audio
  setRecording(recording: boolean): void

  // used to know if a user is talking or not, for the UI
  onUserTalking(cb: (userId: string, talking: boolean) => void): void

  onError(cb: (message: string) => void): void

  onRecording(cb: (recording: boolean) => void): void

  // Controls Methods
  reportPosition(recording: rfc4.Position): void

  setVolume(volume: number): void

  setMute(mute: boolean): void

  setInputStream(stream: MediaStream): Promise<void>

  hasInput(): boolean

  // Play audio when we recive it from comms (only for opus)
  playEncodedAudio?(src: string, relativePosition: rfc4.Position, encoded: rfc4.Voice): Promise<void>

  destroy(): Promise<void>
}
