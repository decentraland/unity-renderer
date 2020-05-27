import { Profile, RootProfileState, Wearable } from './types'
import { RootDaoState } from '../dao/types'

export const getProfileDownloadServer = (store: RootDaoState) => store.dao.profileServer

export const getProfile = (store: RootProfileState, userId: string): Profile | null =>
  store.profiles &&
  store.profiles.userInfo &&
  store.profiles.userInfo[userId] &&
  store.profiles.userInfo[userId].status === 'ok'
    ? (store.profiles.userInfo[userId].data as Profile)
    : null

export const hasConnectedWeb3 = (store: RootProfileState, userId: string): boolean =>
  store.profiles &&
  store.profiles.userInfo &&
  store.profiles.userInfo[userId] &&
  store.profiles.userInfo[userId].status === 'ok'
    ? !!store.profiles.userInfo[userId].hasConnectedWeb3
    : false

export const findProfileByName = (store: RootProfileState, userName: string): Profile | null =>
  store.profiles && store.profiles.userInfo
    ? Object.values(store.profiles.userInfo)
        .filter(user => user.status === 'ok')
        .find(user => user.data.name?.toLowerCase() === userName.toLowerCase())?.data
    : null

export const isAddedToCatalog = (store: RootProfileState, userId: string): boolean =>
  store.profiles &&
    store.profiles.userInfo &&
    store.profiles.userInfo[userId] &&
    store.profiles.userInfo[userId].status === 'ok' ? !!store.profiles.userInfo[userId].addedToCatalog : false

export const getEthereumAddress = (store: RootProfileState, userId: string): string | undefined =>
  store.profiles &&
  store.profiles.userInfo &&
  store.profiles.userInfo[userId] &&
  store.profiles.userInfo[userId].status === 'ok'
    ? (store.profiles.userInfo[userId].data as Profile).ethAddress
    : undefined

export const getInventory = (store: RootProfileState, userId: string): Wearable[] | null =>
  store.profiles &&
  store.profiles.userInventory &&
  store.profiles.userInventory[userId] &&
  store.profiles.userInventory[userId].status === 'ok'
    ? ((store.profiles.userInventory[userId] as any).data as Wearable[])
    : null

export const getPlatformCatalog = (store: RootProfileState): Wearable[] | null =>
  store.profiles &&
  store.profiles.catalogs &&
  store.profiles.catalogs['base-avatars'] &&
  store.profiles.catalogs['base-avatars'].status === 'ok'
    ? (store.profiles.catalogs['base-avatars'].data as Wearable[])
    : null

export const getExclusiveCatalog = (store: RootProfileState): Wearable[] | null =>
  store.profiles.catalogs &&
  store.profiles.catalogs['base-exclusive'] &&
  store.profiles.catalogs['base-exclusive'].status === 'ok'
    ? (store.profiles.catalogs['base-exclusive'].data as Wearable[])
    : null

export const baseCatalogsLoaded = (store: RootProfileState) =>
  store.profiles.catalogs &&
  store.profiles.catalogs['base-avatars'] &&
  store.profiles.catalogs['base-avatars'].status === 'ok' &&
  store.profiles.catalogs['base-exclusive'] &&
  store.profiles.catalogs['base-exclusive'].status === 'ok'
