import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { Observable } from 'babylonjs'

export const getUserProfile = () => getFromLocalStorage('dcl-profile') || {}
export const getBlockedUsers: () => Set<string> = () => new Set(getFromLocalStorage('dcl-blocked-users') || [])
export const getMutedUsers: () => Set<string> = () => new Set(getFromLocalStorage('dcl-muted-users') || [])
export const isMuted = (name: string) => getMutedUsers().has(name)

export function addToBlockedUsers(publicKey: string): Set<string> {
  const blockedUsers = getBlockedUsers()

  if (!blockedUsers.has(publicKey)) {
    const updatedSet = blockedUsers.add(publicKey)
    saveToLocalStorage('dcl-blocked-users', Array.from(updatedSet))

    return updatedSet
  }

  return blockedUsers
}

export function removeFromBlockedUsers(publicKey: string): Set<string> {
  const blockedUsers = getBlockedUsers()
  blockedUsers.delete(publicKey)
  saveToLocalStorage('dcl-blocked-users', Array.from(blockedUsers))

  return blockedUsers
}

export function addToMutedUsers(publicKey: string): Set<string> {
  const mutedUsers = getMutedUsers()

  if (!mutedUsers.has(publicKey)) {
    const updatedSet = mutedUsers.add(publicKey)
    saveToLocalStorage('dcl-muted-users', Array.from(updatedSet))

    return updatedSet
  }

  return mutedUsers
}

export function removeFromMutedUsers(publicKey: string): Set<string> {
  const mutedUsers = getMutedUsers()
  mutedUsers.delete(publicKey)
  saveToLocalStorage('dcl-muted-users', Array.from(mutedUsers))

  return mutedUsers
}

/**
 * Ensuring the backward-compatibility after we started using Sets
 */
export function ensureLocalStorageStructure() {
  const blockedUsers = getFromLocalStorage('dcl-blocked-users')
  const mutedUsers = getFromLocalStorage('dcl-muted-users')

  // Migrate to new structure
  if (!Array.isArray(blockedUsers)) {
    saveToLocalStorage('dcl-blocked-users', [])
  }

  if (!Array.isArray(mutedUsers)) {
    saveToLocalStorage('dcl-muted-users', [])
  }
}

export const ProfileEvent = {
  MUTE: 'MUTE',
  UNMUTE: 'UNMUTE',
  BLOCK: 'BLOCK',
  UNBLOCK: 'UNBLOCK',
  ADD_FRIEND: 'ADD_FRIEND',
  SHOW_PROFILE: 'SHOW_PROFILE',
  HIDE_PROFILE: 'HIDE_PROFILE'
}

export const profileObservable = new Observable()
