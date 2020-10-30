import { RootState, StoreContainer } from '../store/rootTypes'
import { Store } from 'redux'
import { getProfile, getProfileStatusAndData } from './selectors'
import { profileRequest } from './actions'
import { Profile, ProfileType } from './types'

declare const globalThis: StoreContainer

export function ProfileAsPromise(userId: string, version?: number, profileType?: ProfileType): Promise<Profile> {
  function isExpectedVersion(aProfile: Profile) {
    return !version || aProfile.version >= version || aProfile.version === -1 // We signal random profiles with -1
  }

  const store: Store<RootState> = globalThis.globalStore

  const [status, existingProfile] = getProfileStatusAndData(store.getState(), userId)
  const existingProfileWithCorrectVersion = existingProfile && isExpectedVersion(existingProfile)
  if (existingProfile && existingProfileWithCorrectVersion && status === 'ok') {
    return Promise.resolve(existingProfile)
  }
  return new Promise((resolve, reject) => {
    const unsubscribe = store.subscribe(() => {
      const [status, data] = getProfileStatusAndData(store.getState(), userId)

      if (status === 'error') {
        unsubscribe()
        return reject(data)
      }

      const profile = getProfile(store.getState(), userId)
      if (profile && isExpectedVersion(profile) && status === 'ok') {
        unsubscribe()
        return resolve(profile)
      }
    })
    store.dispatch(profileRequest(userId, profileType))
  })
}

export function EnsureProfile(userId: string, version?: number): Promise<Profile> {
  const store: Store<RootState> = globalThis.globalStore
  const existingProfile = getProfile(store.getState(), userId)
  const existingProfileWithCorrectVersion = existingProfile && (!version || existingProfile.version >= version)
  if (existingProfile && existingProfileWithCorrectVersion) {
    return Promise.resolve(existingProfile)
  }
  return new Promise((resolve) => {
    const unsubscribe = store.subscribe(() => {
      const profile = getProfile(store.getState(), userId)
      if (profile) {
        unsubscribe()
        return resolve(profile)
      }
    })
  })
}
