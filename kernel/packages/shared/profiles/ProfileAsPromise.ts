import { RootState, StoreContainer } from '../store/rootTypes'
import { Store } from 'redux'
import { getProfile } from './selectors'
import { profileRequest } from './actions'
import { Profile } from './types'

declare const globalThis: StoreContainer

export function ProfileAsPromise(userId: string, version?: number): Promise<Profile> {
  const store: Store<RootState> = globalThis.globalStore

  const existingProfile = getProfile(store.getState(), userId)
  if (existingProfile && (!version || existingProfile.version >= version)) {
    return Promise.resolve(existingProfile)
  }
  return new Promise(resolve => {
    const unsubscribe = store.subscribe(() => {
      const profile = getProfile(store.getState(), userId)
      if (profile) {
        unsubscribe()
        return resolve(profile)
      }
      // TODO (eordano, 16/Sep/2019): Timeout or catch errors
    })
    store.dispatch(profileRequest(userId))
  })
}
