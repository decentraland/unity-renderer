import { Avatar } from '@dcl/schemas'
import { isFeatureToggleEnabled } from 'shared/selectors'
import { isFriend } from 'shared/friends/selectors'
import { RootFriendsState } from 'shared/friends/types'
import { getBannedUsers } from 'shared/meta/selectors'
import { BannedUsers, RootMetaState } from 'shared/meta/types'
import { getCurrentUserProfile, getProfile } from 'shared/profiles/selectors'
import { RootProfileState } from 'shared/profiles/types'
import { RootVoiceChatState, VoicePolicy } from './types'
import { VOICE_CHAT_FEATURE_TOGGLE } from 'shared/types'
import { RootWorldState } from 'shared/world/types'
import { getCurrentIdentity } from 'shared/session/selectors'
import { RootSessionState } from 'shared/session/types'
import { getSceneWorkerBySceneID } from 'shared/world/parcelSceneManager'
import { store } from '../store/isolatedStore'

export const hasJoinedVoiceChat = (store: RootVoiceChatState) => store.voiceChat.joined

export const getVoiceChatState = (store: RootVoiceChatState) => store.voiceChat

export const isRequestedVoiceChatRecording = (store: RootVoiceChatState) => store.voiceChat.requestRecording

export const isVoiceChatRecording = (store: RootVoiceChatState) => store.voiceChat.recording

export const getVoicePolicy = (store: RootVoiceChatState) => store.voiceChat.policy

export const getVoiceHandler = (store: RootVoiceChatState) => store.voiceChat.voiceHandler

export function isVoiceChatAllowedByCurrentScene(store: RootVoiceChatState & RootWorldState) {
  const currentScene = store.world.currentScene ? getSceneWorkerBySceneID(store.world.currentScene) : undefined
  return isFeatureToggleEnabled(VOICE_CHAT_FEATURE_TOGGLE, currentScene?.loadableScene.entity.metadata)
}

export function isBlockedOrBanned(profile: Avatar, bannedUsers: BannedUsers, userId: string): boolean {
  return isBlocked(profile, userId) || isBannedFromChat(bannedUsers, userId)
}

function isBannedFromChat(bannedUsers: BannedUsers, userId: string): boolean {
  const bannedUser = bannedUsers[userId]
  return bannedUser && bannedUser.some((it) => it.type === 'VOICE_CHAT_AND_CHAT' && it.expiration > Date.now())
}

function isBlocked(profile: Avatar, userId: string): boolean {
  return !!profile.blocked && profile.blocked.includes(userId)
}

function hasBlockedMe(state: RootProfileState, myAddress: string | undefined, theirAddress: string): boolean {
  const profile = getProfile(state, theirAddress)

  return !!profile && !!myAddress && isBlocked(profile, myAddress)
}

function isMuted(profile: Avatar, userId: string): boolean {
  return !!profile.muted && profile.muted.includes(userId)
}

export function shouldPlayVoice(
  state: RootVoiceChatState & RootFriendsState & RootProfileState & RootMetaState & RootWorldState & RootSessionState,
  profile: Avatar,
  voiceUserId: string
) {
  const myAddress = getCurrentIdentity(state)?.address
  return (
    isVoiceAllowedByPolicy(state, voiceUserId) &&
    !isBlockedOrBanned(profile, getBannedUsers(state), voiceUserId) &&
    !isMuted(profile, voiceUserId) &&
    !hasBlockedMe(state, myAddress, voiceUserId) &&
    isVoiceChatAllowedByCurrentScene(state)
  )
}

export function isVoiceAllowedByPolicy(
  state: RootVoiceChatState & RootFriendsState & RootProfileState,
  voiceUserId: string
): boolean {
  const policy = getVoicePolicy(state)

  switch (policy) {
    case VoicePolicy.ALLOW_FRIENDS_ONLY:
      return isFriend(state, voiceUserId)
    case VoicePolicy.ALLOW_VERIFIED_ONLY:
      const theirProfile = getProfile(state, voiceUserId)
      return !!theirProfile?.hasClaimedName
    default:
      return true
  }
}
