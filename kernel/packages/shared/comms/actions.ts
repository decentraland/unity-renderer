import { action } from 'typesafe-actions'
import { VoicePolicy } from './types'

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
 * Action to toggle voice chat recording
 */
export const TOGGLE_VOICE_CHAT_RECORDING = 'Toggle Voice Chat Recording'
export const toggleVoiceChatRecording = () => action(TOGGLE_VOICE_CHAT_RECORDING)
export type ToggleVoiceChatRecording = ReturnType<typeof toggleVoiceChatRecording>

/**
 * Action triggered when recording starts or stops
 */
export const VOICE_RECORDING_UPDATE = 'Voice Recording Update'
export const voiceRecordingUpdate = (recording: boolean) => action(VOICE_RECORDING_UPDATE, { recording })
export type VoiceRecordingUpdate = ReturnType<typeof voiceRecordingUpdate>

export const SET_VOICE_VOLUME = 'Set Voice Volume'
export const setVoiceVolume = (volume: number) => action(SET_VOICE_VOLUME, { volume })
export type SetVoiceVolume = ReturnType<typeof setVoiceVolume>

export const SET_VOICE_MUTE = 'Set Voice Mute'
export const setVoiceMute = (mute: boolean) => action(SET_VOICE_MUTE, { mute })
export type SetVoiceMute = ReturnType<typeof setVoiceMute>

export const SET_VOICE_POLICY = 'Set Voice Policy'
export const setVoicePolicy = (voicePolicy: VoicePolicy) => action(SET_VOICE_POLICY, { voicePolicy })
export type SetVoicePolicy = ReturnType<typeof setVoicePolicy>
