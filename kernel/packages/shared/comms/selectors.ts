import { RootCommsState } from './types'

export const isVoiceChatRecording = (store: RootCommsState) => store.comms.voiceChatRecording

export const getVoicePolicy = (store: RootCommsState) => store.comms.voicePolicy
