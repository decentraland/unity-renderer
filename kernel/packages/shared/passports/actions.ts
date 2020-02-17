import { action } from 'typesafe-actions'
import { Profile, Wearable, Catalog, WearableId } from './types'

export const PASSPORT_REQUEST = '[Request] Passport fetch'
export const PASSPORT_SUCCESS = '[Success] Passport fetch'
export const PASSPORT_FAILURE = '[Failure] Passport fetch'
export const PASSPORT_RANDOM = '[?] Passport randomized'

export const SET_PROFILE_SERVER = 'Set avatar profile server'
export const setProfileServer = (url: string) => action(SET_PROFILE_SERVER, { url })
export type SetProfileServerAction = ReturnType<typeof setProfileServer>

export const passportRequest = (userId: string) => action(PASSPORT_REQUEST, { userId })
export const passportSuccess = (userId: string, profile: Profile) => action(PASSPORT_SUCCESS, { userId, profile })
export const passportFailure = (userId: string, error: any) => action(PASSPORT_FAILURE, { userId, error })
export const passportRandom = (userId: string, profile: Profile) => action(PASSPORT_RANDOM, { userId, profile })

export type PassportRandomAction = ReturnType<typeof passportRandom>
export type PassportRequestAction = ReturnType<typeof passportRequest>
export type PassportSuccessAction = ReturnType<typeof passportSuccess>
export type PassportFailureAction = ReturnType<typeof passportFailure>

export const SET_BASE_WEARABLES_CATALOG = 'Set base wearables catalog'
export const setBaseWearablesCatalog = (wearables: Wearable[]) => action(SET_BASE_WEARABLES_CATALOG, wearables)
export const SET_BASE_EXCLUSIVES_CATALOG = 'Set base exclusives catalog'
export const setBaseExclusivesCatalog = (wearables: Wearable[]) => action(SET_BASE_EXCLUSIVES_CATALOG, wearables)

export const ADD_CATALOG = 'Add Catalog'
export const addCatalog = (name: string, catalog: Catalog) => action(ADD_CATALOG, { name, catalog })
export type AddCatalogAction = ReturnType<typeof addCatalog>
export const CATALOG_LOADED = 'Catalog Loaded'
export const catalogLoaded = (name: string) => action(CATALOG_LOADED, { name })
export type CatalogLoadedAction = ReturnType<typeof catalogLoaded>

export const INVENTORY_REQUEST = '[Request] Inventory fetch'
export const inventoryRequest = (userId: string, ethAddress: string) =>
  action(INVENTORY_REQUEST, { userId, ethAddress })
export type InventoryRequest = ReturnType<typeof inventoryRequest>

export const INVENTORY_SUCCESS = '[Success] Inventory fetch'
export const inventorySuccess = (userId: string, inventory: WearableId[]) =>
  action(INVENTORY_SUCCESS, { userId, inventory })
export type InventorySuccess = ReturnType<typeof inventorySuccess>

export const INVENTORY_FAILURE = '[Failure] Inventory fetch'
export const inventoryFailure = (userId: string, error: any) => action(INVENTORY_FAILURE, { userId, error })
export type InventoryFailure = ReturnType<typeof inventoryFailure>

export const SAVE_AVATAR_REQUEST = '[Request] Save Avatar'
export const SAVE_AVATAR_SUCCESS = '[Success] Save Avatar'
export const SAVE_AVATAR_FAILURE = '[Failure] Save Avatar'

export const NOTIFY_NEW_INVENTORY_ITEM = '[Inventory] New inventory item'
export const notifyNewInventoryItem = () => action(NOTIFY_NEW_INVENTORY_ITEM)
export type NotifyNewInventoryItem = ReturnType<typeof notifyNewInventoryItem>

export const saveAvatarRequest = (profile: Partial<Profile>, userId?: string) =>
  action(SAVE_AVATAR_REQUEST, { userId, profile })
export const saveAvatarSuccess = (userId: string, version: number, profile: Profile) =>
  action(SAVE_AVATAR_SUCCESS, { userId, version, profile })
export const saveAvatarFailure = (userId: string, error: any) => action(SAVE_AVATAR_FAILURE, { userId, error })

export type SaveAvatarRequest = ReturnType<typeof saveAvatarRequest>
export type SaveAvatarSuccess = ReturnType<typeof saveAvatarSuccess>
export type SaveAvatarFailure = ReturnType<typeof saveAvatarFailure>
