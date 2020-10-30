import { Profile } from './types'
import { getFromLocalStorage, saveToLocalStorage } from 'atomicHelpers/localStorage'
import { getDefaultTLD } from 'config'

const LOCAL_PROFILES_KEY = 'dcl-local-profile'

export class LocalProfilesRepository {
  private _profiles: Record<string, Profile> = {}

  persist(address: string, profile: Profile) {
    this._profiles[address] = {
      ...profile
    }

    // For now, we use local storage. BUT DON'T USE THIS KEY OUTSIDE BECAUSE THIS MIGHT CHANGE EVENTUALLY
    saveToLocalStorage(this.profileKey(address), this._profiles[address])
  }

  get(address: string) {
    if (this._profiles[address]) {
      return { ...this._profiles[address] }
    }

    const profile: Profile | null = getFromLocalStorage(this.profileKey(address))

    if (profile) {
      this._profiles[address] = { ...profile }
    }

    return profile
  }

  private profileKey(address: string): string {
    return `${LOCAL_PROFILES_KEY}-${getDefaultTLD()}-${address}`
  }
}
