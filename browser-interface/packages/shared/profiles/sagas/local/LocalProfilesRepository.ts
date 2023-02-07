import { Avatar } from '@dcl/schemas'
import {
  getFromPersistentStorage,
  removeFromPersistentStorage,
  saveToPersistentStorage
} from 'lib/browser/persistentStorage'
import { ETHEREUM_NETWORK } from 'config'

const LOCAL_PROFILES_KEY = 'dcl-local-profile'

export class LocalProfilesRepository {
  async persist(address: string, network: ETHEREUM_NETWORK, profile: Avatar) {
    await saveToPersistentStorage(this.profileKey(address, network), profile)
  }

  async remove(address: string, network: ETHEREUM_NETWORK) {
    await removeFromPersistentStorage(this.profileKey(address, network))
  }

  async get(address: string, network: ETHEREUM_NETWORK): Promise<unknown> {
    return getFromPersistentStorage(this.profileKey(address, network))
  }

  private profileKey(address: string, network: ETHEREUM_NETWORK): string {
    return `${LOCAL_PROFILES_KEY}-${network}-${address}`
  }
}
