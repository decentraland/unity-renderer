import { action } from 'typesafe-actions'
import { VoicePolicy } from './types'
import { VoiceHandler } from './VoiceHandler'

export const JOIN_VOICE_CHAT = '[VC] JoinVoiceChat'
export const joinVoiceChat = () => action(JOIN_VOICE_CHAT, {})
export type JoinVoiceChatAction = ReturnType<typeof joinVoiceChat>

export const LEAVE_VOICE_CHAT = '[VC] LeaveVoiceChat'
export const leaveVoiceChat = () => action(LEAVE_VOICE_CHAT, {})
export type LeaveVoiceChatAction = ReturnType<typeof leaveVoiceChat>

export const VOICE_PLAYING_UPDATE = '[VC] voicePlayingUpdate'
export const voicePlayingUpdate = (userId: string, playing: boolean) =>
  action(VOICE_PLAYING_UPDATE, { userId, playing })
export type VoicePlayingUpdateAction = ReturnType<typeof voicePlayingUpdate>

export const SET_VOICE_CHAT_HANDLER = '[VC] setVoiceChatHandler'
export const setVoiceChatHandler = (voiceHandler: VoiceHandler | null) =>
  action(SET_VOICE_CHAT_HANDLER, { voiceHandler })
export type SetVoiceChatHandlerAction = ReturnType<typeof setVoiceChatHandler>

export const SET_VOICE_CHAT_ERROR = '[VC] setVoiceChatError'
export const setVoiceChatError = (message: string) => action(SET_VOICE_CHAT_ERROR, { message })
export const clearVoiceChatError = () => action(SET_VOICE_CHAT_ERROR, { message: null })
export type SetVoiceChatErrorAction = ReturnType<typeof setVoiceChatError>

/**
 * Action to trigger voice chat recording
 */
export const REQUEST_VOICE_CHAT_RECORDING = '[VC] requestVoiceChatRecording'
export const requestVoiceChatRecording = (recording: boolean) => action(REQUEST_VOICE_CHAT_RECORDING, { recording })
export type RequestVoiceChatRecordingAction = ReturnType<typeof requestVoiceChatRecording>

/**
 * Action triggered when recording starts or stops
 */
export const VOICE_RECORDING_UPDATE = '[VC] voiceRecordingUpdate'
export const voiceRecordingUpdate = (recording: boolean) => action(VOICE_RECORDING_UPDATE, { recording })
export type VoiceRecordingUpdateAction = ReturnType<typeof voiceRecordingUpdate>

export const SET_VOICE_CHAT_VOLUME = '[VC] setVoiceChatVolume'
export const setVoiceChatVolume = (volume: number) => action(SET_VOICE_CHAT_VOLUME, { volume })
export type SetVoiceChatVolumeAction = ReturnType<typeof setVoiceChatVolume>

export const SET_VOICE_CHAT_MUTE = '[VC] setVoiceChatMute'
export const setVoiceChatMute = (mute: boolean) => action(SET_VOICE_CHAT_MUTE, { mute })
export type SetVoiceChatMuteAction = ReturnType<typeof setVoiceChatMute>

export const SET_VOICE_CHAT_POLICY = '[VC] setVoiceChatPolicy'
export const setVoiceChatPolicy = (policy: VoicePolicy) => action(SET_VOICE_CHAT_POLICY, { policy })
export type SetVoiceChatPolicyAction = ReturnType<typeof setVoiceChatPolicy>

export const SET_VOICE_CHAT_MEDIA = '[VC] setVoiceChatMedia'
export const setVoiceChatMedia = (media: MediaStream | undefined) => action(SET_VOICE_CHAT_MEDIA, { media })
export type SetVoiceChatMediaAction = ReturnType<typeof setVoiceChatMedia>

export const SET_AUDIO_DEVICE = 'Set audio device'
export const setAudioDevice = (devices: { inputDeviceId?: string; outputDeviceId?: string }) =>
  action(SET_AUDIO_DEVICE, { devices })
export type SetAudioDevice = ReturnType<typeof setAudioDevice>

export type VoiceChatActions =
  | JoinVoiceChatAction
  | LeaveVoiceChatAction
  | VoicePlayingUpdateAction
  | SetVoiceChatHandlerAction
  | SetVoiceChatErrorAction
  | RequestVoiceChatRecordingAction
  | VoiceRecordingUpdateAction
  | SetVoiceChatVolumeAction
  | SetVoiceChatMuteAction
  | SetVoiceChatPolicyAction
  | SetVoiceChatMediaAction
  | SetAudioDevice
