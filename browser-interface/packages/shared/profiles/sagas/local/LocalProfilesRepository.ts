import { Avatar } from '@dcl/schemas'
import {
  getFromPersistentStorage,
  removeFromPersistentStorage,
  saveToPersistentStorage
} from 'lib/browser/persistentStorage'
import { ETHEREUM_NETWORK } from 'config'
import {ProfileHash} from "../../types";

const LOCAL_PROFILES_KEY = 'dcl-local-profile'
const LOCAL_PROFILE_HASH_KEY = 'dcl-local-profile-hash'

export class LocalProfilesRepository {
  async persist(address: string, network: ETHEREUM_NETWORK, profile: Avatar) {
    await saveToPersistentStorage(this.profileKey(address, network), profile)
  }

  async persistHash(address: string, network: ETHEREUM_NETWORK, hash: ProfileHash) {
    await saveToPersistentStorage(this.hashKey(address, network), hash)
  }

  async remove(address: string, network: ETHEREUM_NETWORK) {
    await removeFromPersistentStorage(this.profileKey(address, network))
    await removeFromPersistentStorage(this.hashKey(address, network))
  }

  async get(address: string, network: ETHEREUM_NETWORK): Promise<unknown> {
    return getFromPersistentStorage(this.profileKey(address, network))
  }

  async getHash(address: string, network: ETHEREUM_NETWORK): Promise<ProfileHash> {
    return getFromPersistentStorage(this.hashKey(address, network))
  }

  private profileKey(address: string, network: ETHEREUM_NETWORK): string {
    return `${LOCAL_PROFILES_KEY}-${network}-${address}`
  }

  private hashKey(address: string, network: ETHEREUM_NETWORK): string {
    return `${LOCAL_PROFILE_HASH_KEY}-${network}-${address}`
  }
}
