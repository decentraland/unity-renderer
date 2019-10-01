import { Profile, RootPassportState, Wearable } from './types'

export const getProfileDownloadServer = (store: RootPassportState) => store.passports.profileServer

export const getProfile = (store: RootPassportState, userId: string): Profile | null =>
  store.passports &&
  store.passports.userInfo &&
  store.passports.userInfo[userId] &&
  store.passports.userInfo[userId].status === 'ok'
    ? (store.passports.userInfo[userId].data as Profile)
    : null

export const getEthereumAddress = (store: RootPassportState, userId: string): string | undefined =>
  store.passports &&
  store.passports.userInfo &&
  store.passports.userInfo[userId] &&
  store.passports.userInfo[userId].status === 'ok'
    ? (store.passports.userInfo[userId].data as Profile).ethAddress
    : undefined

export const getInventory = (store: RootPassportState, userId: string): Wearable[] | null =>
  store.passports &&
  store.passports.userInventory &&
  store.passports.userInventory[userId] &&
  store.passports.userInventory[userId].status === 'ok'
    ? ((store.passports.userInventory[userId] as any).data as Wearable[])
    : null

export const getPlatformCatalog = (store: RootPassportState): Wearable[] | null =>
  store.passports &&
  store.passports.catalogs &&
  store.passports.catalogs['base-avatars'] &&
  store.passports.catalogs['base-avatars'].status === 'ok'
    ? (store.passports.catalogs['base-avatars'].data as Wearable[])
    : null

export const getExclusiveCatalog = (store: RootPassportState): Wearable[] | null =>
  store.passports.catalogs &&
  store.passports.catalogs['base-exclusive'] &&
  store.passports.catalogs['base-exclusive'].status === 'ok'
    ? (store.passports.catalogs['base-exclusive'].data as Wearable[])
    : null

export const baseCatalogsLoaded = (store: RootPassportState) =>
  store.passports.catalogs &&
  store.passports.catalogs['base-exclusive'] &&
  store.passports.catalogs['base-exclusive'].status === 'ok' &&
  store.passports.catalogs &&
  store.passports.catalogs['base-exclusive'] &&
  store.passports.catalogs['base-exclusive'].status === 'ok'
