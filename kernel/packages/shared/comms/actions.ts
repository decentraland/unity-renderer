import { action } from 'typesafe-actions'

export const VOICE_PLAYING_UPDATE = 'Voice Playing Update'
export const voicePlayingUpdate = (userId: string, playing: boolean) =>
  action(VOICE_PLAYING_UPDATE, { userId, playing })
export type VoicePlayingUpdate = ReturnType<typeof voicePlayingUpdate>

/**
 * Action to trigger voice chat recording
 */
export const SET_VOICE_CHAT_RECORDING = 'Set Voice Chat Recording'
export const setVoiceChatRecording = (recording: boolean) => action(SET_VOICE_CHAT_RECORDING, { recording })
export type SetVoiceChatRecording = ReturnType<typeof setVoiceChatRecording>

/**
 * Action triggered when recording starts or stops
 */
export const VOICE_RECORDING_UPDATE = 'Voice Recording Update'
export const voiceRecordingUpdate = (recording: boolean) => action(VOICE_RECORDING_UPDATE, { recording })
export type VoiceRecordingUpdate = ReturnType<typeof voiceRecordingUpdate>
